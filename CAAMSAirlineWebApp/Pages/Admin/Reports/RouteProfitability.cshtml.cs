using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

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

        [BindProperty(SupportsGet = true)]
        public string Origin { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Destination { get; set; }

        [BindProperty(SupportsGet = true)]
        public string AircraftModel { get; set; }

        public SelectList Airports { get; set; }
        public List<string> AircraftModels { get; set; }

        public string TopRoute { get; set; } = "—";
        public decimal TopRouteProfit { get; set; }
        public double AverageMargin { get; set; }
        public string UnderperformingRoute { get; set; } = "—";
        public decimal LowestProfit { get; set; }
        public decimal TotalSystemNetProfit { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            await LoadDataAsync();

            if (!Routes.Any()) return Page();

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Origin,Destination,Flights,Load Factor %,Revenue (USD),Cost (USD),Net Profit (USD)");

            foreach (var r in Routes)
            {
                csvBuilder.AppendLine($"{r.DepartureCode},{r.ArrivalCode},{r.TotalLegs},{r.LoadFactor:F1},{r.TotalRevenue:F2},{r.TotalCost:F2},{r.NetProfit:F2}");
            }

            var fileName = $"RouteProfitability_{DateTime.Now:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), "text/csv", fileName);
        }

        private async Task LoadDataAsync()
        {
            // 1. Populate Dropdowns (Always needed for the page)
            var airportList = await _context.Airports
                .Select(a => new { a.AirportCode, Display = $"{a.AirportCode} - {a.City}" })
                .OrderBy(a => a.AirportCode)
                .ToListAsync();
            Airports = new SelectList(airportList, "AirportCode", "Display");

            AircraftModels = await _context.Aircrafts
                .Select(a => a.Model)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            // 2. Build Base Query
            IQueryable<CAAMSAirlineWebApp.Models.FlightLeg> query = _context.FlightLegs
                .Include(fl => fl.DepartureAirportNavigation)
                .Include(fl => fl.ArrivalAirportNavigation)
                .Include(fl => fl.Flight).ThenInclude(f => f.Aircraft)
                .Include(fl => fl.Tickets);

            // 3. Apply Filters
            if (!string.IsNullOrEmpty(Origin))
                query = query.Where(fl => fl.DepartureAirport == Origin);

            if (!string.IsNullOrEmpty(Destination))
                query = query.Where(fl => fl.ArrivalAirport == Destination);

            if (!string.IsNullOrEmpty(AircraftModel))
                query = query.Where(fl => fl.Flight.Aircraft.Model == AircraftModel);

            var legs = await query.ToListAsync();

            // 4. Group and Calculate
            Routes = legs
                .GroupBy(fl => new { fl.DepartureAirport, fl.ArrivalAirport })
                .Select(g =>
                {
                    var first = g.First();
                    int totalSeats = g.Sum(fl => fl.Flight.Aircraft.Capacity);
                    int ticketsSold = g.Sum(fl => fl.Tickets.Count);
                    decimal revenue = g.Sum(fl => fl.Tickets.Sum(t => t.Price));
                    decimal totalCost = g.Count() * 2000; // Your placeholder cost

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
                        TotalRevenue = revenue,
                        TotalCost = totalCost
                    };
                })
                .OrderByDescending(r => r.NetProfit)
                .ToList();

            // 5. Update Summary Metrics
            if (Routes.Any())
            {
                var top = Routes.First();
                TopRoute = $"{top.DepartureCode} → {top.ArrivalCode}";
                TopRouteProfit = top.NetProfit;

                var bottom = Routes.Last();
                UnderperformingRoute = $"{bottom.DepartureCode} → {bottom.ArrivalCode}";
                LowestProfit = bottom.NetProfit;

                TotalSystemNetProfit = Routes.Sum(r => r.NetProfit);
                decimal totalRevenue = Routes.Sum(r => r.TotalRevenue);
                AverageMargin = totalRevenue > 0
                    ? (double)(TotalSystemNetProfit / totalRevenue) * 100
                    : 0;
            }
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
        public decimal TotalCost { get; set; }

        public decimal NetProfit => TotalRevenue - TotalCost;
        public string LoadFactorBadge => LoadFactor >= 70 ? "success" : LoadFactor >= 40 ? "warning" : "danger";
    }
}