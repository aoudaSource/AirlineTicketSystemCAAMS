using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Models
{
    public class Seat
    {
        [Key] // Primary key
        public int SeatId { get; set; }

        [Required]
        public int AircraftId { get; set; } // Foreign key to Aircraft
        [ForeignKey("AircraftId")]
        public required Aircraft Aircraft { get; set; } // Navigation property

        // Link seat to a flight
        [Required]
        public int FlightId { get; set; } // new foreign key to Flight
        [ForeignKey("FlightId")]
        public required Flight Flight { get; set; }

        [Required]
        [MaxLength(10)]
        public required string SeatNumber { get; set; }

        [MaxLength(20)]
        public required string SeatClass { get; set; } // Economy, Business, etc.

        public bool IsBooked { get; set; } // Track if booked

    }
}
