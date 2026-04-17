using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages
{
    [Authorize(Roles = "Customer")]
    public class BookFlightModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BookFlightModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Bound from GET query string — [ValidateNever] prevents it being validated on POSTs
        [BindProperty(SupportsGet = true)]
        [ValidateNever]
        public FlightSearchInput Search { get; set; } = new();

        public List<FlightResultItem> SearchResults { get; set; } = new();

        public string PageStep { get; set; } = "search";
        public string SuccessMessage { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public bool ShowAll { get; set; } = false;

        public async Task OnGetAsync()
        {
            bool hasSearch = !string.IsNullOrWhiteSpace(Search.Origin)
                          && !string.IsNullOrWhiteSpace(Search.Destination)
                          && Search.DepartureDate.HasValue
                          && Search.DepartureDate.Value > DateTime.MinValue;

            if (!hasSearch && !ShowAll)
            {
                PageStep = "search";
                return;
            }

            IQueryable<CAAMSAirlineWebApp.Models.FlightLeg> query = _context.FlightLegs
                .Include(fl => fl.Flight)
                    .ThenInclude(f => f.Aircraft)
                .Include(fl => fl.DepartureAirportNavigation)
                .Include(fl => fl.ArrivalAirportNavigation);

            if (!ShowAll)
            {
                var originLower = Search.Origin.Trim().ToLower();
                var destLower = Search.Destination.Trim().ToLower();
                var depStart = Search.DepartureDate!.Value.Date;
                var depEnd = depStart.AddDays(1);

                query = query.Where(fl =>
                    fl.DepartureTime >= depStart && fl.DepartureTime < depEnd &&
                    (fl.DepartureAirport.ToLower().Contains(originLower) ||
                     fl.DepartureAirportNavigation.City.ToLower().Contains(originLower) ||
                     fl.DepartureAirportNavigation.AirportName.ToLower().Contains(originLower)) &&
                    (fl.ArrivalAirport.ToLower().Contains(destLower) ||
                     fl.ArrivalAirportNavigation.City.ToLower().Contains(destLower) ||
                     fl.ArrivalAirportNavigation.AirportName.ToLower().Contains(destLower)));
            }

            var legs = await query.ToListAsync();

            var legIds = legs.Select(fl => fl.LegId).ToList();
            var bookedCountsPerLeg = await _context.Tickets
                .Where(t => legIds.Contains(t.LegId))
                .GroupBy(t => t.LegId)
                .Select(g => new { LegId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.LegId, x => x.Count);

            SearchResults = legs.Select(fl => new FlightResultItem
            {
                FlightId = fl.FlightId,
                FlightNumber = fl.Flight.FlightNumber,
                DepartureCode = fl.DepartureAirport,
                DepartureCity = fl.DepartureAirportNavigation.City,
                ArrivalCode = fl.ArrivalAirport,
                ArrivalCity = fl.ArrivalAirportNavigation.City,
                DepartureTime = fl.DepartureTime,
                ArrivalTime = fl.ArrivalTime,
                BasePrice = fl.Flight.BasePrice,
                IsFull = bookedCountsPerLeg.TryGetValue(fl.LegId, out var booked)
                         && booked >= fl.Flight.Aircraft.Capacity
            }).ToList();

            PageStep = "results";
        }

    }

    public class FlightResultItem
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = "";
        public string DepartureCode { get; set; } = "";
        public string DepartureCity { get; set; } = "";
        public string ArrivalCode { get; set; } = "";
        public string ArrivalCity { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsFull { get; set; }
    }
}
