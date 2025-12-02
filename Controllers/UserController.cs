using Microsoft.AspNetCore.Mvc;
using Bc_exercise_and_healthy_nutrition.Models;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            HttpContext.Session.SetString("LoggedIn", "true");

            return RedirectToAction("Index", "Home");
        }



        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            HttpContext.Session.SetString("LoggedIn", "true");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Welcome");
        }




    }
}

