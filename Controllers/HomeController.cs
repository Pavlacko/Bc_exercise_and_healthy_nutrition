using Bc_exercise_and_healthy_nutrition.Filters;
using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    [RequireLogin]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            
            if (HttpContext.Session.GetString("LoggedIn") != "true")
            {
                return RedirectToAction("Index", "Welcome");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}