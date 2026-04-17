using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [MaxLength(11, ErrorMessage = "Phone number cannot exceed 11 characters.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [ValidRegistrationDOB]
        public DateTime? DOB { get; set; }
    }

    public class ValidRegistrationDOBAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is not DateTime dob)
                return ValidationResult.Success; // [Required] handles null

            var today = DateTime.Today;

            if (dob >= today)
                return new ValidationResult("Date of birth must be in the past.");

            var age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;

            if (age < 18)
                return new ValidationResult("You must be at least 18 years old to register.");

            if (age > 120)
                return new ValidationResult("Please enter a valid date of birth.");

            return ValidationResult.Success;
        }
    }
}
