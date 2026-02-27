using EventBokningApp.Data;
using EventBokningApp.DTOs;
using EventBokningApp.Exceptions;
using EventBokningApp.Models;
using EventBokningApp.Services;
using Microsoft.EntityFrameworkCore;

namespace EventBokningApp.Tests;

/// <summary>
/// Unit tests for BookingService – verifierar bokningslogik,
/// avbokning och felhantering.
/// </summary>
public class BookingServiceTests
{
    private AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private (AppDbContext db, Ticket ticket) SeedDbWithTicket(string dbName, int quantityAvailable = 10)
    {
        var db = CreateInMemoryDb(dbName);
        var venue = new Venue { Name = "Test Arena", Address = "Testgatan 1", City = "Stockholm", Capacity = 1000 };
        db.Venues.Add(venue);
        db.SaveChanges();

        var ev = new Event { Name = "Test Event", Description = "Beskrivning", Date = DateTime.UtcNow.AddDays(7), VenueId = venue.Id };
        db.Events.Add(ev);
        db.SaveChanges();

        var ticket = new Ticket
        {
            TicketType = "Standard",
            Price = 200,
            QuantityTotal = 10,
            QuantityAvailable = quantityAvailable,
            EventId = ev.Id
        };
        db.Tickets.Add(ticket);
        db.SaveChanges();

        return (db, ticket);
    }

    // ── Test 1 ────────────────────────────────────────────────
    [Fact]
    public async Task CreateBookingAsync_ValidDto_ReturnsBookingWithCorrectTotalPrice()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_ValidDto");
        var service = new BookingService(db);

        var dto = new CreateBookingDto(ticket.Id, "Anna Svensson", "anna@test.se", 3);
        var booking = await service.CreateBookingAsync(dto);

