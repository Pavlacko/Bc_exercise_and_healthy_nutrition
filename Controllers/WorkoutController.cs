using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Filters;
using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    [RequireLogin]
    public class WorkoutController : Controller
    {
        private readonly AppDbContext _context;

        public WorkoutController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(DateTime? date)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "User");

            var d = (date ?? DateTime.Today).Date;

            var workout = _context.Workouts
                .Include(w => w.Exercises)
                .FirstOrDefault(w => w.AppUserId == userId.Value && w.Date == d);

            ViewBag.SelectedDate = d;
            ViewBag.Exercises = _context.Exercises
                .OrderBy(e => e.MuscleGroup)
                .ThenBy(e => e.Name)
                .Select(e => new SelectListItem
                {
                    Value = e.Name,
                    Text = $"{e.Name} ({e.MuscleGroup})"
                })
                .ToList();

            return View(workout);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddExercise(DateTime date, string exerciseName, int sets, int reps, double weight, string? note)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "User");

            var d = date.Date;

            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                TempData["Err"] = "Vyber cvik.";
                return RedirectToAction(nameof(Index), new { date = d.ToString("yyyy-MM-dd") });
            }

            if (sets <= 0 || reps <= 0 || weight < 0)
            {
                TempData["Err"] = "Zadaj platné hodnoty.";
                return RedirectToAction(nameof(Index), new { date = d.ToString("yyyy-MM-dd") });
            }

            var workout = _context.Workouts
                .Include(w => w.Exercises)
                .FirstOrDefault(w => w.AppUserId == userId.Value && w.Date == d);

            if (workout == null)
            {
                workout = new Workout
                {
                    AppUserId = userId.Value,
                    Date = d,
                    Note = note
                };

                _context.Workouts.Add(workout);
                _context.SaveChanges();
            }
            else
            {
                workout.Note = note;
            }

            var item = new WorkoutExercise
            {
                WorkoutId = workout.Id,
                ExerciseName = exerciseName.Trim(),
                Sets = sets,
                Reps = reps,
                Weight = weight
            };

            _context.WorkoutExercises.Add(item);
            _context.SaveChanges();

            TempData["Ok"] = "Cvik bol pridaný do tréningu.";
            return RedirectToAction(nameof(Index), new { date = d.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteExercise(int id, DateTime date)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "User");

            var item = _context.WorkoutExercises
                .Include(x => x.Workout)
                .FirstOrDefault(x => x.Id == id);

            if (item == null) return NotFound();
            if (item.Workout == null || item.Workout.AppUserId != userId.Value) return Forbid();

            _context.WorkoutExercises.Remove(item);
            _context.SaveChanges();

            TempData["Ok"] = "Cvik bol zmazaný.";
            return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd") });
        }
    }
}