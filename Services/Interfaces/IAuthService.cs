using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Auth;

namespace RideHailingAPI.Services.Interfaces;

public interface IAuthService
{
    Task<ApiReponse> LoginAsync(LoginRequest request);
    Task<ApiReponse> VerifyEmailOtpAsync(VerifyEmailOtpRequest request);
    Task<ApiReponse> VerifyPhoneOtpAsync(VerifyPhoneOtpRequest request);
    Task<ApiReponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiReponse> ResetPasswordAsync(ResetPasswordRequest request);
}
