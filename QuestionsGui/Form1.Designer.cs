namespace QuestionsGui;

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
        listQuestions = new ListBox();
        btnRefresh = new Button();
        header = new Panel();
        lblHeader = new Label();
        lblHint = new Label();
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel();
        header.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // listQuestions
        // 
        listQuestions.BackColor = Color.White;
        listQuestions.BorderStyle = BorderStyle.FixedSingle;
        listQuestions.DrawMode = DrawMode.OwnerDrawVariable;
        listQuestions.FormattingEnabled = true;
        listQuestions.ItemHeight = 58;
        listQuestions.Location = new Point(20, 84);
        listQuestions.Name = "listQuestions";
        listQuestions.ScrollAlwaysVisible = true;
        listQuestions.Size = new Size(860, 480);
        listQuestions.TabIndex = 1;
        listQuestions.DrawItem += listQuestions_DrawItem;
        listQuestions.MeasureItem += listQuestions_MeasureItem;
        listQuestions.KeyDown += listQuestions_KeyDown;
        listQuestions.MouseDoubleClick += listQuestions_MouseDoubleClick;
        // 
        // btnRefresh
        // 
        btnRefresh.BackColor = Color.FromArgb(59, 130, 246);
        btnRefresh.Cursor = Cursors.Hand;
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnRefresh.ForeColor = Color.White;
        btnRefresh.Location = new Point(800, 14);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(80, 36);
        btnRefresh.TabIndex = 2;
        btnRefresh.Text = "Refresh";
        btnRefresh.UseVisualStyleBackColor = false;
        btnRefresh.Click += btnRefresh_Click;
        // 
        // header
        // 
        header.BackColor = Color.FromArgb(23, 37, 84);
        header.Controls.Add(lblHeader);
        header.Controls.Add(lblHint);
        header.Dock = DockStyle.Top;
        header.Location = new Point(0, 0);
        header.Name = "header";
        header.Size = new Size(900, 64);
        header.TabIndex = 0;
        // 
        // lblHeader
        // 
        lblHeader.AutoSize = true;
        lblHeader.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblHeader.ForeColor = Color.White;
        lblHeader.Location = new Point(20, 16);
        lblHeader.Name = "lblHeader";
        lblHeader.Size = new Size(224, 37);
        lblHeader.TabIndex = 0;
        lblHeader.Text = "SODIUM  Forum";
        // 
        // lblHint
        // 
        lblHint.AutoSize = true;
        lblHint.Font = new Font("Segoe UI", 8.5F);
        lblHint.ForeColor = Color.FromArgb(148, 163, 184);
        lblHint.Location = new Point(250, 23);
        lblHint.Name = "lblHint";
        lblHint.Size = new Size(293, 20);
        lblHint.TabIndex = 1;
        lblHint.Text = "Double-click a question to view its answers";
        // 
        // statusStrip
        // 
        statusStrip.BackColor = Color.FromArgb(23, 37, 84);
        statusStrip.ImageScalingSize = new Size(20, 20);
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
        statusStrip.Location = new Point(0, 575);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(900, 25);
        statusStrip.TabIndex = 3;
        statusStrip.Text = "statusStrip";
        // 
        // lblStatus
        // 
        lblStatus.ForeColor = Color.White;
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(53, 19);
        lblStatus.Text = "Ready";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(241, 245, 249);
        ClientSize = new Size(900, 600);
        Controls.Add(statusStrip);
        Controls.Add(btnRefresh);
        Controls.Add(listQuestions);
        Controls.Add(header);
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimumSize = new Size(900, 600);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sodium Forum";
        Load += Form1_Load;
        header.ResumeLayout(false);
        header.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.ListBox listQuestions;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Panel header;
    private System.Windows.Forms.Label lblHeader;
    private System.Windows.Forms.Label lblHint;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel lblStatus;
}
