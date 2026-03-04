using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace High6
{
    /// <summary>
    /// Initial setup form — lets the operator assign a physical ZPL printer
    /// (detected from Windows/WMI) and a floor number to each printer slot
    /// stored in the high6middleware.printers table.
    /// 
    /// Open this form once per PC before starting the polling service.
    /// </summary>
    public class PrinterSetup : Form
    {
        // ── Same connection string as Printer.cs ──────────────────
        private const string DbConn =
            "Server=localhost;Port=3306;Database=high6middleware;Uid=root;Pwd=;";

        // ── Controls ──────────────────────────────────────────────
        private DataGridView dgv;
        private Button btnRefreshPrinters;
        private Button btnSave;
        private Button btnClose;
        private Label lblStatus;
        private Panel pnlTop;
        private Panel pnlBottom;

        // Column name constants — keeps magic strings in one place
        private const string ColId = "col_id";
        private const string ColPrinterNo = "col_printer_number";
        private const string ColPrinterCode = "col_printer_code";
        private const string ColPrinterName = "col_printer_name";   // ComboBox
        private const string ColFloor = "col_floor";
        private const string ColCreatedAt = "col_created_at";

        // All ZPL printer names found on this PC (populated once on load / refresh)
        private List<string> _localPrinterNames = new List<string>();

        public PrinterSetup()
        {
            BuildUI();
            this.Load += async (s, e) => await InitAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // UI Construction
        // ═══════════════════════════════════════════════════════════
        private void BuildUI()
        {
            this.Text = "Printer Setup — Assign Printers & Floors";
            this.Size = new Size(860, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9f);
            this.BackColor = Color.FromArgb(245, 246, 250);

            // ── Top panel ─────────────────────────────────────────
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(30, 30, 46),
                Padding = new Padding(12, 0, 12, 0)
            };

            var lblTitle = new Label
            {
                Text = "🖨  Printer Setup",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlTop.Controls.Add(lblTitle);

            // ── Bottom panel ──────────────────────────────────────
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.FromArgb(235, 236, 240),
                Padding = new Padding(12, 8, 12, 8)
            };

            btnRefreshPrinters = MakeButton("🔄  Refresh Printers", Color.FromArgb(52, 152, 219));
            btnSave = MakeButton("💾  Save", Color.FromArgb(39, 174, 96));
            btnClose = MakeButton("✕  Close", Color.FromArgb(192, 57, 43));

            btnRefreshPrinters.Width = 160;
            btnSave.Width = 100;
            btnClose.Width = 100;
            btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;

            lblStatus = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI", 8.5f),
                Text = "Loading…",
                Location = new Point(278, 14)
            };

            pnlBottom.Controls.Add(btnRefreshPrinters);
            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnClose);
            pnlBottom.Controls.Add(lblStatus);

            // Manual layout for bottom buttons
            btnRefreshPrinters.Location = new Point(12, 8);
            btnSave.Location = new Point(180, 8);
            btnClose.Location = new Point(288, 8);

            // ── Grid ──────────────────────────────────────────────
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(220, 220, 220),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EditMode = DataGridViewEditMode.EditOnEnter,
                Font = new Font("Segoe UI", 9f)
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 46);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 36;
            dgv.EnableHeadersVisualStyles = false;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            BuildColumns();

            // ── Wire up ───────────────────────────────────────────
            btnRefreshPrinters.Click += async (s, e) => await RefreshLocalPrintersAsync();
            btnSave.Click += async (s, e) => await SaveAsync();
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(dgv);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);
        }

        private void BuildColumns()
        {
            dgv.Columns.Clear();

            // Hidden: DB id
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColId,
                Visible = false
            });

            // Read-only: Printer No
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColPrinterNo,
                HeaderText = "Printer #",
                ReadOnly = true,
                Width = 80,
                FillWeight = 10,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter,
                                     Font      = new Font("Segoe UI", 9f, FontStyle.Bold) }
            });

            // Read-only: Code
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColPrinterCode,
                HeaderText = "Code",
                ReadOnly = true,
                FillWeight = 15
            });

            // ★ Editable ComboBox: assign a local ZPL printer
            var cboCol = new DataGridViewComboBoxColumn
            {
                Name = ColPrinterName,
                HeaderText = "Physical Printer (detected on this PC)",
                FillWeight = 50,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            dgv.Columns.Add(cboCol);

            // ★ Editable: floor
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColFloor,
                HeaderText = "Floor",
                FillWeight = 12,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Read-only: created_at
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColCreatedAt,
                HeaderText = "Created At",
                ReadOnly = true,
                FillWeight = 20,
                DefaultCellStyle = { ForeColor = Color.Gray }
            });
        }

        // ═══════════════════════════════════════════════════════════
        // Initialisation — detect local printers + load DB rows
        // ═══════════════════════════════════════════════════════════
        private async Task InitAsync()
        {
            SetStatus("Scanning local ZPL printers…", Color.Gray);
            await RefreshLocalPrintersAsync(loadDbAfter: true);
        }

        /// <summary>
        /// Re-scans WMI for ZPL printers and rebuilds the ComboBox item list.
        /// If loadDbAfter is true the DB rows are loaded afterwards.
        /// </summary>
        private async Task RefreshLocalPrintersAsync(bool loadDbAfter = false)
        {
            btnRefreshPrinters.Enabled = false;
            SetStatus("Scanning…", Color.Gray);

            _localPrinterNames = await Task.Run(() => GetLocalZplPrinterNames());

            // Rebuild ComboBox column items
            var cboCol = (DataGridViewComboBoxColumn)dgv.Columns[ColPrinterName];
            cboCol.Items.Clear();
            cboCol.Items.Add("");                           // empty = not assigned
            foreach (var n in _localPrinterNames)
                cboCol.Items.Add(n);

            // Re-validate existing cells so they don't show an error if value is now valid
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var cell = (DataGridViewComboBoxCell)row.Cells[ColPrinterName];
                if (cell.Value != null && !cboCol.Items.Contains(cell.Value))
                    cell.Value = "";
            }

            SetStatus($"Found {_localPrinterNames.Count} ZPL printer(s) on this PC.", Color.FromArgb(39, 174, 96));
            btnRefreshPrinters.Enabled = true;

            if (loadDbAfter)
                await LoadDbRowsAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // Load printer rows from DB
        // ═══════════════════════════════════════════════════════════
        private async Task LoadDbRowsAsync()
        {
            try
            {
                dgv.Rows.Clear();

                using var conn = new MySqlConnection(DbConn);
                await conn.OpenAsync();

                var sql = "SELECT id, printer_number, printer_code, printer_name, floor, created_at " +
                          "FROM printers ORDER BY printer_number;";

                using var cmd = new MySqlCommand(sql, conn);
                using var rdr = await cmd.ExecuteReaderAsync();

                var cboCol = (DataGridViewComboBoxColumn)dgv.Columns[ColPrinterName];

                while (await rdr.ReadAsync())
                {
                    string dbPrinterName = rdr.IsDBNull(3) ? "" : rdr.GetString(3);

                    // Ensure the saved name exists in the dropdown (even if printer is
                    // currently disconnected — show it greyed out by leaving it selectable)
                    if (!string.IsNullOrEmpty(dbPrinterName) && !cboCol.Items.Contains(dbPrinterName))
                        cboCol.Items.Add(dbPrinterName + "  ⚠ (not detected)");

                    dgv.Rows.Add(
                        rdr.GetInt32(0),                                     // id
                        rdr.GetByte(1),                                      // printer_number
                        rdr.IsDBNull(2) ? "" : rdr.GetString(2),             // printer_code
                        dbPrinterName,                                       // printer_name (combo)
                        rdr.IsDBNull(4) ? "" : rdr.GetByte(4).ToString(),    // floor
                        rdr.GetDateTime(5).ToString("yyyy-MM-dd HH:mm")      // created_at
                    );
                }

                SetStatus($"Loaded {dgv.Rows.Count} printer slot(s) from DB. " +
                          $"Assign a physical printer and floor, then click 💾 Save.",
                          Color.FromArgb(30, 30, 46));
            }
            catch (Exception ex)
            {
                SetStatus("✗ DB load error: " + ex.Message, Color.FromArgb(192, 57, 43));
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Save — UPDATE each printer row
        // ═══════════════════════════════════════════════════════════
        private async Task SaveAsync()
        {
            // Force end of any active edit
            dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            dgv.EndEdit();

            btnSave.Enabled = false;
            SetStatus("Saving…", Color.Gray);
            int saved = 0, errors = 0;

            try
            {
                using var conn = new MySqlConnection(DbConn);
                await conn.OpenAsync();

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;

                    int id = Convert.ToInt32(row.Cells[ColId].Value);
                    string printerName = row.Cells[ColPrinterName].Value?.ToString()?.Trim() ?? "";

                    // Strip the "⚠ (not detected)" suffix if present
                    if (printerName.Contains("⚠"))
                        printerName = printerName.Substring(0, printerName.IndexOf("⚠")).Trim();

                    string floorRaw = row.Cells[ColFloor].Value?.ToString()?.Trim() ?? "";
                    int? floor = null;

                    if (!string.IsNullOrEmpty(floorRaw))
                    {
                        if (int.TryParse(floorRaw, out int f))
                            floor = f;
                        else
                        {
                            SetStatus($"✗ Row {id}: Floor must be a number.", Color.FromArgb(192, 57, 43));
                            errors++;
                            continue;
                        }
                    }

                    try
                    {
                        var sql = @"UPDATE printers
                                    SET printer_name = @name,
                                        floor        = @floor
                                    WHERE id = @id;";

                        using var cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@name", string.IsNullOrEmpty(printerName) ? (object)DBNull.Value : printerName);
                        cmd.Parameters.AddWithValue("@floor", floor.HasValue ? (object)floor.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", id);

                        await cmd.ExecuteNonQueryAsync();
                        saved++;
                    }
                    catch (Exception ex)
                    {
                        SetStatus($"✗ DB error on row {id}: " + ex.Message, Color.FromArgb(192, 57, 43));
                        errors++;
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus("✗ Connection error: " + ex.Message, Color.FromArgb(192, 57, 43));
                btnSave.Enabled = true;
                return;
            }

            btnSave.Enabled = true;

            if (errors == 0)
                SetStatus($"✅ {saved} printer slot(s) saved successfully!", Color.FromArgb(39, 174, 96));
            else
                SetStatus($"⚠ {saved} saved, {errors} error(s). Check rows above.", Color.FromArgb(243, 156, 18));
        }

        // ═══════════════════════════════════════════════════════════
        // WMI — get all ZPL printer names on this Windows PC
        // ═══════════════════════════════════════════════════════════
        private List<string> GetLocalZplPrinterNames()
        {
            var list = new List<string>();
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, DriverName FROM Win32_Printer");

                foreach (ManagementObject p in searcher.Get())
                {
                    string name = p["Name"]?.ToString() ?? "";
                    string driver = p["DriverName"]?.ToString() ?? "";

                    bool isZebra =
                        ContainsAny(name.ToLower(), "zebra", "zpl", " zd", " zt", " zp", "tlp", "lp28", "gk", "gz") ||
                        ContainsAny(driver.ToLower(), "zebra", "zpl", "zdesigner");

                    if (isZebra && !string.IsNullOrWhiteSpace(name))
                        list.Add(name);
                }
            }
            catch { /* WMI not available */ }
            return list;
        }

        // ═══════════════════════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════════════════════
        private static bool ContainsAny(string src, params string[] kws)
        {
            foreach (var k in kws) if (src.Contains(k)) return true;
            return false;
        }

        private void SetStatus(string msg, Color color)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => SetStatus(msg, color)));
                return;
            }
            lblStatus.Text = msg;
            lblStatus.ForeColor = color;
        }

        private static Button MakeButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 34,
                Width = 120,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }
    }
}