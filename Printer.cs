using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Management;

namespace High6
{
    public partial class Printer : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private CancellationTokenSource _cts = null;

        // ── Runtime-loaded settings (from AppSettings) ────────────
        private string _pseBaseUrl = "";
        private string _pseSharedSecret = "";
        private string _pseForPrintUrl => _pseBaseUrl.TrimEnd('/') + "/api/v1/printer-device/for-printing/{0}";

        // ── DB connection string — loaded from DbSettings ─────────
        // Never hardcoded; always read from AppSettings at startup.
        private string _dbConn = "";

        // ── Detected & Verified ZPL Printers ─────────────────────
        // Key = printer_no (1-based), Value = (PrinterName, PortName, PrinterCode)
        private Dictionary<int, (string PrinterName, string PortName, string PrinterCode)> _detectedPrinters
            = new Dictionary<int, (string, string, string)>();

        private bool _isRunning = false;
        private bool _isPaused = false;

        public Printer()
        {
            InitializeComponent();
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        // ══════════════════════════════════════════════════════════
        // Form Load — read DB & API settings from local config
        // ══════════════════════════════════════════════════════════
        private void Printer_Load(object sender, EventArgs e)
        {
            LoadAppSettings();
            Log("ℹ  High6 Printer Service ready.");
            Log("   1. Click ⚙ Setup DB to configure your database connection.");
            Log("   2. Click ⚙ Setup Printers to assign physical printers.");
            Log("   3. Click ▶ Start — it will test, detect, then poll automatically.");
        }

        // ══════════════════════════════════════════════════════════
        // Load settings from AppSettings.json (next to the .exe)
        // ══════════════════════════════════════════════════════════
        private void LoadAppSettings()
        {
            try
            {
                string path = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "AppSettings.json");

                if (!System.IO.File.Exists(path))
                {
                    Log("⚠ AppSettings.json not found — using empty defaults.");
                    Log("   ➜ Click ⚙ Setup DB to create your settings.");
                    return;
                }

                string json = System.IO.File.ReadAllText(path);
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // DB
                string host = root.TryGetProperty("db_host", out var h) ? h.GetString() : "192.168.0.71";
                string port = root.TryGetProperty("db_port", out var po) ? po.GetString() : "3306";
                string db = root.TryGetProperty("db_name", out var d) ? d.GetString() : "high6middleware";
                string user = root.TryGetProperty("db_user", out var u) ? u.GetString() : "root";
                string pass = root.TryGetProperty("db_pass", out var pw) ? pw.GetString() : "";

                _dbConn = $"Server={host};Port={port};Database={db};Uid={user};Pwd={pass};";

                // API
                _pseBaseUrl = root.TryGetProperty("api_base_url", out var url) ? url.GetString() : "";
                _pseSharedSecret = root.TryGetProperty("api_secret", out var sec) ? sec.GetString() : "";

                Log($"✓ Settings loaded — DB: {host}:{port}/{db}");
                Log($"✓ API endpoint: {_pseBaseUrl}");
            }
            catch (Exception ex)
            {
                Log("✗ Failed to load AppSettings.json: " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════
        // ⚙ Setup DB Button — open DbSetup form
        // ══════════════════════════════════════════════════════════
        private void btnSetupDb_Click(object sender, EventArgs e)
        {
            using var setup = new DbSetup();
            if (setup.ShowDialog(this) == DialogResult.OK)
            {
                LoadAppSettings();   // reload after save
                Log("✓ Database settings updated and reloaded.");
            }
        }

        // ══════════════════════════════════════════════════════════
        // ⚙ Setup Printers Button — open PrinterSetup form
        // ══════════════════════════════════════════════════════════
        private void btnSetupPrinters_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_dbConn))
            {
                MessageBox.Show("Please configure the database connection first (⚙ Setup DB).",
                                "No DB Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var setup = new PrinterSetup(_dbConn);
            setup.ShowDialog(this);
            Log("ℹ  Printer setup closed.");
        }

        // ══════════════════════════════════════════════════════════
        // HMAC-SHA256 Signature  (body + timestamp, GET body = "")
        // ══════════════════════════════════════════════════════════
        private string GeneratePseSignature(string body, long timestamp)
        {
            string toSign = body + timestamp.ToString();
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_pseSharedSecret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        // ══════════════════════════════════════════════════════════
        // Start Button — Test → Detect → Poll
        // ══════════════════════════════════════════════════════════
        private async void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStart.Text = "Starting…";

            Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log("▶ Starting High6 Printer Service…");

            // ── Step 1: Validate settings ─────────────────────────
            if (string.IsNullOrWhiteSpace(_dbConn) ||
                string.IsNullOrWhiteSpace(_pseBaseUrl) ||
                string.IsNullOrWhiteSpace(_pseSharedSecret))
            {
                Log("✗ Missing settings. Please configure ⚙ Setup DB and ⚙ Setup Printers first.");
                btnStart.Enabled = true;
                btnStart.Text = "▶  Start";
                return;
            }

            // ── Step 2: Test API connection ───────────────────────
            Log("── Step 1/3 — Testing API connection…");
            bool apiOk = await TestApiConnectionAsync();
            if (!apiOk)
            {
                Log("✗ Cannot reach the PSE API. Check your endpoint or network.");
                Log("   ➜ You can update the API URL in ⚙ Setup DB.");
                btnStart.Enabled = true;
                btnStart.Text = "▶  Start";
                return;
            }
            Log("✓ API reachable.");

            // ── Step 3: Test DB connection ────────────────────────
            Log("── Step 2/3 — Testing database connection…");
            bool dbOk = await TestDbConnectionAsync();
            if (!dbOk)
            {
                Log("✗ Cannot connect to the database. Check your DB settings (⚙ Setup DB).");
                btnStart.Enabled = true;
                btnStart.Text = "▶  Start";
                return;
            }
            Log("✓ Database connected.");

            // ── Step 4: Detect printers ───────────────────────────
            Log("── Step 3/3 — Detecting active ZPL printers…");
            SetConnectionStatus("Detecting…", System.Drawing.Color.Gray);

            _detectedPrinters.Clear();
            _detectedPrinters = await Task.Run(() => DetectZplPrinters());

            if (_detectedPrinters.Count == 0)
            {
                Log("✗ No active ZPL printers detected. Check cables or run ⚙ Setup Printers.");
                SetConnectionStatus("No Printers", System.Drawing.Color.FromArgb(192, 57, 43));
                btnStart.Enabled = true;
                btnStart.Text = "▶  Start";
                return;
            }

            Log($"✓ {_detectedPrinters.Count} printer(s) ready:");
            foreach (var kv in _detectedPrinters)
                Log($"   Printer {kv.Key} [{kv.Value.PrinterCode}] → \"{kv.Value.PrinterName}\"");

            SetConnectionStatus("Online", System.Drawing.Color.FromArgb(39, 174, 96));

            // ── Step 5: Begin polling ─────────────────────────────
            _isRunning = true;
            _isPaused = false;

            btnPause.Enabled = true;
            btnEnd.Enabled = true;

            Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log("✅ Service started — polling every 3 seconds per printer.");

            _cts = new CancellationTokenSource();
            _ = PollLoopAsync(_cts.Token);
        }

        // ── Test API (returns true = reachable) ───────────────────
        private async Task<bool> TestApiConnectionAsync()
        {
            try
            {
                long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string sig = GeneratePseSignature("", ts);
                string probeUrl = string.Format(_pseForPrintUrl, "PRINTER_001");

                var req = new HttpRequestMessage(HttpMethod.Get, probeUrl);
                req.Headers.Add("X-Printer-Signature", sig);
                req.Headers.Add("X-Printer-Timestamp", ts.ToString());
                req.Headers.Add("Accept", "application/json");

                var res = await _httpClient.SendAsync(req);

                // 401 = server alive but auth rejected — still "reachable"
                if (res.IsSuccessStatusCode || (int)res.StatusCode == 401 || (int)res.StatusCode == 404)
                {
                    Log($"   API responded: HTTP {(int)res.StatusCode}");
                    return true;
                }

                Log($"   API error: HTTP {(int)res.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Log("   API exception: " + ex.Message);
                return false;
            }
        }

        // ── Test DB ───────────────────────────────────────────────
        private async Task<bool> TestDbConnectionAsync()
        {
            try
            {
                using var conn = new MySqlConnection(_dbConn);
                await conn.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log("   DB exception: " + ex.Message);
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════
        // Pause / End
        // ══════════════════════════════════════════════════════════
        private void btnPause_Click(object sender, EventArgs e)
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                btnPause.Text = "▶  Resume";
                btnPause.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
                Log("⏸ Polling paused.");
            }
            else
            {
                btnPause.Text = "⏸  Pause";
                btnPause.BackColor = System.Drawing.Color.FromArgb(243, 156, 18);
                Log("▶ Polling resumed.");
            }
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            _isRunning = false;
            _isPaused = false;

            btnStart.Enabled = true;
            btnStart.Text = "▶  Start";
            btnPause.Enabled = false;
            btnEnd.Enabled = false;
            btnPause.Text = "⏸  Pause";
            btnPause.BackColor = System.Drawing.Color.FromArgb(243, 156, 18);

            SetConnectionStatus("Stopped", System.Drawing.Color.FromArgb(127, 140, 141));
            Log("⏹ Service stopped.");
        }

        // ══════════════════════════════════════════════════════════
        // DetectZplPrinters — DB-driven matching
        // ══════════════════════════════════════════════════════════
        private Dictionary<int, (string PrinterName, string PortName, string PrinterCode)> DetectZplPrinters()
        {
            var result = new Dictionary<int, (string, string, string)>();
            var dbAssignments = LoadDbPrinterAssignments();

            if (dbAssignments.Count == 0)
            {
                Log("⚠ No printer assignments in DB. Use ⚙ Setup Printers to assign them.");
                return result;
            }

            var wmiPrinters = new Dictionary<string, (string Name, string Port, bool Online)>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, DriverName, PortName, PrinterStatus, WorkOffline, DetectedErrorState " +
                    "FROM Win32_Printer");

                foreach (ManagementObject printer in searcher.Get())
                {
                    string name = printer["Name"]?.ToString() ?? "";
                    string driver = printer["DriverName"]?.ToString() ?? "";
                    string port = printer["PortName"]?.ToString() ?? "";
                    bool offline = printer["WorkOffline"] is bool b && b;
                    string status = printer["PrinterStatus"]?.ToString() ?? "0";
                    string err = printer["DetectedErrorState"]?.ToString() ?? "0";

                    bool isZebra =
                        ContainsAny(name.ToLower(), "zebra", "zpl", " zd", " zt", " zp", "tlp", "lp28", "gk", "gz") ||
                        ContainsAny(driver.ToLower(), "zebra", "zpl", "zdesigner");

                    if (!isZebra || string.IsNullOrWhiteSpace(name)) continue;

                    bool isUsb = port.StartsWith("USB", StringComparison.OrdinalIgnoreCase);
                    bool isReady = status == "3" || status == "4" || status == "5";
                    bool noError = err == "0" || err == "2";
                    bool isOnline = !offline && isReady && noError && isUsb;

                    wmiPrinters[name] = (name, port, isOnline);
                }
            }
            catch (Exception ex)
            {
                Log("✗ WMI query failed: " + ex.Message);
                return result;
            }

            foreach (var kv in dbAssignments)
            {
                int printerNo = kv.Key;
                string configured = kv.Value.PrinterName;
                string printerCode = kv.Value.PrinterCode;

                if (wmiPrinters.TryGetValue(configured, out var info))
                {
                    if (info.Online)
                    {
                        Log($"   ✓ Printer {printerNo} [{printerCode}] → \"{info.Name}\" [{info.Port}]");
                        result[printerNo] = (info.Name, info.Port, printerCode);
                    }
                    else
                        Log($"   ⚠ Printer {printerNo} [{printerCode}] → \"{info.Name}\" is OFFLINE.");
                }
                else
                {
                    Log($"   ✗ Printer {printerNo} [{printerCode}] → \"{configured}\" NOT FOUND.");
                    Log($"      ➜ Check cable/driver or use ⚙ Setup Printers to reassign.");
                }
            }

            return result;
        }

        private Dictionary<int, (string PrinterName, string PrinterCode)> LoadDbPrinterAssignments()
        {
            var map = new Dictionary<int, (string, string)>();
            try
            {
                using var conn = new MySqlConnection(_dbConn);
                conn.Open();

                var sql = "SELECT printer_number, printer_name, printer_code " +
                          "FROM printers " +
                          "WHERE printer_name IS NOT NULL AND printer_name <> '';";

                using var cmd = new MySqlCommand(sql, conn);
                using var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    int num = rdr.GetByte(0);
                    string name = rdr.GetString(1);
                    string code = rdr.IsDBNull(2) ? $"PRINTER_{num:D3}" : rdr.GetString(2);
                    map[num] = (name, code);
                }
            }
            catch (Exception ex)
            {
                Log("⚠ Could not load printer assignments from DB: " + ex.Message);
            }
            return map;
        }

