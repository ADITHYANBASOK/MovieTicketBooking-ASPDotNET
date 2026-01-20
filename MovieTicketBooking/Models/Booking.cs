namespace MovieTicketBooking.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid HoldId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
