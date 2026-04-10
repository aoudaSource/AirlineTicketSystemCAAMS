using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Pages.Admin.Bookings
{
    [Authorize(Roles = "Admin,Staff")]
    public class EditPassengerModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditPassengerModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EditPassengerInput Input { get; set; } = new();

        public string PassengerFullName { get; set; } = "";
        public int BookingId { get; set; }

        public async Task<IActionResult> OnGetAsync(int passengerId, int bookingId)
        {
            var passenger = await _context.Passengers.FindAsync(passengerId);
            if (passenger == null) return NotFound();

            BookingId = bookingId;
            PassengerFullName = $"{passenger.FirstName} {passenger.LastName}";

            Input = new EditPassengerInput
            {
                PassengerId = passenger.PassengerId,
                PassportNumber = passenger.PassportNumber,
                DOB = passenger.DOB
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var passenger = await _context.Passengers.FindAsync(Input.PassengerId);
            if (passenger == null) return NotFound();

            // Check new passport isn't already used by another passenger in the same booking
            var duplicate = await _context.Passengers.AnyAsync(p =>
                p.BookingId == passenger.BookingId &&
                p.PassengerId != passenger.PassengerId &&
                p.PassportNumber.ToLower() == Input.PassportNumber.Trim().ToLower());

            if (duplicate)
            {
                ModelState.AddModelError("Input.PassportNumber",
                    "Another passenger in this booking already has that passport number.");
                PassengerFullName = $"{passenger.FirstName} {passenger.LastName}";
                Input.PassengerId = passenger.PassengerId;
                return Page();
            }

            passenger.PassportNumber = Input.PassportNumber.Trim().ToUpper();
            passenger.DOB = Input.DOB;
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Bookings/Details",
                new { bookingId = passenger.BookingId });
        }
    }

    public class EditPassengerInput
    {
        public int PassengerId { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z0-9]{6,9}$",
            ErrorMessage = "Passport number must be 6–9 alphanumeric characters.")]
        [Display(Name = "Passport Number")]
        public string PassportNumber { get; set; } = "";

        [Required]
        [Display(Name = "Date of Birth")]
        [ValidPassengerDOBAdmin]
        public DateTime DOB { get; set; }
    }

    public class ValidPassengerDOBAdminAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is not DateTime dob)
                return ValidationResult.Success;

            var today = DateTime.Today;

            if (dob >= today)
                return new ValidationResult("Date of birth must be in the past.");

            var age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;

            if (age < 2)
                return new ValidationResult("Passenger must be at least 2 years old.");

            if (age > 120)
                return new ValidationResult("Please enter a valid date of birth.");

            return ValidationResult.Success;
        }
    }
}