        // ══════════════════════════════════════════════════════════
        // Print Label via Windows Spooler
        // 3" × 2"  =  812 × 609 dots  @  203 dpi  (standard ZPL resolution)
        //             (3 × 203 = 609 width,  2 × 203 = 406 … rounded to 812×609
        //              or use 300 dpi: 900×600 — we'll use 203 dpi standard)
        // Final chosen: PW=609, LL=406  (3"W × 2"H at 203 dpi)
        // ══════════════════════════════════════════════════════════
        private void PrintLabel(string name, string company, string participantNo,
                                string qrCodeUrl, int printerNo)
        {
            if (!_detectedPrinters.TryGetValue(printerNo, out var info))
            {
                Log($"✗ Printer {printerNo} not in active list. Restart service to redetect.");
                return;
            }

            string zpl = BuildZplLabel(name, company, participantNo, qrCodeUrl);

            try
            {
                bool sent = RawPrinterHelper.SendStringToPrinter(info.PrinterName, zpl);
                if (sent)
                    Log($"🖨 Printed on \"{info.PrinterName}\" [{info.PrinterCode}]");
                else
                    Log($"✗ Spooler rejected job for \"{info.PrinterName}\"");
            }
            catch (Exception ex)
            {
                Log($"✗ Print error on \"{info.PrinterName}\": " + ex.Message);
            }
        }

