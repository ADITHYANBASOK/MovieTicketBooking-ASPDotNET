using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs
{
    public class CreateShowDto
    {
        [Required]
        [MaxLength(200)]
        public string MovieName { get; set; } = null!;
        [Range(1, 500)]
        public int TotalSeats { get; set; }
    }
}
