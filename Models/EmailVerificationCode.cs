namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class EmailVerificationCode
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Code { get; set; }

        public DateTime Expiration { get; set; }
    }
}