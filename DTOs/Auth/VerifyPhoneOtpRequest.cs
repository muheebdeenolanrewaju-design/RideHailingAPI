using System.ComponentModel.DataAnnotations;

namespace RideHailingAPI.DTOs.Auth;

public class VerifyPhoneOtpRequest
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    public string OtpCode { get; set; } = string.Empty;
}