using HabitTracker.Services;

namespace HabitTracker.Forms;

public class StatsForm : Form
{
    private readonly HabitService _service;
    private Panel pnlChart = null!;

    private static readonly Color BG    = Color.FromArgb(245, 244, 240);
    private static readonly Color CARD  = Color.White;
    private static readonly Color INK   = Color.FromArgb(30, 28, 25);
    private static readonly Color INK2  = Color.FromArgb(120, 115, 105);
    private static readonly Color WARM1 = Color.FromArgb(220, 100, 60);
    private static readonly Color WARM2 = Color.FromArgb(240, 180, 60);
    private static readonly Color COOL1 = Color.FromArgb(60, 140, 180);
    private static readonly Color GREEN1= Color.FromArgb(80, 160, 100);

    public StatsForm(HabitService service)
    {
        _service = service;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text          = "Estadísticas";
        this.Size          = new Size(800, 700);
        this.MinimumSize   = new Size(700, 580);
        this.BackColor     = BG;
        this.ForeColor     = INK;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Font          = new Font("Segoe UI", 10f);

        // ── Header ────────────────────────────────────────────────
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = WARM1 };
        pnlHeader.Controls.Add(new Label
        {
            Text      = "ESTADÍSTICAS",
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(28, 0, 0, 0)
        });

        // ── Scrollable body ───────────────────────────────────────
        var scroll = new Panel
        {
            Dock      = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Padding   = new Padding(24, 16, 24, 16)
        };

        // ── Summary cards row ─────────────────────────────────────
        var habits   = _service.GetAllHabits();
        int doneToday = habits.Count(h => h.IsCompletedToday);
        int total     = habits.Count;
        int bestStreak= total > 0 ? habits.Max(h => h.CurrentStreak) : 0;
        double avgRate= total > 0 ? habits.Average(h => h.CompletionRateThisMonth) : 0;

        var summaryItems = new[]
        {
            (doneToday + " / " + total, "completados hoy",    WARM1),
            (bestStreak + " días",       "mejor racha actual", WARM2),
            ((int)avgRate + "%",          "promedio del mes",   COOL1),
        };

