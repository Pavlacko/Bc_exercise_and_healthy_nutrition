using System.ComponentModel.DataAnnotations;

namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Meno je povinné.")]
        public string Meno { get; set; }

        [Required(ErrorMessage = "Email je povinný.")]
        [EmailAddress(ErrorMessage = "Zadajte platný email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vek je povinný.")]
        [Range(10, 100, ErrorMessage = "Vek musí byť medzi 10 a 100 rokmi.")]
        public int? Vek { get; set; }

        [Required(ErrorMessage = "Výška je povinná.")]
        [Range(50, 250, ErrorMessage = "Výška musí byť medzi 50 a 250 cm.")]
        public int? Vyska { get; set; }

        [Required(ErrorMessage = "Hmotnosť je povinná.")]
        [Range(20, 300, ErrorMessage = "Hmotnosť musí byť medzi 20 a 300 kg.")]
        public double? Vaha { get; set; }

        [Required(ErrorMessage = "Heslo je povinné.")]
        [MinLength(6, ErrorMessage = "Heslo musí mať aspoň 6 znakov.")]
        public string Heslo { get; set; }
    }
}