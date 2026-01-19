using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.EntityFrameworkCore;

namespace Bc_exercise_and_healthy_nutrition.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
    }
}