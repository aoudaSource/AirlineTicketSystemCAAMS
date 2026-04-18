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

        // Display Properties
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

        [BindProperty]
        public string SelectedSeats { get; set; } = "";

        [BindProperty]
        public string PaymentMethod { get; set; } = "";

        [BindProperty]
        public string? CardHolderName { get; set; }

        [BindProperty]
        public string? CardNumber { get; set; }

        [BindProperty]
        public string? ExpiryDate { get; set; }

        [BindProperty]
        public string? CVV { get; set; }

        [BindProperty]
        public string? PayPalEmail { get; set; }

        public async Task<IActionResult> OnGetAsync(int flightId, int passengerCount, string cabinClass = "Economy")
        {
            if (!await LoadFlightAsync(flightId))
                return NotFound();

            Input.FlightId = flightId;
            Input.PassengerCount = passengerCount;
            Input.TicketClass = cabinClass;

            for (int i = 0; i < passengerCount; i++)
                Input.Passengers.Add(new PassengerInput());

            var multiplier = cabinClass switch
            {
                "Business" => 1.5m,
                "First" => 2m,
                _ => 1m
            };
            EstimatedTotal = passengerCount * BasePrice * multiplier;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await LoadFlightAsync(Input.FlightId))
                return NotFound();

            // 1. Recalculate Total
            var multiplier = Input.TicketClass switch
            {
                "Business" => 1.5m,
                "First" => 2m,
                _ => 1m
            };
            EstimatedTotal = Input.PassengerCount * BasePrice * multiplier;

            if (!ModelState.IsValid)
                return Page();

            // 2. Validate Passenger Data
            var today = DateTime.Today;
            static int CalcAge(DateTime dob, DateTime today)
            {
                var age = today.Year - dob.Year;
                if (dob > today.AddYears(-age)) age--;
                return age;
            }

            bool hasAdult = Input.Passengers.Any(p => p.DOB.HasValue && CalcAge(p.DOB.Value, today) > 14);
            bool hasMinor = Input.Passengers.Any(p => p.DOB.HasValue && CalcAge(p.DOB.Value, today) <= 14);

            if (hasMinor && !hasAdult)
            {
                ModelState.AddModelError(string.Empty, "Passengers aged 14 or under must be accompanied by an adult.");
                return Page();
            }

            var passports = Input.Passengers.Select(p => p.PassportNumber?.Trim().ToUpper()).ToList();
            if (passports.Count != passports.Distinct().Count())
            {
                ModelState.AddModelError(string.Empty, "Duplicate passport numbers detected.");
                return Page();
            }

            if (string.IsNullOrEmpty(SelectedSeats))
            {
                ModelState.AddModelError(string.Empty, "Please select seats.");
                return Page();
            }

            var seatList = SelectedSeats.Split(',').Select(s => s.Trim()).ToList();
            if (seatList.Count != Input.PassengerCount)
            {
                ModelState.AddModelError(string.Empty, $"You must select exactly {Input.PassengerCount} seat(s).");
                return Page();
            }

            // 4. Validate Payment Method
            if (string.IsNullOrEmpty(PaymentMethod))
            {
                ModelState.AddModelError(string.Empty, "Please select a payment method.");
                return Page();
            }

            if (PaymentMethod == "PayPal" && string.IsNullOrEmpty(PayPalEmail))
            {
                ModelState.AddModelError(string.Empty, "PayPal email is required.");
                return Page();
            }

            if ((PaymentMethod == "Credit Card" || PaymentMethod == "Debit Card") && 
                (string.IsNullOrEmpty(CardNumber) || string.IsNullOrEmpty(CVV)))
            {
                ModelState.AddModelError(string.Empty, "Card details are required.");
                return Page();
            }

            // 5. Get Customer
            var username = User.Identity?.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
            if (customer == null)
            {
                ModelState.AddModelError(string.Empty, "Customer not found.");
                return Page();
            }

            Input.CustomerId = customer.CustomerId;

            // 6. Create Booking with Manual Seats
            try
            {
                var bookingId = await _bookingService.CreateBookingWithManualSeatsAsync(Input, seatList);
                
                // 7. Create Payment Record
                var payment = new Models.Payment
                {
                    BookingId = bookingId,
                    Amount = EstimatedTotal,
                    PaymentMethod = PaymentMethod,
                    PaymentDate = DateTime.Now
                };
                
                
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return RedirectToPage("/BookingConfirmation", new { bookingId });
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