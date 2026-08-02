using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuestionsGui;

public partial class Form1 : Form
{
    private static readonly HttpClient client = new HttpClient();
    private List<Question> questions = new List<Question>();
    private Dictionary<string, int> answerCountCache = new Dictionary<string, int>();

    public Form1()
    {
        InitializeComponent();
        StyleHeader();
    }

    private async void Form1_Load(object? sender, EventArgs e)
    {
        await LoadQuestions();
    }

    private async void btnRefresh_Click(object? sender, EventArgs e)
    {
        await LoadQuestions();
    }

    private void StyleHeader()
    {
        header.BackColor = Ui.HeaderDark;
        lblHeader.ForeColor = Color.White;
        lblHeader.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
        lblHeader.Text = "SODIUM  Forum";
        btnRefresh.BackColor = Ui.Primary;
        btnRefresh.ForeColor = Color.White;
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        btnRefresh.Cursor = Cursors.Hand;
    }

    private async Task LoadQuestions()
    {
        SetStatus("Loading questions...");
        Cursor = Cursors.WaitCursor;
        try
        {
            HttpResponseMessage response = await client.GetAsync("http://localhost:5000/questions");
            if (!response.IsSuccessStatusCode)
            {
                SetStatus($"Failed to retrieve questions. Status code: {(int)response.StatusCode}");
                return;
            }

            string json = await response.Content.ReadAsStringAsync();
            questions = JsonSerializer.Deserialize<List<Question>>(json) ?? new List<Question>();
            answerCountCache.Clear();

            listQuestions.BeginUpdate();
            listQuestions.Items.Clear();
            foreach (var q in questions)
            {
                listQuestions.Items.Add(q);
            }
            listQuestions.EndUpdate();

            SetStatus($"{questions.Count} question(s) loaded");
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void listQuestions_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        int index = listQuestions.IndexFromPoint(e.Location);
        if (index >= 0 && index < listQuestions.Items.Count)
        {
            OpenAnswers((Question)listQuestions.Items[index]);
        }
    }

    private void listQuestions_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && listQuestions.SelectedItem is Question q)
        {
            OpenAnswers(q);
        }
    }

    private async void OpenAnswers(Question q)
    {
        SetStatus($"Loading \"{q.Title}\"...");
        try
        {
            if (!answerCountCache.ContainsKey(q.Id ?? ""))
            {
                HttpResponseMessage response = await client.GetAsync($"http://localhost:5000/questions/{q.Id}/answers");
                if (response.IsSuccessStatusCode)
                {
                    var answers = JsonSerializer.Deserialize<List<Answer>>(await response.Content.ReadAsStringAsync());
                    answerCountCache[q.Id ?? ""] = answers?.Count ?? 0;
                    listQuestions.Invalidate();
                }
            }
        }
        catch
        {
            // popup will surface its own error
        }

        SetStatus("Ready");
        using (var dialog = new AnswersDialog(q))
        {
            dialog.ShowDialog(this);
        }

        if (answerCountCache.TryGetValue(q.Id ?? "", out int count))
        {
            SetStatus($"\"{q.Title}\" - {count} answer(s)");
        }
    }

    private void SetStatus(string text)
    {
        lblStatus.Text = text;
    }

    private void listQuestions_MeasureItem(object? sender, MeasureItemEventArgs e)
    {
        e.ItemHeight = 58;
    }

    private void listQuestions_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        e.DrawBackground();

        bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color back = selected ? Ui.Primary : (e.Index % 2 == 0 ? Ui.CardBack : Ui.CardBackAlt);

        using (var brush = new SolidBrush(back))
        {
            e.Graphics.FillRectangle(brush, e.Bounds);
        }

        if (e.Index < questions.Count)
        {
            var q = questions[e.Index];
            using var titleFont = new Font(Font.FontFamily, 10.5f, FontStyle.Bold);
            using var timeFont = new Font(Font.FontFamily, 8.5f);
            using var hintFont = new Font(Font.FontFamily, 8f);

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color titleColor = selected ? Color.White : Ui.TextDark;
            Color timeColor = selected ? Color.FromArgb(224, 231, 255) : Ui.TextMuted;

            e.Graphics.DrawString(q.Title ?? string.Empty, titleFont, new SolidBrush(titleColor),
                new RectangleF(e.Bounds.Left + 16, e.Bounds.Top + 10, e.Bounds.Width - 32, 26));
            e.Graphics.DrawString(q.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A", timeFont, new SolidBrush(timeColor),
                new RectangleF(e.Bounds.Left + 16, e.Bounds.Bottom - 22, e.Bounds.Width - 32, 16));

            string hint = selected ? "Double-click to view answers" : "Double-click to view answers";
            Size hintSize = TextRenderer.MeasureText(hint, hintFont);
            var hintRect = new Rectangle(e.Bounds.Right - hintSize.Width - 16, e.Bounds.Top + 8, hintSize.Width, hintSize.Height);
            TextRenderer.DrawText(e.Graphics, hint, hintFont, hintRect, selected ? Color.FromArgb(199, 210, 254) : Ui.TextMuted);

            int answers = answerCountCache.TryGetValue(q.Id ?? "", out var cached) ? cached : -1;
            if (answers >= 0)
            {
                string label = answers == 0 ? "no answers" : $"{answers} answer(s)";
                var badgeRect = new Rectangle(e.Bounds.Right - 110, e.Bounds.Bottom - 24, 96, 18);
                using var path = Ui.RoundedRect(badgeRect, 9);
                using var badgeBrush = new SolidBrush(selected ? Color.FromArgb(76, 110, 220) : Ui.BadgeBack);
                Color textColor = selected ? Color.White : Ui.BadgeText;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(badgeBrush, path);
                TextRenderer.DrawText(e.Graphics, label, hintFont, badgeRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        e.DrawFocusRectangle();
    }
}

public class Question
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("body")]
    public string? Body { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    public override string ToString() => Title ?? string.Empty;
}
