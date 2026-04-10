using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Pages.Admin.Airports
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
        public AirportEditInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string airportCode)
        {
            var airport = await _context.Airports.FindAsync(airportCode);
            if (airport == null) return NotFound();

            Input = new AirportEditInput
            {
                AirportCode = airport.AirportCode,
                AirportName = airport.AirportName,
                City = airport.City,
                State = airport.State,
                Country = airport.Country
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var airport = await _context.Airports.FindAsync(Input.AirportCode);
            if (airport == null) return NotFound();

            airport.AirportName = Input.AirportName.Trim();
            airport.City = Input.City.Trim();
            airport.State = string.IsNullOrWhiteSpace(Input.State) ? null : Input.State.Trim();
            airport.Country = Input.Country.Trim();

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Airports/Index", new { success = "updated" });
        }
    }

    public class AirportEditInput
    {
        // Read-only — IATA codes cannot be changed once set
        public string AirportCode { get; set; } = "";

        [Required]
        [StringLength(100)]
        [Display(Name = "Airport Name")]
        public string AirportName { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string City { get; set; } = "";

        [StringLength(50)]
        public string? State { get; set; }

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = "";
    }
}
