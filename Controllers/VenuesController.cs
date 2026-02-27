using EventBokningApp.DTOs;
using EventBokningApp.Models;
using EventBokningApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBokningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly IVenueRepository _venueRepo;

    public VenuesController(IVenueRepository venueRepo)
    {
        _venueRepo = venueRepo;
    }

    /// <summary>Get all venues.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VenueDto>>> GetAll()
    {
        var venues = await _venueRepo.GetAllAsync();
        return Ok(venues.Select(v => new VenueDto(v.Id, v.Name, v.Address, v.City, v.Capacity)));
    }

    /// <summary>Get a single venue.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VenueDto>> GetById(int id)
    {
        var venue = await _venueRepo.GetByIdAsync(id);
        return venue is null
            ? NotFound(new { message = $"Venue {id} not found." })
            : Ok(new VenueDto(venue.Id, venue.Name, venue.Address, venue.City, venue.Capacity));
    }

    /// <summary>Create a new venue. (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VenueDto>> Create(CreateVenueDto dto)
    {
        var venue = new Venue
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Capacity = dto.Capacity
        };

        await _venueRepo.AddAsync(venue);
        await _venueRepo.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = venue.Id },
            new VenueDto(venue.Id, venue.Name, venue.Address, venue.City, venue.Capacity));
    }

    /// <summary>Update a venue. (Admin only)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateVenueDto dto)
    {
        var venue = await _venueRepo.GetByIdAsync(id);
        if (venue is null) return NotFound(new { message = $"Venue {id} not found." });

        venue.Name = dto.Name;
        venue.Address = dto.Address;
        venue.City = dto.City;
        venue.Capacity = dto.Capacity;

        await _venueRepo.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Delete a venue. (Admin only)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var venue = await _venueRepo.GetByIdAsync(id);
        if (venue is null) return NotFound(new { message = $"Venue {id} not found." });

        _venueRepo.Remove(venue);
        await _venueRepo.SaveChangesAsync();
        return NoContent();
    }
}
