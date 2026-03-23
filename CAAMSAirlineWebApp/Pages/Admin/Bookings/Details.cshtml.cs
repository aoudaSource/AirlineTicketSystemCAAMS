using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages.Admin.Bookings
{
    [Authorize(Roles = "Admin,Staff")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Booking Booking { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Passengers)
                    .ThenInclude(p => p.Tickets)
                        .ThenInclude(t => t.FlightLeg)
                            .ThenInclude(fl => fl.Flight)
                .Include(b => b.Passengers)
                    .ThenInclude(p => p.Tickets)
                        .ThenInclude(t => t.FlightLeg)
                            .ThenInclude(fl => fl.DepartureAirportNavigation)
                .Include(b => b.Passengers)
                    .ThenInclude(p => p.Tickets)
                        .ThenInclude(t => t.FlightLeg)
                            .ThenInclude(fl => fl.ArrivalAirportNavigation)
                .Include(b => b.Passengers)
                    .ThenInclude(p => p.Tickets)
                        .ThenInclude(t => t.Seat)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return NotFound();

            Booking = booking;
            return Page();
        }
    }
}
