using Microsoft.EntityFrameworkCore;
using RideHailingAPI.Data;
using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Domain.Enum;
using RideHailingAPI.Repository.Interfaces;

namespace RideHailingAPI.Repository.Implementations;

public class DriverRepository(AppDbContext context) : IDriverRepository
{
    private readonly AppDbContext _context = context;

    public async Task<DriverProfile?> GetDriverProfileByIdAsync(int driverProfileId)
    {
        return await _context.DriverProfiles
            .Include(d => d.User)
            .Include(d => d.Vehicle)
            .FirstOrDefaultAsync(d => d.Id == driverProfileId);
    }

    public async Task<DriverProfile?> GetDriverProfileByUserIdAsync(int userId)
    {
        return await _context.DriverProfiles
            .Include(d => d.User)
            .Include(d => d.Vehicle)
            .FirstOrDefaultAsync(d => d.UserId == userId);
    }
    
    public async Task<List<DriverProfile>> GetPendingDriversAsync()
    {
        return await _context.DriverProfiles
            .Include(d => d.User)
            .Include(d => d.Vehicle)
            .Where(d => d.ApprovalStatus == DriverApprovalStatus.Pending)
            .ToListAsync();
    }

    public async Task AddDriverProfileAsync(DriverProfile driverProfile)
    {
        await _context.DriverProfiles.AddAsync(driverProfile);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}