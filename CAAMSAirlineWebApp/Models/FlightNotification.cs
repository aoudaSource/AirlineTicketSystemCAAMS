using System;

namespace CAAMSAirlineWebApp.Models
{
    public class FlightNotification
    {
        public int NotificationId { get; set; }
        public int CustomerId { get; set; }
        public int FlightId { get; set; }
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

        public Customer Customer { get; set; } = null!;
    }
}
