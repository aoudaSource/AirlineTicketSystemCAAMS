using Microsoft.AspNetCore.Mvc.RazorPages;
using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CAAMSAirlineWebApp.Pages
{
    [Authorize]
    public class NotificationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public NotificationsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<FlightNotification> Notifications { get; set; } = new();

        public async Task OnGetAsync()
        {
            var username = User.Identity!.Name;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null) return;

            Notifications = await _context.FlightNotifications
                .Where(n => n.CustomerId == customer.CustomerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            foreach (var n in Notifications.Where(n => !n.IsRead))
                n.IsRead = true;

            await _context.SaveChangesAsync();
        }
    }
}