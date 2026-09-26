using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Domain.Enum;
using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Admin;
using RideHailingAPI.Repository.Interfaces;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IRideRepository _rideRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository userRepository,
        IDriverRepository driverRepository,
        IRideRepository rideRepository,
        IAuditRepository auditRepository,
        ILogger<AdminService> logger)
    {
        _userRepository = userRepository;
        _driverRepository = driverRepository;
        _rideRepository = rideRepository;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task<ApiReponse> GetAllUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userList = users.Select(u => new UserManagementDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role.ToString(),
                IsEmailVerified = u.IsEmailVerified,
                IsPhoneVerified = u.IsPhoneVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Users retrieved successfully.",
                Data = userList
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching all users.");
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching users.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> GetPendingDriversAsync()
    {
        try
        {
            var drivers = await _driverRepository.GetPendingDriversAsync();
            var pendingList = drivers.Select(d => new PendingDriverDto
            {
                DriverProfileId = d.Id,
                UserId = d.UserId,
                FullName = d.User?.FullName ?? "N/A",
                Email = d.User?.Email ?? "N/A",
                PhoneNumber = d.User?.PhoneNumber ?? "N/A",
                LicenseNumber = d.LicenseNumber,
                ApprovalStatus = d.ApprovalStatus.ToString(),
                VehicleMake = d.Vehicle?.Make ?? "N/A",
                VehicleModel = d.Vehicle?.Model ?? "N/A",
                PlateNumber = d.Vehicle?.PlateNumber ?? "N/A",
                CreatedAt = d.User?.CreatedAt ?? DateTime.MinValue
            }).ToList();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Pending driver profiles retrieved successfully.",
                Data = pendingList
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching pending driver profiles.");
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching pending drivers.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> ApproveDriverAsync(int driverProfileId, int adminUserId)
    {
        try
        {
            var driver = await _driverRepository.GetDriverProfileByIdAsync(driverProfileId);
            if (driver is null)
            {
                _logger.LogWarning("ApproveDriver failed: Driver Profile ID {DriverProfileId} not found.", driverProfileId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Driver profile not found.",
                    Data = null
                };
            }

            driver.ApprovalStatus = DriverApprovalStatus.Approved;
            driver.ApprovedAt = DateTime.UtcNow;

            await _auditRepository.AddAuditLogAsync(new AuditLog
            {
                UserId = adminUserId,
                Action = "DRIVER_APPROVAL",
                Details = $"Approved driver profile ID {driverProfileId} (User: {driver.User?.Email})",
                Timestamp = DateTime.UtcNow
            });

            await _driverRepository.SaveChangesAsync();
            await _auditRepository.SaveChangesAsync();

            _logger.LogInformation("Driver profile ID {DriverProfileId} approved by Admin User ID {AdminUserId}.", driverProfileId, adminUserId);

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Driver approved successfully.",
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while approving driver profile ID {DriverProfileId} by Admin User ID {AdminUserId}.", driverProfileId, adminUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while approving driver.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> RejectDriverAsync(int driverProfileId, int adminUserId)
    {
        try
        {
            var driver = await _driverRepository.GetDriverProfileByIdAsync(driverProfileId);
            if (driver is null)
            {
                _logger.LogWarning("RejectDriver failed: Driver Profile ID {DriverProfileId} not found.", driverProfileId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "Driver profile not found.",
                    Data = null
                };
            }

            driver.ApprovalStatus = DriverApprovalStatus.Rejected;

            await _auditRepository.AddAuditLogAsync(new AuditLog
            {
                UserId = adminUserId,
                Action = "DRIVER_REJECTION",
                Details = $"Rejected driver profile ID {driverProfileId} (User: {driver.User?.Email})",
                Timestamp = DateTime.UtcNow
            });

            await _driverRepository.SaveChangesAsync();
            await _auditRepository.SaveChangesAsync();

            _logger.LogInformation("Driver profile ID {DriverProfileId} rejected by Admin User ID {AdminUserId}.", driverProfileId, adminUserId);

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Driver profile rejected.",
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while rejecting driver profile ID {DriverProfileId} by Admin User ID {AdminUserId}.", driverProfileId, adminUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while rejecting driver.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> ToggleUserStatusAsync(int userId, int adminUserId)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("ToggleUserStatus failed: User ID {UserId} not found.", userId);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "User not found.",
                    Data = null
                };
            }

            user.IsActive = !user.IsActive;

            await _auditRepository.AddAuditLogAsync(new AuditLog
            {
                UserId = adminUserId,
                Action = user.IsActive ? "USER_ACTIVATED" : "USER_DEACTIVATED",
                Details = $"Changed status for user ID {userId} to IsActive={user.IsActive}",
                Timestamp = DateTime.UtcNow
            });

            await _userRepository.SaveChangesAsync();
            await _auditRepository.SaveChangesAsync();

            _logger.LogInformation("User ID {UserId} active status set to {IsActive} by Admin User ID {AdminUserId}.", userId, user.IsActive, adminUserId);

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = $"User account status changed to {(user.IsActive ? "Active" : "Deactivated")}.",
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while toggling status for User ID {UserId} by Admin User ID {AdminUserId}.", userId, adminUserId);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while toggling user status.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> GetAllRidesAsync()
    {
        try
        {
            var rides = await _rideRepository.GetAllRidesAsync();
            var rideList = rides.Select(r => new AdminRideDto
            {
                Id = r.Id,
                RideReference = r.RideReference,
                PassengerName = r.Passenger?.FullName ?? "N/A",
                DriverName = r.Driver?.User?.FullName ?? "N/A",
                PickupLocation = r.PickupLocation,
                Destination = r.Destination,
                Status = r.CurrentStatus.ToString(),
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt
            }).ToList();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "All rides retrieved successfully.",
                Data = rideList
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching all rides.");
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching rides.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> GetAuditLogsAsync()
    {
        try
        {
            var logs = await _auditRepository.GetAuditLogsAsync();
            var logList = logs.Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                Action = l.Action,
                Details = l.Details,
                IpAddress = l.IpAddress,
                Timestamp = l.Timestamp
            }).ToList();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Audit logs retrieved successfully.",
                Data = logList
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching audit logs.");
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while fetching audit logs.",
                Data = null
            };
        }
    }
}