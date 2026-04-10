using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Pages.Admin.Flights
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EditFlightInput Input { get; set; } = new();

        public SelectList AircraftOptions { get; set; } = null!;
        public SelectList AirportOptions { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int flightId, int legId)
        {
            var leg = await _context.FlightLegs
                .Include(fl => fl.Flight)
                .FirstOrDefaultAsync(fl => fl.FlightId == flightId && fl.LegId == legId);

            if (leg == null) return NotFound();

            Input = new EditFlightInput
            {
                FlightId = leg.FlightId,
                LegId = leg.LegId,
                FlightNumber = leg.Flight.FlightNumber,
                AircraftId = leg.Flight.AircraftId,
                BasePrice = leg.Flight.BasePrice,
                DepartureAirport = leg.DepartureAirport,
                ArrivalAirport = leg.ArrivalAirport,
                DepartureTime = leg.DepartureTime,
                ArrivalTime = leg.ArrivalTime
            };

            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            if (Input.ArrivalTime <= Input.DepartureTime)
            {
                ModelState.AddModelError("Input.ArrivalTime", "Arrival time must be after departure time.");
                await LoadSelectListsAsync();
                return Page();
            }

            if (Input.DepartureAirport == Input.ArrivalAirport)
            {
                ModelState.AddModelError("Input.ArrivalAirport", "Departure and arrival airports must be different.");
                await LoadSelectListsAsync();
                return Page();
            }

            var flight = await _context.Flights.FindAsync(Input.FlightId);
            var leg = await _context.FlightLegs.FindAsync(Input.LegId);

            if (flight == null || leg == null) return NotFound();

            flight.FlightNumber = Input.FlightNumber.Trim().ToUpper();
            flight.AircraftId = Input.AircraftId;
            flight.BasePrice = Input.BasePrice;

            leg.DepartureAirport = Input.DepartureAirport;
            leg.ArrivalAirport = Input.ArrivalAirport;
            leg.DepartureTime = Input.DepartureTime;
            leg.ArrivalTime = Input.ArrivalTime;

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Flights/Index", new { success = "updated" });
        }

        private async Task LoadSelectListsAsync()
        {
            var aircrafts = await _context.Aircrafts
                .OrderBy(a => a.Manufacturer)
                .Select(a => new { a.AircraftId, Display = $"{a.Manufacturer} {a.Model} (cap. {a.Capacity})" })
                .ToListAsync();

            AircraftOptions = new SelectList(aircrafts, "AircraftId", "Display");

            var airports = await _context.Airports
                .OrderBy(a => a.Country).ThenBy(a => a.City)
                .Select(a => new { a.AirportCode, Display = $"{a.AirportCode} – {a.AirportName} ({a.City}, {a.Country})" })
                .ToListAsync();

            AirportOptions = new SelectList(airports, "AirportCode", "Display");
        }
    }

    public class EditFlightInput
    {
        public int FlightId { get; set; }
        public int LegId { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 2)]
        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; } = "";

        [Required]
        [Display(Name = "Aircraft")]
        public int AircraftId { get; set; }

        [Required]
        [Range(1, 99999, ErrorMessage = "Base price must be greater than 0.")]
        [Display(Name = "Base Price (MYR)")]
        public decimal BasePrice { get; set; }

        [Required]
        [Display(Name = "Departure Airport")]
        public string DepartureAirport { get; set; } = "";

        [Required]
        [Display(Name = "Arrival Airport")]
        public string ArrivalAirport { get; set; } = "";

        [Required]
        [Display(Name = "Departure Date & Time")]
        public DateTime DepartureTime { get; set; }

        [Required]
        [Display(Name = "Arrival Date & Time")]
        public DateTime ArrivalTime { get; set; }
    }
}
