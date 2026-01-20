namespace MovieTicketBooking.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int ShowId { get; set; }
        public string SeatNumber { get; set; } = null!;
        public string Status { get; set; } = "Available"; // Available | Held | Booked

        public Guid? HoldId { get; set; }
        public DateTime? HoldExpiry { get; set; }

        public Show Show { get; set; } = null!;
    }
}
