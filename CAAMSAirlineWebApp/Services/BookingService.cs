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
        // 1. Get outbound flight + legs
        var flight = await _context.Flights
            .Include(f => f.FlightLegs)
            .FirstOrDefaultAsync(f => f.FlightId == request.FlightId);

        if (flight == null)
            throw new Exception("Flight not found");

        // 2. Create booking
        var booking = new Booking
        {
            CustomerId = request.CustomerId,
            BookingDate = DateTime.Now,
            TotalPrice = 0
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

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

        // 4. Create tickets for outbound flight
        decimal totalPrice = 0;
        totalPrice += await CreateTicketsForFlight(flight, passengers, request.TicketClass);

        // 5. If round trip, create tickets for return flight too
        if (request.ReturnFlightId.HasValue)
        {
            var returnFlight = await _context.Flights
                .Include(f => f.FlightLegs)
                .FirstOrDefaultAsync(f => f.FlightId == request.ReturnFlightId.Value);

            if (returnFlight == null)
                throw new Exception("Return flight not found");

            totalPrice += await CreateTicketsForFlight(returnFlight, passengers, request.TicketClass);
        }

        await _context.SaveChangesAsync();

        // 6. Update booking total
        booking.TotalPrice = totalPrice;
        await _context.SaveChangesAsync();

        return booking.BookingId;
    }

    private async Task<decimal> CreateTicketsForFlight(Flight flight, List<Passenger> passengers, string ticketClass)
    {
        decimal total = 0;

        var allSeats = await _context.Seats
            .Where(s => s.AircraftId == flight.AircraftId && s.SeatClass == ticketClass)
            .OrderBy(s => s.SeatNumber)
            .ToListAsync();

        foreach (var leg in flight.FlightLegs)
        {
            var takenSeatIds = await _context.Tickets
                .Where(t => t.LegId == leg.LegId && t.SeatId != null)
                .Select(t => t.SeatId!.Value)
                .ToListAsync();

            var assignedThisBooking = new HashSet<int>();

            foreach (var passenger in passengers)
            {
                var price = CalculatePrice(flight.BasePrice, ticketClass);

                var availableSeat = allSeats.FirstOrDefault(
                    s => !takenSeatIds.Contains(s.SeatId) && !assignedThisBooking.Contains(s.SeatId));

                if (availableSeat == null)
                    throw new Exception($"No available {ticketClass} seats on flight {flight.FlightNumber}. Please choose a different class.");

                assignedThisBooking.Add(availableSeat.SeatId);

                _context.Tickets.Add(new Ticket
                {
                    PassengerId = passenger.PassengerId,
                    BookingId = passenger.BookingId,
                    LegId = leg.LegId,
                    SeatId = availableSeat.SeatId,
                    TicketClass = ticketClass,
                    Price = price
                });

                total += price;
            }
        }

        return total;
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

