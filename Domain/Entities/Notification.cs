namespace RideHailingAPI.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Channel { get; set; } = string.Empty; // "Email" or "SMS"
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = true;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}