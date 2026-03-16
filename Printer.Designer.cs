namespace High6
{
    partial class Printer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnEnd = new System.Windows.Forms.Button();
            this.btnSetupDb = new System.Windows.Forms.Button();
            this.btnSetupPrinters = new System.Windows.Forms.Button();
            this.btnManual = new System.Windows.Forms.Button();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblConnection = new System.Windows.Forms.Label();
            this.lblConnectionDot = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();   // ← FlowLayoutPanel
            this.pnlConnection = new System.Windows.Forms.Panel();
            this.pnlLogHeader = new System.Windows.Forms.Panel();

            this.pnlHeader.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.pnlConnection.SuspendLayout();
            this.pnlLogHeader.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ─────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(30, 39, 46);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 56;
            this.pnlHeader.Controls.Add(this.lblTitle);

            // ── lblTitle ──────────────────────────────────────────
            this.lblTitle.Text = "⬡  High6 Printer Service";
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(16, 14);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.AutoSize = false;

            // ── flpButtons (FlowLayoutPanel) ──────────────────────
            // Docks Top, flows buttons left-to-right with spacing
            this.flpButtons.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.flpButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpButtons.Height = 62;
            this.flpButtons.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.flpButtons.WrapContents = false;   // single row; scroll if too narrow
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;

            // Add buttons in LEFT-TO-RIGHT display order
            this.flpButtons.Controls.Add(this.btnStart);
            this.flpButtons.Controls.Add(this.btnPause);
            this.flpButtons.Controls.Add(this.btnEnd);
            this.flpButtons.Controls.Add(this.btnSetupDb);
            this.flpButtons.Controls.Add(this.btnSetupPrinters);
            this.flpButtons.Controls.Add(this.btnManual);

            // ── btnStart ──────────────────────────────────────────
            this.btnStart.Text = "▶  Start";
            this.btnStart.Size = new System.Drawing.Size(110, 40);
            this.btnStart.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);

            // ── btnPause ──────────────────────────────────────────
            this.btnPause.Text = "⏸  Pause";
            this.btnPause.Size = new System.Drawing.Size(110, 40);
            this.btnPause.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPause.BackColor = System.Drawing.Color.FromArgb(243, 156, 18);
            this.btnPause.ForeColor = System.Drawing.Color.White;
            this.btnPause.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPause.FlatAppearance.BorderSize = 0;
            this.btnPause.Enabled = false;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);

            // ── btnEnd ────────────────────────────────────────────
            this.btnEnd.Text = "⏹  End";
            this.btnEnd.Size = new System.Drawing.Size(110, 40);
            this.btnEnd.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);   // extra gap before setup buttons
            this.btnEnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnd.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this.btnEnd.ForeColor = System.Drawing.Color.White;
            this.btnEnd.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEnd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEnd.FlatAppearance.BorderSize = 0;
            this.btnEnd.Enabled = false;
            this.btnEnd.Click += new System.EventHandler(this.btnEnd_Click);

            // ── btnSetupDb ────────────────────────────────────────
            this.btnSetupDb.Text = "⚙  Setup DB";
            this.btnSetupDb.Size = new System.Drawing.Size(120, 40);
            this.btnSetupDb.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnSetupDb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetupDb.BackColor = System.Drawing.Color.FromArgb(44, 62, 80);
            this.btnSetupDb.ForeColor = System.Drawing.Color.White;
            this.btnSetupDb.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSetupDb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetupDb.FlatAppearance.BorderSize = 0;
            this.btnSetupDb.Click += new System.EventHandler(this.btnSetupDb_Click);

            // ── btnSetupPrinters ──────────────────────────────────
            this.btnSetupPrinters.Text = "⚙  Setup Printers";
            this.btnSetupPrinters.Size = new System.Drawing.Size(150, 40);
            this.btnSetupPrinters.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnSetupPrinters.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetupPrinters.BackColor = System.Drawing.Color.FromArgb(142, 68, 173);
            this.btnSetupPrinters.ForeColor = System.Drawing.Color.White;
            this.btnSetupPrinters.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSetupPrinters.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetupPrinters.FlatAppearance.BorderSize = 0;
            this.btnSetupPrinters.Click += new System.EventHandler(this.btnSetupPrinters_Click);

            // ── btnManual ─────────────────────────────────────────
            this.btnManual.Text = "✎  Registration Manual";
            this.btnManual.Size = new System.Drawing.Size(180, 40);
            this.btnManual.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.btnManual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManual.BackColor = System.Drawing.Color.FromArgb(22, 160, 133);
            this.btnManual.ForeColor = System.Drawing.Color.White;
            this.btnManual.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnManual.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManual.FlatAppearance.BorderSize = 0;
            this.btnManual.Click += new System.EventHandler(this.btnManual_Click);

            // ── pnlConnection ─────────────────────────────────────
            this.pnlConnection.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.pnlConnection.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlConnection.Height = 36;
            this.pnlConnection.Controls.Add(this.lblConnectionDot);
            this.pnlConnection.Controls.Add(this.lblConnection);

            // ── lblConnectionDot ──────────────────────────────────
            this.lblConnectionDot.Text = "●";
            this.lblConnectionDot.Location = new System.Drawing.Point(12, 8);
            this.lblConnectionDot.Size = new System.Drawing.Size(22, 22);
            this.lblConnectionDot.ForeColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.lblConnectionDot.Font = new System.Drawing.Font("Segoe UI", 13F);

            // ── lblConnection ─────────────────────────────────────
            this.lblConnection.Text = "Not started — click ▶ Start to begin";
            this.lblConnection.Location = new System.Drawing.Point(38, 10);
            this.lblConnection.Size = new System.Drawing.Size(700, 20);
            this.lblConnection.Anchor = System.Windows.Forms.AnchorStyles.Left
                                         | System.Windows.Forms.AnchorStyles.Right
                                         | System.Windows.Forms.AnchorStyles.Top;
            this.lblConnection.ForeColor = System.Drawing.Color.FromArgb(127, 140, 141);
            this.lblConnection.Font = new System.Drawing.Font("Segoe UI", 9F);

            // ── pnlLogHeader ──────────────────────────────────────
            this.pnlLogHeader.BackColor = System.Drawing.Color.FromArgb(236, 240, 241);
            this.pnlLogHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogHeader.Height = 28;
            this.pnlLogHeader.Controls.Add(this.lblStatus);

            // ── lblStatus ─────────────────────────────────────────
            this.lblStatus.Text = "  📋  Response Log";
            this.lblStatus.Location = new System.Drawing.Point(0, 5);
            this.lblStatus.Size = new System.Drawing.Size(300, 20);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // ── txtStatus ─────────────────────────────────────────
            this.txtStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStatus.Multiline = true;
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtStatus.ReadOnly = true;
            this.txtStatus.WordWrap = false;
            this.txtStatus.BackColor = System.Drawing.Color.FromArgb(30, 39, 46);
            this.txtStatus.ForeColor = System.Drawing.Color.FromArgb(149, 236, 105);
            this.txtStatus.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtStatus.Text = "// Waiting for response...\r\n";

            // ── Form ──────────────────────────────────────────────
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 580);
            this.MinimumSize = new System.Drawing.Size(640, 420);
            this.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.Text = "High6 Printer Service";
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Printer_Load);

            // Controls added in reverse dock order:
            // Fill first, then Top panels from bottom-most to topmost
            this.Controls.Add(this.txtStatus);       // Fill — takes remaining space
            this.Controls.Add(this.pnlLogHeader);    // Top
            this.Controls.Add(this.pnlConnection);   // Top
            this.Controls.Add(this.flpButtons);      // Top
            this.Controls.Add(this.pnlHeader);       // Top — sits at very top

            this.pnlHeader.ResumeLayout(false);
            this.flpButtons.ResumeLayout(false);
            this.pnlConnection.ResumeLayout(false);
            this.pnlLogHeader.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnEnd;
        private System.Windows.Forms.Button btnSetupDb;
        private System.Windows.Forms.Button btnSetupPrinters;
        private System.Windows.Forms.Button btnManual;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblConnection;
        private System.Windows.Forms.Label lblConnectionDot;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.Panel pnlConnection;
        private System.Windows.Forms.Panel pnlLogHeader;
    }
}