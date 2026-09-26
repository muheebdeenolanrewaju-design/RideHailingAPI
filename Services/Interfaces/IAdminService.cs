using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Admin;

namespace RideHailingAPI.Services.Interfaces;

public interface IAdminService
{
    Task<ApiReponse> GetAllUsersAsync();
    Task<ApiReponse> GetPendingDriversAsync();
    Task<ApiReponse> ApproveDriverAsync(int driverProfileId, int adminUserId);
    Task<ApiReponse> RejectDriverAsync(int driverProfileId, int adminUserId);
    Task<ApiReponse> ToggleUserStatusAsync(int userId, int adminUserId);
    Task<ApiReponse> GetAllRidesAsync();
    Task<ApiReponse> GetAuditLogsAsync();
}