        // ── Word-wrap helper ──────────────────────────────────────
        private (string Line1, string Line2) SplitIntoTwoLines(string text, int threshold)
        {
            if (text.Length <= threshold) return (text, "");

            int mid = text.Length / 2;
            int splitPos = -1;

            for (int i = 0; i <= mid; i++)
            {
                if (mid + i < text.Length && text[mid + i] == ' ') { splitPos = mid + i; break; }
                if (mid - i >= 0 && text[mid - i] == ' ') { splitPos = mid - i; break; }
            }

            if (splitPos < 0) splitPos = mid;
            return (text.Substring(0, splitPos).Trim(), text.Substring(splitPos).Trim());
        }

        // ══════════════════════════════════════════════════════════
        //  ZPL LABEL — 3" W × 2" H  @ 203 dpi
        //  PW = 609  (3 × 203)
        //  LL = 406  (2 × 203)
        //
        //  Layout:
        //    Outer border  : ^FO5,5^GB597,394,12
        //    Right QR panel: ^FO405,5^GB199,392,9   (x=405..604)
        //    Left text area : x=18..395, y=15..390  (377w × 375h)
        // ══════════════════════════════════════════════════════════
        //private string BuildZplLabel(string name, string company, string participantNo, string qrCodeUrl)
        //{
        //    int labelWidth = 609;
        //    int labelHeight = 406;

