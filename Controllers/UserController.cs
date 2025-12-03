using Microsoft.AspNetCore.Mvc;
using Bc_exercise_and_healthy_nutrition.Models;
using Bc_exercise_and_healthy_nutrition.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

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

           
            var exists = _context.Users.Any(u => u.Email == model.Email);
            if (exists)
            {
                ModelState.AddModelError("Email", "Používateľ s týmto emailom už existuje.");
                return View(model);
            }

            var user = new AppUser
            {
                Meno = model.Meno,
                Email = model.Email,
                Vek = model.Vek ?? 0,
                Vyska = model.Vyska ?? 0,
                Vaha = model.Vaha ?? 0,
                Heslo = model.Heslo
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetString("LoggedIn", "true");

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                .FirstOrDefault(u => u.Email == model.Email && u.Heslo == model.Heslo);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Nesprávny email alebo heslo.");
                return View(model);
            }

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

