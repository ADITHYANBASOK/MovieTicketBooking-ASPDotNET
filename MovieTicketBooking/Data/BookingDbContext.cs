using MovieTicketBooking.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;



namespace MovieTicketBooking.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options)
            : base(options) { }

        public DbSet<Show> Shows => Set<Show>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<SeatHold> SeatHolds => Set<SeatHold>();
        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Seat>()
             .HasIndex(s => new { s.ShowId, s.SeatNumber })
             .IsUnique();
        }
    }
}
