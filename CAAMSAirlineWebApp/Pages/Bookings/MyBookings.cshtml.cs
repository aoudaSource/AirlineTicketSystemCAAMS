using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CAAMSAirlineWebApp.Pages.Bookings
{
    [Authorize]
    public class MyBookingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MyBookingsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Booking> UpcomingBookings { get; set; } = new();
        public List<Booking> PastBookings { get; set; } = new();

        public async Task OnGetAsync()
        {
            var username = User.Identity!.Name;

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null) return;

            var allBookings = await _context.Bookings
                .Where(b => b.CustomerId == customer.CustomerId)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.FlightLeg)
                        .ThenInclude(fl => fl.Flight)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.FlightLeg)
                        .ThenInclude(fl => fl.DepartureAirportNavigation)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.FlightLeg)
                        .ThenInclude(fl => fl.ArrivalAirportNavigation)
                .Include(b => b.Passengers)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var now = DateTime.Now;

            UpcomingBookings = allBookings
                .Where(b => b.Tickets.Any(t => t.FlightLeg.DepartureTime > now))
                .ToList();

            PastBookings = allBookings
                .Where(b => b.Tickets.All(t => t.FlightLeg.DepartureTime <= now))
                .ToList();
        }

        public async Task<IActionResult> OnPostCancelAsync(int bookingId)
        {
            var username = User.Identity!.Name;

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null) return Forbid();

            var booking = await _context.Bookings
                .Include(b => b.Tickets)
                .Include(b => b.Passengers)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.CustomerId == customer.CustomerId);

            if (booking == null) return NotFound();

            //deleteeeeee
            foreach (var ticket in booking.Tickets)
                _context.Baggages.RemoveRange(ticket.Baggages);

            _context.Tickets.RemoveRange(booking.Tickets);
            _context.Passengers.RemoveRange(booking.Passengers);
            _context.Payments.RemoveRange(booking.Payments);
            _context.Bookings.Remove(booking);

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}

