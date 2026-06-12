using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B_and_V_Part_2.Data;
using B_and_V_Part_2.Models;
using B_and_V_Part_2.Services;

namespace B_and_V_Part_2.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobService _blobService;

        public EventsController(ApplicationDbContext context, IBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: Events
        public async Task<IActionResult> Index(string search, int? eventTypeId, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.Events
                .Include(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.EventName.Contains(search) || e.Description.Contains(search));

            if (eventTypeId.HasValue)
                query = query.Where(e => e.EventTypeId == eventTypeId.Value);

            if (dateFrom.HasValue)
                query = query.Where(e => e.EventDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(e => e.EventDate <= dateTo.Value.AddDays(1).AddTicks(-1));

            var eventTypes = await _context.EventTypes.OrderBy(t => t.Name).ToListAsync();

            var vm = new EventFilterViewModel
            {
                Search = search,
                EventTypeId = eventTypeId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Events = await query.OrderBy(e => e.EventDate).ToListAsync(),
                EventTypes = new SelectList(eventTypes, "EventTypeId", "Name", eventTypeId)
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // GET: Create
        public IActionResult Create()
        {
            PopulateEventTypesDropdown();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,EventDate,Description,EventTypeId")] Event ev, IFormFile imageFile)
        {
            ModelState.Remove("Bookings");
            ModelState.Remove("EventType");
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"events/{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    using var stream = imageFile.OpenReadStream();
                    ev.ImageUrl = await _blobService.UploadAsync(stream, fileName, imageFile.ContentType);
                }

                _context.Add(ev);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            PopulateEventTypesDropdown(ev.EventTypeId);
            return View(ev);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            PopulateEventTypesDropdown(ev.EventTypeId);
            return View(ev);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,EventTypeId,ImageUrl")] Event ev, IFormFile imageFile)
        {
            if (id != ev.EventId) return NotFound();
            ModelState.Remove("Bookings");
            ModelState.Remove("EventType");
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"events/{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    using var stream = imageFile.OpenReadStream();
                    ev.ImageUrl = await _blobService.UploadAsync(stream, fileName, imageFile.ContentType);
                }

                try
                {
                    _context.Update(ev);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Events.AnyAsync(e => e.EventId == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateEventTypesDropdown(ev.EventTypeId);
            return View(ev);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();

            if (await _context.HasActiveBookingsForEventAsync(ev.EventId))
                TempData["DeleteError"] = "This event has active or upcoming bookings and cannot be deleted.";

            return View(ev);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();

            if (ev.Bookings.Any())
            {
                TempData["DeleteError"] = "Cannot delete event with active bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateEventTypesDropdown(int? selectedId = null)
        {
            var types = _context.EventTypes.OrderBy(t => t.Name).ToList();
            ViewBag.EventTypeId = new SelectList(types, "EventTypeId", "Name", selectedId);
        }
    }
}