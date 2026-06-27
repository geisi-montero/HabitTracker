namespace HabitTracker.Models;

public class Habit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Emoji { get; set; } = "⭐";
    public string Color { get; set; } = "#6C63FF";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public int TargetDaysPerWeek { get; set; } = 7;

    public virtual ICollection<HabitLog> Logs { get; set; } = new List<HabitLog>();

    public int CurrentStreak
    {
        get
        {
            var sortedLogs = Logs
                .Where(l => l.Completed)
                .Select(l => l.Date.Date)
                .OrderByDescending(d => d)
                .ToList();

            if (!sortedLogs.Any()) return 0;

            var streak = 0;
            var today = DateTime.Today;
            var checkDate = sortedLogs.Contains(today) ? today : today.AddDays(-1);

            foreach (var date in sortedLogs)
            {
                if (date == checkDate)
                {
                    streak++;
                    checkDate = checkDate.AddDays(-1);
                }
                else if (date < checkDate)
                    break;
            }
            return streak;
        }
    }

    public bool IsCompletedToday => Logs.Any(l => l.Date.Date == DateTime.Today && l.Completed);

    public double CompletionRateThisMonth
    {
        get
        {
            var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            var daysPassed = DateTime.Now.Day;
            var completed = Logs.Count(l =>
                l.Completed &&
                l.Date.Month == DateTime.Now.Month &&
                l.Date.Year == DateTime.Now.Year);
            return daysPassed == 0 ? 0 : Math.Round((double)completed / daysPassed * 100, 1);
        }
    }
}
