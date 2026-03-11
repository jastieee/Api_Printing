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
            this.components = new System.ComponentModel.Container();

            this.btnStart = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnEnd = new System.Windows.Forms.Button();
            this.btnSetupDb = new System.Windows.Forms.Button();
            this.btnSetupPrinters = new System.Windows.Forms.Button();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblConnection = new System.Windows.Forms.Label();
            this.lblConnectionDot = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.pnlConnection = new System.Windows.Forms.Panel();
            this.pnlLogHeader = new System.Windows.Forms.Panel();

            this.pnlHeader.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlConnection.SuspendLayout();
            this.pnlLogHeader.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ─────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(30, 39, 46);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Size = new System.Drawing.Size(860, 56);
            this.pnlHeader.Controls.Add(this.lblTitle);

            // ── lblTitle ──────────────────────────────────────────
            this.lblTitle.Text = "⬡  High6 Printer Service";
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(16, 14);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.AutoSize = false;

            // ── pnlButtons ────────────────────────────────────────
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.pnlButtons.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.pnlButtons.Location = new System.Drawing.Point(12, 68);
            this.pnlButtons.Size = new System.Drawing.Size(836, 60);
            this.pnlButtons.Controls.Add(this.btnStart);
            this.pnlButtons.Controls.Add(this.btnPause);
            this.pnlButtons.Controls.Add(this.btnEnd);
            this.pnlButtons.Controls.Add(this.btnSetupDb);
            this.pnlButtons.Controls.Add(this.btnSetupPrinters);

            // ── btnStart ──────────────────────────────────────────
            this.btnStart.Text = "▶  Start";
            this.btnStart.Location = new System.Drawing.Point(0, 10);
            this.btnStart.Size = new System.Drawing.Size(120, 40);
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);

            // ── btnPause ──────────────────────────────────────────
            this.btnPause.Text = "⏸  Pause";
            this.btnPause.Location = new System.Drawing.Point(130, 10);
            this.btnPause.Size = new System.Drawing.Size(120, 40);
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
            this.btnEnd.Location = new System.Drawing.Point(260, 10);
            this.btnEnd.Size = new System.Drawing.Size(120, 40);
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
            this.btnSetupDb.Location = new System.Drawing.Point(400, 10);
            this.btnSetupDb.Size = new System.Drawing.Size(130, 40);
            this.btnSetupDb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetupDb.BackColor = System.Drawing.Color.FromArgb(44, 62, 80);
            this.btnSetupDb.ForeColor = System.Drawing.Color.White;
            this.btnSetupDb.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSetupDb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetupDb.FlatAppearance.BorderSize = 0;
            this.btnSetupDb.Click += new System.EventHandler(this.btnSetupDb_Click);

            // ── btnSetupPrinters ──────────────────────────────────
            this.btnSetupPrinters.Text = "⚙  Setup Printers";
            this.btnSetupPrinters.Location = new System.Drawing.Point(542, 10);
            this.btnSetupPrinters.Size = new System.Drawing.Size(160, 40);
            this.btnSetupPrinters.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetupPrinters.BackColor = System.Drawing.Color.FromArgb(142, 68, 173);
            this.btnSetupPrinters.ForeColor = System.Drawing.Color.White;
            this.btnSetupPrinters.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSetupPrinters.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSetupPrinters.FlatAppearance.BorderSize = 0;
            this.btnSetupPrinters.Click += new System.EventHandler(this.btnSetupPrinters_Click);

            // ── pnlConnection ─────────────────────────────────────
            this.pnlConnection.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.pnlConnection.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.pnlConnection.Location = new System.Drawing.Point(12, 140);
            this.pnlConnection.Size = new System.Drawing.Size(836, 50);
            this.pnlConnection.Controls.Add(this.lblConnectionDot);
            this.pnlConnection.Controls.Add(this.lblConnection);

            // ── lblConnectionDot ──────────────────────────────────
            this.lblConnectionDot.Text = "●";
            this.lblConnectionDot.Location = new System.Drawing.Point(0, 14);
            this.lblConnectionDot.Size = new System.Drawing.Size(22, 22);
            this.lblConnectionDot.ForeColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.lblConnectionDot.Font = new System.Drawing.Font("Segoe UI", 13F);

            // ── lblConnection ─────────────────────────────────────
            this.lblConnection.Text = "Not started — click ▶ Start to begin";
            this.lblConnection.Location = new System.Drawing.Point(26, 16);
            this.lblConnection.Size = new System.Drawing.Size(700, 20);
            this.lblConnection.ForeColor = System.Drawing.Color.FromArgb(127, 140, 141);
            this.lblConnection.Font = new System.Drawing.Font("Segoe UI", 9F);

            // ── pnlLogHeader ──────────────────────────────────────
            this.pnlLogHeader.BackColor = System.Drawing.Color.FromArgb(236, 240, 241);
            this.pnlLogHeader.Location = new System.Drawing.Point(12, 200);
            this.pnlLogHeader.Size = new System.Drawing.Size(836, 30);
            this.pnlLogHeader.Controls.Add(this.lblStatus);

            // ── lblStatus ─────────────────────────────────────────
            this.lblStatus.Text = "  📋  Response Log";
            this.lblStatus.Location = new System.Drawing.Point(0, 6);
            this.lblStatus.Size = new System.Drawing.Size(300, 20);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // ── txtStatus ─────────────────────────────────────────
            this.txtStatus.Location = new System.Drawing.Point(12, 234);
            this.txtStatus.Size = new System.Drawing.Size(836, 280);
            this.txtStatus.Multiline = true;
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatus.ReadOnly = true;
            this.txtStatus.BackColor = System.Drawing.Color.FromArgb(30, 39, 46);
            this.txtStatus.ForeColor = System.Drawing.Color.FromArgb(149, 236, 105);
            this.txtStatus.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtStatus.Text = "// Waiting for response...\r\n";

            // ── Form ──────────────────────────────────────────────
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 530);
            this.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.Text = "High6 Printer Service";
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Printer_Load);

            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlConnection);
            this.Controls.Add(this.pnlLogHeader);
            this.Controls.Add(this.txtStatus);

            this.pnlHeader.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
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
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblConnection;
        private System.Windows.Forms.Label lblConnectionDot;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Panel pnlConnection;
        private System.Windows.Forms.Panel pnlLogHeader;
    }
}