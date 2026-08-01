using System.Drawing.Drawing2D;

namespace QuestionsGuiSqlite;

public class AnswersDialog : Form
{
    private readonly Question question;
    private readonly List<Answer> answers;
    private readonly FlowLayoutPanel body;
    private readonly Button btnClose;

    public AnswersDialog(Question question, List<Answer> answers)
    {
        this.question = question;
        this.answers = answers;

        Text = "Answers";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(560, 520);
        MinimumSize = new Size(420, 320);
        BackColor = Ui.Background;
        Font = new Font("Segoe UI", 9f);
        ShowInTaskbar = false;
        CancelButton = CreateCloseButton();

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 92,
            BackColor = Ui.HeaderDark
        };
        var lblTitle = new Label
        {
            AutoSize = false,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            Location = new Point(20, 14),
            Size = new Size(500, 40),
            Text = question.Title ?? string.Empty
        };
        var lblMeta = new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(21, 58),
            Text = $"Posted {question.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}   |   ID: {question.Id}"
        };
        header.Controls.Add(lblTitle);
        header.Controls.Add(lblMeta);

        body = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12),
            BackColor = Ui.Background
        };

        var bottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        btnClose = CreateCloseButton();
        btnClose.Anchor = AnchorStyles.Right;
        btnClose.Location = new Point(bottom.ClientSize.Width - btnClose.Width - 12, (bottom.ClientSize.Height - btnClose.Height) / 2);
        bottom.Resize += (s, e) =>
        {
            btnClose.Location = new Point(bottom.ClientSize.Width - btnClose.Width - 12, (bottom.ClientSize.Height - btnClose.Height) / 2);
        };
        bottom.Controls.Add(btnClose);

        Controls.Add(body);
        Controls.Add(bottom);
        Controls.Add(header);

        Render();
    }

    private Button CreateCloseButton()
    {
        var btn = Ui.StyledButton("Close", Ui.Primary);
        btn.Click += (s, e) => Close();
        return btn;
    }

    private void Render()
    {
        if (answers.Count == 0)
        {
            body.Controls.Add(CreateMessage("No answers yet."));
        }
        else
        {
            body.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = Ui.Primary,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Margin = new Padding(6, 6, 0, 6),
                Text = $"{answers.Count} answer(s)"
            });

            foreach (var answer in answers)
            {
                body.Controls.Add(CreateAnswerCard(answer));
            }
        }
    }

    private static Panel CreateAnswerCard(Answer answer)
    {
        var card = new Panel
        {
            BackColor = Ui.CardBack,
            Padding = new Padding(14, 12, 14, 12),
            Margin = new Padding(0, 0, 0, 10),
            Width = 510
        };

        var meta = new Label
        {
            AutoSize = true,
            ForeColor = Ui.TextMuted,
            Font = new Font("Segoe UI", 8.5f),
            Margin = new Padding(0),
            Text = $"{(answer.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A")}   \u00b7   user: {answer.UserId}   \u00b7   votes: {answer.Votes}"
        };

        var bodyLabel = new Label
        {
            AutoSize = false,
            ForeColor = Ui.TextDark,
            Font = new Font("Segoe UI", 9.5f),
            Margin = new Padding(0),
            Text = answer.Body ?? string.Empty
        };

        card.Controls.Add(meta);
        card.Controls.Add(bodyLabel);

        int metaH = TextRenderer.MeasureText(meta.Text, meta.Font).Height;
        int bodyW = 480;
        int bodyH = TextRenderer.MeasureText(bodyLabel.Text, bodyLabel.Font, new Size(bodyW, int.MaxValue), TextFormatFlags.WordBreak).Height;
        bodyLabel.SetBounds(14, 10 + metaH + 4, bodyW, bodyH);
        card.Height = 14 + metaH + 4 + bodyH + 12;

        card.Resize += (s, e) =>
        {
            meta.Location = new Point(14, 10);
            bodyLabel.Location = new Point(14, meta.Bottom + 4);
        };

        card.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = Ui.RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12);
            using var border = new Pen(Ui.CardBorder);
            e.Graphics.DrawPath(border, path);
        };

        return card;
    }

    private static Label CreateMessage(string text)
    {
        return new Label
        {
            AutoSize = true,
            ForeColor = Ui.TextMuted,
            Font = new Font("Segoe UI", 10f, FontStyle.Italic),
            Margin = new Padding(12, 16, 0, 0),
            Text = text
        };
    }
}

public class Answer
{
    public string? Id { get; set; }
    public string? QuestionId { get; set; }
    public string? UserId { get; set; }
    public string? Body { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long Votes { get; set; }
    public string? Comments { get; set; }
}
