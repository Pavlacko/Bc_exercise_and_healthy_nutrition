using Microsoft.AspNetCore.Mvc;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    public class StatsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}