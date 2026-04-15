using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages
{
    [Authorize]
    public class HomeModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public HomeModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public int TripCount { get; set; }

        public async Task OnGetAsync()
        {
            var username = User.Identity?.Name;
            if (username != null)
            {
                TripCount = await _db.Bookings
                    .CountAsync(b => b.Customer.Username == username);
            }
        }
    }
}
