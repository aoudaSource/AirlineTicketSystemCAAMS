using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Pages
{
    public class TestBookingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TestBookingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Flight dropdown
        public List<SelectListItem> FlightOptions { get; set; }

        // Seat dropdown
        public List<SelectListItem> SeatOptions { get; set; }

        [BindProperty]
        public int SelectedFlightId { get; set; }

        [BindProperty]
        public int SelectedSeatId { get; set; }

        [BindProperty]
        public int CustomerId { get; set; } // For testing, pick existing customer

        public void OnGet()
        {
            // Populate flight dropdown
            FlightOptions = _context.Flights
                .Select(f => new SelectListItem { Value = f.FlightId.ToString(), Text = f.FlightNumber })
                .ToList();

            if (FlightOptions.Any())
            {
                SelectedFlightId = int.Parse(FlightOptions.First().Value);

                // Populate seats for first flight by default
                SeatOptions = _context.Seats
                    .Where(s => s.FlightId == SelectedFlightId && !s.IsBooked)
                    .Select(s => new SelectListItem { Value = s.SeatId.ToString(), Text = s.SeatNumber })
                    .ToList();
            }
        }

        public IActionResult OnPost()
        {
            // Get the seat
            var seat = _context.Seats.Find(SelectedSeatId);
            if (seat == null || seat.IsBooked)
            {
                ModelState.AddModelError("", "Seat not available.");
                return Page();
            }

            // Ensure the customer exists and set the required navigation property
            var customer = _context.Customers.Find(CustomerId);
            if (customer == null)
            {
                ModelState.AddModelError("", "Customer not found.");
                return Page();
            }

            // Create a new booking and set required Customer navigation property
            var booking = new Booking
            {
                CustomerId = CustomerId,
                Customer = customer,
                BookingDate = DateTime.Now,
                TotalPrice = 300M // test value (decimal)
            };
            _context.Bookings.Add(booking);
            _context.SaveChanges();

            // Retrieve flight leg for the selected flight (keep the entity for required navigation)
            var flightLeg = _context.FlightLegs.First(f => f.FlightId == SelectedFlightId);

            // Create ticket
            var ticket = new Ticket
            {
                // set scalar FKs as before
                BookingId = booking.BookingId,
                SeatId = seat.SeatId,
                LegId = flightLeg.LegId,

                // set required navigation properties to satisfy CS9035
                Booking = booking,
                Seat = seat,
                FlightLeg = flightLeg,

                // required and optional scalar properties
                TicketClass = seat.SeatClass,
                Price = 300M
            };
            _context.Tickets.Add(ticket);

            // Mark seat as booked
            seat.IsBooked = true;
            _context.SaveChanges();

            return RedirectToPage("/Index");
        }
    }
}