namespace EventBokningApp.Models;

public class Ticket
{
    public int Id { get; set; }
    public string TicketType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int QuantityAvailable { get; set; }
    public int QuantityTotal { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
