using EventBokningApp.DTOs;
using EventBokningApp.Exceptions;
using EventBokningApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventBokningApp.Repositories;

namespace EventBokningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IBookingRepository _bookingRepo;

    public BookingsController(IBookingService bookingService, IBookingRepository bookingRepo)
    {
        _bookingService = bookingService;
        _bookingRepo = bookingRepo;
    }

    /// <summary>
    /// Create a new booking. (Protected – requires JWT)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingDto dto)
    {
        try
        {
            var booking = await _bookingService.CreateBookingAsync(dto);
            var full = await _bookingRepo.GetByIdAsync(booking.Id);
            if (full is null) return StatusCode(500);

            return CreatedAtAction(nameof(GetById), new { id = full.Id }, MapToDto(full));
        }
        catch (NotFoundException ex)  { return NotFound(new { message = ex.Message }); }
        catch (BusinessException ex)  { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Get a single booking by ID. (Protected – requires JWT)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<BookingDto>> GetById(int id)
    {
        var booking = await _bookingRepo.GetByIdAsync(id);
        return booking is null
            ? NotFound(new { message = $"Booking {id} not found." })
            : Ok(MapToDto(booking));
    }

    /// <summary>
    /// Get all bookings. (Protected – Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetAll()
    {
        var bookings = await _bookingRepo.GetAllAsync();
        return Ok(bookings.Select(MapToDto));
    }

    /// <summary>
    /// Cancel a booking. (Protected – requires JWT)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _bookingService.CancelBookingAsync(id);
            return Ok(new { message = $"Booking {id} successfully cancelled." });
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (BusinessException ex) { return BadRequest(new { message = ex.Message }); }
    }

    private static BookingDto MapToDto(Models.Booking b) => new(
        b.Id, b.CustomerName, b.CustomerEmail, b.Quantity,
        b.TotalPrice, b.BookingDate, b.IsCancelled,
        b.TicketId, b.Ticket.TicketType, b.Ticket.EventId, b.Ticket.Event.Name
    );
}
