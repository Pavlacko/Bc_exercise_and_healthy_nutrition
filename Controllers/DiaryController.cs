using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Filters;
using Bc_exercise_and_healthy_nutrition.Models;
using Bc_exercise_and_healthy_nutrition.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    [RequireLogin]
    public class DiaryController : Controller
    {
        private readonly AppDbContext _context;

        public DiaryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(DateTime? date)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "User");

            //ak nepride parameter, tak berie dnesok .Date - bez casu
            var d = (date ?? DateTime.Today).Date;

            var vm = new DiaryIndexViewModel
            {
                Date = d,
                Foods = _context.FoodItems
                    .OrderBy(f => f.Name)
                    .Select(f => new SelectListItem
                    {
                        Value = f.Id.ToString(),
                        Text = $"{f.Name} ({f.KcalPer100g} kcal/100g)"
                    })
                    .ToList(),

                Entries = _context.MealEntries
                    .Include(e => e.FoodItem)
                    .Where(e => e.AppUserId == userId.Value && e.Date == d)
                    .OrderByDescending(e => e.Id)
                    .ToList(),

                Goal = _context.DailyGoals.FirstOrDefault(g => g.AppUserId == userId.Value && g.Date == d)
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult AddEntry([FromBody] AddEntryDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            if (dto.Grams <= 0 || dto.Grams > 5000) return BadRequest("Neplatná gramáž.");

            var food = _context.FoodItems.Find(dto.FoodItemId);
            if (food == null) return BadRequest("Jedlo neexistuje.");

            var entry = new MealEntry
            {
                AppUserId = userId.Value,
                FoodItemId = dto.FoodItemId,
                Grams = dto.Grams,
                Date = dto.Date.Date
            };

            _context.MealEntries.Add(entry);
            _context.SaveChanges();

            var created = _context.MealEntries.Include(e => e.FoodItem).First(e => e.Id == entry.Id);
            var mul = created.Grams / 100.0;

            return Json(new
            {
                id = created.Id,
                foodName = created.FoodItem!.Name,
                grams = created.Grams,
                kcal = Math.Round(mul * created.FoodItem!.KcalPer100g, 1),
                protein = Math.Round(mul * created.FoodItem!.ProteinPer100g, 1),
                carbs = Math.Round(mul * created.FoodItem!.CarbsPer100g, 1),
                fat = Math.Round(mul * created.FoodItem!.FatPer100g, 1)
            });
        }


        [HttpPost]
        public IActionResult UpdateEntry([FromBody] UpdateEntryDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            if (dto.Grams <= 0 || dto.Grams > 5000)
                return BadRequest("Neplatná gramáž.");

            var entry = _context.MealEntries
                .Include(e => e.FoodItem)
                .FirstOrDefault(e => e.Id == dto.Id);

            if (entry == null) return NotFound();
            if (entry.AppUserId != userId.Value) return Forbid();

            entry.Grams = dto.Grams;
            _context.SaveChanges();

            var mul = entry.Grams / 100.0;

            return Json(new
            {
                id = entry.Id,
                grams = entry.Grams,
                kcal = Math.Round(mul * entry.FoodItem!.KcalPer100g, 1),
                protein = Math.Round(mul * entry.FoodItem!.ProteinPer100g, 1),
                carbs = Math.Round(mul * entry.FoodItem!.CarbsPer100g, 1),
                fat = Math.Round(mul * entry.FoodItem!.FatPer100g, 1)
            });
        }

        [HttpPost]
        public IActionResult DeleteEntry([FromBody] DeleteEntryDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var entry = _context.MealEntries.Find(dto.Id);
            if (entry == null) return NotFound();
            if (entry.AppUserId != userId.Value) return Forbid();

            _context.MealEntries.Remove(entry);
            _context.SaveChanges();

            return Json(new { ok = true });
        }

        [HttpPost]
        public IActionResult SaveGoal([FromBody] SaveGoalDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var d = dto.Date.Date;

            if (dto.KcalGoal < 0 || dto.KcalGoal > 15000) return BadRequest("Neplatný cieľ kcal.");
            if (dto.ProteinGoal < 0 || dto.ProteinGoal > 500) return BadRequest("Neplatný cieľ proteínu.");
            if (dto.CarbsGoal < 0 || dto.CarbsGoal > 1000) return BadRequest("Neplatný cieľ sacharidov.");
            if (dto.FatGoal < 0 || dto.FatGoal > 500) return BadRequest("Neplatný cieľ tukov.");

            var goal = _context.DailyGoals
                .FirstOrDefault(g => g.AppUserId == userId.Value && g.Date == d);

            if (goal == null)
            {
                goal = new DailyGoal
                {
                    AppUserId = userId.Value,
                    Date = d
                };
                _context.DailyGoals.Add(goal);
            }

            goal.KcalGoal = dto.KcalGoal;
            goal.ProteinGoal = dto.ProteinGoal;
            goal.CarbsGoal = dto.CarbsGoal;
            goal.FatGoal = dto.FatGoal;

            _context.SaveChanges();

            return Json(new { ok = true });
        }

        [HttpGet]
        public IActionResult Summary(DateTime date)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var d = date.Date;

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

            return Json(new
            {
                count = entries.Count,
                kcal = Math.Round(kcal, 1),
                protein = Math.Round(p, 1),
                carbs = Math.Round(c, 1),
                fat = Math.Round(f, 1)
            });
        }

        public class AddEntryDto
        {
            public int FoodItemId { get; set; }
            public double Grams { get; set; }
            public DateTime Date { get; set; }
        }

        public class DeleteEntryDto
        {
            public int Id { get; set; }
        }

        public class UpdateEntryDto
        {
            public int Id { get; set; }
            public double Grams { get; set; }
        }

        public class SaveGoalDto
        {
            public DateTime Date { get; set; }
            public double KcalGoal { get; set; }
            public double ProteinGoal { get; set; }
            public double CarbsGoal { get; set; }
            public double FatGoal { get; set; }
        }
    }
}
