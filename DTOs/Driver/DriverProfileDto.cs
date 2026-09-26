namespace RideHailingAPI.DTOs.Driver;

public class DriverProfileDto
{
    public int DriverProfileId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    
    // Vehicle Info
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public int VehicleYear { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string VehicleColor { get; set; } = string.Empty;
}