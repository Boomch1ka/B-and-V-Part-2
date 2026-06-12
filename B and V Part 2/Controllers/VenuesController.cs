using B_and_V_Part_2.Data;
using B_and_V_Part_2.Models;
using B_and_V_Part_2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace B_and_V_Part_2.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobService _blobService;

        // SINGLE constructor only
        public VenuesController(ApplicationDbContext context, IBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index(string search, bool? isAvailable)
        {
            var query = _context.Venues.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(v => v.VenueName.Contains(search) || v.Location.Contains(search));

            if (isAvailable.HasValue)
                query = query.Where(v => v.IsAvailable == isAvailable.Value);

            var list = await query.OrderBy(v => v.VenueName).ToListAsync();
            ViewData["Search"] = search;
            ViewData["IsAvailable"] = isAvailable;
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        // GET Create
        public IActionResult Create() => View();

        // POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity")] Venue venue, IFormFile imageFile)
        {
            ModelState.Remove("Bookings");
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"venues/{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    using var stream = imageFile.OpenReadStream();
                    venue.ImageUrl = await _blobService.UploadAsync(stream, fileName, imageFile.ContentType);
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Venue created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        // POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue, IFormFile imageFile)
        {
            if (id != venue.VenueId) return NotFound();
            ModelState.Remove("Bookings");
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"venues/{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    using var stream = imageFile.OpenReadStream();
                    venue.ImageUrl = await _blobService.UploadAsync(stream, fileName, imageFile.ContentType);
                }

                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Venue updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Venues.AnyAsync(e => e.VenueId == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();

            if (await _context.HasActiveBookingsForVenueAsync(venue.VenueId))
                TempData["DeleteError"] = "This venue has active or upcoming bookings and cannot be deleted.";

            return View(venue);
        }

        // POST Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            if (await _context.HasActiveBookingsForVenueAsync(id))
            {
                TempData["DeleteError"] = "This venue has active or upcoming bookings and cannot be deleted.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Venue deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}