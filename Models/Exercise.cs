namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class Exercise
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public string VideoPath { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PrimaryMuscle { get; set; }
        public string? SecondaryMuscles { get; set; }
        public string? Equipment { get; set; }
        public string? Difficulty { get; set; }
        public string? Instructions { get; set; }
        public string? Tips { get; set; }
    }
}