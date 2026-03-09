using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAAMSAirlineWebApp.Models
{
    public class FlightStatus
    {
        [Key] // Primary key
        public int StatusId { get; set; }

        [Required]
        public int FlightId { get; set; } // Foreign key to Flight

        [ForeignKey("FlightId")]
        public required Flight Flight { get; set; } // Navigation property

        [MaxLength(50)]
        public required string Status { get; set; }

        public DateTime? UpdatedAt { get; set; } // Nullable if not updated yet
    }
}
