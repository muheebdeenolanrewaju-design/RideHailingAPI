using RideHailingAPI.Domain.Entities;

namespace RideHailingAPI.Repository.Interfaces;

public interface IDriverRepository
{
    Task<DriverProfile?> GetDriverProfileByIdAsync(int driverProfileId);
    Task<DriverProfile?> GetDriverProfileByUserIdAsync(int userId);
    Task AddDriverProfileAsync(DriverProfile driverProfile);
    Task<List<DriverProfile>> GetPendingDriversAsync();
    Task SaveChangesAsync();
}