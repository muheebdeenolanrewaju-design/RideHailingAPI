using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Ride;

namespace RideHailingAPI.Services.Interfaces;

public interface IRideService
{
    Task<ApiReponse> CreateRideAsync(int passengerUserId, CreateRideRequest request);
    Task<ApiReponse> CancelRideAsync(int passengerUserId, int rideId);
    Task<ApiReponse> GetRideByIdAsync(int userId, string userRole, int rideId);
    Task<ApiReponse> GetPassengerRideHistoryAsync(int passengerUserId);
}