        var pnlSummary = new TableLayoutPanel
        {
            ColumnCount = 3,
            RowCount    = 1,
            Height      = 90,
            Dock        = DockStyle.Top,
            BackColor   = Color.Transparent,
            Margin      = new Padding(0, 0, 0, 16)
        };
        pnlSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        pnlSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        pnlSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.4f));

        int ci2 = 0;
        foreach (var (val, lbl, col) in summaryItems)
        {
            var card = new Panel
            {
                BackColor = CARD,
                Dock      = DockStyle.Fill,
                Margin    = new Padding(ci2 == 0 ? 0 : 8, 0, ci2 == 2 ? 0 : 0, 0)
            };
            var accent = col;
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(accent, 4);
                e.Graphics.DrawLine(pen, 0, 0, 0, card.Height);
            };
            card.Controls.Add(new Label
            {
                Text      = val,
                ForeColor = col,
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                Location  = new Point(14, 10),
                AutoSize  = true,
                BackColor = Color.Transparent
            });
            card.Controls.Add(new Label
            {
                Text      = lbl,
                ForeColor = INK2,
                Font      = new Font("Segoe UI", 9f),
                Location  = new Point(14, 52),
                AutoSize  = true,
                BackColor = Color.Transparent
            });
            pnlSummary.Controls.Add(card, ci2++, 0);
        }

        // ── Chart card ────────────────────────────────────────────
        var pnlChartCard = new Panel
        {
            BackColor = CARD,
            Height    = 230,
            Dock      = DockStyle.Top,
            Margin    = new Padding(0, 0, 0, 16),
            Padding   = new Padding(0)
        };
        var lblChartTitle = new Label
        {
            Text      = "Completados esta semana",
            ForeColor = INK2,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            Dock      = DockStyle.Top,
            Height    = 38,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(18, 0, 0, 0)
        };
        pnlChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        pnlChart.Paint += DrawBarChart;
        pnlChartCard.Controls.Add(pnlChart);
        pnlChartCard.Controls.Add(lblChartTitle);

        // ── Habits completion list ────────────────────────────────
        var lblHabitsTitle = new Label
        {
            Text      = "TASA DE COMPLETADO — ESTE MES",
            ForeColor = INK2,
            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Height    = 36,
            Dock      = DockStyle.Top,
            TextAlign = ContentAlignment.BottomLeft,
            Margin    = new Padding(0, 0, 0, 8),
            BackColor = Color.Transparent
        };

        // Inner panel holds chart + labels + list, stacked top-down
        var inner = new Panel
        {
            Dock      = DockStyle.Top,
            BackColor = Color.Transparent,
            AutoSize  = true
        };

        // Build habit rows
        var barColors = new[] { WARM1, COOL1, GREEN1, WARM2, Color.FromArgb(150, 80, 180) };
        int habitY = 0;
        if (!habits.Any())
        {
            inner.Controls.Add(new Label
            {
                Text      = "No hay hábitos todavía.",
                ForeColor = INK2,
                AutoSize  = true,
                Location  = new Point(0, 0),
                BackColor = Color.Transparent
            });
            habitY = 30;
        }
        else
        {
            int ci = 0;
            foreach (var h in habits)
            {
                var rowColor = barColors[ci++ % barColors.Length];
                var row = BuildHabitRow(h.Emoji + "  " + h.Name,
                                        h.CompletionRateThisMonth,
                                        h.CurrentStreak,
                                        rowColor);
                row.Top  = habitY;
                row.Left = 0;
                row.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                inner.Controls.Add(row);
                habitY += row.Height + 10;
            }
        }
        inner.Height = habitY + 8;

        // Wire resize so habit rows stretch
        inner.Resize += (s, e) =>
        {
            foreach (Control c in inner.Controls)
                if (c is Panel row && row.Tag?.ToString() == "habitrow")
                    row.Width = inner.ClientSize.Width;
        };

        // ── Outer scroll layout (top-down stacking) ───────────────
        // We use a TableLayoutPanel inside the scroll panel so items stack cleanly
        var stack = new TableLayoutPanel
        {
            Dock        = DockStyle.Top,
            ColumnCount = 1,
            RowCount    = 4,
            BackColor   = Color.Transparent,
            AutoSize    = true,
            AutoSizeMode= AutoSizeMode.GrowAndShrink
        };
        stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));   // summary cards
        stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 246));   // chart
        stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));    // section label
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // habit rows

        stack.Controls.Add(pnlSummary,    0, 0);
        stack.Controls.Add(pnlChartCard,  0, 1);
        stack.Controls.Add(lblHabitsTitle,0, 2);
        stack.Controls.Add(inner,         0, 3);

        scroll.Controls.Add(stack);

        this.Controls.Add(scroll);
        this.Controls.Add(pnlHeader);
    }

    private Panel BuildHabitRow(string name, double pct, int streak, Color accent)
    {
        var row = new Panel
        {
            Height    = 68,
            BackColor = CARD,
            Tag       = "habitrow",
            Padding   = new Padding(16, 0, 16, 0)
        };

        var lblName = new Label
        {
            Text      = name,
            ForeColor = INK,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            Location  = new Point(16, 10),
            AutoSize  = true,
            BackColor = Color.Transparent
        };

        var lblPct = new Label
        {
            Text      = $"{pct:F0}%",
            ForeColor = accent,
            Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
            Size      = new Size(60, 24),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent
        };

        var lblStreak = new Label
        {
            Text      = $"🔥 {streak}d",
            ForeColor = INK2,
            Font      = new Font("Segoe UI", 9f),
            AutoSize  = true,
            BackColor = Color.Transparent
        };

        // Progress track — anchored so it stretches
        var track = new Panel
        {
            Height    = 10,
            BackColor = Color.FromArgb(225, 222, 216),
            Top       = 46,
            Left      = 16,
            Anchor    = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };
        var fill = new Panel
        {
            Location  = new Point(0, 0),
            Height    = 10,
            BackColor = accent
        };
        track.Controls.Add(fill);

        // Position pct + streak labels on resize
        row.Resize += (s, e) =>
        {
            int w = row.ClientSize.Width;
            track.Width    = w - 32;
            lblPct.Location   = new Point(w - 80, 8);
            lblStreak.Location= new Point(w - 80, 34);
            int fillW = (int)(track.Width * Math.Min(pct, 100) / 100.0);
            fill.Width = fillW;
        };

        row.Controls.AddRange(new Control[] { lblName, lblPct, lblStreak, track });
        return row;
    }

    private void DrawBarChart(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var stats  = _service.GetWeeklyStats();
        int maxVal = stats.Values.Any() ? Math.Max(stats.Values.Max(), 1) : 1;

        int pad    = 36;
        int bottom = pnlChart.Height - 40;
        int top    = 12;
        int chartH = bottom - top;
        int totalW = pnlChart.Width - pad * 2;
        int slotW  = totalW / 7;
        int barW   = Math.Max(20, slotW - 18);

        var dayColors = new[] { WARM1, WARM2, COOL1, GREEN1, WARM1, WARM2, COOL1 };

        int i = 0;
        foreach (var kvp in stats)
        {
            int x     = pad + i * slotW + (slotW - barW) / 2;
            int barH  = chartH > 0 ? (int)((double)kvp.Value / maxVal * chartH) : 0;

            // Track
            using (var tb = new SolidBrush(Color.FromArgb(230, 228, 224)))
                g.FillRectangle(tb, new Rectangle(x, top, barW, chartH));

            if (barH > 0)
            {
                using (var bb = new SolidBrush(dayColors[i % dayColors.Length]))
                    g.FillRectangle(bb, new Rectangle(x, bottom - barH, barW, barH));

                g.DrawString(kvp.Value.ToString(),
                    new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    new SolidBrush(INK),
                    x + barW / 2 - 5, bottom - barH - 18);
            }

            // Day label
            g.DrawString(kvp.Key,
                new Font("Segoe UI", 9f),
                new SolidBrush(INK2),
                x + barW / 2 - 9, bottom + 8);

            i++;
        }

        // Baseline
        using var pen = new Pen(Color.FromArgb(200, 195, 188), 1);
        g.DrawLine(pen, pad, bottom, pnlChart.Width - pad, bottom);
    }
}
