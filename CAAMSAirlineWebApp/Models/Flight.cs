using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Flight
    {
        [Key] // Primary key
        public int FlightId { get; set; }

        [Required]
        [MaxLength(10)]
        public required string FlightNumber { get; set; }

        [Required]
        public int AircraftId { get; set; } // Foreign key

        [ForeignKey("AircraftId")]
        public Aircraft? Aircraft { get; set; } // Navigation property
        public ICollection<Seat> Seats { get; set; } = new List<Seat>(); //This lets EF Core know that one flight can have many seats.
    }
}
