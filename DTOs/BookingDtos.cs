using System.ComponentModel.DataAnnotations;

namespace EventBokningApp.DTOs;

public record CreateBookingDto(
    [Required] int TicketId,
    [Required][MaxLength(200)] string CustomerName,
    [Required][EmailAddress] string CustomerEmail,
    [Required][Range(1, 100)] int Quantity
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
