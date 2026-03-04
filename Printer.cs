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

        // ── PSE Printer Device API ────────────────────────────────
        private const string PseBaseUrl = "https://pse-ems-staging.h6app.site";
        private const string PseForPrintUrl = PseBaseUrl + "/api/v1/printer-device/for-printing/{0}";
        private const string PseSharedSecret = "8f3c2a9d7b1e4f6c9a0d2e5f8b7c4a1d";

        // ── C# own MySQL (XAMPP high6middleware) ──────────────────
        private const string DbConn =
            "Server=localhost;Port=3306;Database=high6middleware;Uid=root;Pwd=;";

        // ── Detected & Verified ZPL Printers ─────────────────────
        // Key = printer_no (1-based), Value = (PrinterName, PortName, PrinterCode e.g. "PRINTER_001")
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
        // ⚙  Setup Button — open PrinterSetup form
        // ══════════════════════════════════════════════════════════
        private void btnSetup_Click(object sender, EventArgs e)
        {
            using var setup = new PrinterSetup();
            setup.ShowDialog(this);
            Log("ℹ Setup closed. Run 🔍 Detect Printers to apply new assignments.");
        }

        // ══════════════════════════════════════════════════════════
        // Test Connection
        // ══════════════════════════════════════════════════════════
        private async void btnTestConnection_Click(object sender, EventArgs e)
        {
            btnTestConnection.Enabled = false;
            btnTestConnection.Text = "Testing...";
            SetConnectionStatus("Testing...", System.Drawing.Color.Gray);

            // ── Test PSE API with a real signed request ───────────
            // Use PRINTER_001 as a probe (any valid printer code will do)
            try
            {
                long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string sig = GeneratePseSignature("", ts);
                string probeUrl = string.Format(PseForPrintUrl, "PRINTER_001");

                var req = new HttpRequestMessage(HttpMethod.Get, probeUrl);
                req.Headers.Add("X-Printer-Signature", sig);
                req.Headers.Add("X-Printer-Timestamp", ts.ToString());
                req.Headers.Add("Accept", "application/json");

                var res = await _httpClient.SendAsync(req);

                if (res.IsSuccessStatusCode || (int)res.StatusCode == 401)
                {
                    // 401 still means the server is reachable (auth issue ≠ offline)
                    SetConnectionStatus("Online", System.Drawing.Color.FromArgb(39, 174, 96));
                    Log($"✓ PSE API reachable — HTTP {(int)res.StatusCode}");
                }
                else
                {
                    SetConnectionStatus($"API Error ({(int)res.StatusCode})",
                        System.Drawing.Color.FromArgb(192, 57, 43));
                    Log($"✗ PSE API responded: {(int)res.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                SetConnectionStatus("API Offline", System.Drawing.Color.FromArgb(192, 57, 43));
                Log("✗ PSE API unreachable: " + ex.Message);
            }

            // ── Test MySQL ─────────────────────────────────────────
            try
            {
                using var conn = new MySqlConnection(DbConn);
                await conn.OpenAsync();
                Log("✓ MySQL (high6middleware) connected.");
            }
            catch (Exception ex)
            {
                Log("✗ MySQL error: " + ex.Message);
            }

            btnTestConnection.Enabled = true;
            btnTestConnection.Text = "Test Connection";
        }

        // ══════════════════════════════════════════════════════════
        // HMAC-SHA256 Signature  (body + timestamp, GET body = "")
        // ══════════════════════════════════════════════════════════
        private static string GeneratePseSignature(string body, long timestamp)
        {
            string toSign = body + timestamp.ToString();
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(PseSharedSecret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        // ══════════════════════════════════════════════════════════
        // Detect Printers Button
        // ══════════════════════════════════════════════════════════
        private async void btnDetectPrinters_Click(object sender, EventArgs e)
        {
            btnDetectPrinters.Enabled = false;
            btnDetectPrinters.Text = "Scanning...";

            _detectedPrinters.Clear();

            Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log("🔍 Scanning for active ZPL printers (DB-matched)...");

            _detectedPrinters = await Task.Run(() => DetectZplPrinters());

            Log("──────────────────────────────────────────────────────");
            if (_detectedPrinters.Count > 0)
            {
                Log("✅ " + _detectedPrinters.Count + " active ZPL printer(s) ready:");
                foreach (var kv in _detectedPrinters)
                    Log($"   Printer {kv.Key} [{kv.Value.PrinterCode}] → \"{kv.Value.PrinterName}\"  [{kv.Value.PortName}]");
            }
            else
            {
                Log("❌ No active ZPL printers matched.");
                Log("   ➜ Open ⚙ Setup to assign printers to slots, then try again.");
            }
            Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            btnDetectPrinters.Enabled = true;
            btnDetectPrinters.Text = "🔍  Detect Printers";
        }

        // ══════════════════════════════════════════════════════════
        // DetectZplPrinters — DB-driven matching
        // ══════════════════════════════════════════════════════════
        private Dictionary<int, (string PrinterName, string PortName, string PrinterCode)> DetectZplPrinters()
        {
            var result = new Dictionary<int, (string, string, string)>();

            // ── Step 1: Load configured assignments from DB ───────
            // Returns: printer_number → (printer_name, printer_code)
            var dbAssignments = LoadDbPrinterAssignments();

            if (dbAssignments.Count == 0)
            {
                Log("⚠ No printer assignments found in DB.");
                Log("   ➜ Click ⚙ Setup to assign physical printers to each slot.");
                return result;
            }

            // ── Step 2: Build WMI snapshot of all ZPL printers ───
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

            // ── Step 3: Match DB assignments → WMI results ────────
            foreach (var kv in dbAssignments)
            {
                int printerNo = kv.Key;
                string configured = kv.Value.PrinterName;   // Windows printer name
                string printerCode = kv.Value.PrinterCode;   // e.g. "PRINTER_001"

                if (wmiPrinters.TryGetValue(configured, out var info))
                {
                    if (info.Online)
                    {
                        Log($"   ✓ Printer {printerNo} [{printerCode}] → \"{info.Name}\" [{info.Port}]");
                        result[printerNo] = (info.Name, info.Port, printerCode);
                    }
                    else
                    {
                        Log($"   ⚠ Printer {printerNo} [{printerCode}] → \"{info.Name}\" is OFFLINE or has an error.");
                    }
                }
                else
                {
                    Log($"   ✗ Printer {printerNo} [{printerCode}] → \"{configured}\" NOT FOUND on this PC.");
                    Log($"      ➜ Check cable / driver, or open ⚙ Setup to reassign.");
                }
            }

            return result;
        }

        // ── Load printer_number → (printer_name, printer_code) from DB ──
        private Dictionary<int, (string PrinterName, string PrinterCode)> LoadDbPrinterAssignments()
        {
            var map = new Dictionary<int, (string, string)>();
            try
            {
                using var conn = new MySqlConnection(DbConn);
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
        // ══════════════════════════════════════════════════════════
        private void PrintLabel(string name, string company, string participantNo,
                                string qrCodeUrl, int printerNo)
        {
            if (!_detectedPrinters.TryGetValue(printerNo, out var info))
            {
                Log($"✗ Printer {printerNo} not in active list. Run 🔍 Detect Printers first.");
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

        // ── Smarter word wrap helper ──────────────────────────────────
        private (string Line1, string Line2) SplitIntoTwoLines(string text, int threshold)
        {
            if (text.Length <= threshold) return (text, "");

            // Try to find best split point near the middle
            int mid = text.Length / 2;
            int splitPos = -1;

            // Search outward from center for a space
            for (int i = 0; i <= mid; i++)
            {
                if (mid + i < text.Length && text[mid + i] == ' ') { splitPos = mid + i; break; }
                if (mid - i >= 0 && text[mid - i] == ' ') { splitPos = mid - i; break; }
            }

            // No space found at all → hard split at midpoint
            if (splitPos < 0) splitPos = mid;

            return (text.Substring(0, splitPos).Trim(), text.Substring(splitPos).Trim());
        }

        // ── Build full ZPL label ──────────────────────────────────
        private string BuildZplLabel(string name, string company, string participantNo, string qrCodeUrl)
        {
            string qrZpl = BuildQrSection(qrCodeUrl, participantNo);

            // ── NAME font + wrap ──────────────────────────────────────
            int nameFontSize;
            string nameLine1, nameLine2;

            if (name.Length <= 10) { nameFontSize = 72; (nameLine1, nameLine2) = (name, ""); }
            else if (name.Length <= 16) { nameFontSize = 58; (nameLine1, nameLine2) = (name, ""); }
            else if (name.Length <= 22) { nameFontSize = 46; (nameLine1, nameLine2) = (name, ""); }
            else if (name.Length <= 32) { nameFontSize = 42; (nameLine1, nameLine2) = SplitIntoTwoLines(name, 22); }
            else { nameFontSize = 36; (nameLine1, nameLine2) = SplitIntoTwoLines(name, 18); }

            // ── COMPANY font + wrap ───────────────────────────────────
            int compFontSize;
            string compLine1, compLine2;

            if (company.Length <= 14) { compFontSize = 38; (compLine1, compLine2) = (company, ""); }
            else if (company.Length <= 20) { compFontSize = 30; (compLine1, compLine2) = (company, ""); }
            else if (company.Length <= 30) { compFontSize = 24; (compLine1, compLine2) = (company, ""); }
            else { compFontSize = 22; (compLine1, compLine2) = SplitIntoTwoLines(company, 20); }

            // ── Y positions ───────────────────────────────────────────
            int leftMargin = 22;
            int partFontSize = 28;
            int partY = 122;

            int nameY = partY + partFontSize + 20;
            int name2Y = nameY + nameFontSize + 8;

            int compY = (nameLine2 != "" ? name2Y + nameFontSize : nameY + nameFontSize) + 18;
            int comp2Y = compY + compFontSize + 6;

            var zpl = new StringBuilder();

            // ── Config block ──────────────────────────────────────────
            zpl.AppendLine("^XA");
            zpl.AppendLine("~TA000");
            zpl.AppendLine("~JSN");
            zpl.AppendLine("^LT0");
            zpl.AppendLine("^MNW");
            zpl.AppendLine("^MTT");
            zpl.AppendLine("^PON");
            zpl.AppendLine("^PMN");
            zpl.AppendLine("^LH0,0");
            zpl.AppendLine("^JMA");
            zpl.AppendLine("^PR8,8");
            zpl.AppendLine("~SD15");
            zpl.AppendLine("^JUS");
            zpl.AppendLine("^LRN");
            zpl.AppendLine("^CI28");
            zpl.AppendLine("^PA0,1,1,0");
            zpl.AppendLine("^XZ");

            // ── Label block ───────────────────────────────────────────
            zpl.AppendLine("^XA");
            zpl.AppendLine("^MMT");
            zpl.AppendLine("^PW711");
            zpl.AppendLine("^LL406");
            zpl.AppendLine("^LS0");

            // Borders
            zpl.AppendLine("^FO7,5^GB695,394,15^FS");
            zpl.AppendLine("^FO9,5^GB693,100,11^FS");

            // Logo GRF (static branding in header)
            zpl.AppendLine("^FO33,31^GFA,569,1440,36,:Z64:eJzl07Fu2zAQBuATOHDk2qn3GhkC6bVURIhkaPDoR/CryJPHvgKFPIDP6FAWJXi5o2TFjp3aewlwIPEN/Hl3AP/xarvru1eg82PB/q6xTNemvTSOw12Du3jrjZcGmrumlAcmOAaEIwEXb1RCgHYgGxyuqIiTeTLc/Y6tbMOrQ1DzMhBG1+6DSWoajTZw0m15xxELghdPVXJ8iI67yTj2zLotj5zU/BDDjjktBntK1d6nyq3H+JoNUVO5NzlshtkUFLH3EY0ZQ+l6DzXRN3S7UKKdTVWEgMYH15mRJhMI0K2oLI2fjCSZjRNj+wHqmqD9wrCavZiGLKup+LNZL+ZZc52Zp8Vsfokx+w7qktrthymhXMy2V3MQ812S5vfM9WqWXFuTDagJZwZBf19NRFRTqHGzwVP/yD+rSRVu1HA2MWfHdTf1odRLDTNqLukUqC1xzuXym6Wfpe7ZSAU/TDsbrYXMhfSPmr/R/jkZQ9VPNTbXXfpbbnMuMuNRTJMNuo2YIl028Wk+bvT3lbkxS59N8YDBR8yN2b4y/gHT/cN8vd4Bm3FU6Q==:9959");

            // QR box border
            zpl.AppendLine("^FO494,94^GB206,303,11^FS");

            // QR code (downloaded from PSE CDN or fallback)
            zpl.AppendLine(qrZpl);

            // Participant No
            if (!string.IsNullOrEmpty(participantNo))
                zpl.AppendLine($"^FO{leftMargin},{partY}^A0N,{partFontSize},{partFontSize}^FD{participantNo}^FS");

            // Name
            zpl.AppendLine($"^FO{leftMargin},{nameY}^A0N,{nameFontSize},{nameFontSize}^FD{nameLine1}^FS");
            if (!string.IsNullOrEmpty(nameLine2))
                zpl.AppendLine($"^FO{leftMargin},{name2Y}^A0N,{nameFontSize},{nameFontSize}^FD{nameLine2}^FS");

            // Company
            zpl.AppendLine($"^FO{leftMargin},{compY}^A0N,{compFontSize},{compFontSize}^FD{compLine1}^FS");
            if (!string.IsNullOrEmpty(compLine2))
                zpl.AppendLine($"^FO{leftMargin},{comp2Y}^A0N,{compFontSize},{compFontSize}^FD{compLine2}^FS");

            zpl.AppendLine("^PQ1,0,1,Y");
            zpl.AppendLine("^XZ");

            return zpl.ToString();
        }

        // ── Download QR from PSE URL → convert to ZPL GRF ────────
        private string BuildQrSection(string qrCodeUrl, string participantNo)
        {
            int qrSize = 180;
            int qrX = 710 - qrSize - 15;   // right side, 15-dot margin = 515
            int qrY = (406 - qrSize) / 2;  // vertically centered = 113

            if (!string.IsNullOrEmpty(qrCodeUrl))
            {
                try
                {
                    // Download the QR image from the PSE CDN URL
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
                    Log("⚠ QR image download failed, falling back to ^BQ: " + ex.Message);
                }
            }

            // Fallback: native ZPL QR using participant_no as data
            string qrData = !string.IsNullOrEmpty(participantNo) ? participantNo : "PARTICIPANT";
            return $"^FO{qrX},{qrY}\n^BQN,2,5\n^FDLA,{qrData}^FS\n";
        }

        // ── Bitmap → ZPL GRF hex ─────────────────────────────────
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
        // Start / Pause / End
        // ══════════════════════════════════════════════════════════
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_detectedPrinters.Count == 0)
            {
                Log("⚠ No active printers detected. Please run 🔍 Detect Printers before starting.");
                return;
            }

            _isRunning = true;
            _isPaused = false;

            btnStart.Enabled = false;
            btnPause.Enabled = true;
            btnEnd.Enabled = true;

            Log("▶ Service started — polling PSE API every 3 seconds per printer...");

            _cts = new CancellationTokenSource();
            _ = PollLoopAsync(_cts.Token);
        }

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
            btnPause.Enabled = false;
            btnEnd.Enabled = false;
            btnPause.Text = "⏸  Pause";
            btnPause.BackColor = System.Drawing.Color.FromArgb(243, 156, 18);

            Log("⏹ Service stopped.");
        }

        // ══════════════════════════════════════════════════════════
        // Poll Loop — calls each detected printer's own endpoint
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

        // ══════════════════════════════════════════════════════════
        // Fetch from PSE API for every detected printer → Print
        // ══════════════════════════════════════════════════════════
        private async Task FetchAndPrintAllPrintersAsync()
        {
            // Each PC only polls the printers it has physically detected
            foreach (var kv in _detectedPrinters)
            {
                int printerNo = kv.Key;
                string printerCode = kv.Value.PrinterCode;   // e.g. "PRINTER_001"

                await FetchAndPrintForPrinterAsync(printerNo, printerCode);
            }
        }

        private async Task FetchAndPrintForPrinterAsync(int printerNo, string printerCode)
        {
            try
            {
                // ── Build signed request ──────────────────────────
                long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string sig = GeneratePseSignature("", ts);   // GET body is empty
                string url = string.Format(PseForPrintUrl, printerCode);

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
                if (count == 0) return;   // nothing queued — silent

                Log($"↓ [{printerCode}] {count} participant(s) to print.");

                foreach (var p in participants.EnumerateArray())
                    await ProcessParticipantAsync(p, printerNo, printerCode);
            }
            catch (Exception ex)
            {
                Log($"✗ [{printerCode}] Poll error: " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════
        // Process one participant from the PSE API response
        // ══════════════════════════════════════════════════════════
        private async Task ProcessParticipantAsync(JsonElement p, int printerNo, string printerCode)
        {
            // PSE response fields:
            //   id            → stored as job_id in our DB (serves as dedup key)
            //   printer_id    → e.g. "PRINTER_001"
            //   name          → full name
            //   company       → company name
            //   participant_no→ e.g. "P202600123"
            //   qr_code_path  → full URL to QR PNG on PSE CDN

            //string pseId = p.GetProperty("id").GetString();
            var idProp = p.GetProperty("id");
            string pseId = idProp.ValueKind switch
            {
                JsonValueKind.String => idProp.GetString(),           // alphanumeric string → get as string
                JsonValueKind.Number => idProp.GetInt64().ToString(), // numeric → convert to string cleanly
                _ => idProp.GetRawText().Trim('"')                    // fallback: strip any stray quotes
            };
            string name = p.GetProperty("name").GetString();
            string company = p.GetProperty("company").GetString();
            string participantNo = null;
            if (p.TryGetProperty("participant_no", out var pno))
            {
                participantNo = pno.ValueKind == JsonValueKind.String ? pno.GetString()
                              : pno.ValueKind == JsonValueKind.Number ? pno.GetRawText()
                              : null;
            }
            string qrCodeUrl = p.TryGetProperty("qr_code_path", out var qr) && qr.ValueKind != JsonValueKind.Null
                                   ? qr.GetString() : null;

            // Save to local DB — use PSE's `id` as our job_id (dedup key).
            // qr_code_url stores the PSE CDN URL; the actual GRF bytes are downloaded at print time.
            bool saved = await SaveToLocalDbAsync(pseId, name, company, participantNo, qrCodeUrl, printerNo);

            if (saved)
            {
                // Download QR image from PSE CDN and send to printer
                PrintLabel(name, company, participantNo, qrCodeUrl, printerNo);
                Log($"✓ Printed [{printerCode}] — {name} | {company} | {participantNo} (id: {pseId})");
            }
            else
            {
                // Already in DB = already printed (PSE server should not resend, but guard anyway)
                Log($"⚠ [{printerCode}] Entry {pseId} already in local DB — skipped.");
            }
        }

        // ══════════════════════════════════════════════════════════
        // Save to high6middleware DB
        // job_id   = PSE's `id` field  (e.g. "01JABC123456789")
        // qr_code_url = PSE CDN URL    (downloaded fresh at print time)
        // ══════════════════════════════════════════════════════════
        private async Task<bool> SaveToLocalDbAsync(
            string pseId, string name, string company,
            string participantNo, string qrCodeUrl, int printerNo)
        {
            try
            {
                using var conn = new MySqlConnection(DbConn);
                await conn.OpenAsync();

                // ── Duplicate check on PSE id ─────────────────────
                using var checkCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM print_jobs WHERE job_id = @jobId;", conn);
                checkCmd.Parameters.AddWithValue("@jobId", pseId);
                if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0) return false;

                // ── Resolve printer_number → printers.id (FK) ─────
                using var printerCmd = new MySqlCommand(
                    "SELECT id FROM printers WHERE printer_number = @num LIMIT 1;", conn);
                printerCmd.Parameters.AddWithValue("@num", printerNo);
                var printerIdObj = await printerCmd.ExecuteScalarAsync();

                if (printerIdObj == null)
                {
                    Log($"✗ Printer number {printerNo} not found in printers table.");
                    return false;
                }

                int printerId = Convert.ToInt32(printerIdObj);

                // ── Insert ────────────────────────────────────────
                // job_id      = PSE's id  (dedup / audit trail)
                // qr_code_url = PSE CDN URL (not a local path; downloaded at print time)
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

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log("✗ DB save error: " + ex.Message);
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