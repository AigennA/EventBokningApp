using EventBokningApp.Data;
using EventBokningApp.DTOs;
using EventBokningApp.Exceptions;
using EventBokningApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBokningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly AppDbContext _context;

    public VenuesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VenueDto>>> GetAll()
    {
        var venues = await _context.Venues
            .Select(v => new VenueDto(v.Id, v.Name, v.Address, v.City, v.Capacity))
            .ToListAsync();
        return Ok(venues);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VenueDto>> GetById(int id)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue is null)
            return NotFound(new { message = $"Venue with id {id} not found." });

        return Ok(new VenueDto(venue.Id, venue.Name, venue.Address, venue.City, venue.Capacity));
    }

    [HttpPost]
    public async Task<ActionResult<VenueDto>> Create(CreateVenueDto dto)
    {
        var venue = new Venue
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Capacity = dto.Capacity
        };

        _context.Venues.Add(venue);
        await _context.SaveChangesAsync();

        var result = new VenueDto(venue.Id, venue.Name, venue.Address, venue.City, venue.Capacity);
        return CreatedAtAction(nameof(GetById), new { id = venue.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateVenueDto dto)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue is null)
            return NotFound(new { message = $"Venue with id {id} not found." });

        venue.Name = dto.Name;
        venue.Address = dto.Address;
        venue.City = dto.City;
        venue.Capacity = dto.Capacity;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue is null)
            return NotFound(new { message = $"Venue with id {id} not found." });

        _context.Venues.Remove(venue);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
