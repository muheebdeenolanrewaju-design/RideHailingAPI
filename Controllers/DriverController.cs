using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Driver;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriverController(IDriverService driverService) : ControllerBase
{
    private readonly IDriverService _driverService = driverService;

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : 0;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] DriverRegisterRequest request)
    {
        try
        {
            var result = await _driverService.RegisterDriverAsync(request);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Driver registered successfully. Account is pending Admin approval.",
                Data = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiReponse
            {
                ResponseCode = ResponseCodes.BadRequest,
                ResponseMessage = ex.Message,
                Data = null
            });
        }
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var profile = await _driverService.GetDriverProfileAsync(GetCurrentUserId());
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Driver profile retrieved successfully",
                Data = profile
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiReponse
            {
                ResponseCode = ResponseCodes.BadRequest,
                ResponseMessage = ex.Message,
                Data = null
            });
        }
    }

    [HttpPut("availability")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> ToggleAvailability([FromBody] ToggleAvailabilityRequest request)
    {
        try
        {
            var updatedProfile = await _driverService.ToggleAvailabilityAsync(GetCurrentUserId(), request.IsAvailable);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = $"Availability status set to {(request.IsAvailable ? "Available" : "Unavailable")}",
                Data = updatedProfile
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiReponse
            {
                ResponseCode = ResponseCodes.BadRequest,
                ResponseMessage = ex.Message,
                Data = null
            });
        }
    }

    [HttpGet("available-rides")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> GetAvailableRides()
    {
        try
        {
            var rides = await _driverService.GetAvailableRidesAsync(GetCurrentUserId());
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Available ride requests retrieved successfully",
                Data = rides
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiReponse
            {
                ResponseCode = ResponseCodes.BadRequest,
                ResponseMessage = ex.Message,
                Data = null
            });
        }
    }

    [HttpPost("rides/{rideId}/accept")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> AcceptRide(int rideId)
    {
        try
        {
            var ride = await _driverService.AcceptRideAsync(GetCurrentUserId(), rideId);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride request accepted successfully",
                Data = ride
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiReponse
            {
                ResponseCode = ResponseCodes.BadRequest,
                ResponseMessage = ex.Message,
                Data = null
            });
        }
    }

    [HttpPost("rides/{rideId}/reject")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> RejectRide(int rideId)
    {
        try
        {
            await _driverService.RejectRideAsync(GetCurrentUserId(), rideId);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride request rejected",
                Data = null
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiReponse
            {
                ResponseCode = ResponseCodes.BadRequest,
                ResponseMessage = ex.Message,
                Data = null
            });
        }
    }

    [HttpPut("rides/{rideId}/status")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> UpdateRideStatus(int rideId, [FromBody] UpdateRideStatusRequest request)
    {
        try
        {
            var ride = await _driverService.UpdateRideStatusAsync(GetCurrentUserId(), rideId, request);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride status updated successfully",
                Data = ride
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiReponse
            {
                ResponseCode = ResponseCodes.UnaAuthorized,
                ResponseMessage = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiReponse
            {
                ResponseCode = ResponseCodes.BadRequest,
                ResponseMessage = ex.Message,
                Data = null
            });
        }
    }
}