        //    int boxLeft = 5;
        //    int boxTop = 5;
        //    int boxW = 597;
        //    int boxH = 394;
        //    int borderThk = 8;

        //    int contentWidth = 540;
        //    int contentX = (labelWidth - contentWidth) / 2;

        //    // Name font (slightly bigger)
        //    int nameFont = 64;

        //    // Company font
        //    int companyFont = 40;

        //    // QR
        //    int qrSize = 130;
        //    int qrX = (labelWidth - qrSize) / 2;

        //    int nameY = 80;
        //    int companyY = 170;
        //    int qrY = 220;

        //    string qrZpl = BuildQrSection(qrCodeUrl, participantNo, qrX, qrY, qrSize);

        //    var zpl = new StringBuilder();

        //    zpl.AppendLine("^XA");
        //    zpl.AppendLine("^PW609");
        //    zpl.AppendLine("^LL406");
        //    zpl.AppendLine("^CI28");

        //    // Border
        //    zpl.AppendLine($"^FO{boxLeft},{boxTop}^GB{boxW},{boxH},{borderThk}^FS");

        //    // NAME (centered)
        //    zpl.AppendLine($"^FO{contentX},{nameY}");
        //    zpl.AppendLine($"^A0N,{nameFont},{nameFont}");
        //    zpl.AppendLine($"^FB{contentWidth},2,10,C");
        //    zpl.AppendLine($"^FD{EscapeZpl(name)}^FS");

        //    // COMPANY (centered)
        //    if (!string.IsNullOrEmpty(company))
        //    {
        //        zpl.AppendLine($"^FO{contentX},{companyY}");
        //        zpl.AppendLine($"^A0N,{companyFont},{companyFont}");
        //        zpl.AppendLine($"^FB{contentWidth},2,6,C");
        //        zpl.AppendLine($"^FD{EscapeZpl(company)}^FS");
        //    }

        //    // QR
        //    zpl.AppendLine(qrZpl);

        //    zpl.AppendLine("^PQ1,0,1,Y");
        //    zpl.AppendLine("^XZ");

        //    return zpl.ToString();
        //}

        //update wrap layout without overlapping text

        // update layout with wrapping
        //private string BuildZplLabel(string name, string company, string participantNo, string qrCodeUrl)
        //{
        //    int labelWidth = 609;
        //    int labelHeight = 406;
        //    int boxLeft = 5;
        //    int boxTop = 5;
        //    int boxW = 597;
        //    int boxH = 394;
        //    int borderThk = 8;

        //    int contentWidth = 540;
        //    int contentX = (labelWidth - contentWidth) / 2;

        //    // ── Font sizes ────────────────────────────────────────────
        //    // Name: scale down if very long
        //    int nameFont;
        //    if (name.Length <= 14) nameFont = 64;
        //    else if (name.Length <= 20) nameFont = 54;
        //    else if (name.Length <= 28) nameFont = 44;
        //    else nameFont = 36;

        //    // Company: scale down if very long
        //    int compFont;
        //    if (company.Length <= 18) compFont = 38;
        //    else if (company.Length <= 26) compFont = 32;
        //    else if (company.Length <= 36) compFont = 26;
        //    else compFont = 22;

        //    // ── Estimate chars per line at each font size ─────────────
        //    // At 203dpi, ZPL A0 font: approx contentWidth / (fontSize * 0.6)
        //    int nameCharsPerLine = (int)(contentWidth / (nameFont * 0.60));
        //    int compCharsPerLine = (int)(contentWidth / (compFont * 0.60));

        //    int nameLines = name.Length <= nameCharsPerLine ? 1 : 2;
        //    int compLines = company.Length <= compCharsPerLine ? 1 : 2;

        //    int nameLineGap = 8;
        //    int compLineGap = 6;

        //    int nameBlockH = nameLines == 1 ? nameFont : nameFont * 2 + nameLineGap;
        //    int compBlockH = compLines == 1 ? compFont : compFont * 2 + compLineGap;

