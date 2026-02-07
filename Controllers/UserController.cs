using Bc_exercise_and_healthy_nutrition.Data;
using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Bc_exercise_and_healthy_nutrition.Filters;
using Bc_exercise_and_healthy_nutrition.ViewModels;


namespace Bc_exercise_and_healthy_nutrition.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

        [HttpGet]
        [RequireLogin]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.First(u => u.Id == userId.Value);
            return View(user);
        }

        [HttpPost]
        [RequireLogin]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            if (avatar == null || avatar.Length == 0)
            {
                TempData["Err"] = "Vyber súbor.";
                return RedirectToAction(nameof(Profile));
            }

            if (avatar.Length > 2 * 1024 * 1024)
            {
                TempData["Err"] = "Súbor je príliš veľký (max 2 MB).";
                return RedirectToAction(nameof(Profile));
            }

            var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowed.Contains(ext))
            {
                TempData["Err"] = "Povolené formáty: JPG, PNG.";
                return RedirectToAction(nameof(Profile));
            }

            var ct = (avatar.ContentType ?? "").ToLowerInvariant();
            if (ct != "image/jpeg" && ct != "image/png")
            {
                TempData["Err"] = "Neplatný typ súboru.";
                return RedirectToAction(nameof(Profile));
            }

            var user = await _context.Users.FirstAsync(u => u.Id == userId.Value);

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadDir);

            foreach (var oldExt in allowed)
            {
                var p = Path.Combine(uploadDir, $"{user.Id}{oldExt}");
                if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
            }

            var fileName = $"{user.Id}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);

            using (var fs = new FileStream(fullPath, FileMode.Create))
            {
                await avatar.CopyToAsync(fs);
            }

            user.AvatarPath = $"/uploads/avatars/{fileName}";
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Profilová fotka uložená.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [RequireLogin]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvatar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FirstAsync(u => u.Id == userId.Value);

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            var allowed = new[] { ".jpg", ".jpeg", ".png" };

            foreach (var ext in allowed)
            {
                var p = Path.Combine(uploadDir, $"{user.Id}{ext}");
                if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
            }

            user.AvatarPath = null;
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Profilová fotka zmazaná.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [RequireLogin]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [RequireLogin]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string newPassword2)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(newPassword2))
            {
                TempData["Err"] = "Vyplň všetky polia.";
                return RedirectToAction(nameof(ChangePassword));
            }

            if (newPassword.Length < 6)
            {
                TempData["Err"] = "Nové heslo musí mať aspoň 6 znakov.";
                return RedirectToAction(nameof(ChangePassword));
            }

            if (newPassword != newPassword2)
            {
                TempData["Err"] = "Nové heslá sa nezhodujú.";
                return RedirectToAction(nameof(ChangePassword));
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null) return RedirectToAction("Login");

            var hasher = new PasswordHasher<AppUser>();
            var check = hasher.VerifyHashedPassword(user, user.PasswordHash, oldPassword);

            if (check == PasswordVerificationResult.Failed)
            {
                TempData["Err"] = "Aktuálne heslo nie je správne.";
                return RedirectToAction(nameof(ChangePassword));
            }

            user.PasswordHash = hasher.HashPassword(user, newPassword);
            _context.SaveChanges();

            TempData["Ok"] = "Heslo bolo zmenené.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [RequireLogin]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(int id, string meno, string email, int vek, int vyska, double vaha)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            if (userId.Value != id)
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(meno))
            {
                TempData["Err"] = "Meno je povinné.";
                return RedirectToAction(nameof(Profile));
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Err"] = "Email je povinný.";
                return RedirectToAction(nameof(Profile));
            }

            var emailExists = _context.Users.Any(u => u.Email == email && u.Id != id);
            if (emailExists)
            {
                TempData["Err"] = "Používateľ s týmto emailom už existuje.";
                return RedirectToAction(nameof(Profile));
            }

            user.Meno = meno.Trim();
            user.Email = email.Trim();
            user.Vek = vek;
            user.Vyska = vyska;
            user.Vaha = vaha;

            _context.SaveChanges();

            HttpContext.Session.SetString("UserEmail", user.Email);

            TempData["Ok"] = "Profil uložený.";
            return RedirectToAction(nameof(Profile));
        }

    }
}

