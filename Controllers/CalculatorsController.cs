using Microsoft.AspNetCore.Mvc;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    public class CalculatorsController : Controller
    {
        public IActionResult BMI()
        {
            return View();
        }

        public IActionResult BMR()
        {
            return View();
        }

        public IActionResult Calories()
        {
            return View();
        }
    }
}