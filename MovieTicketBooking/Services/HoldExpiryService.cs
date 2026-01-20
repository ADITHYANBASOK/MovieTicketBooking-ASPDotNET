using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MovieTicketBooking.Data;
namespace MovieTicketBooking.Services
{
    public class HoldExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public HoldExpiryService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

                var expiredSeats = await db.Seats
                    .Where(s => s.Status == "Held" &&
                                s.HoldExpiry < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var seat in expiredSeats)
                {
                    seat.Status = "Available";
                    seat.HoldId = null;
                    seat.HoldExpiry = null;
                }

                var expiredHolds = await db.SeatHolds
                    .Where(h => h.Status == "Held" &&
                                h.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var hold in expiredHolds)
                    hold.Status = "Expired";

                await db.SaveChangesAsync();
                await Task.Delay(30000, stoppingToken);
            }
        }
    }

}
