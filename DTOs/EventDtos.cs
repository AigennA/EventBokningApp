namespace EventBokningApp.DTOs;

public record CreateTicketDto(
    string TicketType,
    decimal Price,
    int QuantityTotal
);

public record CreateEventDto(
    string Name,
    string Description,
    DateTime Date,
    int VenueId,
    List<CreateTicketDto> Tickets
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
