using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.DTOs
{
    public class FlightSearchInput
    {
        [Required(ErrorMessage = "Origin is required.")]
        public string Origin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination is required.")]
        public string Destination { get; set; } = string.Empty;

        public string TripType { get; set; } = "OneWay";

        [DataType(DataType.Date)]
        public DateTime? DepartureDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        public int PassengerCount { get; set; } = 1;
    }
}
