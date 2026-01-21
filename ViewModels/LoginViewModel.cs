using System.ComponentModel.DataAnnotations;

namespace Bc_exercise_and_healthy_nutrition.ViewModels
{
    public class LoginViewModel
    {
        
        [Required(ErrorMessage = "Email je povinný.")]
        [EmailAddress(ErrorMessage = "Zadajte platný email.")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Heslo je povinné.")]
        public string Heslo { get; set; }
    }
}