        Assert.Equal(600m, booking.TotalPrice);   // 3 × 200 kr
        Assert.Equal("Anna Svensson", booking.CustomerName);
        Assert.Equal(3, booking.Quantity);
        Assert.False(booking.IsCancelled);
    }

    // ── Test 2 ────────────────────────────────────────────────
    [Fact]
    public async Task CreateBookingAsync_ValidDto_DecreasesTicketQuantity()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_DecreaseQty");
        var service = new BookingService(db);

        var dto = new CreateBookingDto(ticket.Id, "Erik Larsson", "erik@test.se", 4);
        await service.CreateBookingAsync(dto);

        var updatedTicket = await db.Tickets.FindAsync(ticket.Id);
        Assert.Equal(6, updatedTicket!.QuantityAvailable);
    }

    // ── Test 3 ────────────────────────────────────────────────
    [Fact]
    public async Task CreateBookingAsync_NotEnoughTickets_ThrowsBusinessException()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_NotEnough", quantityAvailable: 2);
        var service = new BookingService(db);

        var dto = new CreateBookingDto(ticket.Id, "Maria Test", "maria@test.se", 5);

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateBookingAsync(dto));
    }

    // ── Test 4 ────────────────────────────────────────────────
    [Fact]
    public async Task CreateBookingAsync_InvalidTicketId_ThrowsNotFoundException()
    {
        var db = CreateInMemoryDb("BookingTest_InvalidTicketId");
        var service = new BookingService(db);

        var dto = new CreateBookingDto(9999, "Test Person", "test@test.se", 1);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateBookingAsync(dto));
    }

    // ── Test 5 ────────────────────────────────────────────────
    [Fact]
    public async Task CancelBookingAsync_ValidBooking_SetsCancelledFlag()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_Cancel");
        var service = new BookingService(db);

        var booking = new Booking
        {
            TicketId = ticket.Id,
            CustomerName = "Test",
            CustomerEmail = "test@test.se",
            Quantity = 2,
            TotalPrice = 400,
            BookingDate = DateTime.UtcNow,
            IsCancelled = false
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        await service.CancelBookingAsync(booking.Id);

        var updated = await db.Bookings.FindAsync(booking.Id);
        Assert.True(updated!.IsCancelled);
    }

    // ── Test 6 ────────────────────────────────────────────────
    [Fact]
    public async Task CancelBookingAsync_ValidBooking_RestoresTicketQuantity()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_CancelRestore");
        var service = new BookingService(db);

        var booking = new Booking
        {
            TicketId = ticket.Id,
            CustomerName = "Test",
            CustomerEmail = "t@t.se",
            Quantity = 3,
            TotalPrice = 600,
            BookingDate = DateTime.UtcNow,
            IsCancelled = false
        };
        db.Bookings.Add(booking);
        ticket.QuantityAvailable -= 3;
        await db.SaveChangesAsync();

        await service.CancelBookingAsync(booking.Id);

        var updatedTicket = await db.Tickets.FindAsync(ticket.Id);
        Assert.Equal(10, updatedTicket!.QuantityAvailable);
    }

    // ── Test 7 ────────────────────────────────────────────────
    [Fact]
    public async Task CancelBookingAsync_AlreadyCancelled_ThrowsBusinessException()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_AlreadyCancelled");
        var service = new BookingService(db);

        var booking = new Booking
        {
            TicketId = ticket.Id,
            CustomerName = "Test",
            CustomerEmail = "t@t.se",
            Quantity = 1,
            TotalPrice = 200,
            BookingDate = DateTime.UtcNow,
            IsCancelled = true     // already cancelled
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessException>(() => service.CancelBookingAsync(booking.Id));
    }

    // ── Test 8 ────────────────────────────────────────────────
    [Fact]
    public async Task CancelBookingAsync_InvalidBookingId_ThrowsNotFoundException()
    {
        var db = CreateInMemoryDb("BookingTest_InvalidBookingId");
        var service = new BookingService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CancelBookingAsync(9999));
    }

    // ── Test 9 ────────────────────────────────────────────────
    [Fact]
    public async Task CreateBookingAsync_ExactlyAllAvailableTickets_Succeeds()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_ExactAll", quantityAvailable: 5);
        var service = new BookingService(db);

        var dto = new CreateBookingDto(ticket.Id, "Test", "t@t.se", 5);
        var booking = await service.CreateBookingAsync(dto);

        Assert.Equal(5, booking.Quantity);
        var updatedTicket = await db.Tickets.FindAsync(ticket.Id);
        Assert.Equal(0, updatedTicket!.QuantityAvailable);
    }

    // ── Test 10 ───────────────────────────────────────────────
    [Fact]
    public async Task CreateBookingAsync_SetsBookingDateToUtcNow()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_DateSet");
        var service = new BookingService(db);

        var before = DateTime.UtcNow;
        var dto = new CreateBookingDto(ticket.Id, "Test", "t@t.se", 1);
        var booking = await service.CreateBookingAsync(dto);
        var after = DateTime.UtcNow;

        Assert.True(booking.BookingDate >= before && booking.BookingDate <= after);
    }

    // ── Test 11 ───────────────────────────────────────────────
    [Fact]
    public async Task CreateBookingAsync_OneTicket_TotalPriceEqualsTicketPrice()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_SingleTicketPrice");
        var service = new BookingService(db);

        var dto = new CreateBookingDto(ticket.Id, "Single", "s@s.se", 1);
        var booking = await service.CreateBookingAsync(dto);

        Assert.Equal(ticket.Price, booking.TotalPrice);
    }

    // ── Test 12 ───────────────────────────────────────────────
    [Fact]
    public async Task CreateBookingAsync_MultipleBookings_EachDecreasesAvailability()
    {
        var (db, ticket) = SeedDbWithTicket("BookingTest_MultipleDecreases", quantityAvailable: 10);
        var service = new BookingService(db);

        await service.CreateBookingAsync(new CreateBookingDto(ticket.Id, "A", "a@a.se", 2));
        await service.CreateBookingAsync(new CreateBookingDto(ticket.Id, "B", "b@b.se", 3));

        var updatedTicket = await db.Tickets.FindAsync(ticket.Id);
        Assert.Equal(5, updatedTicket!.QuantityAvailable);
    }
}
