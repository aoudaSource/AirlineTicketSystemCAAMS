using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.DTOs;
using CAAMSAirlineWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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


        //for return bc it kept using the same variable

        public string ReturnFlightNumber { get; set; } = "";

        public string ReturnDepartureCode { get; set; } = "";

        public string ReturnDepartureCity { get; set; } = "";

        public string ReturnArrivalCode { get; set; } = "";

        public string ReturnArrivalCity { get; set; } = "";

        public DateTime ReturnDepartureTime { get; set; } 

        public DateTime ReturnArrivalTime { get; set; }
        
        public decimal ReturnBasePrice { get; set; }

        public bool IsRoundTrip => ReturnFlightId.HasValue;

        //autofill, should grab customers last first and passport #

        public string CustomerFirstName { get; set; } = "";

        public string CustomerLastName { get; set; } = "";

        public string CustomerPassportNumber { get; set; } = "";

        public DateTime? CustomerDOB { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ReturnFlightId { get; set; }

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

        public async Task<IActionResult> OnGetAsync(int flightId, int passengerCount, string cabinClass = "Economy", int? returnFlightId = null)

        {
            if (!await LoadFlightAsync(flightId))
                return NotFound();

            Input.FlightId = flightId;
            Input.PassengerCount = passengerCount;
            Input.TicketClass = cabinClass;

            ReturnFlightId = returnFlightId;

            //return for roundtrip
            if (returnFlightId.HasValue)
            {
                if (!await LoadReturnFlightAsync(returnFlightId.Value))
                    return NotFound();
                Input.ReturnFlightId = returnFlightId;
            }

            var username = User.Identity!.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
            if (customer != null)
            {
                CustomerFirstName = customer.FirstName ?? "";
                CustomerLastName = customer.LastName ?? "";
                CustomerPassportNumber =  "";
                CustomerDOB = customer.DOB;
            }

            for (int i = 0; i < passengerCount; i++)
                Input.Passengers.Add(new PassengerInput());

            var multiplier = cabinClass switch
            {
                "Business" => 1.5m,
                "First" => 2m,
                _ => 1m
            };

            decimal outboundTotal = passengerCount * BasePrice * multiplier;
            decimal returnTotal = IsRoundTrip ? passengerCount * ReturnBasePrice * multiplier : 0;
            EstimatedTotal = outboundTotal + returnTotal;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await LoadFlightAsync(Input.FlightId))
                return NotFound();

            var multiplier = Input.TicketClass switch
            {
                "Business" => 1.5m,
                "First" => 2m,
                _ => 1m
            };
            decimal outboundTotal = Input.PassengerCount * BasePrice * multiplier;
            decimal returnTotal = IsRoundTrip ? Input.PassengerCount * ReturnBasePrice * multiplier : 0;
            EstimatedTotal = outboundTotal + returnTotal;

            if (!ModelState.IsValid)
                return Page();

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
            int expectedSeats = IsRoundTrip ? Input.PassengerCount * 2 : Input.PassengerCount;
            if (seatList.Count != expectedSeats)
            {
                ModelState.AddModelError(string.Empty, $"You must select exactly {expectedSeats} seat(s).");
                return Page();
            }

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
                var outboundSeats = seatList.Take(Input.PassengerCount).ToList();
                var returnSeats = IsRoundTrip ? seatList.Skip(Input.PassengerCount).ToList() : new List<string>();

                var outboundBookingId = await _bookingService.CreateBookingWithManualSeatsAsync(Input, outboundSeats);
                _context.Payments.Add(new Models.Payment
                {
                    BookingId = outboundBookingId,
                    Amount = outboundTotal,
                    PaymentMethod = PaymentMethod,
                    PaymentDate = DateTime.Now
                });

                int? returnBookingId = null;
                if (IsRoundTrip && ReturnFlightId.HasValue)
                {
                    var returnRequest = new CreateBookingRequest
                    {
                        CustomerId = Input.CustomerId,
                        FlightId = ReturnFlightId.Value,
                        TicketClass = Input.TicketClass,
                        PassengerCount = Input.PassengerCount,
                        Passengers = Input.Passengers
                    };
                    returnBookingId = await _bookingService.CreateBookingWithManualSeatsAsync(returnRequest, returnSeats);
                    _context.Payments.Add(new Models.Payment
                    {
                        BookingId = returnBookingId.Value,
                        Amount = returnTotal,
                        PaymentMethod = PaymentMethod,
                        PaymentDate = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                return IsRoundTrip && returnBookingId.HasValue
                    ? RedirectToPage("/BookingConfirmation", new { bookingId = outboundBookingId, returnBookingId = returnBookingId.Value })
                    : RedirectToPage("/BookingConfirmation", new { bookingId = outboundBookingId });
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

        private async Task<bool> LoadReturnFlightAsync(int flightId)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightLegs)
                    .ThenInclude(fl => fl.DepartureAirportNavigation)
                .Include(f => f.FlightLegs)
                    .ThenInclude(fl => fl.ArrivalAirportNavigation)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null) return false;

            var sortedLegs = flight.FlightLegs.OrderBy(l => l.LegNumber).ToList();
            var first = sortedLegs.First();
            var last = sortedLegs.Last();

            ReturnFlightNumber = flight.FlightNumber;
            ReturnDepartureCode = first.DepartureAirport;
            ReturnDepartureCity = first.DepartureAirportNavigation.City;
            ReturnArrivalCode = last.ArrivalAirport;
            ReturnArrivalCity = last.ArrivalAirportNavigation.City;
            ReturnDepartureTime = first.DepartureTime;
            ReturnArrivalTime = last.ArrivalTime;
            ReturnBasePrice = flight.BasePrice;

            return true;
        }

    }
}