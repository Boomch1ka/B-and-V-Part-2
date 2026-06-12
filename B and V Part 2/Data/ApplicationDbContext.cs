using Microsoft.EntityFrameworkCore;
using B_and_V_Part_2.Models;

namespace B_and_V_Part_2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; }  // NEW

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

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.VenueId, b.BookingDate })
                .IsUnique();

            // NEW: Event -> EventType relationship
            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventType)
                .WithMany()
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            // NEW: Seed predefined event types
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, Name = "Conference" },
                new EventType { EventTypeId = 2, Name = "Wedding" },
                new EventType { EventTypeId = 3, Name = "Concert" },
                new EventType { EventTypeId = 4, Name = "Corporate Function" },
                new EventType { EventTypeId = 5, Name = "Birthday Party" },
                new EventType { EventTypeId = 6, Name = "Exhibition" },
                new EventType { EventTypeId = 7, Name = "Workshop" },
                new EventType { EventTypeId = 8, Name = "Other" }
            );
        }

        public async Task<bool> HasBookingConflictAsync(int venueId, DateTime bookingDate, CancellationToken cancellationToken = default)
        {
            var start = bookingDate.Date;
            var end = start.AddDays(1);
            return await Bookings.AnyAsync(b => b.VenueId == venueId && b.BookingDate >= start && b.BookingDate < end, cancellationToken);
        }

        public async Task<bool> HasActiveBookingsForVenueAsync(int venueId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await Bookings.AnyAsync(b => b.VenueId == venueId && b.BookingDate >= now, cancellationToken);
        }

        public async Task<bool> HasActiveBookingsForEventAsync(int eventId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await Bookings.AnyAsync(b => b.EventId == eventId && b.BookingDate >= now, cancellationToken);
        }
    }
}