using System.ComponentModel.DataAnnotations;

namespace RideHailingAPI.DTOs.Auth;

public class LoginRequest
{
    [Required]
    public string Identifier { get; set; } = string.Empty; // Email or Phone Number

    [Required]
    public string Password { get; set; } = string.Empty;
}