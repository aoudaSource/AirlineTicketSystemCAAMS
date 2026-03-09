using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Models
{
    public class Staff
    {
        [Key] // Primary key
        public int StaffId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Role { get; set; }
    }
}
