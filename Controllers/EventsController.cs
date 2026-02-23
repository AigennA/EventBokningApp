using EventBokningApp.Data;
using EventBokningApp.DTOs;
using EventBokningApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBokningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/events - Lista events med Venue
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventSummaryDto>>> GetAll()
    {
        var events = await _context.Events
            .Include(e => e.Venue)
            .Select(e => new EventSummaryDto(
                e.Id, e.Name, e.Description, e.Date,
                new VenueDto(e.Venue.Id, e.Venue.Name, e.Venue.Address, e.Venue.City, e.Venue.Capacity)
            ))
            .ToListAsync();

        return Ok(events);
    }

    // GET /api/events/upcoming - Endast framtida events
    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<EventSummaryDto>>> GetUpcoming()
    {
        var now = DateTime.UtcNow;
        var events = await _context.Events
            .Include(e => e.Venue)
            .Where(e => e.Date > now)
            .OrderBy(e => e.Date)
            .Select(e => new EventSummaryDto(
                e.Id, e.Name, e.Description, e.Date,
                new VenueDto(e.Venue.Id, e.Venue.Name, e.Venue.Address, e.Venue.City, e.Venue.Capacity)
            ))
            .ToListAsync();

        return Ok(events);
    }

    // GET /api/events/{id} - Event med Venue och Tickets
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDetailDto>> GetById(int id)
    {
        var ev = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Tickets)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev is null)
            return NotFound(new { message = $"Event with id {id} not found." });

        var dto = new EventDetailDto(
            ev.Id, ev.Name, ev.Description, ev.Date,
            new VenueDto(ev.Venue.Id, ev.Venue.Name, ev.Venue.Address, ev.Venue.City, ev.Venue.Capacity),
            ev.Tickets.Select(t => new TicketDto(t.Id, t.TicketType, t.Price, t.QuantityAvailable, t.QuantityTotal)).ToList()
        );

        return Ok(dto);
    }

    // POST /api/events - Skapa event med biljetttyper
    [HttpPost]
    public async Task<ActionResult<EventDetailDto>> Create(CreateEventDto dto)
    {
        var venueExists = await _context.Venues.AnyAsync(v => v.Id == dto.VenueId);
        if (!venueExists)
            return NotFound(new { message = $"Venue with id {dto.VenueId} not found." });

        var ev = new Event
        {
            Name = dto.Name,
            Description = dto.Description,
            Date = dto.Date,
            VenueId = dto.VenueId,
            Tickets = dto.Tickets.Select(t => new Ticket
            {
                TicketType = t.TicketType,
                Price = t.Price,
                QuantityTotal = t.QuantityTotal,
                QuantityAvailable = t.QuantityTotal
            }).ToList()
        };

        _context.Events.Add(ev);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, new { id = ev.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateEventDto dto)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null)
            return NotFound(new { message = $"Event with id {id} not found." });

        var venueExists = await _context.Venues.AnyAsync(v => v.Id == dto.VenueId);
        if (!venueExists)
            return NotFound(new { message = $"Venue with id {dto.VenueId} not found." });

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.Date = dto.Date;
        ev.VenueId = dto.VenueId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null)
            return NotFound(new { message = $"Event with id {id} not found." });

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/events/{id}/bookings - Bokningar för ett event
    [HttpGet("{id}/bookings")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings(int id)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.Id == id);
        if (!eventExists)
            return NotFound(new { message = $"Event with id {id} not found." });

        var bookings = await _context.Bookings
            .Include(b => b.Ticket)
            .ThenInclude(t => t.Event)
            .Where(b => b.Ticket.EventId == id)
            .Select(b => new BookingDto(
                b.Id,
                b.CustomerName,
                b.CustomerEmail,
                b.Quantity,
                b.TotalPrice,
                b.BookingDate,
                b.IsCancelled,
                b.TicketId,
                b.Ticket.TicketType,
                b.Ticket.EventId,
                b.Ticket.Event.Name
            ))
            .ToListAsync();

        return Ok(bookings);
    }
}
