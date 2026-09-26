using RideHailingAPI.Domain.Entities;

namespace RideHailingAPI.Repository.Interfaces;

public interface IRideRepository
{
    Task AddRideAsync(Ride ride);
    Task<Ride?> GetRideByIdAsync(int rideId);
    Task<List<Ride>> GetRequestedRidesAsync();
    Task<List<Ride>> GetRidesByPassengerIdAsync(int passengerId);
    Task<List<Ride>> GetAllRidesAsync();
    Task AddRideStatusHistoryAsync(RideStatusHistory history);
    Task SaveChangesAsync();
}