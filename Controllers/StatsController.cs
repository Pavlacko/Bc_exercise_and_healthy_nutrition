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
                p += mul * e.FoodItem.ProteinPer100g;
                c += mul * e.FoodItem.CarbsPer100g;
                f += mul * e.FoodItem.FatPer100g;
            }

            var goal = _context.DailyGoals
                .Where(g => g.AppUserId == userId.Value && g.Date <= d)
                .OrderByDescending(g => g.Date)
                .FirstOrDefault();

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
        public IActionResult CaloriesChart(DateTime? date, int days = 7)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            if (days != 7 && days != 30 && days != 365)
                days = 7;

            var endDate = (date ?? DateTime.Today).Date;
            var fromDate = endDate.AddDays(-(days - 1)).Date;

            var entries = _context.MealEntries
                .Include(e => e.FoodItem)
                .Where(e => e.AppUserId == userId.Value && e.Date >= fromDate && e.Date <= endDate)
                .ToList();

            var map = new Dictionary<DateTime, double>();
            for (var d = fromDate; d <= endDate; d = d.AddDays(1))
                map[d] = 0;

            foreach (var e in entries)
            {
                var mul = e.Grams / 100.0;
                var day = e.Date.Date;
                if (map.ContainsKey(day))
                {
                    map[day] += mul * e.FoodItem!.KcalPer100g;
                }
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