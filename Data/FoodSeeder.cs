using Bc_exercise_and_healthy_nutrition.Models;

namespace Bc_exercise_and_healthy_nutrition.Data
{
    public static class FoodSeeder
    {
        public static void SeedFoods(AppDbContext db)
        {
            if (db.FoodItems.Any()) return;

            var list = new List<FoodItem>();
            for (int i = 1; i <= 200; i++)
            {
                list.Add(new FoodItem
                {
                    Name = $"Food {i}",
                    KcalPer100g = 50 + (i % 350),
                    ProteinPer100g = (i % 40),
                    CarbsPer100g = (i % 80),
                    FatPer100g = (i % 30)
                });
            }

            db.FoodItems.AddRange(list);
            db.SaveChanges();
        }
    }
}