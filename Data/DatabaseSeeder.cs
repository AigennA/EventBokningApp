using EventBokningApp.Models;

namespace EventBokningApp.Data;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Venues.Any()) return;

        var venues = new List<Venue>
        {
            new() { Name = "Stockholm Arena", Address = "Evenemangsgatan 31", City = "Stockholm", Capacity = 30000 },
            new() { Name = "Gothenburg Concert Hall", Address = "Götaplatsen 8", City = "Göteborg", Capacity = 1200 },
            new() { Name = "Malmö Live", Address = "Miguel Cervantes väg 10", City = "Malmö", Capacity = 1600 }
        };

        db.Venues.AddRange(venues);
        db.SaveChanges();

        var events = new List<Event>
        {
            new()
            {
                Name = "Sommarfestival 2025",
                Description = "Årets största musikfestival med toppnamn från hela Sverige och världen.",
                Date = new DateTime(2025, 7, 15, 18, 0, 0, DateTimeKind.Utc),
                VenueId = venues[0].Id,
                Tickets = new List<Ticket>
                {
                    new() { TicketType = "VIP", Price = 1500, QuantityTotal = 200, QuantityAvailable = 200 },
                    new() { TicketType = "Standard", Price = 500, QuantityTotal = 2000, QuantityAvailable = 2000 },
                    new() { TicketType = "Student", Price = 250, QuantityTotal = 500, QuantityAvailable = 500 }
                }
            },
            new()
            {
                Name = "Jazz Evening – Göteborg",
                Description = "En magisk kväll med Sveriges bästa jazzmusikers i världsklass.",
                Date = new DateTime(2025, 9, 20, 19, 30, 0, DateTimeKind.Utc),
                VenueId = venues[1].Id,
                Tickets = new List<Ticket>
                {
                    new() { TicketType = "Premium", Price = 800, QuantityTotal = 100, QuantityAvailable = 100 },
                    new() { TicketType = "Ordinarie", Price = 400, QuantityTotal = 400, QuantityAvailable = 400 }
                }
            },
            new()
            {
                Name = "Tech Conference Malmö",
                Description = "Framtidens teknik – AI, cloud och fullstack-utveckling på en dag.",
                Date = new DateTime(2025, 11, 5, 9, 0, 0, DateTimeKind.Utc),
                VenueId = venues[2].Id,
                Tickets = new List<Ticket>
                {
                    new() { TicketType = "Early Bird", Price = 1200, QuantityTotal = 150, QuantityAvailable = 150 },
                    new() { TicketType = "Standard", Price = 1800, QuantityTotal = 500, QuantityAvailable = 500 },
                    new() { TicketType = "Workshop Add-on", Price = 600, QuantityTotal = 100, QuantityAvailable = 100 }
                }
            }
        };

        db.Events.AddRange(events);
        db.SaveChanges();

        // Seed admin user
        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@bokningssystem.se",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }
}
