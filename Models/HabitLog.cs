namespace HabitTracker.Models;

public class HabitLog
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public bool Completed { get; set; } = true;
    public string? Note { get; set; }
    public int MoodRating { get; set; } = 0; // 0-5

    public virtual Habit? Habit { get; set; }
}
