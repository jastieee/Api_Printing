using System;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using System.Drawing.Printing;

namespace High6
{
    public partial class PrintManual : Form
    {
        private readonly string _dbConn;
        private string _excelPath;
        private string _bartenderPath;
        private const string BartenderPathKey = "bartender_path";

        public PrintManual(string dbConn)
        {
            InitializeComponent();
            _dbConn = dbConn;

            ExcelPackage.License.SetNonCommercialPersonal("High6");

            _bartenderPath = LoadBartenderPath();
            txtBartenderPath.Text = _bartenderPath;

            _excelPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ManualRegistrations.xlsx");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadPrinters();
            LoadRegistrations();
            txtFullName.Focus();
        }

        // ══════════════════════════════════════════════════════════
        // Load printers from DB into combo
        // ══════════════════════════════════════════════════════════
        private void LoadPrinters()
        {
            cboPrinter.Items.Clear();

            try
            {
                using var conn = new MySqlConnection(_dbConn);
                conn.Open();

                const string sql = @"
                    SELECT printer_number, printer_name, printer_code
                    FROM printers
                    WHERE printer_name IS NOT NULL AND printer_name <> ''
                    ORDER BY printer_number;";

                using var cmd = new MySqlCommand(sql, conn);
                using var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    cboPrinter.Items.Add(new PrinterItem(
                        rdr.GetByte(0),
                        rdr.GetString(1),
                        rdr.IsDBNull(2) ? "" : rdr.GetString(2)
                    ));
                }

                if (cboPrinter.Items.Count > 0)
                    cboPrinter.SelectedIndex = 0;
                else
                {
                    cboPrinter.Items.Add("— No printers configured —");
                    cboPrinter.SelectedIndex = 0;
                    cboPrinter.Enabled = false;
                    btnSave.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load printers: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ══════════════════════════════════════════════════════════
        // Save — DB + Excel + Print
        // ══════════════════════════════════════════════════════════
        private void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtFullName.Text.Trim();
            string company = txtCompany.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a Full Name.", "Required",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (cboPrinter.SelectedItem is not PrinterItem selectedPrinter)
            {
                MessageBox.Show("Please select a printer.", "Required",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime now = DateTime.Now;

            int newId = SaveToDb(name, company, now);
            if (newId < 0) return;

            SaveToExcel(name, company, now);         // append to full history log
            WritePrintQueue(name, company, now);     // overwrite single-row print file
            PrintViaBartender(selectedPrinter.PrinterName);

            txtFullName.Clear();
            txtCompany.Clear();
            txtFullName.Focus();
            LoadRegistrations();
        }

        private void WritePrintQueue(string name, string company, DateTime now)
        {
            try
            {
                string queuePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ManualPrintQueue.xlsx");

                // Always delete and recreate so it has exactly ONE data row
                if (File.Exists(queuePath))
                    File.Delete(queuePath);

                FileInfo file = new FileInfo(queuePath);
                using var package = new ExcelPackage(file);

                var ws = package.Workbook.Worksheets.Add("Print");

                // Headers — must match exactly what your .btw template expects
                ws.Cells[1, 1].Value = "Full Name";
                ws.Cells[1, 2].Value = "Company Name";
                ws.Cells[1, 3].Value = "Created At";

                // Single data row
                ws.Cells[2, 1].Value = name;
                ws.Cells[2, 2].Value = company;
                ws.Cells[2, 3].Value = now.ToString("yyyy-MM-dd HH:mm:ss");

                ws.Column(1).Width = 32;
                ws.Column(2).Width = 32;
                ws.Column(3).Width = 22;

                package.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write print queue: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Insert into DB ────────────────────────────────────────
        private int SaveToDb(string name, string company, DateTime now)
        {
            try
            {
                using var conn = new MySqlConnection(_dbConn);
                conn.Open();

                const string sql = @"
                    INSERT INTO manual_registrations (name, company_name, created_at)
                    VALUES (@name, @company, @now);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@company", string.IsNullOrEmpty(company)
                                                        ? (object)DBNull.Value : company);
                cmd.Parameters.AddWithValue("@now", now.ToString("yyyy-MM-dd HH:mm:ss"));

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB save failed: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        // ── Append to Excel ───────────────────────────────────────
        private void SaveToExcel(string name, string company, DateTime now)
        {
            try
            {
                FileInfo file = new FileInfo(_excelPath);
                using var package = new ExcelPackage(file);

                var ws = package.Workbook.Worksheets["Registrations"]
                      ?? package.Workbook.Worksheets.Add("Registrations");

                if (ws.Dimension == null)
                {
                    ws.Cells[1, 1].Value = "Full Name";
                    ws.Cells[1, 2].Value = "Company Name";
                    ws.Cells[1, 3].Value = "Created At";

                    using var headerRange = ws.Cells[1, 1, 1, 3];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.FromArgb(22, 160, 133));
                    headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);

                    ws.Column(1).Width = 32;
                    ws.Column(2).Width = 32;
                    ws.Column(3).Width = 22;
                }

                int nextRow = (ws.Dimension?.End.Row ?? 1) + 1;

                ws.Cells[nextRow, 1].Value = name;
                ws.Cells[nextRow, 2].Value = company;
                ws.Cells[nextRow, 3].Value = now.ToString("yyyy-MM-dd HH:mm:ss");

                if (nextRow % 2 == 0)
                {
                    using var rowRange = ws.Cells[nextRow, 1, nextRow, 3];
                    rowRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.FromArgb(236, 240, 241));
                }

                package.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel save failed: " + ex.Message, "Excel Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ══════════════════════════════════════════════════════════
        // Silent print via Bartender — pass printer name via /PRN
        // ══════════════════════════════════════════════════════════
        // ── P/Invoke ──────────────────────────────────────────────
        // ── P/Invoke — correct declaration with EntryPoint ────────
        [System.Runtime.InteropServices.DllImport("winspool.drv",
            CharSet = System.Runtime.InteropServices.CharSet.Auto,
            SetLastError = true,
            EntryPoint = "SetDefaultPrinter")]   // ← real DLL function name
        private static extern bool SetDefaultPrinterNative(string name);  // ← our C# alias

        // ── Get current Windows default printer ───────────────────
        private string GetDefaultPrinter()
        {
            try
            {
                foreach (string printer in
                    System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    var ps = new System.Drawing.Printing.PrinterSettings
                    {
                        PrinterName = printer
                    };
                    if (ps.IsDefaultPrinter) return printer;
                }
            }
            catch { }
            return "";
        }

        // ── Set Windows default printer ───────────────────────────
        private void SetDefaultPrinter(string printerName)
        {
            try
            {
                SetDefaultPrinterNative(printerName);   // ← calls the real DLL via alias
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not set default printer to \"{printerName}\": " + ex.Message,
                    "Printer Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ══════════════════════════════════════════════════════════
        // Silent print via Bartender
        // ══════════════════════════════════════════════════════════
        private void PrintViaBartender(string printerName)
        {
            string previousDefault = GetDefaultPrinter();

            try
            {
                if (!File.Exists(_bartenderPath))
                {
                    MessageBox.Show(
                        $"Bartender executable not found:\n{_bartenderPath}\n\nPlease use 📂 Browse to locate bartend.exe.",
                        "Bartender Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string btwFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ManualRegistrations.btw");

                if (!File.Exists(btwFile))
                {
                    MessageBox.Show(
                        $"Bartender template not found:\n{btwFile}",
                        "Template Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Set selected printer as Windows default so Bartender uses it
                SetDefaultPrinter(printerName);

                // Small delay to let Windows register the change
                System.Threading.Thread.Sleep(400);

                string arguments = $"/F=\"{btwFile}\" /P /X";

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _bartenderPath,
                    Arguments = arguments,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized,
                    UseShellExecute = true
                };

                using var process = System.Diagnostics.Process.Start(psi);
                process?.WaitForExit(30000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bartender print failed: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Always restore previous default printer
                if (!string.IsNullOrEmpty(previousDefault))
                    SetDefaultPrinter(previousDefault);
            }
        }
        // ══════════════════════════════════════════════════════════
        // Load registrations into DataGridView
        // ══════════════════════════════════════════════════════════
        private void LoadRegistrations()
        {
            dgvRegistrations.Rows.Clear();

            try
            {
                using var conn = new MySqlConnection(_dbConn);
                conn.Open();

                const string sql = @"
                    SELECT name, company_name, created_at
                    FROM manual_registrations
                    ORDER BY id DESC;";

                using var cmd = new MySqlCommand(sql, conn);
                using var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    dgvRegistrations.Rows.Add(
                        rdr.GetString(0),
                        rdr.IsDBNull(1) ? "" : rdr.GetString(1),
                        rdr.GetDateTime(2).ToString("yyyy-MM-dd HH:mm")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load registrations: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ══════════════════════════════════════════════════════════
        // Bartender path helpers
        // ══════════════════════════════════════════════════════════
        private string LoadBartenderPath()
        {
            try
            {
                string settingsPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "AppSettings.json");

                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    var doc = System.Text.Json.JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty(BartenderPathKey, out var val))
                    {
                        string saved = val.GetString() ?? "";
                        if (File.Exists(saved)) return saved;
                    }
                }
            }
            catch { }

            return @"C:\Users\Windows\Documents\Installers\Bartender 6.12\bartend.exe";
        }

        private void SaveBartenderPath(string path)
        {
            try
            {
                string settingsPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "AppSettings.json");

                var dict = new System.Collections.Generic.Dictionary<string, string>();

                if (File.Exists(settingsPath))
                {
                    string existing = File.ReadAllText(settingsPath);
                    var doc = System.Text.Json.JsonDocument.Parse(existing);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                        dict[prop.Name] = prop.Value.ToString();
                }

                dict[BartenderPathKey] = path;

                string newJson = System.Text.Json.JsonSerializer.Serialize(
                    dict,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(settingsPath, newJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Bartender path: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBrowseBartender_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog();
            dlg.Title = "Select Bartender Executable (bartend.exe)";
            dlg.Filter = "Bartender Executable (bartend.exe)|bartend.exe|All Executables (*.exe)|*.exe";
            dlg.InitialDirectory = File.Exists(_bartenderPath)
                                   ? Path.GetDirectoryName(_bartenderPath)
                                   : @"C:\Users\Windows\Documents\Installers\Bartender 6.12";

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _bartenderPath = dlg.FileName;
                txtBartenderPath.Text = _bartenderPath;
                SaveBartenderPath(_bartenderPath);

                MessageBox.Show("Bartender path saved successfully.", "Saved",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();

        // ── Printer combo item ────────────────────────────────────
        private class PrinterItem
        {
            public int PrinterNumber { get; }
            public string PrinterName { get; }
            public string PrinterCode { get; }

            public PrinterItem(int number, string name, string code)
            {
                PrinterNumber = number;
                PrinterName = name;
                PrinterCode = code;
            }

            public override string ToString() =>
                $"Printer {PrinterNumber}  [{PrinterCode}]  —  {PrinterName}";
        }
    }
}