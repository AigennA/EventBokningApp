using System.ComponentModel.DataAnnotations;

namespace EventBokningApp.DTOs;

public record CreateVenueDto(
    [Required][MaxLength(200)] string Name,
    [Required][MaxLength(300)] string Address,
    [Required][MaxLength(100)] string City,
    [Required][Range(1, 100000)] int Capacity
);

public record UpdateVenueDto(
    [Required][MaxLength(200)] string Name,
    [Required][MaxLength(300)] string Address,
    [Required][MaxLength(100)] string City,
    [Required][Range(1, 100000)] int Capacity
);

public record VenueDto(
    int Id,
    string Name,
    string Address,
    string City,
    int Capacity
);
