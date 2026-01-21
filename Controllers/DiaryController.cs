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
                    .ToList()
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

            var food = _context.FoodItems.Find(dto.FoodItemId);
            if (food == null) return BadRequest("Jedlo neexistuje.");

            entry.FoodItemId = dto.FoodItemId;
            entry.Grams = dto.Grams;
            entry.Date = dto.Date.Date;

            _context.SaveChanges();

            var mul = entry.Grams / 100.0;

            return Json(new
            {
                id = entry.Id,
                foodName = food.Name,
                grams = entry.Grams,
                kcal = Math.Round(mul * food.KcalPer100g, 1),
                protein = Math.Round(mul * food.ProteinPer100g, 1),
                carbs = Math.Round(mul * food.CarbsPer100g, 1),
                fat = Math.Round(mul * food.FatPer100g, 1)
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
            public int FoodItemId { get; set; }
            public double Grams { get; set; }
            public DateTime Date { get; set; }
        }
    }
}