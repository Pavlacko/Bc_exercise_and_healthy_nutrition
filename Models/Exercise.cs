namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class Exercise
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public string VideoPath { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}