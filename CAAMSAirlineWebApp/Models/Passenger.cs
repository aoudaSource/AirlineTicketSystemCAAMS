using System;
using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        public int BookingId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PassportNumber { get; set; } = null!;
        public DateTime DOB { get; set; }

        // Navigation properties
        public Booking Booking { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}