namespace CAAMSAirlineWebApp.Models
{
    public class FlightStaff
    {
        public int FlightStaffId { get; set; }
        public int FlightId { get; set; }
        public int StaffId { get; set; }

        // Navigation properties
        public Flight Flight { get; set; } = null!;
        public Staff Staff { get; set; } = null!;
    }
}