using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Ride;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RidesController(IRideService rideService) : ControllerBase
{
    private readonly IRideService _rideService = rideService;

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : 0;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    [HttpPost]
    [Authorize(Roles = "Passenger")]
    public async Task<IActionResult> RequestRide([FromBody] CreateRideRequest request)
    {
        try
        {
            var ride = await _rideService.CreateRideAsync(GetCurrentUserId(), request);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride requested successfully",
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

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Passenger")]
    public async Task<IActionResult> CancelRide(int id)
    {
        try
        {
            var ride = await _rideService.CancelRideAsync(GetCurrentUserId(), id);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride cancelled successfully",
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiReponse
            {
                ResponseCode = ResponseCodes.NotFound,
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

    [HttpGet("my-history")]
    [Authorize(Roles = "Passenger")]
    public async Task<IActionResult> GetMyRideHistory()
    {
        try
        {
            var rides = await _rideService.GetPassengerRideHistoryAsync(GetCurrentUserId());
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride history retrieved successfully",
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRideById(int id)
    {
        try
        {
            var ride = await _rideService.GetRideByIdAsync(GetCurrentUserId(), GetCurrentUserRole(), id);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Ride details retrieved successfully",
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiReponse
            {
                ResponseCode = ResponseCodes.NotFound,
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