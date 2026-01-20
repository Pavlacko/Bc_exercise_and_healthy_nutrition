using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Filters;
using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    [RequireAdmin]
    public class FoodController : Controller
    {
        private readonly AppDbContext _context;

        public FoodController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var foods = _context.FoodItems
                .OrderBy(f => f.Name)
                .ToList();

            return View(foods);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FoodItem model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.FoodItems.Add(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = _context.FoodItems.Find(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, FoodItem model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var item = _context.FoodItems.Find(id);
            if (item == null)
                return NotFound();

            item.Name = model.Name;
            item.KcalPer100g = model.KcalPer100g;
            item.ProteinPer100g = model.ProteinPer100g;
            item.CarbsPer100g = model.CarbsPer100g;
            item.FatPer100g = model.FatPer100g;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var item = _context.FoodItems.Find(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.FoodItems.Find(id);
            if (item == null)
                return NotFound();

            _context.FoodItems.Remove(item);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}