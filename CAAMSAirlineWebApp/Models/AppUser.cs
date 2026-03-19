using System;

namespace CAAMSAirlineWebApp.Models
{
    public class AppUser
    {
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DOB { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Customer? Customer { get; set; }
        public Staff? Staff { get; set; }
    }
}