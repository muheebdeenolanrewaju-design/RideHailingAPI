using Microsoft.EntityFrameworkCore;
using RideHailingAPI.Data;
using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Repository.Interfaces;

namespace RideHailingAPI.Repository.Implementations;

public class OtpRepository(AppDbContext context) : IOtpRepository
{
    private readonly AppDbContext _context = context;

    public async Task AddEmailOtpAsync(EmailOtp otp)
    {
        await _context.EmailOtps.AddAsync(otp);
    }

    public async Task<EmailOtp?> GetValidEmailOtpAsync(int userId, string code)
    {
        return await _context.EmailOtps
            .Where(o => o.UserId == userId && o.Code == code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddPhoneOtpAsync(PhoneOtp otp)
    {
        await _context.PhoneOtps.AddAsync(otp);
    }

    public async Task<PhoneOtp?> GetValidPhoneOtpAsync(int userId, string code)
    {
        return await _context.PhoneOtps
            .Where(o => o.UserId == userId && o.Code == code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddPasswordResetOtpAsync(PasswordResetOtp otp)
    {
        await _context.PasswordResetOtps.AddAsync(otp);
    }

    public async Task<PasswordResetOtp?> GetValidPasswordResetOtpAsync(int userId, string code)
    {
        return await _context.PasswordResetOtps
            .Where(o => o.UserId == userId && o.Code == code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}