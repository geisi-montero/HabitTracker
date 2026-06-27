using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.Forms;

public class AddEditHabitForm : Form
{
    private readonly HabitService _service;
    private readonly Habit?       _existing;

    private TextBox        txtName   = null!;
    private TextBox        txtDesc   = null!;
    private ComboBox       cboEmoji  = null!;
    private Button         btnColor  = null!;
    private NumericUpDown  nudDays   = null!;
    private string         _color    = "#DC6440";

    private static readonly Color BG    = Color.FromArgb(245, 244, 240);
    private static readonly Color CARD  = Color.White;
    private static readonly Color INK   = Color.FromArgb(30, 28, 25);
    private static readonly Color INK2  = Color.FromArgb(120, 115, 105);
    private static readonly Color TERRA = Color.FromArgb(220, 100, 60);

    public AddEditHabitForm(HabitService service, Habit? habit)
    {
        _service  = service;
        _existing = habit;
        InitializeComponent();
        if (habit != null) Populate(habit);
    }

    private void InitializeComponent()
    {
        this.Text            = _existing == null ? "Nuevo hábito" : "Editar hábito";
        this.Size            = new Size(460, 510);
        this.BackColor       = BG;
        this.ForeColor       = INK;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox     = false;
        this.StartPosition   = FormStartPosition.CenterParent;
        this.Font            = new Font("Segoe UI", 10f);

        // Header
        var pnlHead = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 52,
            BackColor = TERRA
        };
        pnlHead.Controls.Add(new Label
        {
            Text      = this.Text.ToUpper(),
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(20, 0, 0, 0)
        });

        // Body
        var pnl = new Panel
        {
            Dock    = DockStyle.Fill,
            Padding = new Padding(28, 20, 28, 20),
            BackColor = BG
        };

        int y = 20;

        AddLabel(pnl, "Nombre *", y); y += 22;
        txtName = AddTextBox(pnl, y, 380); y += 44;

        AddLabel(pnl, "Descripción", y); y += 22;
        txtDesc = AddTextBox(pnl, y, 380);
        txtDesc.Height    = 58;
        txtDesc.Multiline = true;
        y += 72;

        AddLabel(pnl, "Emoji", y); y += 22;
        cboEmoji = new ComboBox
        {
            Location         = new Point(0, y),
            Width            = 200,
            BackColor        = CARD,
            ForeColor        = INK,
            FlatStyle        = FlatStyle.Flat,
            Font             = new Font("Segoe UI Emoji", 14f),
            DropDownStyle    = ComboBoxStyle.DropDownList
        };
        var emojis = new[] { "⭐","💪","📚","🧘","💧","🏃","🎯","🍎","🌞","🎨","🎵","💤","🧠","✍️","🙏","🌿","🎮","💊","🐕","📝" };
        cboEmoji.Items.AddRange(emojis);
        cboEmoji.SelectedIndex = 0;
        pnl.Controls.Add(cboEmoji);
        y += 44;

        AddLabel(pnl, "Color", y); y += 22;
        btnColor = new Button
        {
            Location  = new Point(0, y),
            Width     = 110,
            Height    = 34,
            BackColor = ColorTranslator.FromHtml(_color),
            ForeColor = Color.White,
            Text      = "Elegir color",
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        btnColor.FlatAppearance.BorderSize = 0;
        btnColor.Click += (s, e) =>
        {
            using var dlg = new ColorDialog { Color = ColorTranslator.FromHtml(_color) };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _color = ColorTranslator.ToHtml(dlg.Color);
                btnColor.BackColor = dlg.Color;
            }
        };
        pnl.Controls.Add(btnColor);
        y += 48;

        AddLabel(pnl, "Días objetivo / semana", y); y += 22;
        nudDays = new NumericUpDown
        {
            Location  = new Point(0, y),
            Width     = 70,
            Minimum   = 1,
            Maximum   = 7,
            Value     = 7,
            BackColor = CARD,
            ForeColor = INK,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold)
        };
        pnl.Controls.Add(nudDays);
        y += 56;

        // Buttons
        var btnSave = new Button
        {
            Text      = _existing == null ? "Crear hábito" : "Guardar cambios",
            Location  = new Point(0, y),
            Width     = 180,
            Height    = 40,
            BackColor = TERRA,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += Save;
        pnl.Controls.Add(btnSave);

        if (_existing != null)
        {
            var btnDel = new Button
            {
                Text      = "Eliminar",
                Location  = new Point(190, y),
                Width     = 110,
                Height    = 40,
                BackColor = Color.FromArgb(190, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (s, e) =>
            {
                if (MessageBox.Show("¿Eliminar este hábito?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _service.DeleteHabit(_existing.Id);
                    this.Close();
                }
            };
            pnl.Controls.Add(btnDel);
        }

        this.Controls.Add(pnl);
        this.Controls.Add(pnlHead);
    }

    private static void AddLabel(Panel p, string text, int y) =>
        p.Controls.Add(new Label
        {
            Text      = text,
            ForeColor = Color.FromArgb(120, 115, 105),
            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Location  = new Point(0, y),
            AutoSize  = true
        });

    private static TextBox AddTextBox(Panel p, int y, int w)
    {
        var t = new TextBox
        {
            Location    = new Point(0, y),
            Width       = w,
            BackColor   = Color.White,
            ForeColor   = Color.FromArgb(30, 28, 25),
            BorderStyle = BorderStyle.FixedSingle,
            Font        = new Font("Segoe UI", 10.5f)
        };
        p.Controls.Add(t);
        return t;
    }

    private void Populate(Habit h)
    {
        txtName.Text  = h.Name;
        txtDesc.Text  = h.Description;
        _color        = h.Color;
        try { btnColor.BackColor = ColorTranslator.FromHtml(h.Color); } catch { }
        nudDays.Value = h.TargetDaysPerWeek;
        var emojiList = new[] { "⭐","💪","📚","🧘","💧","🏃","🎯","🍎","🌞","🎨","🎵","💤","🧠","✍️","🙏","🌿","🎮","💊","🐕","📝" };
        int idx = Array.IndexOf(emojiList, h.Emoji);
        if (idx >= 0) cboEmoji.SelectedIndex = idx;
    }

    private void Save(object? s, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("El nombre es obligatorio.", "Campo requerido",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_existing == null)
        {
            _service.AddHabit(new Habit
            {
                Name               = txtName.Text.Trim(),
                Description        = txtDesc.Text.Trim(),
                Emoji              = cboEmoji.SelectedItem?.ToString() ?? "⭐",
                Color              = _color,
                TargetDaysPerWeek  = (int)nudDays.Value
            });
        }
        else
        {
            _existing.Name              = txtName.Text.Trim();
            _existing.Description       = txtDesc.Text.Trim();
            _existing.Emoji             = cboEmoji.SelectedItem?.ToString() ?? "⭐";
            _existing.Color             = _color;
            _existing.TargetDaysPerWeek = (int)nudDays.Value;
            _service.UpdateHabit(_existing);
        }
        this.Close();
    }
}
