using HabitTracker.Data;
using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Services;

public static class DatabaseInitializer
{
    public static void Initialize(HabitContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Habits.Any())
        {
            var habits = new List<Habit>
            {
                new() { Name = "Ejercicio", Description = "30 minutos de actividad física", Emoji = "💪", Color = "#FF6B6B" },
                new() { Name = "Leer", Description = "Leer al menos 20 páginas", Emoji = "📚", Color = "#4ECDC4" },
                new() { Name = "Meditar", Description = "10 minutos de meditación", Emoji = "🧘", Color = "#45B7D1" },
                new() { Name = "Agua", Description = "Tomar 8 vasos de agua", Emoji = "💧", Color = "#96CEB4" },
            };
            context.Habits.AddRange(habits);
            context.SaveChanges();
        }
    }
}
