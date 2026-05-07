using Microsoft.EntityFrameworkCore;
using B_and_V_Part_2.Models;

namespace B_and_V_Part_2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId);

            // Enforce unique booking per venue per date (prevents double bookings)
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.VenueId, b.BookingDate })
                .IsUnique();
        }

        // Helper: check for booking conflict for a venue on a given date (date-only)
        public async Task<bool> HasBookingConflictAsync(int venueId, DateTime bookingDate, CancellationToken cancellationToken = default)
        {
            var start = bookingDate.Date;
            var end = start.AddDays(1);
            return await Bookings.AnyAsync(b => b.VenueId == venueId && b.BookingDate >= start && b.BookingDate < end, cancellationToken);
        }

        // Helper: determine if a venue has active (upcoming) bookings
        public async Task<bool> HasActiveBookingsForVenueAsync(int venueId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await Bookings.AnyAsync(b => b.VenueId == venueId && b.BookingDate >= now, cancellationToken);
        }

        // Helper: determine if an event has active (upcoming) bookings
        public async Task<bool> HasActiveBookingsForEventAsync(int eventId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await Bookings.AnyAsync(b => b.EventId == eventId && b.BookingDate >= now, cancellationToken);
        }
    }
}