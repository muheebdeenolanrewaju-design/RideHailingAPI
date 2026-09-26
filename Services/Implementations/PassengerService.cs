using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Domain.Enum;
using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Passenger;
using RideHailingAPI.DTOs.Ride;
using RideHailingAPI.Repository.Interfaces;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Services.Implementations;

public class PassengerService : IPassengerService
{
    private readonly IUserRepository _userRepository;
    private readonly IRideRepository _rideRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly ILogger<PassengerService> _logger;

    private const decimal RatePerKm = 500m; // 500 Naira per KM

    public PassengerService(
        IUserRepository userRepository,
        IRideRepository rideRepository,
        IDriverRepository driverRepository,
        ILogger<PassengerService> logger)
    {
        _userRepository = userRepository;
        _rideRepository = rideRepository;
        _driverRepository = driverRepository;
        _logger = logger;
    }

    public async Task<ApiReponse> GetProfileAsync(int passengerUserId)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(passengerUserId);
            if (user is null)
            {
                _logger.LogWarning("GetProfile failed: User ID {UserId} not found.", passengerUserId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "User profile not found.",
                    Data = null
                };
            }

            var profile = new PassengerProfileDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Profile retrieved successfully.",
                Data = profile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving profile for User ID {UserId}.", passengerUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching the profile.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> UpdateProfileAsync(int passengerUserId, UpdatePassengerProfileDto request)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(passengerUserId);
            if (user is null)
            {
                _logger.LogWarning("UpdateProfile failed: User ID {UserId} not found.", passengerUserId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "User profile not found.",
                    Data = null
                };
            }

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;

            await _userRepository.SaveChangesAsync();

            var updatedProfile = new PassengerProfileDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Profile updated successfully.",
                Data = updatedProfile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating profile for User ID {UserId}.", passengerUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while updating the profile.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> RequestRideAsync(int passengerUserId, CreateRideRequestDto request)
    {
        try
        {
            if (request.DistanceInKm <= 0)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Distance must be greater than zero kilometers.",
                    Data = null
                };
            }

            decimal estimatedFare = request.DistanceInKm * RatePerKm;
            string rideRef = $"RIDE-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            var ride = new Ride
            {
                RideReference = rideRef,
                PassengerId = passengerUserId,
                PickupLocation = request.PickupLocation,
                Destination = request.Destination,
                EstimatedFare = estimatedFare,
                CurrentStatus = RideStatus.Requested,
                CreatedAt = DateTime.UtcNow
            };

            await _rideRepository.AddRideAsync(ride);
            await _rideRepository.SaveChangesAsync();

            await _rideRepository.AddRideStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                PreviousStatus = null,
                NewStatus = RideStatus.Requested,
                ChangedByUserId = passengerUserId,
                Timestamp = DateTime.UtcNow,
                Notes = "Ride requested by passenger"
            });

            await _rideRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride requested successfully.",
                Data = MapToRideResponseDto(ride)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while requesting a ride for User ID {UserId}.", passengerUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while placing the ride request.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> GetCurrentRideAsync(int passengerUserId)
    {
        try
        {
            var rides = await _rideRepository.GetRidesByPassengerIdAsync(passengerUserId);

            var currentRide = rides.FirstOrDefault(r =>
                r.CurrentStatus == RideStatus.Requested ||
                r.CurrentStatus == RideStatus.Accepted ||
                r.CurrentStatus == RideStatus.DriverArriving ||
                r.CurrentStatus == RideStatus.DriverArrived ||
                r.CurrentStatus == RideStatus.InProgress);

            if (currentRide is null)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "No active ride found.",
                    Data = null
                };
            }

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Active ride retrieved successfully.",
                Data = MapToRideResponseDto(currentRide)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching current ride for User ID {UserId}.", passengerUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching your current ride.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> GetRideHistoryAsync(int passengerUserId)
    {
        try
        {
            var rides = await _rideRepository.GetRidesByPassengerIdAsync(passengerUserId);
            var history = rides.Select(MapToRideResponseDto).ToList();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride history retrieved successfully.",
                Data = history
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching ride history for User ID {UserId}.", passengerUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching your ride history.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> CancelRideAsync(int passengerUserId, int rideId)
    {
        try
        {
            var ride = await _rideRepository.GetRideByIdAsync(rideId);
            if (ride is null)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Ride not found.",
                    Data = null
                };
            }

            if (ride.PassengerId != passengerUserId)
            {
                _logger.LogWarning("Unauthorized cancellation attempt: User ID {UserId} tried to cancel Ride ID {RideId}.", passengerUserId, rideId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.UnaAuthorized,
                    ResponseMessage = "You are not authorized to cancel this ride.",
                    Data = null
                };
            }

            if (ride.CurrentStatus == RideStatus.Completed || ride.CurrentStatus == RideStatus.Cancelled)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = $"Cannot cancel a ride that is already {ride.CurrentStatus}.",
                    Data = null
                };
            }

            var previousStatus = ride.CurrentStatus;
            ride.CurrentStatus = RideStatus.Cancelled;

            string cancellationNote = "Ride cancelled by passenger.";

            if (previousStatus != RideStatus.Requested)
            {
                cancellationNote += " Cancellation fee applied (Driver was already assigned).";

                if (ride.DriverId.HasValue)
                {
                    var driver = await _driverRepository.GetDriverProfileByIdAsync(ride.DriverId.Value);
                    if (driver is not null)
                    {
                        driver.IsAvailable = true;
                        await _driverRepository.SaveChangesAsync();
                    }
                }
            }

            await _rideRepository.AddRideStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                PreviousStatus = previousStatus,
                NewStatus = RideStatus.Cancelled,
                ChangedByUserId = passengerUserId,
                Timestamp = DateTime.UtcNow,
                Notes = cancellationNote
            });

            await _rideRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride cancelled successfully.",
                Data = MapToRideResponseDto(ride)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while cancelling Ride ID {RideId} for User ID {UserId}.", rideId, passengerUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while cancelling the ride.",
                Data = null
            };
        }
    }

    private static RideResponseDto MapToRideResponseDto(Ride ride) => new()
    {
        Id = ride.Id,
        RideReference = ride.RideReference,
        PassengerId = ride.PassengerId,
        PassengerName = ride.Passenger?.FullName ?? string.Empty,
        PassengerPhone = ride.Passenger?.PhoneNumber ?? string.Empty,
        DriverProfileId = ride.DriverId,
        DriverName = ride.Driver?.User?.FullName,
        DriverPhone = ride.Driver?.User?.PhoneNumber,
        PickupLocation = ride.PickupLocation,
        Destination = ride.Destination,
        EstimatedFare = ride.EstimatedFare,
        Status = ride.CurrentStatus.ToString(),
        CreatedAt = ride.CreatedAt,
        CompletedAt = ride.CompletedAt
    };
}