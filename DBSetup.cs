using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace High6
{
    /// <summary>
    /// DB & API Setup form.
    /// Saves all settings to AppSettings.json beside the .exe.
    /// Both laptops (Floor 1 & Floor 2) point at the same server
    /// by changing the DB host to the server laptop's IP address.
    /// </summary>
    public class DbSetup : Form
    {
        // ── Controls ──────────────────────────────────────────────
        private TextBox txtDbHost;
        private TextBox txtDbPort;
        private TextBox txtDbName;
        private TextBox txtDbUser;
        private TextBox txtDbPass;
        private TextBox txtApiUrl;
        private TextBox txtApiSecret;

        private Button btnTest;
        private Button btnSave;
        private Button btnClose;
        private Label lblStatus;

        private static readonly string SettingsPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.json");

        public DbSetup()
        {
            BuildUI();
            this.Load += (s, e) => LoadExistingSettings();
        }

        // ═══════════════════════════════════════════════════════════
        // UI
        // ═══════════════════════════════════════════════════════════
        private void BuildUI()
        {
            this.Text = "Database & API Setup";
            this.Size = new Size(520, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9f);
            this.BackColor = Color.FromArgb(245, 246, 250);

            // ── Header ────────────────────────────────────────────
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(30, 39, 46)
            };
            var lblTitle = new Label
            {
                Text = "⚙  Database & API Configuration",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };
            pnlTop.Controls.Add(lblTitle);

            // ── Scroll panel for fields ───────────────────────────
            var scroll = new Panel
            {
                AutoScroll = true,
                Location = new Point(0, 52),
                Size = new Size(504, 330),
                BackColor = Color.FromArgb(245, 246, 250),
                Padding = new Padding(20, 14, 20, 0)
            };

            int y = 14;
            int labelW = 140;
            int fieldW = 300;

            // ── Database section ──────────────────────────────────
            scroll.Controls.Add(SectionLabel("DATABASE CONNECTION", ref y));

            txtDbHost = AddRow(scroll, "Host / IP Address", "localhost", ref y, labelW, fieldW);
            txtDbPort = AddRow(scroll, "Port", "3306", ref y, labelW, fieldW);
            txtDbName = AddRow(scroll, "Database Name", "high6middleware", ref y, labelW, fieldW);
            txtDbUser = AddRow(scroll, "Username", "root", ref y, labelW, fieldW);
            txtDbPass = AddRow(scroll, "Password", "", ref y, labelW, fieldW, isPassword: true);

            y += 10;
            scroll.Controls.Add(new Label
            {
                Text = "ℹ  For the second laptop, enter the server laptop's IP address above.",
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                Location = new Point(20, y),
                Size = new Size(450, 18),
                AutoSize = false
            });
            y += 24;

            // ── API section ───────────────────────────────────────
            y += 6;
            scroll.Controls.Add(SectionLabel("PSE PRINTER API", ref y));

            txtApiUrl = AddRow(scroll, "Base URL", "https://pse-ems-staging.h6app.site", ref y, labelW, fieldW);
            txtApiSecret = AddRow(scroll, "Shared Secret", "", ref y, labelW, fieldW, isPassword: true);

            y += 10;
            scroll.Controls.Add(new Label
            {
                Text = "ℹ  When the client changes the endpoint or secret, update both fields here.",
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                Location = new Point(20, y),
                Size = new Size(450, 18),
                AutoSize = false
            });
            y += 28;

            scroll.AutoScrollMinSize = new Size(0, y + 20);

            // ── Bottom bar ────────────────────────────────────────
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(14, 10, 14, 10)
            };

            btnTest = MakeButton("🔌  Test Connection", Color.FromArgb(52, 152, 219), 160);
            btnSave = MakeButton("💾  Save", Color.FromArgb(39, 174, 96), 100);
            btnClose = MakeButton("✕  Close", Color.FromArgb(192, 57, 43), 100);

            btnTest.Location = new Point(14, 12);
            btnSave.Location = new Point(182, 12);
            btnClose.Location = new Point(290, 12);

            lblStatus = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI", 8.5f),
                Text = "Enter your settings and click 💾 Save.",
                Location = new Point(400, 18)
            };

            pnlBottom.Controls.Add(btnTest);
            pnlBottom.Controls.Add(btnSave);
            pnlBottom.Controls.Add(btnClose);
            pnlBottom.Controls.Add(lblStatus);

            btnTest.Click += async (s, e) => await TestConnectionAsync();
            btnSave.Click += (s, e) => SaveSettings();
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(scroll);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);
        }

        // ═══════════════════════════════════════════════════════════
        // Load existing AppSettings.json
        // ═══════════════════════════════════════════════════════════
        private void LoadExistingSettings()
        {
            if (!File.Exists(SettingsPath)) return;

            try
            {
                string json = File.ReadAllText(SettingsPath);
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                SetText(txtDbHost, root, "db_host", "localhost");
                SetText(txtDbPort, root, "db_port", "3306");
                SetText(txtDbName, root, "db_name", "high6middleware");
                SetText(txtDbUser, root, "db_user", "root");
                SetText(txtDbPass, root, "db_pass", "");
                SetText(txtApiUrl, root, "api_base_url", "");
                SetText(txtApiSecret, root, "api_secret", "");

                SetStatus("Settings loaded from AppSettings.json.", Color.FromArgb(39, 174, 96));
            }
            catch (Exception ex)
            {
                SetStatus("⚠ Could not read AppSettings.json: " + ex.Message,
                          Color.FromArgb(192, 57, 43));
            }
        }

        private static void SetText(TextBox tb, JsonElement root, string key, string fallback)
        {
            tb.Text = root.TryGetProperty(key, out var v) ? v.GetString() ?? fallback : fallback;
        }

        // ═══════════════════════════════════════════════════════════
        // Test DB connection with current field values
        // ═══════════════════════════════════════════════════════════
        private async Task TestConnectionAsync()
        {
            btnTest.Enabled = false;
            SetStatus("Testing…", Color.Gray);

            string connStr = BuildConnString();
            try
            {
                using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync();
                SetStatus($"✅ Connected to {txtDbHost.Text}:{txtDbPort.Text}/{txtDbName.Text}",
                          Color.FromArgb(39, 174, 96));
            }
            catch (Exception ex)
            {
                SetStatus("✗ " + ex.Message, Color.FromArgb(192, 57, 43));
            }

            btnTest.Enabled = true;
        }

        // ═══════════════════════════════════════════════════════════
        // Save to AppSettings.json
        // ═══════════════════════════════════════════════════════════
        private void SaveSettings()
        {
            if (string.IsNullOrWhiteSpace(txtDbHost.Text) ||
                string.IsNullOrWhiteSpace(txtApiUrl.Text) ||
                string.IsNullOrWhiteSpace(txtApiSecret.Text))
            {
                SetStatus("⚠ Host, API URL, and API Secret are required.", Color.FromArgb(243, 156, 18));
                return;
            }

            try
            {
                var settings = new
                {
                    db_host = txtDbHost.Text.Trim(),
                    db_port = txtDbPort.Text.Trim(),
                    db_name = txtDbName.Text.Trim(),
                    db_user = txtDbUser.Text.Trim(),
                    db_pass = txtDbPass.Text,
                    api_base_url = txtApiUrl.Text.TrimEnd('/'),
                    api_secret = txtApiSecret.Text.Trim()
                };

                string json = JsonSerializer.Serialize(settings,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(SettingsPath, json);

                SetStatus("✅ Saved to AppSettings.json!", Color.FromArgb(39, 174, 96));
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                SetStatus("✗ Save failed: " + ex.Message, Color.FromArgb(192, 57, 43));
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════════════════════
        private string BuildConnString()
            => $"Server={txtDbHost.Text.Trim()};" +
               $"Port={txtDbPort.Text.Trim()};" +
               $"Database={txtDbName.Text.Trim()};" +
               $"Uid={txtDbUser.Text.Trim()};" +
               $"Pwd={txtDbPass.Text};";

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

        private static Button MakeButton(string text, Color backColor, int width = 120)
            => new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 34,
                Width = width,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };

        private Label SectionLabel(string text, ref int y)
        {
            var lbl = new Label
            {
                Text = text,
                ForeColor = Color.FromArgb(44, 62, 80),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location = new Point(20, y),
                Size = new Size(440, 18),
                AutoSize = false
            };
            y += 24;
            return lbl;
        }

        private TextBox AddRow(Panel parent, string labelText, string placeholder,
                               ref int y, int labelW, int fieldW, bool isPassword = false)
        {
            parent.Controls.Add(new Label
            {
                Text = labelText,
                Location = new Point(20, y + 3),
                Size = new Size(labelW, 20),
                ForeColor = Color.FromArgb(60, 60, 60)
            });

            var tb = new TextBox
            {
                Text = placeholder,
                Location = new Point(20 + labelW + 8, y),
                Size = new Size(fieldW, 24),
                PasswordChar = isPassword ? '●' : '\0',
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            parent.Controls.Add(tb);
            y += 34;
            return tb;
        }
    }
}