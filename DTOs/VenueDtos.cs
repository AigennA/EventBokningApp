namespace EventBokningApp.DTOs;

public record CreateVenueDto(
    string Name,
    string Address,
    string City,
    int Capacity
);

public record UpdateVenueDto(
    string Name,
    string Address,
    string City,
    int Capacity
);

public record VenueDto(
    int Id,
    string Name,
    string Address,
    string City,
    int Capacity
);
