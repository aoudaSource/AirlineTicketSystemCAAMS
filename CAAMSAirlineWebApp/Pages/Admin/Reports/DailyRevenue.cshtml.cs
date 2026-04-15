using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CAAMSAirlineWebApp.Pages.Admin.Reports
{
    [Authorize(Roles = "Admin,Staff")]
    public class DailyRevenueModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DailyRevenueModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string DatePreset { get; set; } = "30";
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public string PaymentMethod { get; set; }

        public string DayLabelsJson { get; set; } = "[]";
        public string RevenueDataJson { get; set; } = "[]";
        public decimal TotalRevenueYear { get; set; }
        public double RevenueChangePercentage { get; set; }
        public bool IsRevenueUp { get; set; }
        public decimal AverageTicketPrice { get; set; }
        public string TopRevenueDay { get; set; } = "—";
        public List<RevenueRow> DailyBreakdown { get; set; } = new();

        public async Task OnGetAsync()
        {
            var now = DateTime.Now;
            DateTime finalStart;
            DateTime finalEnd = EndDate ?? now;

            if (StartDate.HasValue)
            {
                finalStart = StartDate.Value;
            }
            else
            {
                finalStart = DatePreset switch
                {
                    "7" => now.AddDays(-7),
                    "90" => now.AddDays(-90),
                    "YTD" => new DateTime(now.Year, 1, 1),
                    _ => now.AddDays(-30)
                };
            }

            var baseQuery = from b in _context.Bookings
                            join p in _context.Payments on b.BookingId equals p.BookingId
                            select new { b, p };

            var currentQuery = baseQuery.Where(x => x.b.BookingDate >= finalStart && x.b.BookingDate <= finalEnd);
            if (!string.IsNullOrEmpty(PaymentMethod))
                currentQuery = currentQuery.Where(x => x.p.PaymentMethod == PaymentMethod);

            TimeSpan duration = finalEnd - finalStart;
            DateTime prevStart = finalStart.Subtract(duration);
            DateTime prevEnd = finalStart.AddSeconds(-1);

            var prevQuery = baseQuery.Where(x => x.b.BookingDate >= prevStart && x.b.BookingDate <= prevEnd);
            if (!string.IsNullOrEmpty(PaymentMethod))
                prevQuery = prevQuery.Where(x => x.p.PaymentMethod == PaymentMethod);

            var dailyData = await currentQuery
                .GroupBy(x => x.b.BookingDate.Date)
                .Select(g => new RevenueRow
                {
                    DateValue = g.Key,
                    Revenue = g.Sum(x => x.p.Amount),
                    TicketsSold = g.SelectMany(x => x.b.Tickets).Count()
                })
                .OrderBy(r => r.DateValue)
                .ToListAsync();

            decimal currentTotal = dailyData.Sum(d => d.Revenue);
            decimal previousTotal = await prevQuery.SumAsync(x => (decimal?)x.p.Amount) ?? 0;

            if (previousTotal > 0)
            {
                RevenueChangePercentage = (double)((currentTotal - previousTotal) / previousTotal) * 100;
                IsRevenueUp = RevenueChangePercentage >= 0;
            }
            else
            {
                RevenueChangePercentage = currentTotal > 0 ? 100 : 0;
                IsRevenueUp = true;
            }

            TotalRevenueYear = await _context.Bookings
                .Where(b => b.BookingDate.Year == now.Year)
                .SumAsync(b => b.TotalPrice);

            if (dailyData.Any())
            {
                var labels = dailyData.Select(d => d.DateValue.ToString("dd MMM")).ToList();
                var revenues = dailyData.Select(d => d.Revenue).ToList();
                DayLabelsJson = JsonSerializer.Serialize(labels);
                RevenueDataJson = JsonSerializer.Serialize(revenues.Select(v => Math.Round(v, 2)));
                DailyBreakdown = dailyData.OrderByDescending(d => d.DateValue).ToList();
                var topDay = dailyData.OrderByDescending(d => d.Revenue).First();
                TopRevenueDay = topDay.DateValue.ToString("dd MMM yyyy");
                var totalPeriodTickets = dailyData.Sum(d => d.TicketsSold);
                AverageTicketPrice = totalPeriodTickets > 0 ? currentTotal / totalPeriodTickets : 0;
            }
        }
    }

    public class RevenueRow
    {
        public DateTime DateValue { get; set; }
        public string Date => DateValue.ToString("dd MMM yyyy");
        public decimal Revenue { get; set; }
        public int TicketsSold { get; set; }
    }
}
