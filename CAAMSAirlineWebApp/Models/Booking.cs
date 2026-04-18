using System;
using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Confirmed";

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}