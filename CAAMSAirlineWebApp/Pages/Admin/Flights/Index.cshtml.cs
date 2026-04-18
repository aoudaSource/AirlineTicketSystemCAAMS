using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CAAMSAirlineWebApp.Pages.Admin.Flights
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add this property to fix the 'Query' error
        [BindProperty(SupportsGet = true)]
        [ValidateNever]
        public string? Query { get; set; }

        public List<FlightListItem> Flights { get; set; } = new();
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync(string? success)
        {
            if (success == "created") SuccessMessage = "Flight created successfully.";
            if (success == "updated") SuccessMessage = "Flight updated successfully.";
            if (success == "deleted") SuccessMessage = "Flight deleted successfully.";

            // Convert query to lowercase for case-insensitive searching
            var q = Query?.Trim().ToLower();

            // Start with the base query including all necessary navigation properties
            var legsQuery = _context.FlightLegs
                .Include(fl => fl.Flight).ThenInclude(f => f.Aircraft)
                .Include(fl => fl.DepartureAirportNavigation)
                .Include(fl => fl.ArrivalAirportNavigation)
                .AsQueryable();

            // Apply search filtering logic
            if (!string.IsNullOrEmpty(q))
            {
                legsQuery = legsQuery.Where(fl =>
                    fl.Flight.FlightNumber.ToLower().Contains(q) ||
                    fl.Flight.Aircraft.Model.ToLower().Contains(q) ||
                    fl.DepartureAirportNavigation.City.ToLower().Contains(q) ||
                    fl.ArrivalAirportNavigation.City.ToLower().Contains(q) ||
                    fl.DepartureAirport.ToLower().Contains(q) ||
                    fl.ArrivalAirport.ToLower().Contains(q)
                );
            }

            // Execute the query with sorting
            var legs = await legsQuery
                .OrderBy(fl => fl.DepartureTime)
                .ToListAsync();

            // Map the results to your FlightListItem DTD

            Flights = legs.Select(fl => new FlightListItem
            {
                FlightId = fl.FlightId,
                LegId = fl.LegId,
                FlightNumber = fl.Flight.FlightNumber,
                Aircraft = $"{fl.Flight.Aircraft.Manufacturer} {fl.Flight.Aircraft.Model}",

                MaintenanceStatus = fl.Flight.Aircraft.MaintenanceStatus ?? "Active",

                Route = $"{fl.DepartureAirport} → {fl.ArrivalAirport}",
                DepartureCity = fl.DepartureAirportNavigation.City,
                ArrivalCity = fl.ArrivalAirportNavigation.City,
                DepartureTime = fl.DepartureTime,
                ArrivalTime = fl.ArrivalTime,
                BasePrice = fl.Flight.BasePrice
            }).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int flightId)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var flight = await _context.Flights.FindAsync(flightId);
            if (flight != null)
            {
                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { success = "deleted" });
        }
    }

    public class FlightListItem
    {
        public int FlightId { get; set; }
        public int LegId { get; set; }
        public string FlightNumber { get; set; } = "";
        public string Aircraft { get; set; } = "";
        public string Route { get; set; } = "";
        public string DepartureCity { get; set; } = "";
        public string ArrivalCity { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
        public string MaintenanceStatus { get; set; } = "Active";
    }
}
