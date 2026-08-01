namespace QuestionsGuiSqlite;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.listQuestions = new System.Windows.Forms.ListBox();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.header = new System.Windows.Forms.Panel();
        this.lblHeader = new System.Windows.Forms.Label();
        this.lblHint = new System.Windows.Forms.Label();
        this.statusStrip = new System.Windows.Forms.StatusStrip();
        this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
        this.header.SuspendLayout();
        this.statusStrip.SuspendLayout();
        this.SuspendLayout();
        //
        // header
        //
        this.header.BackColor = System.Drawing.Color.FromArgb(23, 37, 84);
        this.header.Controls.Add(this.lblHeader);
        this.header.Controls.Add(this.lblHint);
        this.header.Dock = System.Windows.Forms.DockStyle.Top;
        this.header.Location = new System.Drawing.Point(0, 0);
        this.header.Name = "header";
        this.header.Size = new System.Drawing.Size(900, 64);
        this.header.TabIndex = 0;
        //
        // lblHeader
        //
        this.lblHeader.AutoSize = true;
        this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblHeader.ForeColor = System.Drawing.Color.White;
        this.lblHeader.Location = new System.Drawing.Point(20, 16);
        this.lblHeader.Name = "lblHeader";
        this.lblHeader.Size = new System.Drawing.Size(200, 30);
        this.lblHeader.Text = "SODIUM  Forum SQLite";
        //
        // lblHint
        //
        this.lblHint.AutoSize = true;
        this.lblHint.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblHint.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
        this.lblHint.Location = new System.Drawing.Point(250, 24);
        this.lblHint.Name = "lblHint";
        this.lblHint.Size = new System.Drawing.Size(280, 15);
        this.lblHint.Text = "Downloads forum.db - double-click to view answers";
        //
        // listQuestions
        //
        this.listQuestions.BackColor = System.Drawing.Color.White;
        this.listQuestions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.listQuestions.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
        this.listQuestions.FormattingEnabled = true;
        this.listQuestions.ItemHeight = 58;
        this.listQuestions.Location = new System.Drawing.Point(20, 84);
        this.listQuestions.Name = "listQuestions";
        this.listQuestions.ScrollAlwaysVisible = true;
        this.listQuestions.Size = new System.Drawing.Size(860, 480);
        this.listQuestions.TabIndex = 1;
        this.listQuestions.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listQuestions_DrawItem);
        this.listQuestions.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.listQuestions_MeasureItem);
        this.listQuestions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listQuestions_MouseDoubleClick);
        this.listQuestions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listQuestions_KeyDown);
        //
        // btnRefresh
        //
        this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(59, 130, 246);
        this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnRefresh.FlatAppearance.BorderSize = 0;
        this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnRefresh.ForeColor = System.Drawing.Color.White;
        this.btnRefresh.Location = new System.Drawing.Point(800, 14);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(80, 36);
        this.btnRefresh.TabIndex = 2;
        this.btnRefresh.Text = "Refresh";
        this.btnRefresh.UseVisualStyleBackColor = false;
        this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
        //
        // statusStrip
        //
        this.statusStrip.BackColor = System.Drawing.Color.FromArgb(23, 37, 84);
        this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
        this.statusStrip.Location = new System.Drawing.Point(0, 578);
        this.statusStrip.Name = "statusStrip";
        this.statusStrip.Size = new System.Drawing.Size(900, 22);
        this.statusStrip.TabIndex = 3;
        this.statusStrip.Text = "statusStrip";
        //
        // lblStatus
        //
        this.lblStatus.ForeColor = System.Drawing.Color.White;
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(39, 17);
        this.lblStatus.Text = "Ready";
        //
        // Form1
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
        this.ClientSize = new System.Drawing.Size(900, 600);
        this.Controls.Add(this.statusStrip);
        this.Controls.Add(this.btnRefresh);
        this.Controls.Add(this.listQuestions);
        this.Controls.Add(this.header);
        this.DoubleBuffered = true;
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = true;
        this.MinimumSize = new System.Drawing.Size(900, 600);
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Sodium Forum SQLite";
        this.Load += new System.EventHandler(this.Form1_Load);
        this.header.ResumeLayout(false);
        this.header.PerformLayout();
        this.statusStrip.ResumeLayout(false);
        this.statusStrip.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.ListBox listQuestions;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Panel header;
    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.Label lblHint;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel lblStatus;
}
