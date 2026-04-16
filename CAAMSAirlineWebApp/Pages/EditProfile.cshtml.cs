using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages
{
    [Authorize(Roles = "Customer")]
    public class EditProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Customer CustomerInfo { get; set; } = default!;

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToPage("/Login");

            // Load the customer from the database
            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Username == username);

            if (customer == null) return NotFound();

            CustomerInfo = customer;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("CustomerInfo.AppUser");
            ModelState.Remove("CustomerInfo.Bookings");

            if (!ModelState.IsValid) return Page();


            var customerToUpdate = await _context.Customers
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(c => c.CustomerId == CustomerInfo.CustomerId);

            if (customerToUpdate == null) return NotFound();


            customerToUpdate.FirstName = CustomerInfo.FirstName;
            customerToUpdate.LastName = CustomerInfo.LastName;
            customerToUpdate.Email = CustomerInfo.Email;
            customerToUpdate.Phone = CustomerInfo.Phone;
            customerToUpdate.DOB = CustomerInfo.DOB;


            if (customerToUpdate.AppUser != null)
            {
                customerToUpdate.AppUser.FirstName = CustomerInfo.FirstName;
                customerToUpdate.AppUser.LastName = CustomerInfo.LastName;
                customerToUpdate.AppUser.DOB = CustomerInfo.DOB;

            }

            try
            {

                await _context.SaveChangesAsync();
                SuccessMessage = "Profile updated successfully!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Database error: " + ex.Message);
                return Page();
            }
        }
    }
}