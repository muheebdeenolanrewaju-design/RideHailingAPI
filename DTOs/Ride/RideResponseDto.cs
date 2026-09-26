namespace RideHailingAPI.DTOs.Ride;

public class RideResponseDto
{
    public int Id { get; set; }
    public string RideReference { get; set; } = string.Empty;
    public int PassengerId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerPhone { get; set; } = string.Empty;
    public int? DriverProfileId { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal EstimatedFare { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}