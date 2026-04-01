using System.ComponentModel.DataAnnotations;

namespace Bc_exercise_and_healthy_nutrition.ViewModels
{
    public class VerifyTwoFactorViewModel
    {
        [Required(ErrorMessage = "Zadaj overovací kód.")]
        public string Code { get; set; }
    }
}