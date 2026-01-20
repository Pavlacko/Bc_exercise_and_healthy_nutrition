using System.ComponentModel.DataAnnotations;

namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class MealEntry
    {
        public int Id { get; set; }

        [Required]
        public int AppUserId { get; set; }

        [Required]
        public int FoodItemId { get; set; }

        [Range(1, 1000, ErrorMessage = "Gramáž musí byť medzi 1 a 1000 g.")]
        public double Grams { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        public AppUser? AppUser { get; set; }
        public FoodItem? FoodItem { get; set; }
    }
}