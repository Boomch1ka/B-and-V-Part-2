using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B_and_V_Part_2.Data;
using B_and_V_Part_2.Models;

namespace B_and_V_Part_2.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string search, DateTime? date, int page = 1, int pageSize = 10)
        {
            var query = _context.Bookings.Include(b => b.Venue).Include(b => b.Event).AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Venue.VenueName.Contains(search) || b.Event.EventName.Contains(search));
            }
            if (date.HasValue)
            {
                var start = date.Value.Date;
                var end = start.AddDays(1);
                query = query.Where(b => b.BookingDate >= start && b.BookingDate < end);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages == 0 ? 1 : totalPages;

            var list = await query.OrderBy(b => b.BookingDate)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            ViewData["Search"] = search;
            ViewData["Date"] = date?.ToString("yyyy-MM-dd");
            ViewData["Page"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalCount"] = totalCount;
            return View(list);
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create()
        {
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName");
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,VenueId,BookingDate")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                if (await _context.HasBookingConflictAsync(booking.VenueId, booking.BookingDate))
                {
                    ModelState.AddModelError(string.Empty, "The selected venue is already booked on that date.");
                }
                else
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", booking.VenueId);
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.Include(b => b.Event).Include(b => b.Venue).FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        // VenueController.cs
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null) return NotFound();

            if (venue.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete venue with active bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.Include(b => b.Event).Include(b => b.Venue).FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }
    }
}
