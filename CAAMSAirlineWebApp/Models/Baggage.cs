using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace CAAMSAirlineWebApp.Models
{
    public class Baggage
    {
        [Key] // Primary key
        public int BaggageId { get; set; }

        [Required]
        public int TicketId { get; set; } // Foreign key to Ticket

        [ForeignKey("TicketId")]
        public required Ticket Ticket { get; set; } // Navigation property

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; } // Optional

        [MaxLength(50)]
        public required string Status { get; set; } // e.g., Checked-in, In Transit, Delivered
    }
}
