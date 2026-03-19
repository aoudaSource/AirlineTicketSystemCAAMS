using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Seat
    {
        public int SeatId { get; set; }
        public int AircraftId { get; set; }
        public string SeatNumber { get; set; } = null!;
        public string SeatClass { get; set; } = null!;

        // Navigation properties
        public Aircraft Aircraft { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}