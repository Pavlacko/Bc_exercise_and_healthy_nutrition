using Microsoft.AspNetCore.Mvc;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    public class EducationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Calories()
        {
            return View();
        }

        public IActionResult Macros()
        {
            return View();
        }

        public IActionResult Tips()
        {
            return View();
        }
    }
}