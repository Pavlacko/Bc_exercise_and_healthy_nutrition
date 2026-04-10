namespace Bc_exercise_and_healthy_nutrition.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public AppUser? Sender { get; set; }

        public int ReceiverId { get; set; }
        public AppUser? Receiver { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}