using Microsoft.AspNetCore.Mvc;
using Bc_exercise_and_healthy_nutrition.Models;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    public class UserController : Controller
    {
        // GET: /User/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // GET: /User/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
              
                return View(model);
            }

            // Zatiaľ iba redirect........sem pride logika s DB
            TempData["Success"] = "Registrácia prebehla úspešne!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {

                return View(model);
            }

            // Zatiaľ iba redirect........sem pride logika s DB
            TempData["Success"] = "Prihlásili ste sa úspešne!";
            return RedirectToAction("Index", "Home");
        }


    }
}

