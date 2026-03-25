using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages.Admin.Reports
{
    [Authorize(Roles = "Admin,Staff")]
    public class MostBookedFlightsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MostBookedFlightsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<DestinationRow> Destinations { get; set; } = new();

        public async Task OnGetAsync()
        {
            var legs = await _context.FlightLegs
                .Include(fl => fl.ArrivalAirportNavigation)
                .Include(fl => fl.Tickets)
                .ToListAsync();

            Destinations = legs
                .GroupBy(fl => fl.ArrivalAirport)
                .Select((g, i) =>
                {
                    var first = g.First();
                    int ticketsSold = g.Sum(fl => fl.Tickets.Count);
                    decimal revenue = g.Sum(fl => fl.Tickets.Sum(t => t.Price));

                    return new DestinationRow
                    {
                        AirportCode = g.Key,
                        City = first.ArrivalAirportNavigation?.City ?? "",
                        Country = first.ArrivalAirportNavigation?.Country ?? "",
                        TotalLegs = g.Count(),
                        TicketsSold = ticketsSold,
                        TotalRevenue = revenue
                    };
                })
                .OrderByDescending(d => d.TicketsSold)
                .Select((d, i) => { d.Rank = i + 1; return d; })
                .ToList();
        }
    }

    public class DestinationRow
    {
        public int Rank { get; set; }
        public string AirportCode { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public int TotalLegs { get; set; }
        public int TicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
