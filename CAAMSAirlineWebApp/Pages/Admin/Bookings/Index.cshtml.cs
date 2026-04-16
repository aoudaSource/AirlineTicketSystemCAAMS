using CAAMSAirlineWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages.Admin.Bookings
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        [ValidateNever]
        public string? Query { get; set; }

        public List<BookingRow> Bookings { get; set; } = new();

        public async Task OnGetAsync()
        {
            var q = Query?.Trim().ToLower();

            var bookingsQuery = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Passengers)
                .OrderByDescending(b => b.BookingDate);

            var results = await bookingsQuery.ToListAsync();

            // Apply client-side filtering
            if (!string.IsNullOrEmpty(q))
            {
                results = results.Where(b =>
                    (q.StartsWith("#") && b.BookingId.ToString().Equals(q.Substring(1))) ||
                    (b.Customer.FirstName + " " + b.Customer.LastName).ToLower().Contains(q) ||
                    b.BookingDate.Day.ToString().Equals(q) ||
                    b.BookingDate.ToString("MMM").ToLower().Equals(q) ||
                    b.BookingDate.Year.ToString().Equals(q)).ToList();
            }

            Bookings = results.Select(b => new BookingRow
            {
                BookingId = b.BookingId,
                CustomerName = b.Customer.FirstName + " " + b.Customer.LastName,
                BookingDate = b.BookingDate,
                TotalPrice = b.TotalPrice,
                PassengerCount = b.Passengers.Count
            }).ToList();
        }
    }

    public class BookingRow
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int PassengerCount { get; set; }
    }
}
