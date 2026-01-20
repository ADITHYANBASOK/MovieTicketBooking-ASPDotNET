namespace MovieTicketBooking.DTOs
{
    public class HoldSeatsDto
    {
        public List<string> SeatNumbers { get; set; } = new();
        public int HoldDurationSeconds { get; set; } = 120;
    }
}
