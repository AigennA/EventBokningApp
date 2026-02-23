namespace EventBokningApp.Models;

public class Booking
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookingDate { get; set; }
    public bool IsCancelled { get; set; }

    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
}