        //    // ── QR size ───────────────────────────────────────────────
        //    int qrSize = 120;

        //    // ── Gaps between sections ─────────────────────────────────
        //    int gapNameComp = 16;
        //    int gapCompQr = 18;
        //    int topPad = 14;
        //    int bottomPad = 14;

        //    // ── Total content height ──────────────────────────────────
        //    int totalContent = topPad + nameBlockH + gapNameComp + compBlockH + gapCompQr + qrSize + bottomPad;

        //    // Compress gaps if overflow
        //    int available = boxH - borderThk * 2;
        //    if (totalContent > available)
        //    {
        //        gapNameComp = 8;
        //        gapCompQr = 10;
        //        topPad = 8;
        //        bottomPad = 8;
        //        totalContent = topPad + nameBlockH + gapNameComp + compBlockH + gapCompQr + qrSize + bottomPad;
        //    }

        //    // ── Vertically center the whole block inside the box ─────
        //    int innerTop = boxTop + borderThk;
        //    int startY = innerTop + (available - totalContent) / 2;
        //    if (startY < innerTop + 4) startY = innerTop + 4;

        //    int nameY = startY + topPad;
        //    int compY = nameY + nameBlockH + gapNameComp;
        //    int qrY = compY + compBlockH + gapCompQr;
        //    int qrX = (labelWidth - qrSize) / 2;

        //    string qrZpl = BuildQrSection(qrCodeUrl, participantNo, qrX, qrY, qrSize);

        //    var zpl = new StringBuilder();

        //    // Config block
        //    zpl.AppendLine("^XA");
        //    zpl.AppendLine("^PW609");
        //    zpl.AppendLine("^LL406");
        //    zpl.AppendLine("^CI28");

        //    // Single border box
        //    zpl.AppendLine($"^FO{boxLeft},{boxTop}^GB{boxW},{boxH},{borderThk}^FS");

        //    // ── NAME — centered, bold (drawn twice +1px), wraps via ^FB ──
        //    // Line 1
        //    zpl.AppendLine($"^FO{contentX},{nameY}^A0N,{nameFont},{nameFont}^FB{contentWidth},1,0,C^FD{EscapeZpl(nameLines == 1 ? name : GetLine(name, nameCharsPerLine, 1))}^FS");
        //    zpl.AppendLine($"^FO{contentX + 1},{nameY}^A0N,{nameFont},{nameFont}^FB{contentWidth},1,0,C^FD{EscapeZpl(nameLines == 1 ? name : GetLine(name, nameCharsPerLine, 1))}^FS");

        //    // Line 2 (if wrapped)
        //    if (nameLines == 2)
        //    {
        //        int name2Y = nameY + nameFont + nameLineGap;
        //        string nameLine2 = GetLine(name, nameCharsPerLine, 2);
        //        zpl.AppendLine($"^FO{contentX},{name2Y}^A0N,{nameFont},{nameFont}^FB{contentWidth},1,0,C^FD{EscapeZpl(nameLine2)}^FS");
        //        zpl.AppendLine($"^FO{contentX + 1},{name2Y}^A0N,{nameFont},{nameFont}^FB{contentWidth},1,0,C^FD{EscapeZpl(nameLine2)}^FS");
        //    }

        //    // ── COMPANY — centered, wraps via ^FB ────────────────────
        //    if (!string.IsNullOrEmpty(company))
        //    {
        //        zpl.AppendLine($"^FO{contentX},{compY}^A0N,{compFont},{compFont}^FB{contentWidth},1,0,C^FD{EscapeZpl(compLines == 1 ? company : GetLine(company, compCharsPerLine, 1))}^FS");

        //        if (compLines == 2)
        //        {
        //            int comp2Y = compY + compFont + compLineGap;
        //            string compLine2 = GetLine(company, compCharsPerLine, 2);
        //            zpl.AppendLine($"^FO{contentX},{comp2Y}^A0N,{compFont},{compFont}^FB{contentWidth},1,0,C^FD{EscapeZpl(compLine2)}^FS");
        //        }
        //    }

        //    // ── QR code ───────────────────────────────────────────────
        //    zpl.AppendLine(qrZpl);

        //    zpl.AppendLine("^PQ1,0,1,Y");
        //    zpl.AppendLine("^XZ");

        //    return zpl.ToString();
        //}

