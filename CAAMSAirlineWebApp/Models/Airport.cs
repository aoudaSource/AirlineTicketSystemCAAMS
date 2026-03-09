using System.ComponentModel.DataAnnotations;
namespace CAAMSAirlineWebApp.Models
{
    public class Airport
    {
        [Key] // Primary key
        [MaxLength(3)]
        public required string AirportCode { get; set; } // CHAR(3)

        [Required]
        [MaxLength(100)]
        public required string AirportName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string City { get; set; }

        [MaxLength(50)]
        public required string State { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Country { get; set; }
    }
}
