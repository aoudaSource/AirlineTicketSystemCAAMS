using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAAMSAirlineWebApp.Models
{
    public class FlightLeg
    {
        [Key] // Primary key
        public int LegId { get; set; }

        [Required]
        public int FlightId { get; set; } // Foreign key to Flight

        [ForeignKey("FlightId")]
        public required Flight Flight { get; set; } // Navigation property

        [Required]
        public int LegNumber { get; set; }

        [Required]
        [MaxLength(3)]
        public required string DepartureAirportCode { get; set; } // FK to Airport

        [ForeignKey("DepartureAirportCode")]
        public required Airport DepartureAirport { get; set; } // Navigation property

        [Required]
        [MaxLength(3)]
        public required string ArrivalAirportCode { get; set; } // FK to Airport

        [ForeignKey("ArrivalAirportCode")]
        public required Airport ArrivalAirport { get; set; } // Navigation property

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }
    }
}
