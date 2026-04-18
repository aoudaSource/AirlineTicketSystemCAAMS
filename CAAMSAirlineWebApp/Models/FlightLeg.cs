using System;
using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class FlightLeg
    {
        public int LegId { get; set; }
        public int FlightId { get; set; }
        public int LegNumber { get; set; }
        public string DepartureAirport { get; set; } = null!;
        public string ArrivalAirport { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int AvailableSeats { get; set; }

        // Navigation properties
        public Flight Flight { get; set; } = null!;
        public Airport DepartureAirportNavigation { get; set; } = null!;
        public Airport ArrivalAirportNavigation { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}