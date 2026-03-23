using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages.Admin.Aircrafts
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Aircraft> Aircrafts { get; set; } = new();
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync(string? success)
        {
            if (success == "created") SuccessMessage = "Aircraft added successfully.";
            if (success == "updated") SuccessMessage = "Aircraft updated successfully.";
            if (success == "deleted") SuccessMessage = "Aircraft deleted successfully.";

            Aircrafts = await _context.Aircrafts
                .OrderBy(a => a.Manufacturer).ThenBy(a => a.Model)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int aircraftId)
        {
            bool inUse = await _context.Flights.AnyAsync(f => f.AircraftId == aircraftId);

            if (inUse)
            {
                Aircrafts = await _context.Aircrafts
                    .OrderBy(a => a.Manufacturer).ThenBy(a => a.Model)
                    .ToListAsync();
                ModelState.AddModelError(string.Empty,
                    "Cannot delete this aircraft — it is assigned to one or more flights.");
                return Page();
            }

            var aircraft = await _context.Aircrafts.FindAsync(aircraftId);
            if (aircraft != null)
            {
                _context.Aircrafts.Remove(aircraft);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { success = "deleted" });
        }
    }
}
