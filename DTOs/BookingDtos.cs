namespace EventBokningApp.DTOs;

public record CreateBookingDto(
    int TicketId,
    string CustomerName,
    string CustomerEmail,
    int Quantity
);

public record BookingDto(
    int Id,
    string CustomerName,
    string CustomerEmail,
    int Quantity,
    decimal TotalPrice,
    DateTime BookingDate,
    bool IsCancelled,
    int TicketId,
    string TicketType,
    int EventId,
    string EventName
);
