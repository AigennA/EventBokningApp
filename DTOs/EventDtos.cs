using System.ComponentModel.DataAnnotations;

namespace EventBokningApp.DTOs;

public record CreateTicketDto(
    [Required][MaxLength(100)] string TicketType,
    [Required][Range(0, 100000)] decimal Price,
    [Required][Range(1, 10000)] int QuantityTotal
);

public record CreateEventDto(
    [Required][MaxLength(200)] string Name,
    [MaxLength(2000)] string Description,
    [Required] DateTime Date,
    [Required] int VenueId,
    [Required] List<CreateTicketDto> Tickets
);

public record TicketDto(
    int Id,
    string TicketType,
    decimal Price,
    int QuantityAvailable,
    int QuantityTotal
);

public record EventSummaryDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    VenueDto Venue
);

public record EventDetailDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    VenueDto Venue,
    List<TicketDto> Tickets
);
