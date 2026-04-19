using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages
{
    [Authorize(Roles = "Customer")]
    public class BookingConfirmationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BookingConfirmationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Booking Booking { get; set; } = null!;
        public Booking? ReturnBooking { get; set; }



        public async Task<IActionResult> OnGetAsync(int bookingId, int? returnBookingId = null)
        {
            var username = User.Identity?.Name;

            var booking = await LoadBookingAsync(bookingId);

            if (booking == null)
                return NotFound();

            if (booking.Customer.Username != username)
                return Forbid();

            Booking = booking;

            if (returnBookingId.HasValue)
            {
                var returnBooking = await LoadBookingAsync(returnBookingId.Value);
                if (returnBooking != null && returnBooking.Customer.Username == username)
                    ReturnBooking = returnBooking;
            }

            return Page();
        }                                    
private async Task<Booking?> LoadBookingAsync(int id)
        {
            return await _context.Bookings
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
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }
    }
    

} 


    