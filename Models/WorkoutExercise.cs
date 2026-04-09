namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class WorkoutExercise
    {
        public int Id { get; set; }
        public int WorkoutId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int Reps { get; set; }
        public double Weight { get; set; }

        public Workout? Workout { get; set; }
    }
}