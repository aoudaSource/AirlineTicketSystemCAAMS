using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.DTOs;
using CAAMSAirlineWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Services;
public class BookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateBookingAsync(CreateBookingRequest request)
    {
        // 1. Get flight + legs + aircraft seats
        var flight = await _context.Flights
            .Include(f => f.FlightLegs)
            .FirstOrDefaultAsync(f => f.FlightId == request.FlightId);

        if (flight == null)
            throw new Exception("Flight not found");

        // Pre-load all seats for this aircraft grouped by class
        var allSeats = await _context.Seats
            .Where(s => s.AircraftId == flight.AircraftId && s.SeatClass == request.TicketClass)
            .OrderBy(s => s.SeatNumber)
            .ToListAsync();

        // 2. Create booking
        var booking = new Booking
        {
            CustomerId = request.CustomerId,
            BookingDate = DateTime.Now,
            TotalPrice = 0 // will calculate later
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(); // get BookingId

        // 3. Create passengers
        var passengers = new List<Passenger>();

        foreach (var p in request.Passengers)
        {
            var passenger = new Passenger
            {
                BookingId = booking.BookingId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                PassportNumber = p.PassportNumber,
                DOB = p.DOB!.Value
            };

            passengers.Add(passenger);
        }

        _context.Passengers.AddRange(passengers);
        await _context.SaveChangesAsync();

        // 4. Create tickets + assign seats + calculate price
        decimal totalPrice = 0;

        foreach (var leg in flight.FlightLegs)
        {
            // Find seat IDs already taken on this leg
            var takenSeatIds = await _context.Tickets
                .Where(t => t.LegId == leg.LegId && t.SeatId != null)
                .Select(t => t.SeatId!.Value)
                .ToListAsync();

            // Track seats assigned within this booking so far (avoid double-assigning)
            var assignedThisBooking = new HashSet<int>();

            foreach (var passenger in passengers)
            {
                var price = CalculatePrice(flight.BasePrice, request.TicketClass);

                var availableSeat = allSeats.FirstOrDefault(
                    s => !takenSeatIds.Contains(s.SeatId) && !assignedThisBooking.Contains(s.SeatId));

                assignedThisBooking.Add(availableSeat?.SeatId ?? 0);

                _context.Tickets.Add(new Ticket
                {
                    PassengerId = passenger.PassengerId,
                    BookingId = booking.BookingId,
                    LegId = leg.LegId,
                    SeatId = availableSeat?.SeatId,
                    TicketClass = request.TicketClass,
                    Price = price
                });

                totalPrice += price;
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception(ex.InnerException?.Message ?? ex.Message);
        }

        // 5. Update booking total
        booking.TotalPrice = totalPrice;
        await _context.SaveChangesAsync();

        return booking.BookingId;
    }

    private decimal CalculatePrice(decimal basePrice, string ticketClass)
    {
        return ticketClass switch
        {
            "Economy" => basePrice,
            "Business" => basePrice * 1.5m,
            "First" => basePrice * 2m,
            _ => basePrice
        };
    }
}