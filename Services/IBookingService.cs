using EventBokningApp.DTOs;
using EventBokningApp.Models;

namespace EventBokningApp.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(CreateBookingDto dto);
    Task<bool> CancelBookingAsync(int bookingId);
}
