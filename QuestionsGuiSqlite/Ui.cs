using System.Drawing.Drawing2D;

namespace QuestionsGuiSqlite;

public static class Ui
{
    public static readonly Color HeaderDark = Color.FromArgb(23, 37, 84);
    public static readonly Color Primary = Color.FromArgb(59, 130, 246);
    public static readonly Color PrimaryHover = Color.FromArgb(37, 99, 235);
    public static readonly Color Background = Color.FromArgb(241, 245, 249);
    public static readonly Color CardBack = Color.White;
    public static readonly Color CardBackAlt = Color.FromArgb(248, 250, 252);
    public static readonly Color CardBorder = Color.FromArgb(226, 232, 240);
    public static readonly Color TextDark = Color.FromArgb(17, 24, 39);
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
    public static readonly Color BadgeBack = Color.FromArgb(219, 234, 254);
    public static readonly Color BadgeText = Color.FromArgb(29, 78, 216);

    public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static Button StyledButton(string text, Color back)
    {
        var btn = new Button
        {
            Text = text,
            BackColor = back,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Height = 36,
            Width = 100
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.MouseEnter += (s, e) => btn.BackColor = back == Ui.Primary ? Ui.PrimaryHover : back;
        btn.MouseLeave += (s, e) => btn.BackColor = back;
        return btn;
    }
}
