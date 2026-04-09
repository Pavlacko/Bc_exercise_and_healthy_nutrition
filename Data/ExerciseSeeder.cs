using System.Text.Json;
using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.EntityFrameworkCore;

namespace Bc_exercise_and_healthy_nutrition.Data
{
    public static class ExerciseSeeder
    {
        public static async Task<int> ImportExercisesAsync(IServiceProvider services, IWebHostEnvironment env)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            string seedFolder = Path.Combine(env.ContentRootPath, "Data", "Seed");

            if (!Directory.Exists(seedFolder))
                return 0;

            int addedCount = 0;

            var jsonFiles = Directory.GetFiles(seedFolder, "exercises_*.json");

            foreach (var file in jsonFiles)
            {
                string json = await File.ReadAllTextAsync(file);

                var exercises = JsonSerializer.Deserialize<List<Exercise>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (exercises == null || exercises.Count == 0)
                    continue;

                foreach (var exercise in exercises)
                {
                    if (string.IsNullOrWhiteSpace(exercise.Name))
                        continue;

                    bool exists = await db.Exercises.AnyAsync(e => e.Name == exercise.Name);

                    if (exists)
                        continue;

                    if (!string.IsNullOrWhiteSpace(exercise.VideoPath) && !exercise.VideoPath.StartsWith("/"))
                    {
                        exercise.VideoPath = "/" + exercise.VideoPath;
                    }

                    db.Exercises.Add(new Exercise
                    {
                        Name = exercise.Name?.Trim() ?? string.Empty,
                        MuscleGroup = exercise.MuscleGroup?.Trim() ?? string.Empty,
                        VideoPath = exercise.VideoPath?.Trim() ?? string.Empty,
                        Description = exercise.Description?.Trim(),
                        PrimaryMuscle = exercise.PrimaryMuscle?.Trim(),
                        SecondaryMuscles = exercise.SecondaryMuscles?.Trim(),
                        Equipment = exercise.Equipment?.Trim(),
                        Difficulty = exercise.Difficulty?.Trim(),
                        Instructions = exercise.Instructions?.Trim(),
                        Tips = exercise.Tips?.Trim()
                    });

                    addedCount++;
                }
            }

            await db.SaveChangesAsync();
            return addedCount;
        }
    }
}