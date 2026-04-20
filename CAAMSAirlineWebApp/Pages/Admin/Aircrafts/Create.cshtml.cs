using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Pages.Admin.Aircrafts
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AircraftInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

          

            var aircraft = new Aircraft
            {
                Model = Input.Model.Trim(),
                Manufacturer = string.IsNullOrWhiteSpace(Input.Manufacturer)
          ? null : Input.Manufacturer.Trim(),
                Capacity = Input.Capacity
            };

            _context.Aircrafts.Add(aircraft);
            await _context.SaveChangesAsync();

            // Generate seats
            var seatLetters = new[] { "A", "B", "C", "D", "E", "F" };
            int businessRows = Math.Max(1, Input.Capacity / 10 / 6); // ~10% business
            int totalRows = (int)Math.Ceiling(Input.Capacity / 6.0);

            for (int row = 1; row <= totalRows; row++)
            {
                string seatClass = row <= businessRows ? "Business" : "Economy";
                foreach (var letter in seatLetters)
                {
                    _context.Seats.Add(new Seat
                    {
                        AircraftId = aircraft.AircraftId,
                        SeatNumber = $"{row}{letter}",
                        SeatClass = seatClass
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Aircrafts/Index", new { success = "created" });
        }
    }

    public class AircraftInput
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Aircraft Model")]
        public string Model { get; set; } = "";

        [StringLength(50)]
        [Display(Name = "Manufacturer")]
        public string? Manufacturer { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
        [Display(Name = "Seat Capacity")]
        public int Capacity { get; set; }
    }
}
