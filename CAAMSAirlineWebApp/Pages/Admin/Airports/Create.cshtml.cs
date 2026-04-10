using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CAAMSAirlineWebApp.Pages.Admin.Airports
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
        public AirportInput Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var code = Input.AirportCode.Trim().ToUpper();

            if (await _context.Airports.FindAsync(code) != null)
            {
                ModelState.AddModelError("Input.AirportCode", $"Airport code '{code}' already exists.");
                return Page();
            }

            _context.Airports.Add(new Airport
            {
                AirportCode = code,
                AirportName = Input.AirportName.Trim(),
                City = Input.City.Trim(),
                State = string.IsNullOrWhiteSpace(Input.State) ? null : Input.State.Trim(),
                Country = Input.Country.Trim()
            });

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Airports/Index", new { success = "created" });
        }
    }

    public class AirportInput
    {
        [Required]
        [RegularExpression(@"^[A-Za-z]{3}$", ErrorMessage = "Airport code must be exactly 3 letters (e.g. KUL).")]
        [Display(Name = "IATA Code")]
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
