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

            string videosRoot = Path.Combine(env.WebRootPath, "videos");

            if (!Directory.Exists(videosRoot))
                return 0;

            int addedCount = 0;

            var muscleFolders = Directory.GetDirectories(videosRoot);

            foreach (var folder in muscleFolders)
            {
                string muscleGroup = Path.GetFileName(folder);
                var videoFiles = Directory.GetFiles(folder, "*.mp4");

                foreach (var videoFile in videoFiles)
                {
                    string fileNameWithExtension = Path.GetFileName(videoFile);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(videoFile);

                    string relativePath = $"/videos/{muscleGroup}/{fileNameWithExtension}";
                    string exerciseName = fileNameWithoutExtension.Trim();

                    bool exists = await db.Exercises.AnyAsync(e => e.VideoPath == relativePath);

                    if (!exists)
                    {
                        db.Exercises.Add(new Exercise
                        {
                            Name = exerciseName,
                            MuscleGroup = muscleGroup,
                            VideoPath = relativePath
                        });

                        addedCount++;
                    }
                }
            }

            await db.SaveChangesAsync();
            return addedCount;
        }
    }
}