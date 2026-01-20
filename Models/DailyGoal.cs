using System.ComponentModel.DataAnnotations;

namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class DailyGoal
    {
        public int Id { get; set; }

        [Required]
        public int AppUserId { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        [Range(0, 10000)]
        public double KcalGoal { get; set; }

        [Range(0, 500)]
        public double ProteinGoal { get; set; }

        [Range(0, 1000)]
        public double CarbsGoal { get; set; }

        [Range(0, 500)]
        public double FatGoal { get; set; }

        public AppUser? AppUser { get; set; }
    }
}