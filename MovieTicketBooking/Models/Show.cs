namespace MovieTicketBooking.Models
{
    public class Show
    {
        public int Id { get; set; }
        public string MovieName { get; set; } = null!;
        public int TotalSeats { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }

}
