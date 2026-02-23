using EventBokningApp.DTOs;
using EventBokningApp.Exceptions;
using EventBokningApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventBokningApp.Data;

namespace EventBokningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly AppDbContext _context;

    public BookingsController(IBookingService bookingService, AppDbContext context)
    {
        _bookingService = bookingService;
        _context = context;
    }

    // POST /api/bookings - Skapa bokning
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingDto dto)
    {
        try
        {
            var booking = await _bookingService.CreateBookingAsync(dto);

            await _context.Entry(booking).Reference(b => b.Ticket).LoadAsync();
            await _context.Entry(booking.Ticket).Reference(t => t.Event).LoadAsync();

            var result = new BookingDto(
                booking.Id,
                booking.CustomerName,
                booking.CustomerEmail,
                booking.Quantity,
                booking.TotalPrice,
                booking.BookingDate,
                booking.IsCancelled,
                booking.TicketId,
                booking.Ticket.TicketType,
                booking.Ticket.EventId,
                booking.Ticket.Event.Name
            );

            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> GetById(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Ticket)
            .ThenInclude(t => t.Event)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking is null)
            return NotFound(new { message = $"Booking with id {id} not found." });

        return Ok(new BookingDto(
            booking.Id,
            booking.CustomerName,
            booking.CustomerEmail,
            booking.Quantity,
            booking.TotalPrice,
            booking.BookingDate,
            booking.IsCancelled,
            booking.TicketId,
            booking.Ticket.TicketType,
            booking.Ticket.EventId,
            booking.Ticket.Event.Name
        ));
    }

    // DELETE /api/bookings/{id} - Avboka
    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _bookingService.CancelBookingAsync(id);
            return Ok(new { message = $"Booking {id} successfully cancelled." });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
