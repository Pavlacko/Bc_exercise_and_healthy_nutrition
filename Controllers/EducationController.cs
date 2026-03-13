using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    [RequireLogin]
    public class EducationController : Controller
    {
        private readonly AppDbContext _context;

        public EducationController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Exercises()
        {
            var exercises = await _context.Exercises
                .OrderBy(e => e.MuscleGroup)
                .ThenBy(e => e.Name)
                .ToListAsync();

            return View(exercises);
        }
    }
}