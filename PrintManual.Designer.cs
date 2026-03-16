namespace High6
{
    partial class PrintManual
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();

            lblTitle = new System.Windows.Forms.Label();
            lblFullName = new System.Windows.Forms.Label();
            txtFullName = new System.Windows.Forms.TextBox();
            lblCompany = new System.Windows.Forms.Label();
            txtCompany = new System.Windows.Forms.TextBox();
            lblBartenderPath = new System.Windows.Forms.Label();
            txtBartenderPath = new System.Windows.Forms.TextBox();
            btnBrowseBartender = new System.Windows.Forms.Button();
            lblPrinter = new System.Windows.Forms.Label();
            cboPrinter = new System.Windows.Forms.ComboBox();
            btnSave = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            dgvRegistrations = new System.Windows.Forms.DataGridView();
            colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colCompany = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colCreatedAt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            pnlHeader = new System.Windows.Forms.Panel();
            pnlForm = new System.Windows.Forms.Panel();
            pnlGrid = new System.Windows.Forms.Panel();

            ((System.ComponentModel.ISupportInitialize)dgvRegistrations).BeginInit();
            pnlHeader.SuspendLayout();
            pnlForm.SuspendLayout();
            pnlGrid.SuspendLayout();
            SuspendLayout();

            // ── lblTitle ──────────────────────────────────────────
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.White;
            lblTitle.Location = new System.Drawing.Point(16, 14);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(400, 28);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "✎  Registration Manual";

            // ── lblFullName ───────────────────────────────────────
            lblFullName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblFullName.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            lblFullName.Location = new System.Drawing.Point(20, 14);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new System.Drawing.Size(200, 18);
            lblFullName.TabIndex = 0;
            lblFullName.Text = "Full Name";

            // ── txtFullName ───────────────────────────────────────
            // Width: ClientSize(640) - left(20) - right(20) = 600
            txtFullName.Anchor = System.Windows.Forms.AnchorStyles.Top
                                    | System.Windows.Forms.AnchorStyles.Left
                                    | System.Windows.Forms.AnchorStyles.Right;
            txtFullName.BackColor = System.Drawing.Color.White;
            txtFullName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtFullName.Font = new System.Drawing.Font("Segoe UI", 11F);
            txtFullName.Location = new System.Drawing.Point(20, 36);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new System.Drawing.Size(600, 27);
            txtFullName.TabIndex = 1;

            // ── lblCompany ────────────────────────────────────────
            lblCompany.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblCompany.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            lblCompany.Location = new System.Drawing.Point(20, 80);
            lblCompany.Name = "lblCompany";
            lblCompany.Size = new System.Drawing.Size(200, 18);
            lblCompany.TabIndex = 2;
            lblCompany.Text = "Company Name";

            // ── txtCompany ────────────────────────────────────────
            txtCompany.Anchor = System.Windows.Forms.AnchorStyles.Top
                                   | System.Windows.Forms.AnchorStyles.Left
                                   | System.Windows.Forms.AnchorStyles.Right;
            txtCompany.BackColor = System.Drawing.Color.White;
            txtCompany.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtCompany.Font = new System.Drawing.Font("Segoe UI", 11F);
            txtCompany.Location = new System.Drawing.Point(20, 102);
            txtCompany.Name = "txtCompany";
            txtCompany.Size = new System.Drawing.Size(600, 27);
            txtCompany.TabIndex = 3;

            // ── lblBartenderPath ──────────────────────────────────
            lblBartenderPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblBartenderPath.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            lblBartenderPath.Location = new System.Drawing.Point(20, 148);
            lblBartenderPath.Name = "lblBartenderPath";
            lblBartenderPath.Size = new System.Drawing.Size(200, 18);
            lblBartenderPath.TabIndex = 4;
            lblBartenderPath.Text = "Bartender Path";

            // ── txtBartenderPath ──────────────────────────────────
            // Width: 600 - Browse button(90) - gap(8) = 502
            txtBartenderPath.Anchor = System.Windows.Forms.AnchorStyles.Top
                                         | System.Windows.Forms.AnchorStyles.Left
                                         | System.Windows.Forms.AnchorStyles.Right;
            txtBartenderPath.BackColor = System.Drawing.Color.White;
            txtBartenderPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtBartenderPath.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtBartenderPath.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            txtBartenderPath.Location = new System.Drawing.Point(20, 170);
            txtBartenderPath.Name = "txtBartenderPath";
            txtBartenderPath.ReadOnly = true;
            txtBartenderPath.Size = new System.Drawing.Size(502, 27);
            txtBartenderPath.TabIndex = 5;

            // ── btnBrowseBartender ────────────────────────────────
            // Sits right of txtBartenderPath: 20 + 502 + 8 = 530
            btnBrowseBartender.Anchor = System.Windows.Forms.AnchorStyles.Top
                                         | System.Windows.Forms.AnchorStyles.Right;
            btnBrowseBartender.BackColor = System.Drawing.Color.FromArgb(44, 62, 80);
            btnBrowseBartender.Cursor = System.Windows.Forms.Cursors.Hand;
            btnBrowseBartender.FlatAppearance.BorderSize = 0;
            btnBrowseBartender.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnBrowseBartender.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnBrowseBartender.ForeColor = System.Drawing.Color.White;
            btnBrowseBartender.Location = new System.Drawing.Point(530, 169);
            btnBrowseBartender.Name = "btnBrowseBartender";
            btnBrowseBartender.Size = new System.Drawing.Size(90, 29);
            btnBrowseBartender.TabIndex = 6;
            btnBrowseBartender.Text = "📂  Browse";
            btnBrowseBartender.UseVisualStyleBackColor = false;
            btnBrowseBartender.Click += btnBrowseBartender_Click;

            // ── lblPrinter ────────────────────────────────────────
            lblPrinter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblPrinter.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            lblPrinter.Location = new System.Drawing.Point(20, 216);
            lblPrinter.Name = "lblPrinter";
            lblPrinter.Size = new System.Drawing.Size(200, 18);
            lblPrinter.TabIndex = 7;
            lblPrinter.Text = "Select Printer";

            // ── cboPrinter ────────────────────────────────────────
            cboPrinter.Anchor = System.Windows.Forms.AnchorStyles.Top
                                     | System.Windows.Forms.AnchorStyles.Left
                                     | System.Windows.Forms.AnchorStyles.Right;
            cboPrinter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboPrinter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cboPrinter.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboPrinter.Location = new System.Drawing.Point(20, 238);
            cboPrinter.Name = "cboPrinter";
            cboPrinter.Size = new System.Drawing.Size(600, 27);
            cboPrinter.TabIndex = 8;

            // ── btnSave ───────────────────────────────────────────
            btnSave.BackColor = System.Drawing.Color.FromArgb(22, 160, 133);
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.Location = new System.Drawing.Point(20, 284);   // below cboPrinter(238+27+gap)
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(130, 40);
            btnSave.TabIndex = 9;
            btnSave.Text = "💾  Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;

            // ── btnClose ──────────────────────────────────────────
            btnClose.BackColor = System.Drawing.Color.FromArgb(149, 165, 166);
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnClose.ForeColor = System.Drawing.Color.White;
            btnClose.Location = new System.Drawing.Point(162, 284);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(130, 40);
            btnClose.TabIndex = 10;
            btnClose.Text = "✕  Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;

            // ── dgvRegistrations ──────────────────────────────────
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(44, 62, 80);
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;

            dgvRegistrations.AllowUserToAddRows = false;
            dgvRegistrations.AllowUserToDeleteRows = false;
            dgvRegistrations.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvRegistrations.BackgroundColor = System.Drawing.Color.White;
            dgvRegistrations.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvRegistrations.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dgvRegistrations.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvRegistrations.ColumnHeadersHeight = 32;
            dgvRegistrations.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvRegistrations.EnableHeadersVisualStyles = false;
            dgvRegistrations.Font = new System.Drawing.Font("Segoe UI", 9F);
            dgvRegistrations.GridColor = System.Drawing.Color.FromArgb(220, 220, 220);
            dgvRegistrations.Name = "dgvRegistrations";
            dgvRegistrations.ReadOnly = true;
            dgvRegistrations.RowHeadersVisible = false;
            dgvRegistrations.RowTemplate.Height = 28;
            dgvRegistrations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvRegistrations.TabIndex = 0;

            // ── Columns ───────────────────────────────────────────
            colName.FillWeight = 38F;
            colName.HeaderText = "Full Name";
            colName.Name = "colName";
            colName.ReadOnly = true;

            colCompany.FillWeight = 38F;
            colCompany.HeaderText = "Company";
            colCompany.Name = "colCompany";
            colCompany.ReadOnly = true;

            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            colCreatedAt.DefaultCellStyle = dataGridViewCellStyle2;
            colCreatedAt.FillWeight = 24F;
            colCreatedAt.HeaderText = "Saved At";
            colCreatedAt.Name = "colCreatedAt";
            colCreatedAt.ReadOnly = true;

            dgvRegistrations.Columns.AddRange(
                new System.Windows.Forms.DataGridViewColumn[] { colName, colCompany, colCreatedAt });

            // ── pnlHeader ─────────────────────────────────────────
            pnlHeader.BackColor = System.Drawing.Color.FromArgb(22, 160, 133);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(640, 56);
            pnlHeader.TabIndex = 2;

            // ── pnlForm ───────────────────────────────────────────
            // Height: 14(top pad) + 18(lbl) + 27(txt) + 18(gap)
            //       + 18(lbl) + 27(txt) + 18(gap)
            //       + 18(lbl) + 29(txt+browse) + 18(gap)
            //       + 18(lbl) + 27(cbo) + 10(gap)
            //       + 40(btns) + 14(bot pad) = 344
            pnlForm.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            pnlForm.Controls.Add(lblFullName);
            pnlForm.Controls.Add(txtFullName);
            pnlForm.Controls.Add(lblCompany);
            pnlForm.Controls.Add(txtCompany);
            pnlForm.Controls.Add(lblBartenderPath);
            pnlForm.Controls.Add(txtBartenderPath);
            pnlForm.Controls.Add(btnBrowseBartender);
            pnlForm.Controls.Add(lblPrinter);
            pnlForm.Controls.Add(cboPrinter);
            pnlForm.Controls.Add(btnSave);
            pnlForm.Controls.Add(btnClose);
            pnlForm.Dock = System.Windows.Forms.DockStyle.Top;
            pnlForm.Name = "pnlForm";
            pnlForm.Padding = new System.Windows.Forms.Padding(20, 14, 20, 10);
            pnlForm.Size = new System.Drawing.Size(640, 344);
            pnlForm.TabIndex = 1;

            // ── pnlGrid ───────────────────────────────────────────
            pnlGrid.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            pnlGrid.Controls.Add(dgvRegistrations);
            pnlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlGrid.Name = "pnlGrid";
            pnlGrid.Padding = new System.Windows.Forms.Padding(20, 10, 20, 16);
            pnlGrid.TabIndex = 0;

            // ── Form ──────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            ClientSize = new System.Drawing.Size(640, 700);
            MinimumSize = new System.Drawing.Size(640, 700);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            ControlBox = true;
            Name = "PrintManual";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Registration Manual";

            // Dock order: Fill first, then Top panels bottom to top
            Controls.Add(pnlGrid);
            Controls.Add(pnlForm);
            Controls.Add(pnlHeader);

            ((System.ComponentModel.ISupportInitialize)dgvRegistrations).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlForm.ResumeLayout(false);
            pnlForm.PerformLayout();
            pnlGrid.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblFullName;
        private System.Windows.Forms.TextBox txtFullName;
        private System.Windows.Forms.Label lblCompany;
        private System.Windows.Forms.TextBox txtCompany;
        private System.Windows.Forms.Label lblBartenderPath;
        private System.Windows.Forms.TextBox txtBartenderPath;
        private System.Windows.Forms.Button btnBrowseBartender;
        private System.Windows.Forms.Label lblPrinter;
        private System.Windows.Forms.ComboBox cboPrinter;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridView dgvRegistrations;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCompany;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCreatedAt;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.Panel pnlGrid;
    }
}