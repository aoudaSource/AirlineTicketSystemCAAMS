using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.DTOs;
using CAAMSAirlineWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages
{
    [Authorize(Roles = "Customer")]
    public class BookingFormModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly BookingService _bookingService;

        public BookingFormModel(ApplicationDbContext context, BookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        // Flight summary loaded on GET, re-loaded on POST
        public string FlightNumber { get; set; } = "";
        public string DepartureCode { get; set; } = "";
        public string DepartureCity { get; set; } = "";
        public string ArrivalCode { get; set; } = "";
        public string ArrivalCity { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }

        public decimal EstimatedTotal { get; set; }

        [BindProperty]
        public CreateBookingRequest Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int flightId, int passengerCount)
        {
            if (!await LoadFlightAsync(flightId))
                return NotFound();

            Input.FlightId = flightId;
            Input.PassengerCount = passengerCount;

            for (int i = 0; i < passengerCount; i++)
                Input.Passengers.Add(new PassengerInput());

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await LoadFlightAsync(Input.FlightId))
                return NotFound();

            // Calculate estimated total
            var multiplier = Input.TicketClass switch
            {
                "Business" => 1.5m,
                "First" => 2m,
                _ => 1m
            };
            EstimatedTotal = Input.PassengerCount * 1 * BasePrice * multiplier;

            if (!ModelState.IsValid)
                return Page();

            var username = User.Identity?.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                ModelState.AddModelError(string.Empty, "Customer not found.");
                return Page();
            }

            Input.CustomerId = customer.CustomerId;

            try
            {
                var bookingId = await _bookingService.CreateBookingAsync(Input);
                return RedirectToPage("/Payment", new { bookingId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }

        private async Task<bool> LoadFlightAsync(int flightId)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightLegs.OrderBy(l => l.LegNumber))
                    .ThenInclude(fl => fl.DepartureAirportNavigation)
                .Include(f => f.FlightLegs.OrderBy(l => l.LegNumber))
                    .ThenInclude(fl => fl.ArrivalAirportNavigation)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null) return false;

            var first = flight.FlightLegs.First();
            var last = flight.FlightLegs.Last();

            FlightNumber = flight.FlightNumber;
            DepartureCode = first.DepartureAirport;
            DepartureCity = first.DepartureAirportNavigation.City;
            ArrivalCode = last.ArrivalAirport;
            ArrivalCity = last.ArrivalAirportNavigation.City;
            DepartureTime = first.DepartureTime;
            ArrivalTime = last.ArrivalTime;
            BasePrice = flight.BasePrice;

            return true;
        }
    }
}
