using RideHailingAPI.Domain.Enum;

namespace RideHailingAPI.Domain.Entities;

public class RideStatusHistory
{
    public int Id { get; set; }
    public int RideId { get; set; }
    public Ride Ride { get; set; } = null!;

    public RideStatus? PreviousStatus { get; set; }
    public RideStatus NewStatus { get; set; }

    public int ChangedByUserId { get; set; }
    public User ChangedByUser { get; set; } = null!;

    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}