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
        // 1. Get flight + legs
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

        // 4. Create tickets + calculate price
        decimal totalPrice = 0;

        foreach (var passenger in passengers)
        {
            foreach (var leg in flight.FlightLegs)
            {
                var price = CalculatePrice(flight.BasePrice, request.TicketClass);

                var ticket = new Ticket
                {
                    PassengerId = passenger.PassengerId,
                    BookingId = booking.BookingId,
                    LegId = leg.LegId,
                    TicketClass = request.TicketClass,
                    Price = price
                };

                totalPrice += price;

                _context.Tickets.Add(ticket);
            }
        }

        await _context.SaveChangesAsync();

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