        private string BuildZplLabel(string name, string company, string participantNo, string qrCodeUrl)
        {
            int labelWidth = 609;
            int boxLeft = 5;
            int boxTop = 5;
            int boxW = 597;
            int boxH = 394;
            int borderThk = 8;

            int innerLeft = boxLeft + borderThk;
            int innerTop = boxTop + borderThk;
            int innerRight = boxLeft + boxW - borderThk;
            int innerBottom = boxTop + boxH - borderThk;
            int innerW = innerRight - innerLeft;   // 581
            int innerH = innerBottom - innerTop;    // 378

            int padX = 14;   // horizontal padding from inner edge
            int padY = 10;   // vertical padding from inner edge

            // ═════════════════════════════════════════════════════════
            // NAME SECTION — full width, large font, up to 2 lines
            // ═════════════════════════════════════════════════════════
            int nameFont;
            if (name.Length <= 12) nameFont = 70;
            else if (name.Length <= 18) nameFont = 60;
            else if (name.Length <= 26) nameFont = 50;
            else nameFont = 42;

            int nameContentW = innerW - padX * 2;
            int nameCharsPerLine = (int)(nameContentW / (nameFont * 0.60));

            int nameLines = name.Length <= nameCharsPerLine ? 1 : 2;
            int nameLineGap = 8;
            int nameBlockH = nameLines == 1 ? nameFont : nameFont * 2 + nameLineGap;

            string nameLine1 = GetLine(name, nameCharsPerLine, 1);
            string nameLine2 = nameLines == 2 ? GetLine(name, nameCharsPerLine, 2) : "";

            // ═════════════════════════════════════════════════════════
            // BOTTOM SECTION — divides remaining height after name
            // ═════════════════════════════════════════════════════════
            int gapNameBottom = 16;   // gap between name block and bottom section

            // QR size — fill most of the remaining height
            int bottomH = innerH - padY - nameBlockH - gapNameBottom - padY;
            int qrSize = Math.Min(bottomH, 200);   // cap at 200 dots
            if (qrSize < 80) qrSize = 80;            // minimum readable size

            // Bottom section starts at:
            int bottomY = innerTop + padY + nameBlockH + gapNameBottom;

            // QR — right side, vertically centered in bottom section
            int qrPanelW = qrSize + padX * 2;
            int qrX = innerRight - padX - qrSize;
            int qrY = bottomY + (bottomH - qrSize) / 2;
            if (qrY < bottomY) qrY = bottomY;

            // ═════════════════════════════════════════════════════════
            // COMPANY — left of QR, up to 3 lines
            // ═════════════════════════════════════════════════════════
            int compAreaX = innerLeft + padX;
            int compAreaW = innerW - qrPanelW - padX * 2 - 10;   // 10 = gap between text and QR

            int compFont;
            if (company.Length <= 12) compFont = 40;
            else if (company.Length <= 20) compFont = 34;
            else if (company.Length <= 30) compFont = 28;
            else compFont = 24;

            int compCharsPerLine = (int)(compAreaW / (compFont * 0.60));
            int compLineGap = 8;

            // Split into up to 3 lines
            var compLinesList = SplitIntoLines(company, compCharsPerLine, 3);
            int compLines = compLinesList.Count;
            int compBlockH = compLines * compFont + (compLines - 1) * compLineGap;

            // Vertically center company text within bottom section (same as QR)
            int compY = bottomY + (bottomH - compBlockH) / 2;
            if (compY < bottomY) compY = bottomY;

            // ═════════════════════════════════════════════════════════
            // DIVIDER LINE — between name and bottom section
            // ═════════════════════════════════════════════════════════
            int dividerY = innerTop + padY + nameBlockH + gapNameBottom / 2;

            // ═════════════════════════════════════════════════════════
            // NAME Y — vertically centered in name section
            // ═════════════════════════════════════════════════════════
            int nameSectionH = nameBlockH + gapNameBottom;
            int nameY = innerTop + padY + (nameSectionH - nameBlockH) / 2;
            int nameContentX = innerLeft + padX;
            int name2Y = nameY + nameFont + nameLineGap;

            string qrZpl = BuildQrSection(qrCodeUrl, participantNo, qrX, qrY, qrSize);

            // ═════════════════════════════════════════════════════════
            // BUILD ZPL
            // ═════════════════════════════════════════════════════════
            var zpl = new StringBuilder();

            zpl.AppendLine("^XA");
            zpl.AppendLine("^PW609");
            zpl.AppendLine("^LL406");
            zpl.AppendLine("^CI28");

            // Outer border
            zpl.AppendLine($"^FO{boxLeft},{boxTop}^GB{boxW},{boxH},{borderThk}^FS");

            // Divider line (thin, full width inside box)
            zpl.AppendLine($"^FO{innerLeft},{dividerY}^GB{innerW},2,2^FS");

            // ── NAME (bold = drawn twice +1px offset) ────────────────
            zpl.AppendLine($"^FO{nameContentX},{nameY}^A0N,{nameFont},{nameFont}^FB{nameContentW},1,0,C^FD{EscapeZpl(nameLine1)}^FS");
            zpl.AppendLine($"^FO{nameContentX + 1},{nameY}^A0N,{nameFont},{nameFont}^FB{nameContentW},1,0,C^FD{EscapeZpl(nameLine1)}^FS");
            if (nameLines == 2)
            {
                zpl.AppendLine($"^FO{nameContentX},{name2Y}^A0N,{nameFont},{nameFont}^FB{nameContentW},1,0,C^FD{EscapeZpl(nameLine2)}^FS");
                zpl.AppendLine($"^FO{nameContentX + 1},{name2Y}^A0N,{nameFont},{nameFont}^FB{nameContentW},1,0,C^FD{EscapeZpl(nameLine2)}^FS");
            }

            // ── COMPANY (left panel, up to 3 lines) ──────────────────
            if (!string.IsNullOrEmpty(company))
            {
                for (int i = 0; i < compLinesList.Count; i++)
                {
                    int lineY = compY + i * (compFont + compLineGap);
                    zpl.AppendLine($"^FO{compAreaX},{lineY}^A0N,{compFont},{compFont}^FB{compAreaW},1,0,L^FD{EscapeZpl(compLinesList[i])}^FS");
                }
            }

            // ── QR (right panel) ─────────────────────────────────────
            zpl.AppendLine(qrZpl);

            zpl.AppendLine("^PQ1,0,1,Y");
            zpl.AppendLine("^XZ");

            return zpl.ToString();
        }

