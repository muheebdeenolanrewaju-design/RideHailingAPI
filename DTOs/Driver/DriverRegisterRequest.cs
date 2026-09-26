using System.ComponentModel.DataAnnotations;

namespace RideHailingAPI.DTOs.Driver;

public class DriverRegisterRequest
{
    // Personal Details
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    // Driver License
    [Required]
    public string LicenseNumber { get; set; } = string.Empty;

    // Vehicle Details
    [Required]
    public string Make { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Required, Range(1900, 2100)]
    public int Year { get; set; }

    [Required]
    public string PlateNumber { get; set; } = string.Empty;

    [Required]
    public string Color { get; set; } = string.Empty;
}