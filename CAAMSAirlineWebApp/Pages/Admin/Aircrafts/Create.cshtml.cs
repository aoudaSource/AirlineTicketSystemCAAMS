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

            _context.Aircrafts.Add(new Aircraft
            {
                Model = Input.Model.Trim(),
                Manufacturer = string.IsNullOrWhiteSpace(Input.Manufacturer)
                    ? null : Input.Manufacturer.Trim(),
                Capacity = Input.Capacity
            });

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
