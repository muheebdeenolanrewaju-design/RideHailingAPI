using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideHailingAPI.DTOs;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _adminService.GetAllUsersAsync();
        return HandleApiResponse(result);
    }

    [HttpGet("drivers/pending")]
    public async Task<IActionResult> GetPendingDrivers()
    {
        var result = await _adminService.GetPendingDriversAsync();
        return HandleApiResponse(result);
    }

    [HttpPut("drivers/{id}/approve")]
    public async Task<IActionResult> ApproveDriver(int id)
    {
        int adminUserId = GetCurrentUserId();
        var result = await _adminService.ApproveDriverAsync(id, adminUserId);
        return HandleApiResponse(result);
    }

    [HttpPut("drivers/{id}/reject")]
    public async Task<IActionResult> RejectDriver(int id)
    {
        int adminUserId = GetCurrentUserId();
        var result = await _adminService.RejectDriverAsync(id, adminUserId);
        return HandleApiResponse(result);
    }

    [HttpPut("users/{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        int adminUserId = GetCurrentUserId();
        var result = await _adminService.ToggleUserStatusAsync(id, adminUserId);
        return HandleApiResponse(result);
    }

    [HttpGet("rides")]
    public async Task<IActionResult> GetAllRides()
    {
        var result = await _adminService.GetAllRidesAsync();
        return HandleApiResponse(result);
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var result = await _adminService.GetAuditLogsAsync();
        return HandleApiResponse(result);
    }

    private IActionResult HandleApiResponse(ApiReponse response)
    {
        return response.ResponseCode switch
        {
            ResponseCodes.Success => Ok(response),
            ResponseCodes.NotFound => NotFound(response),
            ResponseCodes.BadRequest => BadRequest(response),
            ResponseCodes.UnaAuthorized => Unauthorized(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}