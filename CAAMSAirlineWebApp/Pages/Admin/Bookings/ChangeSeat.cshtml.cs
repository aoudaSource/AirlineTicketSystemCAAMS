using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages.Admin.Bookings
{
    [Authorize(Roles = "Admin,Staff")]
    public class ChangeSeatModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ChangeSeatModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ticket context shown on the page
        public int TicketId { get; set; }
        public int BookingId { get; set; }
        public string PassengerName { get; set; } = "";
        public string FlightInfo { get; set; } = "";
        public string CurrentSeat { get; set; } = "Unassigned";
        public string TicketClass { get; set; } = "";

        public SelectList AvailableSeats { get; set; } = null!;

        [BindProperty]
        public int SelectedSeatId { get; set; }

        [BindProperty]
        public int HiddenTicketId { get; set; }

        [BindProperty]
        public int HiddenBookingId { get; set; }

        public async Task<IActionResult> OnGetAsync(int ticketId, int bookingId)
        {
            var ticket = await LoadTicketAsync(ticketId);
            if (ticket == null) return NotFound();

            BookingId = bookingId;
            await PopulatePageAsync(ticket);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var ticket = await LoadTicketAsync(HiddenTicketId);
            if (ticket == null) return NotFound();

            BookingId = HiddenBookingId;

            // Verify the chosen seat is still available on this leg
            bool alreadyTaken = await _context.Tickets.AnyAsync(t =>
                t.LegId == ticket.LegId &&
                t.SeatId == SelectedSeatId &&
                t.TicketId != ticket.TicketId);

            if (alreadyTaken)
            {
                ModelState.AddModelError(string.Empty, "That seat was just taken. Please choose another.");
                await PopulatePageAsync(ticket);
                return Page();
            }

            ticket.SeatId = SelectedSeatId;
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Bookings/Details", new { bookingId = HiddenBookingId });
        }

        private async Task<Ticket?> LoadTicketAsync(int ticketId)
        {
            return await _context.Tickets
                .Include(t => t.Passenger)
                .Include(t => t.Seat)
                .Include(t => t.FlightLeg)
                    .ThenInclude(fl => fl.Flight)
                .Include(t => t.FlightLeg)
                    .ThenInclude(fl => fl.DepartureAirportNavigation)
                .Include(t => t.FlightLeg)
                    .ThenInclude(fl => fl.ArrivalAirportNavigation)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }

        private async Task PopulatePageAsync(Ticket ticket)
        {
            TicketId = ticket.TicketId;
            HiddenTicketId = ticket.TicketId;
            HiddenBookingId = BookingId;
            PassengerName = $"{ticket.Passenger.FirstName} {ticket.Passenger.LastName}";
            TicketClass = ticket.TicketClass;
            CurrentSeat = ticket.Seat?.SeatNumber ?? "Unassigned";
            FlightInfo = $"{ticket.FlightLeg.Flight.FlightNumber}  " +
                         $"{ticket.FlightLeg.DepartureAirport} → {ticket.FlightLeg.ArrivalAirport}  " +
                         $"{ticket.FlightLeg.DepartureTime:dd MMM yyyy HH:mm}";

            // Seats taken on this leg by OTHER tickets
            var takenSeatIds = await _context.Tickets
                .Where(t => t.LegId == ticket.LegId &&
                            t.SeatId != null &&
                            t.TicketId != ticket.TicketId)
                .Select(t => t.SeatId!.Value)
                .ToListAsync();

            var aircraftId = ticket.FlightLeg.Flight.AircraftId;

            var available = await _context.Seats
                .Where(s => s.AircraftId == aircraftId &&
                            s.SeatClass == ticket.TicketClass &&
                            !takenSeatIds.Contains(s.SeatId))
                .OrderBy(s => s.SeatNumber)
                .Select(s => new { s.SeatId, Display = s.SeatNumber })
                .ToListAsync();

            AvailableSeats = new SelectList(available, "SeatId", "Display",
                ticket.SeatId); // pre-select current seat
        }
    }
}
