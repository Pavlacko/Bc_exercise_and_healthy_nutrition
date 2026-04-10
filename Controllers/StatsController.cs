using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Filters;
using Bc_exercise_and_healthy_nutrition.Models;
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
        public IActionResult TodaySummary(DateTime? date, int? userId)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int targetUserId = userId ?? currentUserId;

            var targetUser = _context.Users.FirstOrDefault(u => u.Id == targetUserId);
            if (targetUser == null)
                return NotFound();

            if (!CanViewProfile(currentUserId, targetUser))
                return Unauthorized();

            var d = (date ?? DateTime.Today).Date;

            var entries = _context.MealEntries
                .Include(e => e.FoodItem)
                .Where(e => e.AppUserId == targetUserId && e.Date == d)
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
                .Where(g => g.AppUserId == targetUserId && g.Date <= d)
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
        public IActionResult CaloriesChart(DateTime? date, int days = 7, int? userId = null)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int targetUserId = userId ?? currentUserId;

            var targetUser = _context.Users.FirstOrDefault(u => u.Id == targetUserId);
            if (targetUser == null)
                return NotFound();

            if (!CanViewProfile(currentUserId, targetUser))
                return Unauthorized();

            if (days != 7 && days != 30 && days != 365)
                days = 7;

            var endDate = (date ?? DateTime.Today).Date;
            var fromDate = endDate.AddDays(-(days - 1)).Date;

            var entries = _context.MealEntries
                .Include(e => e.FoodItem)
                .Where(e => e.AppUserId == targetUserId && e.Date >= fromDate && e.Date <= endDate)
                .ToList();

            var map = new Dictionary<DateTime, double>();
            for (var d = fromDate; d <= endDate; d = d.AddDays(1))
                map[d] = 0;

            foreach (var e in entries)
            {
                var mul = e.Grams / 100.0;
                var day = e.Date.Date;

                if (map.ContainsKey(day))
                    map[day] += mul * e.FoodItem!.KcalPer100g;
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

        [HttpGet]
        public IActionResult WorkoutSummary(DateTime? date, int? userId)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int targetUserId = userId ?? currentUserId;

            var targetUser = _context.Users.FirstOrDefault(u => u.Id == targetUserId);
            if (targetUser == null)
                return NotFound();

            if (!CanViewProfile(currentUserId, targetUser))
                return Unauthorized();

            var d = (date ?? DateTime.Today).Date;

            var workout = _context.Workouts
                .Include(w => w.Exercises)
                .FirstOrDefault(w => w.AppUserId == targetUserId && w.Date == d);

            if (workout == null)
            {
                return Json(new
                {
                    date = d.ToString("yyyy-MM-dd"),
                    hasWorkout = false,
                    exerciseCount = 0,
                    totalSets = 0,
                    totalReps = 0,
                    totalVolume = 0,
                    note = ""
                });
            }

            var exerciseCount = workout.Exercises.Count;
            var totalSets = workout.Exercises.Sum(x => x.Sets);
            var totalReps = workout.Exercises.Sum(x => x.Sets * x.Reps);
            var totalVolume = workout.Exercises.Sum(x => x.Sets * x.Reps * x.Weight);

            return Json(new
            {
                date = d.ToString("yyyy-MM-dd"),
                hasWorkout = true,
                exerciseCount,
                totalSets,
                totalReps,
                totalVolume = Math.Round(totalVolume, 1),
                note = workout.Note ?? ""
            });
        }

        [HttpGet]
        public IActionResult WorkoutChart(DateTime? date, int days = 7, int? userId = null)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int targetUserId = userId ?? currentUserId;

            var targetUser = _context.Users.FirstOrDefault(u => u.Id == targetUserId);
            if (targetUser == null)
                return NotFound();

            if (!CanViewProfile(currentUserId, targetUser))
                return Unauthorized();

            if (days != 7 && days != 30 && days != 365)
                days = 7;

            var endDate = (date ?? DateTime.Today).Date;
            var fromDate = endDate.AddDays(-(days - 1)).Date;

            var workouts = _context.Workouts
                .Include(w => w.Exercises)
                .Where(w => w.AppUserId == targetUserId && w.Date >= fromDate && w.Date <= endDate)
                .ToList();

            var map = new Dictionary<DateTime, double>();
            for (var d = fromDate; d <= endDate; d = d.AddDays(1))
                map[d] = 0;

            foreach (var workout in workouts)
            {
                var volume = workout.Exercises.Sum(x => x.Sets * x.Reps * x.Weight);
                var day = workout.Date.Date;

                if (map.ContainsKey(day))
                    map[day] += volume;
            }

            var result = map
                .OrderBy(kv => kv.Key)
                .Select(kv => new
                {
                    date = kv.Key.ToString("yyyy-MM-dd"),
                    volume = Math.Round(kv.Value, 1)
                })
                .ToList();

            return Json(result);
        }

        [HttpGet]
        public IActionResult User(int id)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var targetUser = _context.Users.FirstOrDefault(u => u.Id == id);
            if (targetUser == null)
                return NotFound();

            if (!CanViewProfile(currentUserId, targetUser))
                return View("PrivateProfile");

            ViewBag.TargetUserId = id;
            ViewBag.TargetUserName = targetUser.Meno;
            ViewBag.IsForeignProfile = currentUserId != id;

            return View("Index");
        }

        private bool CanViewProfile(int currentUserId, AppUser targetUser)
        {
            if (targetUser.ProfileVisibility == ProfileVisibility.Public)
                return true;

            if (targetUser.ProfileVisibility == ProfileVisibility.Private)
            {
                bool isFriend = _context.FriendRequests.Any(fr =>
                    ((fr.SenderId == currentUserId && fr.ReceiverId == targetUser.Id) ||
                     (fr.SenderId == targetUser.Id && fr.ReceiverId == currentUserId))
                    && fr.Status == "Accepted");

                return isFriend || currentUserId == targetUser.Id;
            }

            return false;
        }
    }
}