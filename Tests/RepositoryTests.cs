using EventBokningApp.Data;
using EventBokningApp.Models;
using EventBokningApp.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventBokningApp.Tests;

/// <summary>
/// Unit tests för Repository-lagret –
/// verifierar CRUD och querys mot in-memory databas.
/// </summary>
public class RepositoryTests
{
    private AppDbContext CreateDb(string name)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name).Options;
        return new AppDbContext(opts);
    }

    // ── VenueRepository ───────────────────────────────────────

    [Fact]
    public async Task VenueRepository_AddAndGetById_ReturnsCorrectVenue()
    {
        var db = CreateDb("VenueRepo_AddGet");
        var repo = new VenueRepository(db);

        var venue = new Venue { Name = "Test Hall", Address = "Gatan 1", City = "Lund", Capacity = 500 };
        await repo.AddAsync(venue);
        await repo.SaveChangesAsync();

        var found = await repo.GetByIdAsync(venue.Id);
        Assert.NotNull(found);
        Assert.Equal("Test Hall", found.Name);
        Assert.Equal("Lund", found.City);
    }

    [Fact]
    public async Task VenueRepository_GetAll_ReturnsAllVenues()
    {
        var db = CreateDb("VenueRepo_GetAll");
        var repo = new VenueRepository(db);

        await repo.AddAsync(new Venue { Name = "A", Address = "Aa", City = "X", Capacity = 100 });
        await repo.AddAsync(new Venue { Name = "B", Address = "Bb", City = "Y", Capacity = 200 });
        await repo.SaveChangesAsync();

        var all = (await repo.GetAllAsync()).ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task VenueRepository_ExistsAsync_ReturnsTrueForExistingVenue()
    {
        var db = CreateDb("VenueRepo_Exists");
        var repo = new VenueRepository(db);

        var venue = new Venue { Name = "Exists", Address = "St", City = "City", Capacity = 50 };
        await repo.AddAsync(venue);
        await repo.SaveChangesAsync();

        Assert.True(await repo.ExistsAsync(venue.Id));
        Assert.False(await repo.ExistsAsync(9999));
    }

    [Fact]
    public async Task VenueRepository_Remove_DeletesVenue()
    {
        var db = CreateDb("VenueRepo_Remove");
        var repo = new VenueRepository(db);

        var venue = new Venue { Name = "Delete me", Address = "St", City = "C", Capacity = 10 };
        await repo.AddAsync(venue);
        await repo.SaveChangesAsync();

        repo.Remove(venue);
        await repo.SaveChangesAsync();

        Assert.Null(await repo.GetByIdAsync(venue.Id));
    }

    // ── EventRepository ───────────────────────────────────────

    [Fact]
    public async Task EventRepository_GetUpcoming_ReturnsOnlyFutureEvents()
    {
        var db = CreateDb("EventRepo_Upcoming");
        var venue = new Venue { Name = "V", Address = "A", City = "C", Capacity = 100 };
        db.Venues.Add(venue);
        await db.SaveChangesAsync();

        db.Events.AddRange(
            new Event { Name = "Past",   Date = DateTime.UtcNow.AddDays(-1),  VenueId = venue.Id, Description = "" },
            new Event { Name = "Future", Date = DateTime.UtcNow.AddDays(10), VenueId = venue.Id, Description = "" }
        );
        await db.SaveChangesAsync();

        var repo = new EventRepository(db);
        var upcoming = (await repo.GetUpcomingAsync()).ToList();

        Assert.Single(upcoming);
        Assert.Equal("Future", upcoming[0].Name);
    }

    // ── UserRepository ────────────────────────────────────────

    [Fact]
    public async Task UserRepository_GetByEmail_ReturnsCorrectUser()
    {
        var db = CreateDb("UserRepo_GetByEmail");
        var repo = new UserRepository(db);

        var user = new User { Username = "alice", Email = "alice@test.se", PasswordHash = "hash", Role = "User" };
        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        var found = await repo.GetByEmailAsync("alice@test.se");
        Assert.NotNull(found);
        Assert.Equal("alice", found.Username);
    }

    [Fact]
    public async Task UserRepository_ExistsByEmail_ReturnsFalseForNonExistent()
    {
        var db = CreateDb("UserRepo_ExistsFalse");
        var repo = new UserRepository(db);

        Assert.False(await repo.ExistsByEmailAsync("notexists@test.se"));
    }

    // ── BookingRepository ─────────────────────────────────────

    [Fact]
    public async Task BookingRepository_GetByEmail_ReturnsOnlyNonCancelled()
    {
        var db = CreateDb("BookingRepo_ByEmail");

        var venue = new Venue { Name = "V", Address = "A", City = "C", Capacity = 100 };
        db.Venues.Add(venue);
        var ev = new Event { Name = "E", Description = "", Date = DateTime.UtcNow.AddDays(5), VenueId = venue.Id };
        db.Events.Add(ev);
        var ticket = new Ticket { TicketType = "Std", Price = 100, QuantityTotal = 10, QuantityAvailable = 10, EventId = ev.Id };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        db.Bookings.AddRange(
            new Booking { TicketId = ticket.Id, CustomerName = "X", CustomerEmail = "x@x.se", Quantity = 1, TotalPrice = 100, BookingDate = DateTime.UtcNow, IsCancelled = false },
            new Booking { TicketId = ticket.Id, CustomerName = "X", CustomerEmail = "x@x.se", Quantity = 1, TotalPrice = 100, BookingDate = DateTime.UtcNow, IsCancelled = true  }
        );
        await db.SaveChangesAsync();

        var repo = new BookingRepository(db);
        var byEmail = (await repo.GetByEmailAsync("x@x.se")).ToList();

        // Only non-cancelled
        Assert.Single(byEmail);
        Assert.False(byEmail[0].IsCancelled);
    }
}
