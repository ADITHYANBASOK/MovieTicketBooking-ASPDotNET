using System.Data;
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
    public class HoldsController : ControllerBase
    {
        private readonly BookingDbContext _db;

        public HoldsController(BookingDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> HoldSeats(int showId, HoldSeatsDto dto)
        {
            if (dto.SeatNumbers == null || !dto.SeatNumbers.Any())
                return BadRequest("Select at least one seat");

            if (dto.SeatNumbers.Count != dto.SeatNumbers.Distinct().Count())
                return BadRequest("Duplicate seat numbers");

            if (dto.HoldDurationSeconds <= 0 || dto.HoldDurationSeconds > 300)
                return BadRequest("Hold duration must be between 1 and 300 seconds");

            if (!await _db.Shows.AnyAsync(s => s.Id == showId))
                return NotFound("Show not found");

            var holdId = Guid.NewGuid();
            var expiry = DateTime.UtcNow.AddSeconds(dto.HoldDurationSeconds);

            using var tx = await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

            var seats = await _db.Seats
                .Where(s => s.ShowId == showId &&
                            dto.SeatNumbers.Contains(s.SeatNumber))
                .ToListAsync();

            if (seats.Count != dto.SeatNumbers.Count ||
                seats.Any(s => s.Status != "Available"))
            {
                return BadRequest("One or more seats unavailable");
            }

            foreach (var seat in seats)
            {
                seat.Status = "Held";
                seat.HoldId = holdId;
                seat.HoldExpiry = expiry;
            }

            _db.SeatHolds.Add(new SeatHold
            {
                Id = holdId,
                ShowId = showId,
                ExpiresAt = expiry
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new { holdId, expiry });
        }

        [HttpPost("{holdId}/confirm")]
        public async Task<IActionResult> Confirm(Guid holdId)
        {
            using var tx = await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

            if (await _db.Bookings.AnyAsync(b => b.HoldId == holdId))
                return Ok("Already booked");

            var seats = await _db.Seats
                .Where(s => s.HoldId == holdId &&
                    s.Status == "Held" &&
                    s.HoldExpiry > DateTime.UtcNow)
                .ToListAsync();

            if (!seats.Any())
                return BadRequest("Invalid or expired hold");

            foreach (var seat in seats)
                seat.Status = "Booked";

            _db.Bookings.Add(new Booking
            {
                Id = Guid.NewGuid(),
                HoldId = holdId
            });

            var hold = await _db.SeatHolds.FindAsync(holdId);
            if (hold != null) hold.Status = "Booked";

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok("Booked");
        }
    }
}
