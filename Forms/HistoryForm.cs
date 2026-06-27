using HabitTracker.Services;
using HabitTracker.Models;

namespace HabitTracker.Forms;

public class HistoryForm : Form
{
    private readonly HabitService _service;
    private ComboBox  cboHabit   = null!;
    private Panel     pnlCal     = null!;
    private List<Habit> _habits  = new();
    private int _year  = DateTime.Now.Year;
    private int _month = DateTime.Now.Month;

    private static readonly Color BG    = Color.FromArgb(245, 244, 240);
    private static readonly Color CARD  = Color.White;
    private static readonly Color INK   = Color.FromArgb(30, 28, 25);
    private static readonly Color INK2  = Color.FromArgb(120, 115, 105);
    private static readonly Color TERRA = Color.FromArgb(220, 100, 60);

    public HistoryForm(HabitService service)
    {
        _service = service;
        InitializeComponent();
        LoadHabits();
    }

    private void InitializeComponent()
    {
        this.Text            = "Historial";
        this.Size            = new Size(620, 580);
        this.BackColor       = BG;
        this.ForeColor       = INK;
        this.StartPosition   = FormStartPosition.CenterParent;
        this.Font            = new Font("Segoe UI", 10f);

        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = TERRA };
        pnlHead.Controls.Add(new Label
        {
            Text = "HISTORIAL", ForeColor = Color.White,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0)
        });

        var pnlControls = new Panel
        {
            Dock = DockStyle.Top, Height = 52, BackColor = CARD,
            Padding = new Padding(16, 10, 16, 10)
        };

        cboHabit = new ComboBox
        {
            Location = new Point(16, 12), Width = 260,
            BackColor = BG, ForeColor = INK,
            FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboHabit.SelectedIndexChanged += (s, e) => pnlCal.Invalidate();

        var btnPrev = MakeNavBtn("◀", new Point(286, 11));
        var btnNext = MakeNavBtn("▶", new Point(326, 11));
        btnPrev.Click += (s, e) => { _month--; if (_month < 1) { _month = 12; _year--; } pnlCal.Invalidate(); };
        btnNext.Click += (s, e) => { _month++; if (_month > 12) { _month = 1; _year++; } pnlCal.Invalidate(); };

        pnlControls.Controls.Add(cboHabit);
        pnlControls.Controls.Add(btnPrev);
        pnlControls.Controls.Add(btnNext);

        pnlCal = new Panel { Dock = DockStyle.Fill, BackColor = CARD, Padding = new Padding(20) };
        pnlCal.Paint += DrawCalendar;

        this.Controls.Add(pnlCal);
        this.Controls.Add(pnlControls);
        this.Controls.Add(pnlHead);
    }

    private Button MakeNavBtn(string text, Point loc)
    {
        var b = new Button
        {
            Text = text, Location = loc, Size = new Size(32, 28),
            FlatStyle = FlatStyle.Flat, BackColor = BG, ForeColor = INK, Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(210, 208, 203);
        b.FlatAppearance.BorderSize  = 1;
        return b;
    }

    private void LoadHabits()
    {
        _habits = _service.GetAllHabits();
        cboHabit.Items.Clear();
        foreach (var h in _habits) cboHabit.Items.Add($"{h.Emoji}  {h.Name}");
        if (_habits.Any()) cboHabit.SelectedIndex = 0;
    }

    private void DrawCalendar(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(CARD);

        if (cboHabit.SelectedIndex < 0 || cboHabit.SelectedIndex >= _habits.Count) return;

        var habit   = _habits[cboHabit.SelectedIndex];
        var logs    = _service.GetLogsForMonth(habit.Id, _year, _month);
        var doneDays = logs.Where(l => l.Completed).Select(l => l.Date.Day).ToHashSet();
        Color accent;
        try   { accent = ColorTranslator.FromHtml(habit.Color); }
        catch { accent = TERRA; }

        // Month + year header
        var monthStr = new DateTime(_year, _month, 1)
            .ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"))
            .ToUpper();
        g.DrawString(monthStr, new Font("Segoe UI", 13f, FontStyle.Bold),
            new SolidBrush(INK), 20, 14);

        // Day-of-week headers
        var days = new[] { "L", "M", "X", "J", "V", "S", "D" };
        int cellW = (pnlCal.Width - 40) / 7;
        int cellH = 54;
        int ox = 20, oy = 52;

        for (int d = 0; d < 7; d++)
            g.DrawString(days[d], new Font("Segoe UI", 9f, FontStyle.Bold),
                new SolidBrush(INK2), ox + d * cellW + cellW / 2 - 5, oy);

        oy += 26;
        var first  = new DateTime(_year, _month, 1);
        // Week starts Monday (0=Mon … 6=Sun)
        int startCol = ((int)first.DayOfWeek + 6) % 7;
        int daysInM  = DateTime.DaysInMonth(_year, _month);
        int col = startCol, row = 0;

        for (int day = 1; day <= daysInM; day++)
        {
            int cx = ox + col * cellW + cellW / 2 - 18;
            int cy = oy + row * cellH + 4;
            bool done    = doneDays.Contains(day);
            bool isToday = day == DateTime.Today.Day && _month == DateTime.Today.Month && _year == DateTime.Today.Year;

            var circle = new Rectangle(cx, cy, 36, 36);
            if (done)
            {
                g.FillEllipse(new SolidBrush(accent), circle);
                g.DrawString("✓", new Font("Segoe UI", 13f, FontStyle.Bold),
                    Brushes.White, cx + 7, cy + 7);
            }
            else if (isToday)
            {
                g.DrawEllipse(new Pen(TERRA, 2), circle);
            }

            var dayColor = done ? Color.White
                         : isToday ? TERRA
                         : Color.FromArgb(100, 98, 92);
            if (!done)
                g.DrawString(day.ToString(), new Font("Segoe UI", 9.5f),
                    new SolidBrush(dayColor), cx + (day >= 10 ? 5 : 11), cy + 9);

            col++;
            if (col == 7) { col = 0; row++; }
        }

        // Footer
        int passed = _month == DateTime.Today.Month && _year == DateTime.Today.Year
                     ? DateTime.Today.Day : daysInM;
        string footer = $"✅  {doneDays.Count} de {passed} días completados  ·  {habit.CompletionRateThisMonth:F0}% del mes";
        g.DrawString(footer, new Font("Segoe UI", 9f),
            new SolidBrush(INK2), 20, pnlCal.Height - 32);
    }
}
