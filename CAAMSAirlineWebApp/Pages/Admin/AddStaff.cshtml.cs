using CAAMSAirlineWebApp.DTOs; // Important for RegisterRequest
using CAAMSAirlineWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAAMSAirlineWebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AddStaffModel : PageModel
    {
        private readonly AuthService _authService;

        public AddStaffModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterRequest Input { get; set; } = new RegisterRequest
        {
            DOB = new DateTime(2000, 1, 1)
        };

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Tell the system to IGNORE the missing email for Staff
            ModelState.Remove("Input.Email");

            // Standard removals
            ModelState.Remove("Input.Customer");
            ModelState.Remove("Input.Staff");

            if (!ModelState.IsValid)
            {
                // This will now pass because we removed the Email requirement
                return Page();
            }

            // 2. We can provide a placeholder email or leave it empty 
            // since the Staff table doesn't actually have an Email column!
            Input.Email = $"{Input.Username}@caamsairline.com";

            var (user, error) = await _authService.RegisterStaffAsync(Input);

            if (user == null)
            {
                ErrorMessage = error ?? "Registration failed.";
                return Page();
            }

            return RedirectToPage("/Admin/Index");
        }
    }
}

