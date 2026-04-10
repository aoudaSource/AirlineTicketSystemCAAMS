using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Airport
    {
        public string AirportCode { get; set; } = null!;
        public string AirportName { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? State { get; set; }
        public string Country { get; set; } = null!;

        // Navigation properties
        public ICollection<FlightLeg> DepartureFlightLegs { get; set; } = new List<FlightLeg>();
        public ICollection<FlightLeg> ArrivalFlightLegs { get; set; } = new List<FlightLeg>();
    }
}