namespace RideHailingAPI.DTOs.Admin;

public class PendingDriverDto
{
    public int DriverProfileId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}