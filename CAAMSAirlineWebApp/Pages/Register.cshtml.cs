using System.Security.Claims;
using CAAMSAirlineWebApp.DTOs;
using CAAMSAirlineWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CAAMSAirlineWebApp.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AuthService _authService;

        public RegisterModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        /*public RegisterRequest Input { get; set; } = new(); */

        public RegisterRequest Input { get; set; } = new RegisterRequest
        {
            // This sets the default value when the page first loads
            DOB = new DateTime(2000, 1, 1)       /*could also do DateTime.Today*/
        };

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var (user, error) = await _authService.RegisterCustomerAsync(Input);

            if (user == null)
            {
                ErrorMessage = error ?? "Registration failed.";
                return Page();
            }

            // Auto-login logic preserved from your teammate's code
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to the page they specified
            return RedirectToPage("/BookFlight");
        }
    }
}
