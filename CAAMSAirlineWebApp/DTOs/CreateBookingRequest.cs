using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.DTOs
{
    public class CreateBookingRequest
    {
        public int CustomerId { get; set; }

        public int FlightId { get; set; }

        [Required(ErrorMessage = "Please select a ticket class.")]
        public string TicketClass { get; set; } = string.Empty;
        public int PassengerCount { get; set; }

        public List<PassengerInput> Passengers { get; set; } = new();
    }

    public class PassengerInput
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [RegularExpression(@"^[A-Za-z0-9]{6,9}$",
            ErrorMessage = "Passport number must be 6–9 alphanumeric characters (letters and digits only).")]
        public string PassportNumber { get; set; } = null!;

        [Required(ErrorMessage = "Date of birth is required.")]
        [ValidPassengerDOB]
        public DateTime? DOB { get; set; }
    }

  
    /// Validates that a passenger's date of birth is:
    ///   - in the past
    ///   - at least 2 years ago (infants have separate booking rules)
    ///   - no more than 120 years ago
 
    public class ValidPassengerDOBAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is not DateTime dob)
                return ValidationResult.Success; // [Required] handles null

            var today = DateTime.Today;

            if (dob >= today)
                return new ValidationResult("Date of birth must be in the past.");

            var age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--; // adjust for birthday not yet reached

            if (age < 2)
                return new ValidationResult("Passenger must be at least 2 years old. Infants must be booked separately.");

            if (age > 120)
                return new ValidationResult("Please enter a valid date of birth.");

            return ValidationResult.Success;
        }
    }
}