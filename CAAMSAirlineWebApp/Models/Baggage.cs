namespace CAAMSAirlineWebApp.Models
{
    public class Baggage
    {
        public int BaggageId { get; set; }
        public int TicketId { get; set; }
        public decimal? Weight { get; set; }
        public string Status { get; set; } = null!;

        // Navigation properties
        public Ticket Ticket { get; set; } = null!;
    }
}