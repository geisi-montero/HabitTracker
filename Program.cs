using HabitTracker.Data;
using HabitTracker.Forms;
using HabitTracker.Services;

namespace HabitTracker;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ApplicationConfiguration.Initialize();

        try
        {
            using var context = new HabitContext();
            DatabaseInitializer.Initialize(context);
            var service = new HabitService(context);

            Application.Run(new DashboardForm(service));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al iniciar la aplicación:\n\n{ex.Message}",
                "Error de inicio",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
