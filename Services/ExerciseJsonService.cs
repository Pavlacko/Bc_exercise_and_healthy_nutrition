using System.Text.Json;
using Bc_exercise_and_healthy_nutrition.Models;

namespace Bc_exercise_and_healthy_nutrition.Services
{
    public class ExerciseJsonService
    {
        private readonly IWebHostEnvironment _env;

        public ExerciseJsonService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public List<Exercise> LoadAllExercises()
        {
            var allExercises = new List<Exercise>();

            var folderPath = Path.Combine(_env.ContentRootPath, "Data", "Seed");

            if (!Directory.Exists(folderPath))
                return allExercises;

            var files = Directory.GetFiles(folderPath, "exercises_*.json");

            int nextId = 1;

            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);

                    var exercises = JsonSerializer.Deserialize<List<Exercise>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (exercises != null && exercises.Any())
                    {
                        foreach (var exercise in exercises)
                        {
                            exercise.Id = nextId;
                            nextId++;
                            allExercises.Add(exercise);
                        }
                    }
                }
                catch
                {

                }
            }

            return allExercises;
        }
    }
}