using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Aircraft
    {
        public int AircraftId { get; set; }
        public string Model { get; set; } = null!;
        public string? Manufacturer { get; set; }
        public int Capacity { get; set; }
        public string MaintenanceStatus { get; set; } = "Active";

        // Navigation properties
        public ICollection<Flight> Flights { get; set; } = new List<Flight>();
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}