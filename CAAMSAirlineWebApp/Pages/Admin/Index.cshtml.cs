using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages.Admin
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalFlights { get; set; }
        public int TotalAirports { get; set; }
        public int TotalAircrafts { get; set; }
        public int TotalBookings { get; set; }
        public int UpcomingFlights { get; set; }

        public async Task OnGetAsync()
        {
            TotalFlights = await _context.Flights.CountAsync();
            TotalAirports = await _context.Airports.CountAsync();
            TotalAircrafts = await _context.Aircrafts.CountAsync();
            TotalBookings = await _context.Bookings.CountAsync();
            UpcomingFlights = await _context.FlightLegs
                .CountAsync(fl => fl.DepartureTime >= DateTime.Today);
        }
    }
}
