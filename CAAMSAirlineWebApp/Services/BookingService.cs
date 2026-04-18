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

        // 5. Update booking totall
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

    public async Task<int> CreateBookingWithManualSeatsAsync(
    CreateBookingRequest request, 
    List<string> selectedSeatNumbers)
{
    // 1. Get flight, legs, and aircraft details
    var flight = await _context.Flights
        .Include(f => f.FlightLegs)
        .Include(f => f.Aircraft)
        .FirstOrDefaultAsync(f => f.FlightId == request.FlightId);

    if (flight == null || flight.AircraftId == null)
        throw new Exception("Flight or Aircraft not found.");

    // 2. Load ALL seats for this aircraft ONCE
    var allSeats = await _context.Seats
        .Where(s => s.AircraftId == flight.AircraftId)
        .ToListAsync();

    // 3. Ensure selected seats exist and match the requested class
        var invalidSeats = selectedSeatNumbers.Where(seatNum => {
        var seat = allSeats.FirstOrDefault(s => s.SeatNumber == seatNum);
        return seat == null || seat.SeatClass != request.TicketClass;
    }).ToList();

    if (invalidSeats.Any())
    {
        throw new Exception($"The following seats are not available in {request.TicketClass} class: {string.Join(", ", invalidSeats)}");
    }

    // 4. Create Booking & Passengers
    var booking = new Booking
    {
        CustomerId = request.CustomerId,
        BookingDate = DateTime.UtcNow,
        TotalPrice = 0
    };
    _context.Bookings.Add(booking);
    await _context.SaveChangesAsync(); // Get BookingId

    var passengers = new List<Passenger>();
    foreach (var p in request.Passengers)
    {
        passengers.Add(new Passenger
        {
            BookingId = booking.BookingId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            PassportNumber = p.PassportNumber,
            DOB = p.DOB!.Value
        });
    }
    _context.Passengers.AddRange(passengers);
    await _context.SaveChangesAsync();

    // 5. Map Passengers to SeatIds
    var passengerSeatMap = new List<(Passenger passenger, int seatId, string seatNumber)>();
    
    for (int i = 0; i < passengers.Count; i++)
    {
        if (i >= selectedSeatNumbers.Count)
            throw new Exception("Not enough seats selected for all passengers.");

        string userSeatNumber = selectedSeatNumbers[i];
        var seatEntity = allSeats.FirstOrDefault(s => s.SeatNumber == userSeatNumber);

        if (seatEntity == null)
            throw new Exception($"Seat '{userSeatNumber}' does not exist on this aircraft."); // Should be caught by validation above

        passengerSeatMap.Add((passengers[i], seatEntity.SeatId, userSeatNumber));
    }

    // 6. Create Tickets for EVERY Leg
    // The same seat assignment applies to every leg of the journey.
    decimal totalPrice = 0;

    foreach (var leg in flight.FlightLegs.OrderBy(l => l.LegNumber))
    {
            var takenSeatIds = await _context.Tickets
            .Where(t => t.LegId == leg.LegId && t.SeatId != null)
            .Select(t => t.SeatId.Value)
            .ToListAsync();

        foreach (var (passenger, seatId, seatNumber) in passengerSeatMap)
        {
            // Check if this specific seat is already taken on this leg
            if (takenSeatIds.Contains(seatId))
            {
                throw new Exception($"Seat {seatNumber} is already booked on leg {leg.LegNumber} ({leg.DepartureAirport} to {leg.ArrivalAirport}).");
            }

            // Calculate price for this leg
            var price = CalculatePrice(flight.BasePrice, request.TicketClass);
            
            _context.Tickets.Add(new Ticket
            {
                PassengerId = passenger.PassengerId,
                BookingId = booking.BookingId,
                LegId = leg.LegId,
                SeatId = seatId,
                TicketClass = request.TicketClass,
                Price = price
            });

            totalPrice += price;
        }
    }

    // 7. Finalize
    booking.TotalPrice = totalPrice;
    await _context.SaveChangesAsync();

    return booking.BookingId;
}

}