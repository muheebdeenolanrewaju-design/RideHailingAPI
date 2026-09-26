using Microsoft.AspNetCore.Mvc;
using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Auth;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Login successful",
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

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailOtpRequest request)
    {
        try
        {
            await _authService.VerifyEmailOtpAsync(request);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Email verified successfully",
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

    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneOtpRequest request)
    {
        try
        {
            await _authService.VerifyPhoneOtpAsync(request);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Phone number verified successfully",
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

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            await _authService.ForgotPasswordAsync(request);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Password reset code sent if account exists",
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

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Password reset successfully",
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