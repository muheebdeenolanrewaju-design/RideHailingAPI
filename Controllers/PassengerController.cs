using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideHailingAPI.DTOs.Passenger;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PassengerController : ControllerBase
{
    private readonly IPassengerService _passengerService;

    public PassengerController(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("Invalid authentication token.");
        }
        return int.Parse(userIdClaim);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        int userId = GetCurrentUserId();
        var profile = await _passengerService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePassengerProfileDto request)
    {
        int userId = GetCurrentUserId();
        var updatedProfile = await _passengerService.UpdateProfileAsync(userId, request);
        return Ok(updatedProfile);
    }

    [HttpPost("rides")]
    public async Task<IActionResult> RequestRide([FromBody] CreateRideRequestDto request)
    {
        int userId = GetCurrentUserId();
        var ride = await _passengerService.RequestRideAsync(userId, request);
        return Ok(ride);
    }

    [HttpGet("rides/current")]
    public async Task<IActionResult> GetCurrentRide()
    {
        int userId = GetCurrentUserId();
        var ride = await _passengerService.GetCurrentRideAsync(userId);
        if (ride is null) return NotFound("No active ride found.");
        return Ok(ride);
    }

    [HttpGet("rides/history")]
    public async Task<IActionResult> GetRideHistory()
    {
        int userId = GetCurrentUserId();
        var history = await _passengerService.GetRideHistoryAsync(userId);
        return Ok(history);
    }

    [HttpPost("rides/{rideId:int}/cancel")]
    public async Task<IActionResult> CancelRide(int rideId)
    {
        int userId = GetCurrentUserId();
        var cancelledRide = await _passengerService.CancelRideAsync(userId, rideId);
        return Ok(cancelledRide);
    }
}