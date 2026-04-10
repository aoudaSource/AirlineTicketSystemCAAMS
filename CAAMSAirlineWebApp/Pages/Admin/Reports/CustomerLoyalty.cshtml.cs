using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages.Admin.Reports
{
    [Authorize(Roles = "Admin,Staff")]
    public class CustomerLoyaltyModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CustomerLoyaltyModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CustomerSpendRow> TopCustomers { get; set; } = new();
        public int TotalCustomers { get; set; }
        public int TopTenPercentCount { get; set; }
        public decimal TopTenPercentRevenue { get; set; }

        public async Task OnGetAsync()
        {
            var allCustomers = await _context.Customers
                .Include(c => c.Bookings)
                .Select(c => new CustomerSpendRow
                {
                    CustomerId = c.CustomerId,
                    FullName = c.FirstName + " " + c.LastName,
                    Email = c.Email,
                    TotalBookings = c.Bookings.Count(),
                    TotalSpent = c.Bookings.Sum(b => (decimal?)b.TotalPrice) ?? 0
                })
                .OrderByDescending(c => c.TotalSpent)
                .ToListAsync();

            TotalCustomers = allCustomers.Count;
            TopTenPercentCount = Math.Max(1, (int)Math.Ceiling(TotalCustomers * 0.1));

            // Assign rank after ordering
            for (int i = 0; i < allCustomers.Count; i++)
                allCustomers[i].Rank = i + 1;

            TopCustomers = allCustomers.Take(TopTenPercentCount).ToList();
            TopTenPercentRevenue = TopCustomers.Sum(c => c.TotalSpent);
        }
    }

    public class CustomerSpendRow
    {
        public int CustomerId { get; set; }
        public int Rank { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
