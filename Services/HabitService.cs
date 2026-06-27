using Microsoft.EntityFrameworkCore;
using HabitTracker.Data;
using HabitTracker.Models;

namespace HabitTracker.Services;

public class HabitService
{
    private readonly HabitContext _context;

    public HabitService(HabitContext context)
    {
        _context = context;
    }

    public List<Habit> GetAllHabits() =>
        _context.Habits
                .Include(h => h.Logs)
                .Where(h => h.IsActive)
                .OrderBy(h => h.Name)
                .ToList();

    public Habit? GetHabitById(int id) =>
        _context.Habits
                .Include(h => h.Logs)
                .FirstOrDefault(h => h.Id == id);

    public void AddHabit(Habit habit)
    {
        _context.Habits.Add(habit);
        _context.SaveChanges();
    }

    public void UpdateHabit(Habit habit)
    {
        _context.Habits.Update(habit);
        _context.SaveChanges();
    }

    public void DeleteHabit(int id)
    {
        var habit = _context.Habits.Find(id);
        if (habit != null)
        {
            habit.IsActive = false;
            _context.SaveChanges();
        }
    }

    public void ToggleToday(int habitId)
    {
        var existing = _context.HabitLogs
            .FirstOrDefault(l => l.HabitId == habitId && l.Date.Date == DateTime.Today);

        if (existing != null)
        {
            existing.Completed = !existing.Completed;
        }
        else
        {
            _context.HabitLogs.Add(new HabitLog
            {
                HabitId = habitId,
                Date = DateTime.Today,
                Completed = true
            });
        }
        _context.SaveChanges();
    }

    public List<HabitLog> GetLogsForMonth(int habitId, int year, int month) =>
        _context.HabitLogs
                .Where(l => l.HabitId == habitId && l.Date.Year == year && l.Date.Month == month)
                .ToList();

    public Dictionary<string, int> GetWeeklyStats()
    {
        var result = new Dictionary<string, int>();
        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

        for (int i = 0; i < 7; i++)
        {
            var date = startOfWeek.AddDays(i);
            var count = _context.HabitLogs
                .Count(l => l.Date.Date == date && l.Completed);
            result[date.ToString("ddd")] = count;
        }
        return result;
    }

    public int GetTotalCompletionsToday() =>
        _context.HabitLogs.Count(l => l.Date.Date == DateTime.Today && l.Completed);

    public int GetLongestStreakEver()
    {
        var allHabits = GetAllHabits();
        return allHabits.Any() ? allHabits.Max(h => h.CurrentStreak) : 0;
    }
}
