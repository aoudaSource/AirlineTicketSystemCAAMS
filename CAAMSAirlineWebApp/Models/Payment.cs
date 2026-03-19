using System;

namespace CAAMSAirlineWebApp.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public DateTime PaymentDate { get; set; }

        // Navigation properties
        public Booking Booking { get; set; } = null!;
    }
}