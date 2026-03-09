using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Models
{
    public class Customer
    {
        [Key] // Primary key
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string LastName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Email { get; set; }

        [MaxLength(20)]
        public required string Phone { get; set; }

        [MaxLength(50)]
        public required string PassportNumber { get; set; } // unique constraint applied via Fluent API

        public DateTime? DOB { get; set; } // nullable if DOB optional
    }
}
