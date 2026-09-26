using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Auth;
using RideHailingAPI.DTOs.Driver;

namespace RideHailingAPI.Services.Interfaces;

public interface IDriverService
{
    Task<ApiReponse> RegisterDriverAsync(DriverRegisterRequest request);
    Task<ApiReponse> GetDriverProfileAsync(int userId);
    Task<ApiReponse> ToggleAvailabilityAsync(int userId, bool isAvailable);
    Task<ApiReponse> GetAvailableRidesAsync(int userId);
    Task<ApiReponse> AcceptRideAsync(int userId, int rideId);
    Task<ApiReponse> RejectRideAsync(int userId, int rideId);
    Task<ApiReponse> UpdateRideStatusAsync(int userId, int rideId, UpdateRideStatusRequest request);
}