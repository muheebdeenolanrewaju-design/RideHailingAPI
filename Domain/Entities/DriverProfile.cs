using RideHailingAPI.Domain.Enum;

namespace RideHailingAPI.Domain.Entities;

public class DriverProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string LicenseNumber { get; set; } = string.Empty;
    public DriverApprovalStatus ApprovalStatus { get; set; } = DriverApprovalStatus.Pending;
    public DriverAvailabilityStatus AvailabilityStatus { get; set; } = DriverAvailabilityStatus.Unavailable;
    public DateTime? ApprovedAt { get; set; }

    // Navigation Properties
    public Vehicle? Vehicle { get; set; }
    public ICollection<Ride> AssignedRides { get; set; } = new List<Ride>();
}