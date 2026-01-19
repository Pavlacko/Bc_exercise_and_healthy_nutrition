using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Bc_exercise_and_healthy_nutrition.Filters;

namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [RequireAdmin]
        [HttpGet]
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);  
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                Rola = "User"
            };

            var hasher = new PasswordHasher<AppUser>();
            user.PasswordHash = hasher.HashPassword(user, model.Heslo);

            _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetString("LoggedIn", "true");
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Rola);
            HttpContext.Session.SetString("UserEmail", user.Email);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Nesprávny email alebo heslo.");
                return View(model);
            }

            var hasher = new PasswordHasher<AppUser>();
            var check = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Heslo);

            if (check == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Nesprávny email alebo heslo.");
                return View(model);
            }

            HttpContext.Session.SetString("LoggedIn", "true");
            HttpContext.Session.SetString("UserRole", user.Rola);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetInt32("UserId", user.Id);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Welcome"); 
        }

        [RequireAdmin]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user); 
        }

        [RequireAdmin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AppUser model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            
            user.Meno = model.Meno;
            user.Email = model.Email;
            user.Vek = model.Vek;
            user.Vyska = model.Vyska;
            user.Vaha = model.Vaha;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [RequireAdmin]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user); 
        }

        [RequireAdmin]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            if (user.Rola == "Admin")
            {
                return BadRequest("Admin účet nie je možné zmazať.");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}