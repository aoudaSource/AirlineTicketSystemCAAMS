using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Pages.Admin.Aircrafts
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AircraftEditInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int aircraftId)
        {
            var aircraft = await _context.Aircrafts.FindAsync(aircraftId);
            if (aircraft == null) return NotFound();

            Input = new AircraftEditInput 

            {
                MaintenanceStatus = aircraft.MaintenanceStatus,
                AircraftId = aircraft.AircraftId,
                Model = aircraft.Model,
                Manufacturer = aircraft.Manufacturer,
                Capacity = aircraft.Capacity
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var aircraft = await _context.Aircrafts.FindAsync(Input.AircraftId);
            if (aircraft == null) return NotFound();

            aircraft.Model = Input.Model.Trim();
            aircraft.Manufacturer = string.IsNullOrWhiteSpace(Input.Manufacturer)
                ? null : Input.Manufacturer.Trim();
            aircraft.Capacity = Input.Capacity;
            aircraft.MaintenanceStatus = Input.MaintenanceStatus;
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Aircrafts/Index", new { success = "updated" });
        }
    }

    public class AircraftEditInput
    {
        public int AircraftId { get; set; }

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
        [Required]
        [Display(Name = "Maintenance Status")]
        public string MaintenanceStatus { get; set; } = "Active";
    }
}
