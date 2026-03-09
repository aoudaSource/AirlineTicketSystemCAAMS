using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAAMSAirlineWebApp.Models
{
    public class Ticket
    {
        [Key] // Primary key
        public int TicketId { get; set; }

        [Required]
        public int BookingId { get; set; } // FK to Booking

        [ForeignKey("BookingId")]
        public required Booking Booking { get; set; } // Navigation property

        [Required]
        public int LegId { get; set; } // FK to FlightLeg

        [ForeignKey("LegId")]
        public required FlightLeg FlightLeg { get; set; } // Navigation property

        public int? SeatId { get; set; } // FK to Seat (optional)

        [ForeignKey("SeatId")]
        public required Seat Seat { get; set; } // Navigation property

        [MaxLength(20)]
        public required string TicketClass { get; set; } // e.g., Economy, Business, First

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Price { get; set; } // Optional
    }
}
