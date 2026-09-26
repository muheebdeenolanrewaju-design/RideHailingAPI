using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Domain.Enum;
using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Ride;
using RideHailingAPI.Repository.Interfaces;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Services.Implementations;

public class RideService : IRideService
{
    private readonly IRideRepository _rideRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly ILogger<RideService> _logger;

    public RideService(
        IRideRepository rideRepository,
        IUserRepository userRepository,
        IDriverRepository driverRepository,
        ILogger<RideService> logger)
    {
        _rideRepository = rideRepository;
        _userRepository = userRepository;
        _driverRepository = driverRepository;
        _logger = logger;
    }

    public async Task<ApiReponse> CreateRideAsync(int passengerUserId, CreateRideRequest request)
    {
        try
        {
            var passenger = await _userRepository.GetUserByIdAsync(passengerUserId);
            if (passenger is null)
            {
                _logger.LogWarning("CreateRide failed: Passenger User ID {UserId} not found.", passengerUserId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Passenger user profile not found.",
                    Data = null
                };
            }

            decimal estimatedFare = CalculateEstimatedFare(request.PickupLocation, request.Destination);

            var ride = new Ride
            {
                RideReference = $"RIDE-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
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
                NewStatus = RideStatus.Requested,
                Timestamp = DateTime.UtcNow,
                Notes = "Ride requested by passenger"
            });

            await _rideRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride created successfully.",
                Data = MapToRideResponseDto(ride)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a ride for User ID {UserId}.", passengerUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while creating the ride.",
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
                _logger.LogWarning("CancelRide failed: Ride ID {RideId} not found.", rideId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Ride not found.",
                    Data = null
                };
            }

            // Resource Ownership Check: Ensure the ride belongs to this passenger
            if (ride.PassengerId != passengerUserId)
            {
                _logger.LogWarning("Unauthorized cancellation: User ID {UserId} attempted to cancel Ride ID {RideId} owned by User ID {OwnerId}.", passengerUserId, rideId, ride.PassengerId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.UnaAuthorized,
                    ResponseMessage = "You are not authorized to cancel another passenger's ride.",
                    Data = null
                };
            }

            if (ride.CurrentStatus == RideStatus.Completed || ride.CurrentStatus == RideStatus.Cancelled)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = $"Cannot cancel a ride that is already {ride.CurrentStatus.ToString().ToLower()}.",
                    Data = null
                };
            }

            var previousStatus = ride.CurrentStatus;
            ride.CurrentStatus = RideStatus.Cancelled;

            if (ride.DriverId.HasValue)
            {
                var driver = await _driverRepository.GetDriverProfileByIdAsync(ride.DriverId.Value);
                if (driver is not null)
                {
                    driver.IsAvailable = true;
                    await _driverRepository.SaveChangesAsync();
                }
            }

            await _rideRepository.AddRideStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                PreviousStatus = previousStatus,
                NewStatus = RideStatus.Cancelled,
                ChangedByUserId = passengerUserId,
                Timestamp = DateTime.UtcNow,
                Notes = "Ride cancelled by passenger"
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

    public async Task<ApiReponse> GetRideByIdAsync(int userId, string userRole, int rideId)
    {
        try
        {
            var ride = await _rideRepository.GetRideByIdAsync(rideId);
            if (ride is null)
            {
                _logger.LogWarning("GetRideById failed: Ride ID {RideId} not found.", rideId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Ride not found.",
                    Data = null
                };
            }

            // Resource Ownership Validation
            if (userRole == "Passenger" && ride.PassengerId != userId)
            {
                _logger.LogWarning("Unauthorized access: Passenger User ID {UserId} requested Ride ID {RideId}.", userId, rideId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.UnaAuthorized,
                    ResponseMessage = "You are not authorized to access this ride.",
                    Data = null
                };
            }

            if (userRole == "Driver")
            {
                var driver = await _driverRepository.GetDriverProfileByUserIdAsync(userId);
                if (driver is null || ride.DriverId != driver.Id)
                {
                    _logger.LogWarning("Unauthorized access: Driver User ID {UserId} requested Ride ID {RideId}.", userId, rideId);
                    return new ApiReponse
                    {
                        ResponseCode = ResponseCodes.UnaAuthorized,
                        ResponseMessage = "You are not authorized to access this ride.",
                        Data = null
                    };
                }
            }

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride details retrieved successfully.",
                Data = MapToRideResponseDto(ride)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching Ride ID {RideId} for User ID {UserId}.", rideId, userId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching ride details.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> GetPassengerRideHistoryAsync(int passengerUserId)
    {
        try
        {
            var rides = await _rideRepository.GetRidesByPassengerIdAsync(passengerUserId);
            var history = rides.Select(MapToRideResponseDto).ToList();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Passenger ride history retrieved successfully.",
                Data = history
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching ride history for Passenger User ID {UserId}.", passengerUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching ride history.",
                Data = null
            };
        }
    }

    private static decimal CalculateEstimatedFare(string pickup, string destination)
    {
        decimal baseFare = 1000m;
        int distanceFactor = Math.Abs(pickup.Length - destination.Length) + 5;
        return baseFare + (distanceFactor * 150m);
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