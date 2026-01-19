using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.AspNetCore.Identity;

namespace Bc_exercise_and_healthy_nutrition.Data
{
    public static class AdminSeeder
    {
        public static void SeedAdmin(AppDbContext context)
        {
            if (context.Users.Any(u => u.Rola == "Admin"))
                return;

            var admin = new AppUser
            {
                Meno = "Admin",
                Email = "admin@local",
                Vek = 30,
                Vyska = 180,
                Vaha = 80,
                Rola = "Admin"
            };

            var hasher = new PasswordHasher<AppUser>();
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
}
