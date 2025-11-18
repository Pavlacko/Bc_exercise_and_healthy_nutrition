using Microsoft.AspNetCore.Mvc;
using SemestralnaPracaVAII.Models;

namespace SemestralnaPracaVAII.Controllers
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
            // Server-side validácia
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Sem neskôr pôjde uloženie do databázy
            // Zatiaľ iba presmerujeme na Dashboard
            return RedirectToAction("Index", "Home");
        }
    }
}

