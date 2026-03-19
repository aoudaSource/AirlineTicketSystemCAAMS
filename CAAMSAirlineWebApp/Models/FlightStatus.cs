using System;

namespace CAAMSAirlineWebApp.Models
{
    public class FlightStatus
    {
        public int StatusId { get; set; }
        public int FlightId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Flight Flight { get; set; } = null!;
    }
}