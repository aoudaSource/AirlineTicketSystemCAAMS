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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreateFlightInput Input { get; set; } = new();

        public SelectList AircraftOptions { get; set; } = null!;
        public SelectList AirportOptions { get; set; } = null!;

        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
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

            var aircraft = await _context.Aircrafts.FindAsync(Input.AircraftId);
            if (aircraft == null) return NotFound();

            var flight = new Flight
            {
                FlightNumber = Input.FlightNumber.Trim().ToUpper(),
                AircraftId = Input.AircraftId,
                BasePrice = Input.BasePrice,
                FlightLegs = new List<FlightLeg>
      {
          new FlightLeg
          {
              LegNumber = 1,
              DepartureAirport = Input.DepartureAirport,
              ArrivalAirport = Input.ArrivalAirport,
              DepartureTime = Input.DepartureTime,
              ArrivalTime = Input.ArrivalTime,
              AvailableSeats = aircraft.Capacity
          }
      }
            };

            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Flights/Index", new { success = "created" });
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

    public class CreateFlightInput
    {
        [Required]
        [StringLength(10, MinimumLength = 2)]
        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; } = "";

        [Required]
        [Display(Name = "Aircraft")]
        public int AircraftId { get; set; }

        [Required]
        [Range(1, 99999, ErrorMessage = "Base price must be greater than 0.")]
        [Display(Name = "Base Price (USD)")]
        public decimal BasePrice { get; set; }

        [Required]
        [Display(Name = "Departure Airport")]
        public string DepartureAirport { get; set; } = "";

        [Required]
        [Display(Name = "Arrival Airport")]
        public string ArrivalAirport { get; set; } = "";

        [Required]
        [Display(Name = "Departure Date & Time")]
        public DateTime DepartureTime { get; set; } = DateTime.Today.AddDays(1).AddHours(9);

        [Required]
        [Display(Name = "Arrival Date & Time")]
        public DateTime ArrivalTime { get; set; } = DateTime.Today.AddDays(1).AddHours(11);
    }
}
