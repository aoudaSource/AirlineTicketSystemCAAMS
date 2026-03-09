using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAAMSAirlineWebApp.Models
{
    public class Booking
    {
        [Key] // Primary key
        public int BookingId { get; set; }

        [Required]
        public int CustomerId { get; set; } // Foreign key to Customer

        [ForeignKey("CustomerId")]
        public required Customer Customer { get; set; } // Navigation property

        [Required]
        public DateTime BookingDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalPrice { get; set; } // Optional field
        public int FlightId { get; internal set; }
    }
}
