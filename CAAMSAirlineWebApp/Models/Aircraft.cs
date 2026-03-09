using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Models
{
    public class Aircraft
    {
        [Key] // Primary key
        public int AircraftId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Model { get; set; }

        [MaxLength(50)]
        public required string Manufacturer { get; set; }

        [Required]
        public int Capacity { get; set; }
    }
}
