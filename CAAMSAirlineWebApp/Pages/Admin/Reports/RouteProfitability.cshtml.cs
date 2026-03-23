using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages.Admin.Reports
{
    [Authorize(Roles = "Admin,Staff")]
    public class RouteProfitabilityModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RouteProfitabilityModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<RouteStats> Routes { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Fetch all legs with their aircraft capacity and sold tickets
            var legs = await _context.FlightLegs
                .Include(fl => fl.DepartureAirportNavigation)
                .Include(fl => fl.ArrivalAirportNavigation)
                .Include(fl => fl.Flight).ThenInclude(f => f.Aircraft)
                .Include(fl => fl.Tickets)
                .ToListAsync();

            Routes = legs
                .GroupBy(fl => new { fl.DepartureAirport, fl.ArrivalAirport })
                .Select(g =>
                {
                    var first = g.First();
                    int totalSeats = g.Sum(fl => fl.Flight.Aircraft.Capacity);
                    int ticketsSold = g.Sum(fl => fl.Tickets.Count);
                    decimal revenue = g.Sum(fl => fl.Tickets.Sum(t => t.Price));
                    double loadFactor = totalSeats > 0
                        ? Math.Round((double)ticketsSold / totalSeats * 100, 1)
                        : 0;

                    return new RouteStats
                    {
                        DepartureCode = g.Key.DepartureAirport,
                        ArrivalCode = g.Key.ArrivalAirport,
                        DepartureCity = first.DepartureAirportNavigation.City,
                        ArrivalCity = first.ArrivalAirportNavigation.City,
                        TotalLegs = g.Count(),
                        TotalSeatsAvailable = totalSeats,
                        TicketsSold = ticketsSold,
                        LoadFactor = loadFactor,
                        TotalRevenue = revenue
                    };
                })
                .OrderByDescending(r => r.LoadFactor)
                .ToList();
        }
    }

    public class RouteStats
    {
        public string DepartureCode { get; set; } = "";
        public string ArrivalCode { get; set; } = "";
        public string DepartureCity { get; set; } = "";
        public string ArrivalCity { get; set; } = "";
        public int TotalLegs { get; set; }
        public int TotalSeatsAvailable { get; set; }
        public int TicketsSold { get; set; }
        public double LoadFactor { get; set; }
        public decimal TotalRevenue { get; set; }

        public string LoadFactorBadge => LoadFactor >= 70 ? "success"
            : LoadFactor >= 40 ? "warning"
            : "danger";
    }
}
