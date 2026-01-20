namespace MovieTicketBooking.Models
{
    public class SeatHold
    {
        public Guid Id { get; set; }
        public int ShowId { get; set; }
        public string Status { get; set; } = "Held"; // Held | Booked | Expired
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}
