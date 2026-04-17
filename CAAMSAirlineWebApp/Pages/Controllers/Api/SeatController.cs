using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAAMSAirlineWebApp.Data;
using CAAMSAirlineWebApp.DTOs;
using CAAMSAirlineWebApp.Models;

namespace CAAMSAirlineWebApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeatsController(ApplicationDbContext context)
        {
            _context = context;
        }

[HttpGet("flight/{flightId}")]
public async Task<ActionResult<IEnumerable<SeatDTO>>> GetSeatsByFlight(int flightId, [FromQuery] string? @class)
{
    var flight = await _context.Flights
        .Include(f => f.Aircraft)
        .FirstOrDefaultAsync(f => f.FlightId == flightId);

    if (flight == null || flight.Aircraft == null)
        return NotFound($"Flight {flightId} or aircraft not found.");

    var query = _context.Seats.Where(s => s.AircraftId == flight.AircraftId);

    // Filter by class if provided
    if (!string.IsNullOrEmpty(@class))
    {
        query = query.Where(s => s.SeatClass == @class);
    }

    var seats = await query
        .OrderBy(s => s.SeatNumber)
        .Select(s => new SeatDTO
        {
            SeatId = s.SeatId,
            SeatNumber = s.SeatNumber,
            SeatClass = s.SeatClass
        })
        .ToListAsync();

    Console.WriteLine($"[DEBUG] Returning {seats.Count} seats for class: {@class ?? "ALL"}");
    return Ok(seats);
}

        [HttpGet("occupied/{flightId}")]
        public async Task<ActionResult<IEnumerable<string>>> GetOccupiedSeats(int flightId)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightLegs)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null)
                return NotFound();

            var legIds = flight.FlightLegs.Select(l => l.LegId).ToList();

            var occupiedSeatNumbers = await _context.Tickets
                .Where(t => legIds.Contains(t.LegId) && t.SeatId != null)
                .Join(_context.Seats,
                    t => t.SeatId,
                    s => s.SeatId,
                    (t, s) => s.SeatNumber)
                .Distinct()
                .ToListAsync();

            return Ok(occupiedSeatNumbers);
        }
    }
}