using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAAMSAirlineWebApp.Models
{
    public class Payment
    {
        [Key] // Primary key
        public int PaymentId { get; set; }

        [Required]
        public int BookingId { get; set; } // Foreign key to Booking

        [ForeignKey("BookingId")]
        public required Booking Booking { get; set; } // Navigation property

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [MaxLength(50)]
        public required string PaymentMethod { get; set; } // e.g., Credit Card, PayPal

        public DateTime? PaymentDate { get; set; } // Optional
    }
}
