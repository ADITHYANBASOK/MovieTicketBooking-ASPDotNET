using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.DTOs;
using MovieTicketBooking.Models;


namespace MovieTicketBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowsController : ControllerBase
    {
        private readonly BookingDbContext _db;

        public ShowsController(BookingDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> CreateShow(CreateShowDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MovieName))
                return BadRequest("Movie name is required");

            var show = new Show
            {
                MovieName = dto.MovieName,
                TotalSeats = dto.TotalSeats
            };

            for (int i = 1; i <= dto.TotalSeats; i++)
            {
                show.Seats.Add(new Seat
                {
                    SeatNumber = $"S{i}",
                    Status = "Available"
                });
            }

            _db.Shows.Add(show);
            await _db.SaveChangesAsync();

            return Ok(new { show.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetShows()
        {
            var shows = await _db.Shows
                .Select(s => new
                {
                    s.Id,
                    s.MovieName
                })
                .ToListAsync();

            return Ok(shows);
        }


        [HttpGet("{id}/seats/summary")]
        public async Task<IActionResult> SeatSummary(int id)
        {
            var exists = await _db.Shows.AnyAsync(s => s.Id == id);
            if (!exists)
                return NotFound("Show not found");

            var seats = await _db.Seats
                .Where(s => s.ShowId == id)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Available = g.Count(x => x.Status == "Available"),
                    Held = g.Count(x => x.Status == "Held"),
                    Booked = g.Count(x => x.Status == "Booked")
                })
                .FirstAsync();

            return Ok(seats);
        }

        [HttpGet("{showId}/seats/available")]
        public async Task<IActionResult> GetAvailableSeats(int showId)
        {
            var exists = await _db.Shows.AnyAsync(s => s.Id == showId);
            if (!exists)
                return NotFound("Show not found");

            var seats = await _db.Seats
                .Where(s => s.ShowId == showId && s.Status == "Available")
                .OrderBy(s => s.SeatNumber)
                .Select(s => s.SeatNumber)
                .ToListAsync();

            return Ok(seats);
        }
    }
}
