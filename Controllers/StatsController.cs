using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    [RequireLogin]
    public class StatsController : Controller
    {
        private readonly AppDbContext _context;

        public StatsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult TodaySummary(DateTime? date)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var d = (date ?? DateTime.Today).Date;

            var entries = _context.MealEntries
                .Include(e => e.FoodItem)
                .Where(e => e.AppUserId == userId.Value && e.Date == d)
                .ToList();

            double kcal = 0, p = 0, c = 0, f = 0;

            foreach (var e in entries)
            {
                var mul = e.Grams / 100.0;
                kcal += mul * e.FoodItem!.KcalPer100g;
                p += mul * e.FoodItem!.ProteinPer100g;
                c += mul * e.FoodItem!.CarbsPer100g;
                f += mul * e.FoodItem!.FatPer100g;
            }

            var goal = _context.DailyGoals
                .FirstOrDefault(g => g.AppUserId == userId.Value && g.Date == d);

            return Json(new
            {
                date = d.ToString("yyyy-MM-dd"),
                count = entries.Count,
                kcal = Math.Round(kcal, 1),
                protein = Math.Round(p, 1),
                carbs = Math.Round(c, 1),
                fat = Math.Round(f, 1),

                goal = goal == null ? null : new
                {
                    kcalGoal = goal.KcalGoal,
                    proteinGoal = goal.ProteinGoal,
                    carbsGoal = goal.CarbsGoal,
                    fatGoal = goal.FatGoal
                }
            });
        }

        [HttpGet]
        public IActionResult WeeklyCalories(int days = 7)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            if (days < 3) days = 3;
            if (days > 31) days = 31;

            var from = DateTime.Today.AddDays(-(days - 1)).Date;
            var to = DateTime.Today.Date;

            var entries = _context.MealEntries
                .Include(e => e.FoodItem)
                .Where(e => e.AppUserId == userId.Value && e.Date >= from && e.Date <= to)
                .ToList();

            var map = new Dictionary<DateTime, double>();
            for (var d = from; d <= to; d = d.AddDays(1))
                map[d] = 0;

            foreach (var e in entries)
            {
                var mul = e.Grams / 100.0;
                map[e.Date.Date] += mul * e.FoodItem!.KcalPer100g;
            }

            var result = map
                .OrderBy(kv => kv.Key)
                .Select(kv => new
                {
                    date = kv.Key.ToString("yyyy-MM-dd"),
                    kcal = Math.Round(kv.Value, 1)
                })
                .ToList();

            return Json(result);
        }
    }
}