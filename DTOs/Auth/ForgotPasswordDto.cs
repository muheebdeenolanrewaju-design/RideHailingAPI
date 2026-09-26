using System.ComponentModel.DataAnnotations;

namespace RideHailingAPI.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}