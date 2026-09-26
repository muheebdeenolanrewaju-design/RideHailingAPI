using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Passenger;
using RideHailingAPI.DTOs.Ride;

namespace RideHailingAPI.Services.Interfaces;

public interface IPassengerService
{
    Task<ApiReponse> GetProfileAsync(int passengerUserId);
    Task<ApiReponse> UpdateProfileAsync(int passengerUserId, UpdatePassengerProfileDto request);
    Task<ApiReponse> RequestRideAsync(int passengerUserId, CreateRideRequestDto request);
    Task<ApiReponse> GetCurrentRideAsync(int passengerUserId);
    Task<ApiReponse> GetRideHistoryAsync(int passengerUserId);
    Task<ApiReponse> CancelRideAsync(int passengerUserId, int rideId);
}