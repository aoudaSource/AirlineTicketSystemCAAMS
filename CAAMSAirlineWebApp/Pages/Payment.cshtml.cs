using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Pages
{
    [Authorize(Roles = "Customer")]
    public class PaymentModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PaymentModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Booking summary displayed on the page
        public int BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public string FlightNumber { get; set; } = "";
        public string Route { get; set; } = "";
        public string DepartureTime { get; set; } = "";
        public int PassengerCount { get; set; }
        public string TicketClass { get; set; } = "";

        [BindProperty]
        public PaymentInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int bookingId)
        {
            if (!await LoadBookingAsync(bookingId))
                return NotFound();

            Input.BookingId = bookingId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await LoadBookingAsync(Input.BookingId))
                return NotFound();

            if (!ModelState.IsValid)
                return Page();

            var payment = new Payment
            {
                BookingId = Input.BookingId,
                Amount = TotalAmount,
                PaymentMethod = Input.PaymentMethod,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return RedirectToPage("/BookingConfirmation", new { bookingId = Input.BookingId });
        }

        private async Task<bool> LoadBookingAsync(int bookingId)
        {
            var username = User.Identity?.Name;

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Passengers)
                    .ThenInclude(p => p.Tickets)
                        .ThenInclude(t => t.FlightLeg)
                            .ThenInclude(fl => fl.Flight)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return false;
            if (booking.Customer.Username != username) return false;

            var firstTicket = booking.Passengers
                .SelectMany(p => p.Tickets)
                .OrderBy(t => t.FlightLeg.LegNumber)
                .FirstOrDefault();

            BookingId = booking.BookingId;
            TotalAmount = booking.TotalPrice;
            PassengerCount = booking.Passengers.Count;
            TicketClass = firstTicket?.TicketClass ?? "";

            if (firstTicket != null)
            {
                var flight = firstTicket.FlightLeg.Flight;
                FlightNumber = flight.FlightNumber;

                var legs = booking.Passengers
                    .SelectMany(p => p.Tickets)
                    .Select(t => t.FlightLeg)
                    .DistinctBy(l => l.LegId)
                    .OrderBy(l => l.LegNumber)
                    .ToList();

                if (legs.Any())
                {
                    Route = $"{legs.First().DepartureAirport} → {legs.Last().ArrivalAirport}";
                    DepartureTime = legs.First().DepartureTime.ToString("MMM dd, yyyy hh:mm tt");
                }
            }

            return true;
        }
    }

    public class PaymentInput
    {
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; } = "";

        // Card fields — required only when method is Credit/Debit Card
        [RequiredIf("PaymentMethod", "Credit Card", "Debit Card", ErrorMessage = "Card holder name is required.")]
        public string? CardHolderName { get; set; }

        [RequiredIf("PaymentMethod", "Credit Card", "Debit Card", ErrorMessage = "Card number is required.")]
        public string? CardNumber { get; set; }

        [RequiredIf("PaymentMethod", "Credit Card", "Debit Card", ErrorMessage = "Expiry date is required.")]
        public string? ExpiryDate { get; set; }

        [RequiredIf("PaymentMethod", "Credit Card", "Debit Card", ErrorMessage = "CVV is required.")]
        public string? CVV { get; set; }

        // PayPal field — required only when method is PayPal
        [RequiredIf("PaymentMethod", "PayPal", ErrorMessage = "PayPal email is required.")]
        public string? PayPalEmail { get; set; }
    }

    // Custom validation: required when the named sibling property equals one of the given values
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _propertyName;
        private readonly string[] _targetValues;

        public RequiredIfAttribute(string propertyName, params string[] targetValues)
        {
            _propertyName = propertyName;
            _targetValues = targetValues;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var property = ctx.ObjectType.GetProperty(_propertyName);
            if (property == null) return ValidationResult.Success;

            var propValue = property.GetValue(ctx.ObjectInstance)?.ToString() ?? "";

            if (_targetValues.Contains(propValue))
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
