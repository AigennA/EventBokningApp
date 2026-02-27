using EventBokningApp.DTOs;
using EventBokningApp.Models;
using EventBokningApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBokningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventRepository _eventRepo;
    private readonly IVenueRepository _venueRepo;
    private readonly IBookingRepository _bookingRepo;

    public EventsController(IEventRepository eventRepo, IVenueRepository venueRepo, IBookingRepository bookingRepo)
    {
        _eventRepo = eventRepo;
        _venueRepo = venueRepo;
        _bookingRepo = bookingRepo;
    }

    /// <summary>Get all events with venue info.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventSummaryDto>>> GetAll()
    {
        var events = await _eventRepo.GetAllAsync();
        return Ok(events.Select(MapToSummary));
    }

    /// <summary>Get only upcoming (future) events.</summary>
    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<EventSummaryDto>>> GetUpcoming()
    {
        var events = await _eventRepo.GetUpcomingAsync();
        return Ok(events.Select(MapToSummary));
    }

    /// <summary>Get a single event with tickets.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDetailDto>> GetById(int id)
    {
        var ev = await _eventRepo.GetByIdAsync(id);
        return ev is null
            ? NotFound(new { message = $"Event {id} not found." })
            : Ok(MapToDetail(ev));
    }

    /// <summary>Create a new event with ticket types. (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EventDetailDto>> Create(CreateEventDto dto)
    {
        if (!await _venueRepo.ExistsAsync(dto.VenueId))
            return NotFound(new { message = $"Venue {dto.VenueId} not found." });

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

        await _eventRepo.AddAsync(ev);
        await _eventRepo.SaveChangesAsync();

        var created = await _eventRepo.GetByIdAsync(ev.Id);
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, MapToDetail(created!));
    }

    /// <summary>Update an event. (Admin only)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CreateEventDto dto)
    {
        var ev = await _eventRepo.GetByIdAsync(id);
        if (ev is null) return NotFound(new { message = $"Event {id} not found." });
        if (!await _venueRepo.ExistsAsync(dto.VenueId))
            return NotFound(new { message = $"Venue {dto.VenueId} not found." });

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.Date = dto.Date;
        ev.VenueId = dto.VenueId;

        await _eventRepo.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Delete an event. (Admin only)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _eventRepo.GetByIdAsync(id);
        if (ev is null) return NotFound(new { message = $"Event {id} not found." });

        _eventRepo.Remove(ev);
        await _eventRepo.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Get all bookings for an event. (Admin only)</summary>
    [HttpGet("{id}/bookings")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings(int id)
    {
        if (!await _eventRepo.ExistsAsync(id))
            return NotFound(new { message = $"Event {id} not found." });

        var bookings = await _bookingRepo.GetByEventIdAsync(id);
        return Ok(bookings.Select(b => new BookingDto(
            b.Id, b.CustomerName, b.CustomerEmail, b.Quantity,
            b.TotalPrice, b.BookingDate, b.IsCancelled,
            b.TicketId, b.Ticket.TicketType, b.Ticket.EventId, b.Ticket.Event.Name
        )));
    }

    private static EventSummaryDto MapToSummary(Event e) => new(
        e.Id, e.Name, e.Description, e.Date,
        new VenueDto(e.Venue.Id, e.Venue.Name, e.Venue.Address, e.Venue.City, e.Venue.Capacity)
    );

    private static EventDetailDto MapToDetail(Event e) => new(
        e.Id, e.Name, e.Description, e.Date,
        new VenueDto(e.Venue.Id, e.Venue.Name, e.Venue.Address, e.Venue.City, e.Venue.Capacity),
        e.Tickets.Select(t => new TicketDto(t.Id, t.TicketType, t.Price, t.QuantityAvailable, t.QuantityTotal)).ToList()
    );
}
