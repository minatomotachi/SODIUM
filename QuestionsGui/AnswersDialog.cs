using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuestionsGui;

public class AnswersDialog : Form
{
    private static readonly HttpClient client = new HttpClient();

    private readonly Question question;
    private readonly Panel header;
    private readonly Label lblTitle;
    private readonly Label lblMeta;
    private readonly FlowLayoutPanel body;
    private readonly Label lblLoading;
    private readonly Button btnClose;

    public AnswersDialog(Question question)
    {
        this.question = question;
        Text = "Answers";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(560, 520);
        MinimumSize = new Size(420, 320);
        BackColor = Ui.Background;
        Font = new Font("Segoe UI", 9f);
        ShowInTaskbar = false;
        CancelButton = CreateCloseButton();

        header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 92,
            BackColor = Ui.HeaderDark
        };
        lblTitle = new Label
        {
            AutoSize = false,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            Location = new Point(20, 14),
            Size = new Size(500, 40),
            Text = question.Title ?? string.Empty
        };
        lblMeta = new Label
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

        lblLoading = new Label
        {
            AutoSize = true,
            ForeColor = Ui.TextMuted,
            Text = "Loading answers...",
            Margin = new Padding(12, 16, 0, 0)
        };
        body.Controls.Add(lblLoading);

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
    }

    private Button CreateCloseButton()
    {
        var btn = Ui.StyledButton("Close", Ui.Primary);
        btn.Width = 100;
        btn.Height = 36;
        btn.Click += (s, e) => Close();
        return btn;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _ = LoadAnswersAsync();
    }

    private async Task LoadAnswersAsync()
    {
        List<Answer> answers;
        try
        {
            HttpResponseMessage response = await client.GetAsync($"http://localhost:5000/questions/{question.Id}/answers");
            if (!response.IsSuccessStatusCode)
            {
                body.Controls.Clear();
                body.Controls.Add(CreateMessage($"Failed to retrieve answers. Status code: {(int)response.StatusCode}"));
                return;
            }

            string json = await response.Content.ReadAsStringAsync();
            answers = JsonSerializer.Deserialize<List<Answer>>(json) ?? new List<Answer>();
        }
        catch (Exception ex)
        {
            body.Controls.Clear();
            body.Controls.Add(CreateMessage($"Error: {ex.Message}"));
            return;
        }

        body.Controls.Clear();

        body.Controls.Add(CreateQuestionBodyCard(question));

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
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var path = Ui.RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12);
            using var border = new Pen(Ui.CardBorder);
            e.Graphics.DrawPath(border, path);
        };

        return card;
    }

    private static Panel CreateQuestionBodyCard(Question question)
    {
        var card = new Panel
        {
            BackColor = Ui.CardBack,
            Padding = new Padding(14, 12, 14, 12),
            Margin = new Padding(0, 0, 0, 10),
            Width = 510
        };

        var tag = new Label
        {
            AutoSize = true,
            ForeColor = Ui.Primary,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Margin = new Padding(0),
            Text = "QUESTION"
        };

        var bodyLabel = new Label
        {
            AutoSize = false,
            ForeColor = Ui.TextDark,
            Font = new Font("Segoe UI", 9.5f),
            Margin = new Padding(0),
            Text = question.Body ?? string.Empty
        };

        card.Controls.Add(tag);
        card.Controls.Add(bodyLabel);

        int tagH = TextRenderer.MeasureText(tag.Text, tag.Font).Height;
        int bodyW = 480;
        int bodyH = TextRenderer.MeasureText(bodyLabel.Text, bodyLabel.Font, new Size(bodyW, int.MaxValue), TextFormatFlags.WordBreak).Height;
        bodyLabel.SetBounds(14, 10 + tagH + 6, bodyW, bodyH);
        card.Height = 14 + tagH + 6 + bodyH + 12;

        card.Resize += (s, e) =>
        {
            tag.Location = new Point(14, 10);
            bodyLabel.Location = new Point(14, tag.Bottom + 6);
        };

        card.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    [JsonPropertyName("question_id")]
    public string? QuestionId { get; set; }
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    [JsonPropertyName("body")]
    public string? Body { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    [JsonPropertyName("votes")]
    public int Votes { get; set; }
    [JsonPropertyName("comments")]
    public List<object>? Comments { get; set; }
}
