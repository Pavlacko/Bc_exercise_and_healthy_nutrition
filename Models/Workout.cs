using Bc_exercise_and_healthy_nutrition.Models;

namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class Workout
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }

        public AppUser? AppUser { get; set; }
        public List<WorkoutExercise> Exercises { get; set; } = new();
    }
}