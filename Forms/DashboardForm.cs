using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.Forms;

public class DashboardForm : Form
{
    private readonly HabitService _service;

    // ── Palette: warm paper / editorial ──────────────────────────
    private static readonly Color BG      = Color.FromArgb(245, 244, 240);
    private static readonly Color CARD    = Color.White;
    private static readonly Color INK     = Color.FromArgb(30, 28, 25);
    private static readonly Color INK2    = Color.FromArgb(120, 115, 105);
    private static readonly Color TERRA   = Color.FromArgb(220, 100, 60);
    private static readonly Color AMBER   = Color.FromArgb(240, 180, 60);
    private static readonly Color TEAL    = Color.FromArgb(60, 160, 140);
    private static readonly Color STEEL   = Color.FromArgb(60, 120, 180);

    private Panel pnlHabitList = null!;
    private Label lblDateSub   = null!;

    public DashboardForm(HabitService service)
    {
        _service = service;
        InitializeComponent();
        LoadAll();
    }

    private void InitializeComponent()
    {
        this.Text            = "HabitTracker";
        this.Size            = new Size(1020, 680);
        this.MinimumSize     = new Size(800, 560);
        this.BackColor       = BG;
        this.ForeColor       = INK;
        this.StartPosition   = FormStartPosition.CenterScreen;
        this.Font            = new Font("Segoe UI", 10f);

        // ── Top bar ───────────────────────────────────────────────
        var pnlTop = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 56,
            BackColor = INK
        };
        var lblAppName = new Label
        {
            Text      = "HABITTRACKER",
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
            Dock      = DockStyle.Left,
            Width     = 220,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var pnlTopRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

        var btnNew = MakeTopBtn("+ Nuevo hábito", TERRA);
        btnNew.Dock = DockStyle.Right;
        btnNew.Click += (s, e) => { new AddEditHabitForm(_service, null).ShowDialog(this); LoadAll(); };

        var btnStats = MakeTopBtn("Estadísticas", Color.FromArgb(70, 68, 62));
        btnStats.Dock = DockStyle.Right;
        btnStats.Click += (s, e) => new StatsForm(_service).ShowDialog(this);

        var btnHist = MakeTopBtn("Historial", Color.FromArgb(70, 68, 62));
        btnHist.Dock = DockStyle.Right;
        btnHist.Click += (s, e) => { new HistoryForm(_service).ShowDialog(this); };

        pnlTopRight.Controls.Add(btnNew);
        pnlTopRight.Controls.Add(btnStats);
        pnlTopRight.Controls.Add(btnHist);
        pnlTop.Controls.Add(pnlTopRight);
        pnlTop.Controls.Add(lblAppName);

        // ── Hero / date strip ─────────────────────────────────────
        var pnlHero = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 90,
            BackColor = TERRA,
            Padding   = new Padding(28, 0, 0, 0)
        };
        var lblHeroGreet = new Label
        {
            Text      = GetGreeting(),
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 20f, FontStyle.Bold),
            Location  = new Point(28, 14),
            AutoSize  = true,
            BackColor = Color.Transparent
        };
        lblDateSub = new Label
        {
            Text      = DateTime.Now.ToString("dddd, dd 'de' MMMM",
                            new System.Globalization.CultureInfo("es-ES")),
            ForeColor = Color.FromArgb(255, 210, 190),
            Font      = new Font("Segoe UI", 10f),
            Location  = new Point(28, 54),
            AutoSize  = true,
            BackColor = Color.Transparent
        };
        pnlHero.Controls.Add(lblHeroGreet);
        pnlHero.Controls.Add(lblDateSub);

        // ── Body ──────────────────────────────────────────────────
        var pnlBody = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 1,
            BackColor   = Color.Transparent,
            Padding     = new Padding(24, 20, 24, 20)
        };
        pnlBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        pnlBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        // Left — habit list
        var pnlLeft = new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill };

        var lblToday = new Label
        {
            Text      = "HOY",
            ForeColor = TERRA,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            Dock      = DockStyle.Top,
            Height    = 28,
            TextAlign = ContentAlignment.BottomLeft
        };

        pnlHabitList = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.Transparent,
            AutoScroll = true
        };

        pnlLeft.Controls.Add(pnlHabitList);
        pnlLeft.Controls.Add(lblToday);

        // Right — stats summary
        var pnlRight = new Panel
        {
            BackColor = Color.Transparent,
            Dock      = DockStyle.Fill,
            Padding   = new Padding(16, 0, 0, 0)
        };

        var lblSummTitle = new Label
        {
            Text      = "RESUMEN",
            ForeColor = TERRA,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            Dock      = DockStyle.Top,
            Height    = 28,
            TextAlign = ContentAlignment.BottomLeft
        };

        var pnlSummCards = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.Transparent
        };
        // We'll fill this in LoadAll()
        pnlSummCards.Tag = "summary";

        pnlRight.Controls.Add(pnlSummCards);
        pnlRight.Controls.Add(lblSummTitle);

        pnlBody.Controls.Add(pnlLeft,  0, 0);
        pnlBody.Controls.Add(pnlRight, 1, 0);

        this.Controls.Add(pnlBody);
        this.Controls.Add(pnlHero);
        this.Controls.Add(pnlTop);
    }

    // ─────────────────────────────────────────────────────────────
    private void LoadAll()
    {
        LoadHabits();
        LoadSummary();
        lblDateSub.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM",
                              new System.Globalization.CultureInfo("es-ES"));
    }

    private void LoadHabits()
    {
        pnlHabitList.Controls.Clear();
        var habits = _service.GetAllHabits();

        if (!habits.Any())
        {
            pnlHabitList.Controls.Add(new Label
            {
                Text      = "Todavía no tienes hábitos.\nPulsa \"+ Nuevo hábito\" para empezar.",
                ForeColor = INK2,
                Font      = new Font("Segoe UI", 11f),
                AutoSize  = true,
                Margin    = new Padding(0, 20, 0, 0)
            });
            return;
        }

        // Build rows bottom-up so Dock works right
        int y = 4;
        foreach (var habit in habits)
        {
            var card = BuildHabitCard(habit);
            card.Top    = y;
            card.Left   = 0;
            card.Width  = pnlHabitList.ClientSize.Width - 8;
            card.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            pnlHabitList.Controls.Add(card);
            y += card.Height + 8;
        }

        pnlHabitList.Resize += (s, e) =>
        {
            foreach (Control c in pnlHabitList.Controls)
                c.Width = pnlHabitList.ClientSize.Width - 8;
        };
    }

    private Panel BuildHabitCard(Habit habit)
    {
        bool done = habit.IsCompletedToday;
        Color accent;
        try   { accent = ColorTranslator.FromHtml(habit.Color); }
        catch { accent = TERRA; }

        var card = new Panel
        {
            Height    = 72,
            BackColor = CARD
        };

        // Left accent stripe
        var stripe = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = 5,
            BackColor = done ? accent : Color.FromArgb(210, 208, 203)
        };

        // Emoji circle
        var pnlEmoji = new Panel
        {
            Width     = 52,
            Height    = 52,
            BackColor = Color.FromArgb(done ? 30 : 18,
                                       accent.R, accent.G, accent.B),
            Location  = new Point(16, 10)
        };
        pnlEmoji.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(
                new SolidBrush(pnlEmoji.BackColor),
                new Rectangle(0, 0, pnlEmoji.Width, pnlEmoji.Height));
        };
        var lblEmoji = new Label
        {
            Text      = habit.Emoji,
            Font      = new Font("Segoe UI Emoji", 18f),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };
        pnlEmoji.Controls.Add(lblEmoji);

        // Name + meta
        var lblName = new Label
        {
            Text      = habit.Name,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = done ? INK : Color.FromArgb(80, 78, 72),
            Location  = new Point(78, 12),
            AutoSize  = true,
            BackColor = Color.Transparent
        };
        var lblMeta = new Label
        {
            Text      = habit.CurrentStreak > 0
                            ? $"🔥 {habit.CurrentStreak} días seguidos  ·  {habit.CompletionRateThisMonth:F0}% este mes"
                            : $"Sin racha  ·  {habit.CompletionRateThisMonth:F0}% este mes",
            ForeColor = INK2,
            Font      = new Font("Segoe UI", 8.5f),
            Location  = new Point(78, 38),
            AutoSize  = true,
            BackColor = Color.Transparent
        };

        // Check button
        var btnCheck = new Button
        {
            Text      = done ? "✓" : "",
            Size      = new Size(36, 36),
            Location  = new Point(card.Width - 100, 18),
            FlatStyle = FlatStyle.Flat,
            BackColor = done ? accent : Color.White,
            ForeColor = done ? Color.White : Color.FromArgb(200, 195, 188),
            Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
            Cursor    = Cursors.Hand,
            Anchor    = AnchorStyles.Right | AnchorStyles.Top
        };
        btnCheck.FlatAppearance.BorderColor = done ? accent : Color.FromArgb(200, 195, 188);
        btnCheck.FlatAppearance.BorderSize  = 2;
        btnCheck.Click += (s, e) => { _service.ToggleToday(habit.Id); LoadAll(); };

        // Edit button
        var btnEdit = new Button
        {
            Text      = "✎",
            Size      = new Size(30, 30),
            Location  = new Point(card.Width - 52, 21),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = INK2,
            Font      = new Font("Segoe UI", 12f),
            Cursor    = Cursors.Hand,
            Anchor    = AnchorStyles.Right | AnchorStyles.Top
        };
        btnEdit.FlatAppearance.BorderSize = 0;
        btnEdit.Click += (s, e) =>
        {
            new AddEditHabitForm(_service, habit).ShowDialog(this);
            LoadAll();
        };

        card.Controls.AddRange(new Control[] { stripe, pnlEmoji, lblName, lblMeta, btnCheck, btnEdit });
        card.Resize += (s, e) =>
        {
            btnCheck.Left = card.Width - 100;
            btnEdit.Left  = card.Width - 52;
        };

        // Hover highlight
        card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(252, 251, 249);
        card.MouseLeave += (s, e) => card.BackColor = CARD;

        return card;
    }

    private void LoadSummary()
    {
        // Find the summary panel
        Panel? pnlRight = null;
        foreach (Control c in this.Controls)
        {
            if (c is TableLayoutPanel tlp)
            {
                foreach (Control child in tlp.Controls)
                    if (child is Panel p)
                        foreach (Control gc in p.Controls)
                            if (gc is Panel gcp && gcp.Tag?.ToString() == "summary")
                                pnlRight = gcp;
            }
        }
        if (pnlRight == null) return;

        pnlRight.Controls.Clear();
        var habits = _service.GetAllHabits();
        int done  = habits.Count(h => h.IsCompletedToday);
        int total = habits.Count;
        int best  = total > 0 ? habits.Max(h => h.CurrentStreak) : 0;
        double avg = total > 0 ? habits.Average(h => h.CompletionRateThisMonth) : 0;

        var items = new[]
        {
            (done.ToString() + " / " + total, "completados hoy",   TERRA),
            (best + " días",                   "mejor racha actual", AMBER),
            (avg.ToString("F0") + "%",         "promedio del mes",   TEAL),
        };

        int y2 = 0;
        foreach (var (val, lbl, col) in items)
        {
            var card = new Panel
            {
                BackColor = CARD,
                Width     = pnlRight.ClientSize.Width - 4,
                Height    = 72,
                Top       = y2,
                Anchor    = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(col, 3);
                e.Graphics.DrawLine(pen, 0, 0, 0, card.Height);
            };
            card.Controls.Add(new Label
            {
                Text      = val,
                ForeColor = col,
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                Location  = new Point(12, 10),
                AutoSize  = true,
                BackColor = Color.Transparent
            });
            card.Controls.Add(new Label
            {
                Text      = lbl,
                ForeColor = INK2,
                Font      = new Font("Segoe UI", 9f),
                Location  = new Point(12, 44),
                AutoSize  = true,
                BackColor = Color.Transparent
            });
            pnlRight.Controls.Add(card);
            y2 += 80;
        }

        pnlRight.Resize += (s, e) =>
        {
            foreach (Control c in pnlRight.Controls)
                c.Width = pnlRight.ClientSize.Width - 4;
        };
    }

    private static Button MakeTopBtn(string text, Color bg)
    {
        var b = new Button
        {
            Text      = text,
            BackColor = bg,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9.5f),
            Height    = 56,
            Width     = 140,
            Cursor    = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    private static string GetGreeting()
    {
        int h = DateTime.Now.Hour;
        return h < 12 ? "Buenos días" : h < 19 ? "Buenas tardes" : "Buenas noches";
    }
}
