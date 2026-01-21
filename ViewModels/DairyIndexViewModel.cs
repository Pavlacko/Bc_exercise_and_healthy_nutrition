using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bc_exercise_and_healthy_nutrition.ViewModels
{
    public class DiaryIndexViewModel
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public List<SelectListItem> Foods { get; set; } = new();
        public List<MealEntry> Entries { get; set; } = new();

        public DailyGoal? Goal { get; set; }
    }
}