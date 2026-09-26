using RideHailingAPI.Domain.Entities;

namespace RideHailingAPI.Repository.Interfaces;

public interface IOtpRepository
{
    Task AddEmailOtpAsync(EmailOtp otp);
    Task<EmailOtp?> GetValidEmailOtpAsync(int userId, string code);
    Task AddPhoneOtpAsync(PhoneOtp otp);
    Task<PhoneOtp?> GetValidPhoneOtpAsync(int userId, string code);
    Task AddPasswordResetOtpAsync(PasswordResetOtp otp);
    Task<PasswordResetOtp?> GetValidPasswordResetOtpAsync(int userId, string code);
    Task SaveChangesAsync();
}