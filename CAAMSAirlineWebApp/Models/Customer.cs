using System;
using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public DateTime DOB { get; set; }
        public string? LoyaltyStatus { get; set; }

        // Navigation properties
        public AppUser AppUser { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}