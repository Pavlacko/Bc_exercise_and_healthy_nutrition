using System.Globalization;
using Bc_exercise_and_healthy_nutrition.Models;

namespace Bc_exercise_and_healthy_nutrition.Data
{
    public static class FoodSeeder
    {
        //generovane pomocou AI
        public static void SeedFoods(AppDbContext db, IWebHostEnvironment env)
        {
            if (db.FoodItems.Any())
                return;

            var path = Path.Combine(env.ContentRootPath, "Data", "Seed", "foods.csv");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Seed file not found: {path}");

            var foods = LoadFoodsFromCsv(path);

            db.FoodItems.AddRange(foods);
            db.SaveChanges();
        }

        //generovane pomocou AI
        private static List<FoodItem> LoadFoodsFromCsv(string path)
        {
            var lines = File.ReadAllLines(path);

            if (lines.Length <= 1)
                return new List<FoodItem>();

            var result = new List<FoodItem>();

            var cultureDot = CultureInfo.InvariantCulture;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length != 5) continue;

                var name = parts[0].Trim();

                if (!double.TryParse(parts[1].Trim(), NumberStyles.Any, cultureDot, out var kcal)) continue;
                if (!double.TryParse(parts[2].Trim(), NumberStyles.Any, cultureDot, out var p)) continue;
                if (!double.TryParse(parts[3].Trim(), NumberStyles.Any, cultureDot, out var c)) continue;
                if (!double.TryParse(parts[4].Trim(), NumberStyles.Any, cultureDot, out var f)) continue;

                result.Add(new FoodItem
                {
                    Name = name,
                    KcalPer100g = kcal,
                    ProteinPer100g = p,
                    CarbsPer100g = c,
                    FatPer100g = f
                });
            }

            return result;
        }
    }
}