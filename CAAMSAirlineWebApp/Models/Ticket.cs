using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int PassengerId { get; set; }
        public int BookingId { get; set; }
        public int LegId { get; set; }
        public int? SeatId { get; set; }
        public string TicketClass { get; set; } = null!;
        public decimal Price { get; set; }

        // Navigation properties
        public Passenger Passenger { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
        public FlightLeg FlightLeg { get; set; } = null!;
        public Seat? Seat { get; set; }
        public ICollection<Baggage> Baggages { get; set; } = new List<Baggage>();
    }
}