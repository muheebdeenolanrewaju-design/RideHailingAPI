using RideHailingAPI.Domain.Enum;

namespace RideHailingAPI.Domain.Entities;

public class Ride
{
    public int Id { get; set; }
    public string RideReference { get; set; } = string.Empty;

    public int PassengerId { get; set; }
    public User Passenger { get; set; } = null!;

    public int? DriverId { get; set; }
    public DriverProfile? Driver { get; set; }

    public string PickupLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public RideStatus CurrentStatus { get; set; } = RideStatus.Requested;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation Properties
    public ICollection<RideStatusHistory> StatusHistories { get; set; } = new List<RideStatusHistory>();
}