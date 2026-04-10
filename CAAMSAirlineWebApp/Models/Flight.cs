using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Flight
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = null!;
        public int AircraftId { get; set; }
        public decimal BasePrice { get; set; }

        // Navigation properties
        public Aircraft Aircraft { get; set; } = null!;
        public ICollection<FlightLeg> FlightLegs { get; set; } = new List<FlightLeg>();
        public ICollection<FlightStaff> FlightStaffs { get; set; } = new List<FlightStaff>();
        public ICollection<FlightStatus> FlightStatuses { get; set; } = new List<FlightStatus>();
    }
}