        // ── Split text into up to maxLines word-wrapped lines ─────────────
        private List<string> SplitIntoLines(string text, int charsPerLine, int maxLines)
        {
            var lines = new List<string>();
            string remaining = text.Trim();

            for (int i = 0; i < maxLines; i++)
            {
                if (remaining.Length == 0) break;

                if (remaining.Length <= charsPerLine || i == maxLines - 1)
                {
                    lines.Add(remaining);
                    break;
                }

                // Find best split point at or before charsPerLine
                int splitPos = charsPerLine;
                for (int j = charsPerLine; j >= 0; j--)
                {
                    if (j < remaining.Length && remaining[j] == ' ')
                    {
                        splitPos = j;
                        break;
                    }
                }

                lines.Add(remaining.Substring(0, splitPos).Trim());
                remaining = remaining.Substring(splitPos).Trim();
            }

            return lines;
        }

        // ── Splits text into word-wrapped lines, returns line N (1-based) ──

        private string GetLine(string text, int charsPerLine, int lineNumber)
        {
            if (text.Length <= charsPerLine)
                return lineNumber == 1 ? text : "";

            // Find best split point near the middle
            int mid = text.Length / 2;
            int splitPos = -1;

            for (int i = 0; i <= mid; i++)
            {
                if (mid + i < text.Length && text[mid + i] == ' ') { splitPos = mid + i; break; }
                if (mid - i >= 0 && text[mid - i] == ' ') { splitPos = mid - i; break; }
            }

            if (splitPos < 0) splitPos = charsPerLine; // hard split if no space found

            string line1 = text.Substring(0, splitPos).Trim();
            string line2 = text.Substring(splitPos).Trim();

            return lineNumber == 1 ? line1 : line2;
        }

        private static string EscapeZpl(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("^", " ").Replace("~", " ");
        }

        private string BuildQrSection(string qrCodeUrl, string participantNo, int qrX, int qrY, int qrSize)
        {
            if (!string.IsNullOrEmpty(qrCodeUrl))
            {
                try
                {
                    using var wc = new System.Net.WebClient();
                    byte[] imgBytes = wc.DownloadData(qrCodeUrl);

                    using var ms = new System.IO.MemoryStream(imgBytes);
                    using var bmp = new System.Drawing.Bitmap(ms);
                    using var resized = new System.Drawing.Bitmap(bmp, qrSize, qrSize);

                    string grf = BitmapToZplGrf(resized, out int totalBytes, out int rowBytes);
                    return $"^FO{qrX},{qrY}\n^GFA,{totalBytes},{totalBytes},{rowBytes},{grf}\n";
                }
                catch (Exception ex)
                {
                    Log("⚠ QR download failed, using ^BQ fallback: " + ex.Message);
                }
            }

            string qrData = !string.IsNullOrEmpty(participantNo) ? participantNo : "PARTICIPANT";
            return $"^FO{qrX},{qrY}\n^BQN,2,5\n^FH\\^FDLA,{qrData}^FS\n";
        }

        private string BitmapToZplGrf(System.Drawing.Bitmap bmp, out int totalBytes, out int rowBytes)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            rowBytes = (int)Math.Ceiling(width / 8.0);
            totalBytes = rowBytes * height;

