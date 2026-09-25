using RideHailingAPI.Domain.Enum;

namespace RideHailingAPI.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public DriverProfile? DriverProfile { get; set; }
    public ICollection<Ride> PassengerRides { get; set; } = new List<Ride>();
    public ICollection<EmailOtp> EmailOtps { get; set; } = new List<EmailOtp>();
    public ICollection<PhoneOtp> PhoneOtps { get; set; } = new List<PhoneOtp>();
    public ICollection<PasswordResetOtp> PasswordResetOtps { get; set; } = new List<PasswordResetOtp>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}