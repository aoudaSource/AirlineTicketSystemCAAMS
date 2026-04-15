using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CAAMSAirlineWebApp.Pages.Admin.Reports
{
    [Authorize(Roles = "Admin,Staff")]
    public class CustomerLoyaltyModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CustomerLoyaltyModel(ApplicationDbContext context) { _context = context; }

        // Added SupportsGet = true AND keeping it as a BindProperty for POST
        [BindProperty(SupportsGet = true)] public string Tier { get; set; }
        [BindProperty(SupportsGet = true)] public int? MinBookings { get; set; }

        public List<CustomerSpendRow> FilteredCustomers { get; set; } = new();
        public decimal FilteredGroupRevenue { get; set; }
        public string HighestValueCustomer { get; set; } = "—";
        public decimal SpendThreshold { get; set; }

        public async Task OnGetAsync()
        {
            await LoadReportData();
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            // Re-run the data loading with the current filter properties
            await LoadReportData();

            if (FilteredCustomers == null || !FilteredCustomers.Any())
            {
                return RedirectToPage(); // Or return Page() with an error
            }

            var csvBuilder = new StringBuilder();
            // Header Row
            csvBuilder.AppendLine("Rank,Customer Name,Email,Total Bookings,Total Spent (MYR)");

            foreach (var c in FilteredCustomers)
            {
                // Format: Wrap in quotes to handle names with commas
                csvBuilder.AppendLine($"{c.Rank},\"{c.FullName}\",\"{c.Email}\",{c.TotalBookings},{c.TotalSpent:F2}");
            }

            var fileName = $"CustomerLoyaltyExport_{DateTime.Now:yyyyMMddHHmm}.csv";
            return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), "text/csv", fileName);
        }

        private async Task LoadReportData()
        {
            var allCustomers = await _context.Customers
                .Select(c => new CustomerSpendRow
                {
                    CustomerId = c.CustomerId,
                    FullName = c.FirstName + " " + c.LastName,
                    Email = c.Email,
                    TotalBookings = c.Bookings.Count(),
                    TotalSpent = c.Bookings.Sum(b => (decimal?)b.TotalPrice) ?? 0
                }).OrderByDescending(c => c.TotalSpent).ToListAsync();

            if (allCustomers.Any())
            {
                int topTenCount = Math.Max(1, (int)Math.Ceiling(allCustomers.Count * 0.1));
                SpendThreshold = allCustomers.Take(topTenCount).Last().TotalSpent;

                for (int i = 0; i < allCustomers.Count; i++) allCustomers[i].Rank = i + 1;

                var query = allCustomers.AsEnumerable();

                if (Tier == "HighValue")
                    query = query.Where(c => c.TotalSpent >= SpendThreshold);
                else if (Tier == "NewCustomers")
                    query = query.Where(c => c.TotalBookings == 1);
                else if (Tier == "MediumValue")
                    query = query.Where(c => c.TotalBookings > 1 && c.TotalSpent < SpendThreshold);

                if (MinBookings.HasValue)
                    query = query.Where(c => c.TotalBookings >= MinBookings.Value);

                FilteredCustomers = query.ToList();

                if (FilteredCustomers.Any())
                {
                    var top = FilteredCustomers.OrderByDescending(c => c.TotalSpent).First();
                    HighestValueCustomer = $"{top.FullName} (#{top.CustomerId})";
                    FilteredGroupRevenue = FilteredCustomers.Sum(c => c.TotalSpent);
                }
            }
        }
    }

    public class CustomerSpendRow
    {
        public int CustomerId { get; set; }
        public int Rank { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
