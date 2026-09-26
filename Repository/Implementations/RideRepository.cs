using Microsoft.EntityFrameworkCore;
using RideHailingAPI.Data;
using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Domain.Enum;
using RideHailingAPI.Repository.Interfaces;

namespace RideHailingAPI.Repository.Implementations;

public class RideRepository(AppDbContext context) : IRideRepository
{
    private readonly AppDbContext _context = context;

    public async Task AddRideAsync(Ride ride)
    {
        await _context.Rides.AddAsync(ride);
    }

    public async Task<Ride?> GetRideByIdAsync(int rideId)
    {
        return await _context.Rides
            .Include(r => r.Passenger)
            .Include(r => r.Driver)
            .ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(r => r.Id == rideId);
    }

    public async Task<List<Ride>> GetRequestedRidesAsync()
    {
        return await _context.Rides
            .Include(r => r.Passenger)
            .Where(r => r.CurrentStatus == RideStatus.Requested)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Ride>> GetRidesByPassengerIdAsync(int passengerId)
    {
        return await _context.Rides
            .Include(r => r.Passenger)
            .Include(r => r.Driver)
            .ThenInclude(d => d!.User)
            .Where(r => r.PassengerId == passengerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Ride>> GetAllRidesAsync()
    {
        return await _context.Rides
            .Include(r => r.Passenger)
            .Include(r => r.Driver)
            .ThenInclude(d => d!.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task AddRideStatusHistoryAsync(RideStatusHistory history)
    {
        await _context.RideStatusHistories.AddAsync(history);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}