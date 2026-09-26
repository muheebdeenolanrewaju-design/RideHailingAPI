namespace RideHailingAPI.DTOs.Admin;

public class AdminRideDto
{
    public int Id { get; set; }
    public string RideReference { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string? DriverName { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}