            var sb = new StringBuilder();
            for (int y = 0; y < height; y++)
                for (int xByte = 0; xByte < rowBytes; xByte++)
                {
                    byte b = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int x = xByte * 8 + bit;
                        if (x < width)
                        {
                            var px = bmp.GetPixel(x, y);
                            if ((px.R + px.G + px.B) / 3 < 128)
                                b |= (byte)(0x80 >> bit);
                        }
                    }
                    sb.Append(b.ToString("X2"));
                }
            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════
        // Poll Loop
        // ══════════════════════════════════════════════════════════
        private async Task PollLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!_isPaused)
                    await FetchAndPrintAllPrintersAsync();

                try { await Task.Delay(3000, token); }
                catch { break; }
            }
        }

        private async Task FetchAndPrintAllPrintersAsync()
        {
            foreach (var kv in _detectedPrinters)
                await FetchAndPrintForPrinterAsync(kv.Key, kv.Value.PrinterCode);
        }

        private async Task FetchAndPrintForPrinterAsync(int printerNo, string printerCode)
        {
            try
            {
                long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string sig = GeneratePseSignature("", ts);
                string url = string.Format(_pseForPrintUrl, printerCode);

                var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Add("X-Printer-Signature", sig);
                req.Headers.Add("X-Printer-Timestamp", ts.ToString());
                req.Headers.Add("Accept", "application/json");

                var response = await _httpClient.SendAsync(req);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    Log($"✗ [{printerCode}] Auth error: {errBody}");
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    Log($"✗ [{printerCode}] HTTP {(int)response.StatusCode}");
                    return;
                }

                var body = await response.Content.ReadAsStringAsync();
                var root = JsonSerializer.Deserialize<JsonElement>(body);

                if (!root.TryGetProperty("participants", out var participants)) return;
                int count = participants.GetArrayLength();
                if (count == 0) return;

                Log($"↓ [{printerCode}] {count} participant(s) to print.");

                foreach (var p in participants.EnumerateArray())
                    await ProcessParticipantAsync(p, printerNo, printerCode);
            }
            catch (Exception ex)
            {
                Log($"✗ [{printerCode}] Poll error: " + ex.Message);
            }
        }

        private async Task ProcessParticipantAsync(JsonElement p, int printerNo, string printerCode)
        {
            var idProp = p.GetProperty("id");
            string pseId = idProp.ValueKind switch
            {
                JsonValueKind.String => idProp.GetString(),
                JsonValueKind.Number => idProp.GetInt64().ToString(),
                _ => idProp.GetRawText().Trim('"')
            };

            string name = p.GetProperty("name").GetString();
            string company = p.GetProperty("company").GetString();

            string participantNo = null;
            if (p.TryGetProperty("participant_no", out var pno))
                participantNo = pno.ValueKind == JsonValueKind.String ? pno.GetString()
                              : pno.ValueKind == JsonValueKind.Number ? pno.GetRawText()
                              : null;

            string qrCodeUrl = p.TryGetProperty("qr_code_path", out var qr) && qr.ValueKind != JsonValueKind.Null
                                   ? qr.GetString() : null;

            bool saved = await SaveToLocalDbAsync(pseId, name, company, participantNo, qrCodeUrl, printerNo);

            if (saved)
            {
                PrintLabel(name, company, participantNo, qrCodeUrl, printerNo);
                Log($"✓ Printed [{printerCode}] — {name} | {company} | {participantNo} (id: {pseId})");
            }
            else
            {
                Log($"⚠ [{printerCode}] Entry {pseId} already in local DB — skipped.");
            }
        }

        private async Task<bool> SaveToLocalDbAsync(
            string pseId, string name, string company,
            string participantNo, string qrCodeUrl, int printerNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pseId))
                {
                    Log("✗ DB save skipped: pseId is null/empty.");
                    return false;
                }

                using var conn = new MySqlConnection(_dbConn);
                await conn.OpenAsync();

                using var checkCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM print_jobs WHERE job_id = @jobId;", conn);
                checkCmd.Parameters.AddWithValue("@jobId", pseId);
                int existing = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (existing > 0) return false;

                using var printerCmd = new MySqlCommand(
                    "SELECT id FROM printers WHERE printer_number = @num LIMIT 1;", conn);
                printerCmd.Parameters.AddWithValue("@num", printerNo);
                var printerIdObj = await printerCmd.ExecuteScalarAsync();

                if (printerIdObj == null)
                {
                    Log($"✗ DB → printer_number {printerNo} not found in printers table.");
                    return false;
                }

                int printerId = Convert.ToInt32(printerIdObj);

                var sql = @"INSERT INTO print_jobs
                                (job_id, name, company_name, participant_no, qr_code_url, printer_id, created_at)
                            VALUES
                                (@jobId, @name, @company, @participantNo, @qrCodeUrl, @printerId, @now);";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@jobId", pseId);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@company", company);
                cmd.Parameters.AddWithValue("@participantNo", (object)participantNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@qrCodeUrl", (object)qrCodeUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@printerId", printerId);
                cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                Log($"✗ DB save error for job_id='{pseId}': {ex.Message}");
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════
        // Helpers
        // ══════════════════════════════════════════════════════════
        private static bool ContainsAny(string source, params string[] keywords)
        {
            foreach (var kw in keywords)
                if (source.Contains(kw)) return true;
            return false;
        }

        private void SetConnectionStatus(string text, System.Drawing.Color color)
        {
            lblConnectionDot.ForeColor = color;
            lblConnection.Text = text;
            lblConnection.ForeColor = color;
        }

        private void Log(string message)
        {
            if (txtStatus.InvokeRequired)
            {
                txtStatus.Invoke(new Action(() => Log(message)));
                return;
            }
            txtStatus.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + "\r\n");
            txtStatus.ScrollToCaret();
        }
    }
}