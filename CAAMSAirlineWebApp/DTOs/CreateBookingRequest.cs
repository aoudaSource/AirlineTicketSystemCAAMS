using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.DTOs
{
    public class CreateBookingRequest
    {
        public int CustomerId { get; set; }

        public int FlightId { get; set; }

        [Required(ErrorMessage = "Please select a ticket class.")]
        public string TicketClass { get; set; } = string.Empty;
        public int PassengerCount { get; set; }

        public List<PassengerInput> Passengers { get; set; } = new();
    }

    public class PassengerInput
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public string PassportNumber { get; set; } = null!;

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime? DOB { get; set; }
    }
}