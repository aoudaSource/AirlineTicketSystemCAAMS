using System.Collections.Generic;

namespace CAAMSAirlineWebApp.Models
{
    public class Staff
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Username { get; set; } = null!;

        // Navigation properties
        public AppUser AppUser { get; set; } = null!;
        public ICollection<FlightStaff> FlightStaffs { get; set; } = new List<FlightStaff>();
    }
}