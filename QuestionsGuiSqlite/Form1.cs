using System.Drawing.Drawing2D;
using System.Net.Http;
using Microsoft.Data.Sqlite;

namespace QuestionsGuiSqlite;

public partial class Form1 : Form
{
    private static readonly HttpClient client = new HttpClient();
    private readonly string dbPath = Path.Combine(AppContext.BaseDirectory, "forum_download.db");

    private List<Question> questions = new List<Question>();
    private Dictionary<string, List<Answer>> answerCache = new Dictionary<string, List<Answer>>();

    public Form1()
    {
        InitializeComponent();
        StyleHeader();
    }

    private async void Form1_Load(object? sender, EventArgs e)
    {
        await RefreshData();
    }

    private async void btnRefresh_Click(object? sender, EventArgs e)
    {
        await RefreshData();
    }

    private void StyleHeader()
    {
        header.BackColor = Ui.HeaderDark;
        lblHeader.ForeColor = Color.White;
        lblHeader.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
        btnRefresh.BackColor = Ui.Primary;
        btnRefresh.ForeColor = Color.White;
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        btnRefresh.Cursor = Cursors.Hand;
    }

    private async Task RefreshData()
    {
        SetStatus("Downloading forum.db...");
        Cursor = Cursors.WaitCursor;
        try
        {
            HttpResponseMessage response = await client.GetAsync("http://localhost:5000/download/db");
            if (!response.IsSuccessStatusCode)
            {
                SetStatus($"Failed to download database. Status code: {(int)response.StatusCode}");
                return;
            }

            await using (var fs = new FileStream(dbPath, FileMode.Create, FileAccess.Write))
            {
                await response.Content.CopyToAsync(fs);
            }

            LoadFromSqlite();
            SetStatus($"Loaded {questions.Count} question(s) from forum.db");
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

    private void LoadFromSqlite()
    {
        questions = new List<Question>();
        answerCache = new Dictionary<string, List<Answer>>();

        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT id, title, body, tags, user_id, created_at, updated_at, views, votes FROM questions";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var q = new Question
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Body = reader.GetString(2),
                    Tags = reader.IsDBNull(3) ? null : reader.GetString(3),
                    UserId = reader.GetString(4),
                    CreatedAt = ParseTime(reader.GetString(5)),
                    UpdatedAt = ParseTime(reader.GetString(6)),
                    Views = reader.GetInt64(7),
                    Votes = reader.GetInt64(8)
                };
                questions.Add(q);
            }
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT id, question_id, user_id, body, created_at, updated_at, votes, comments FROM answers";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var a = new Answer
                {
                    Id = reader.GetString(0),
                    QuestionId = reader.GetString(1),
                    UserId = reader.GetString(2),
                    Body = reader.GetString(3),
                    CreatedAt = ParseTime(reader.GetString(4)),
                    UpdatedAt = ParseTime(reader.GetString(5)),
                    Votes = reader.GetInt64(6),
                    Comments = reader.IsDBNull(7) ? null : reader.GetString(7)
                };
                if (!answerCache.TryGetValue(a.QuestionId, out var list))
                {
                    list = new List<Answer>();
                    answerCache[a.QuestionId] = list;
                }
                list.Add(a);
            }
        }

        listQuestions.BeginUpdate();
        listQuestions.Items.Clear();
        foreach (var q in questions)
        {
            listQuestions.Items.Add(q);
        }
        listQuestions.EndUpdate();
    }

    private static DateTime? ParseTime(string value)
    {
        if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
        {
            return dt;
        }
        return null;
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

    private void OpenAnswers(Question q)
    {
        var answers = answerCache.TryGetValue(q.Id ?? "", out var cached)
            ? cached
            : new List<Answer>();

        SetStatus($"\"{q.Title}\" - {answers.Count} answer(s)");
        using (var dialog = new AnswersDialog(q, answers))
        {
            dialog.ShowDialog(this);
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

            int answers = answerCache.TryGetValue(q.Id ?? "", out var cached) ? cached.Count : 0;
            string label = answers == 0 ? "no answers" : $"{answers} answer(s)";
            var badgeRect = new Rectangle(e.Bounds.Right - 110, e.Bounds.Bottom - 24, 96, 18);
            using var path = Ui.RoundedRect(badgeRect, 9);
            using var badgeBrush = new SolidBrush(selected ? Color.FromArgb(76, 110, 220) : Ui.BadgeBack);
            Color textColor = selected ? Color.White : Ui.BadgeText;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillPath(badgeBrush, path);
            TextRenderer.DrawText(e.Graphics, label, hintFont, badgeRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        e.DrawFocusRectangle();
    }
}

public class Question
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Tags { get; set; }
    public string? UserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long Views { get; set; }
    public long Votes { get; set; }
    public override string ToString() => Title ?? string.Empty;
}
