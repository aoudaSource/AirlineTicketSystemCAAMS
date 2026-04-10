using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages.Admin.Airports
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Airport> Airports { get; set; } = new();
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync(string? success)
        {
            if (success == "created") SuccessMessage = "Airport added successfully.";
            if (success == "updated") SuccessMessage = "Airport updated successfully.";
            if (success == "deleted") SuccessMessage = "Airport deleted successfully.";

            Airports = await _context.Airports
                .OrderBy(a => a.Country).ThenBy(a => a.City)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string airportCode)
        {
            // Block delete if any flight legs reference this airport
            bool inUse = await _context.FlightLegs.AnyAsync(fl =>
                fl.DepartureAirport == airportCode || fl.ArrivalAirport == airportCode);

            if (inUse)
            {
                Airports = await _context.Airports
                    .OrderBy(a => a.Country).ThenBy(a => a.City)
                    .ToListAsync();
                ModelState.AddModelError(string.Empty,
                    $"Cannot delete {airportCode} — it is used by one or more flight legs.");
                return Page();
            }

            var airport = await _context.Airports.FindAsync(airportCode);
            if (airport != null)
            {
                _context.Airports.Remove(airport);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { success = "deleted" });
        }
    }
}
