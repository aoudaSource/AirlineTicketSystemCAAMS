using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
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

        // Passed to Chart.js as JSON
        public string HourLabelsJson { get; set; } = "[]";
        public string RevenueDataJson { get; set; } = "[]";
        public string TicketCountDataJson { get; set; } = "[]";

        public decimal TotalRevenue { get; set; }
        public int TotalTickets { get; set; }
        public int PeakHour { get; set; }
        public decimal PeakRevenue { get; set; }

        public async Task OnGetAsync()
        {
            var since = DateTime.Now.AddHours(-24);

            var bookings = await _context.Bookings
                .Where(b => b.BookingDate >= since)
                .Select(b => new { b.BookingDate, b.TotalPrice })
                .ToListAsync();

            var tickets = await _context.Tickets
                .Include(t => t.Booking)
                .Where(t => t.Booking.BookingDate >= since)
                .Select(t => new { t.Booking.BookingDate })
                .ToListAsync();

            // Build 24 hourly buckets starting from the oldest complete hour
            var buckets = new decimal[24];
            var ticketBuckets = new int[24];
            var labels = new string[24];

            for (int i = 0; i < 24; i++)
            {
                var bucketTime = DateTime.Now.AddHours(-(23 - i));
                labels[i] = bucketTime.ToString("HH:00");
            }

            foreach (var b in bookings)
            {
                double hoursAgo = (DateTime.Now - b.BookingDate).TotalHours;
                if (hoursAgo >= 0 && hoursAgo < 24)
                {
                    int bucket = 23 - (int)hoursAgo;
                    buckets[bucket] += b.TotalPrice;
                }
            }

            foreach (var t in tickets)
            {
                double hoursAgo = (DateTime.Now - t.BookingDate).TotalHours;
                if (hoursAgo >= 0 && hoursAgo < 24)
                {
                    int bucket = 23 - (int)hoursAgo;
                    ticketBuckets[bucket]++;
                }
            }

            TotalRevenue = buckets.Sum();
            TotalTickets = ticketBuckets.Sum();

            int peakIdx = Array.IndexOf(buckets, buckets.Max());
            PeakHour = int.Parse(labels[peakIdx].Replace(":00", ""));
            PeakRevenue = buckets[peakIdx];

            HourLabelsJson = JsonSerializer.Serialize(labels);
            RevenueDataJson = JsonSerializer.Serialize(buckets.Select(v => Math.Round(v, 2)));
            TicketCountDataJson = JsonSerializer.Serialize(ticketBuckets);
        }
    }
}
