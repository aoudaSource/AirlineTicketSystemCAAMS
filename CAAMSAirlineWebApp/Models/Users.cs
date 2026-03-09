using System;
using System.ComponentModel.DataAnnotations;
namespace CAAMSAirlineWebApp.Models
{
    public class Users
    {
        [Key] // Primary key
        [Required]
        [MaxLength(50)]
        public required string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public required string PasswordHash { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Role { get; set; }

        [Required]
        [MaxLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string LastName { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
