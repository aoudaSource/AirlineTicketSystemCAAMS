using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAAMSAirlineWebApp.Models
{
    public class FlightStaff
    {
        [Key] // Primary key
        public int FlightStaffId { get; set; }

        [Required]
        public int FlightId { get; set; } // Foreign key

        [ForeignKey("FlightId")]
        public required Flight Flight { get; set; } // Navigation property

        [Required]
        public int StaffId { get; set; } // Foreign key

        [ForeignKey("StaffId")]
        public Staff? Staff { get; set; } // Navigation property
    }
}
