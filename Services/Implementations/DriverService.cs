using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Domain.Enum;
using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Auth;
using RideHailingAPI.DTOs.Driver;
using RideHailingAPI.Repository.Interfaces;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Services.Implementations;

public class DriverService : IDriverService
{
    private readonly IUserRepository _userRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IRideRepository _rideRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DriverService> _logger;

    public DriverService(
        IUserRepository userRepository,
        IDriverRepository driverRepository,
        IRideRepository rideRepository,
        IConfiguration configuration,
        ILogger<DriverService> logger)
    {
        _userRepository = userRepository;
        _driverRepository = driverRepository;
        _rideRepository = rideRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiReponse> RegisterDriverAsync(DriverRegisterRequest request)
    {
        try
        {
            var existingEmail = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingEmail is not null)
            {
                _logger.LogWarning("Driver registration failed: Email {Email} is already registered.", request.Email);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Email is already in use.",
                    Data = null
                };
            }

            var existingPhone = await _userRepository.GetUserByPhoneNumberAsync(request.PhoneNumber);
            if (existingPhone is not null)
            {
                _logger.LogWarning("Driver registration failed: Phone number {PhoneNumber} is already registered.", request.PhoneNumber);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Phone number is already in use.",
                    Data = null
                };
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email.ToLower().Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Driver,
                IsActive = true,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            var vehicle = new Vehicle
            {
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                PlateNumber = request.PlateNumber,
                Color = request.Color
            };

            var driverProfile = new DriverProfile
            {
                UserId = user.Id,
                LicenseNumber = request.LicenseNumber,
                ApprovalStatus = DriverApprovalStatus.Pending,
                IsAvailable = false,
                Vehicle = vehicle
            };

            await _driverRepository.AddDriverProfileAsync(driverProfile);
            await _driverRepository.SaveChangesAsync();

            var responseData = new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified,
                Token = GenerateJwtToken(user)
            };

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Driver account registered successfully. Account is pending admin approval.",
                Data = responseData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during driver registration for Email {Email}.", request.Email);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred during driver registration.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> GetDriverProfileAsync(int userId)
    {
        try
        {
            var driver = await _driverRepository.GetDriverProfileByUserIdAsync(userId);
            if (driver is null)
            {
                _logger.LogWarning("GetDriverProfile failed: Driver profile for User ID {UserId} not found.", userId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Driver profile not found.",
                    Data = null
                };
            }

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Driver profile retrieved successfully.",
                Data = MapToDriverProfileDto(driver)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching profile for User ID {UserId}.", userId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching the driver profile.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> ToggleAvailabilityAsync(int userId, bool isAvailable)
    {
        try
        {
            var driver = await _driverRepository.GetDriverProfileByUserIdAsync(userId);
            if (driver is null)
            {
                _logger.LogWarning("ToggleAvailability failed: Driver profile for User ID {UserId} not found.", userId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Driver profile not found.",
                    Data = null
                };
            }

            if (driver.ApprovalStatus != DriverApprovalStatus.Approved)
            {
                _logger.LogWarning("ToggleAvailability failed: Driver ID {DriverId} is not approved.", driver.Id);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Your driver account is pending approval by an Admin.",
                    Data = null
                };
            }

            driver.IsAvailable = isAvailable;
            await _driverRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = $"Driver availability status updated to {(isAvailable ? "Available" : "Unavailable")}.",
                Data = MapToDriverProfileDto(driver)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating availability for User ID {UserId}.", userId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while updating availability status.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> GetAvailableRidesAsync(int userId)
    {
        try
        {
            var driver = await _driverRepository.GetDriverProfileByUserIdAsync(userId);
            if (driver is null)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Driver profile not found.",
                    Data = null
                };
            }

            if (driver.ApprovalStatus != DriverApprovalStatus.Approved)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Account pending approval. Cannot view available rides.",
                    Data = null
                };
            }

            if (!driver.IsAvailable)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "You must set your status to Available to view ride requests.",
                    Data = null
                };
            }

            var rides = await _rideRepository.GetRequestedRidesAsync();
            var availableRides = rides.Select(MapToDriverRideDto).ToList();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Available ride requests retrieved successfully.",
                Data = availableRides
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching available rides for User ID {UserId}.", userId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching available rides.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> AcceptRideAsync(int userId, int rideId)
    {
        try
        {
            var driver = await _driverRepository.GetDriverProfileByUserIdAsync(userId);
            if (driver is null)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Driver profile not found.",
                    Data = null
                };
            }

            if (driver.ApprovalStatus != DriverApprovalStatus.Approved)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Account pending approval. Cannot accept rides.",
                    Data = null
                };
            }

            if (!driver.IsAvailable)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "You must be set as Available to accept rides.",
                    Data = null
                };
            }

            var ride = await _rideRepository.GetRideByIdAsync(rideId);
            if (ride is null)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Ride request not found.",
                    Data = null
                };
            }

            if (ride.CurrentStatus != RideStatus.Requested)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "This ride request is no longer available.",
                    Data = null
                };
            }

            ride.DriverId = driver.Id;
            ride.CurrentStatus = RideStatus.Accepted;
            driver.IsAvailable = false;

            await _rideRepository.AddRideStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                NewStatus = RideStatus.Accepted,
                Timestamp = DateTime.UtcNow,
                Notes = $"Ride accepted by driver {driver.User?.FullName}"
            });

            await _rideRepository.SaveChangesAsync();
            await _driverRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride request accepted successfully.",
                Data = MapToDriverRideDto(ride)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while accepting Ride ID {RideId} for User ID {UserId}.", rideId, userId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while accepting the ride.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> RejectRideAsync(int userId, int rideId)
    {
        try
        {
            var driver = await _driverRepository.GetDriverProfileByUserIdAsync(userId);
            if (driver is null)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Driver profile not found.",
                    Data = null
                };
            }

            if (driver.ApprovalStatus != DriverApprovalStatus.Approved)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Account pending approval.",
                    Data = null
                };
            }

            var ride = await _rideRepository.GetRideByIdAsync(rideId);
            if (ride is null)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Ride request not found.",
                    Data = null
                };
            }

            await _rideRepository.AddRideStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                NewStatus = ride.CurrentStatus,
                Timestamp = DateTime.UtcNow,
                Notes = $"Ride rejected by driver {driver.User?.FullName}"
            });

            await _rideRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride request rejected.",
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while rejecting Ride ID {RideId} for User ID {UserId}.", rideId, userId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while rejecting the ride.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> UpdateRideStatusAsync(int userId, int rideId, UpdateRideStatusRequest request)
    {
        try
        {
            var driver = await _driverRepository.GetDriverProfileByUserIdAsync(userId);
            if (driver is null)
            {
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Driver profile not found.",
                    Data = null
                };
            }

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

            if (ride.DriverId != driver.Id)
            {
                _logger.LogWarning("Unauthorized ride update: Driver ID {DriverId} attempted to update Ride ID {RideId} assigned to Driver ID {AssignedDriverId}.", driver.Id, rideId, ride.DriverId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.UnaAuthorized,
                    ResponseMessage = "You are not authorized to update a ride assigned to another driver.",
                    Data = null
                };
            }

            ride.CurrentStatus = request.NewStatus;

            if (request.NewStatus == RideStatus.Completed || request.NewStatus == RideStatus.Cancelled)
            {
                driver.IsAvailable = true;
                if (request.NewStatus == RideStatus.Completed)
                {
                    ride.CompletedAt = DateTime.UtcNow;
                }
            }

            await _rideRepository.AddRideStatusHistoryAsync(new RideStatusHistory
            {
                RideId = ride.Id,
                NewStatus = request.NewStatus,
                Timestamp = DateTime.UtcNow,
                Notes = $"Status updated to {request.NewStatus} by assigned driver"
            });

            await _rideRepository.SaveChangesAsync();
            await _driverRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = $"Ride status updated to {request.NewStatus} successfully.",
                Data = MapToDriverRideDto(ride)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating status for Ride ID {RideId} by User ID {UserId}.", rideId, userId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while updating ride status.",
                Data = null
            };
        }
    }

    private static DriverProfileDto MapToDriverProfileDto(DriverProfile driver) => new()
    {
        DriverProfileId = driver.Id,
        UserId = driver.UserId,
        FullName = driver.User?.FullName ?? string.Empty,
        Email = driver.User?.Email ?? string.Empty,
        PhoneNumber = driver.User?.PhoneNumber ?? string.Empty,
        LicenseNumber = driver.LicenseNumber,
        IsAvailable = driver.IsAvailable,
        ApprovalStatus = driver.ApprovalStatus.ToString(),
        VehicleMake = driver.Vehicle?.Make ?? string.Empty,
        VehicleModel = driver.Vehicle?.Model ?? string.Empty,
        VehicleYear = driver.Vehicle?.Year ?? 0,
        PlateNumber = driver.Vehicle?.PlateNumber ?? string.Empty,
        VehicleColor = driver.Vehicle?.Color ?? string.Empty
    };

    private static DriverRideDto MapToDriverRideDto(Ride ride) => new()
    {
        Id = ride.Id,
        RideReference = ride.RideReference,
        PassengerId = ride.PassengerId,
        PassengerName = ride.Passenger?.FullName ?? string.Empty,
        PassengerPhone = ride.Passenger?.PhoneNumber ?? string.Empty,
        PickupLocation = ride.PickupLocation,
        Destination = ride.Destination,
        EstimatedFare = ride.EstimatedFare,
        Status = ride.CurrentStatus.ToString(),
        CreatedAt = ride.CreatedAt
    };

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}