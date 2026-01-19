using System.ComponentModel.DataAnnotations;

namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 1000)]
        public double KcalPer100g { get; set; }

        [Range(0, 200)]
        public double ProteinPer100g { get; set; }

        [Range(0, 200)]
        public double CarbsPer100g { get; set; }

        [Range(0, 200)]
        public double FatPer100g { get; set; }
    }
}