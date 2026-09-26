using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RideHailingAPI.Domain.Entities;
using RideHailingAPI.DTOs;
using RideHailingAPI.DTOs.Auth;
using RideHailingAPI.Repository.Interfaces;
using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiReponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailOrPhoneAsync(request.Identifier);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login attempt failed for identifier: {Identifier}", request.Identifier);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Invalid credentials.",
                    Data = null
                };
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for deactivated user: {UserId}", user.Id);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Your account has been deactivated. Please contact support.",
                    Data = null
                };
            }

            var token = GenerateJwtToken(user);

            var responseData = new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified,
                Token = token
            };

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Login successful.",
                Data = responseData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for identifier {Identifier}.", request.Identifier);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred during login.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> VerifyEmailOtpAsync(VerifyEmailOtpRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user is null)
            {
                _logger.LogWarning("Email verification failed: User not found for email {Email}", request.Email);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "User not found.",
                    Data = null
                };
            }

            var otp = await _otpRepository.GetValidEmailOtpAsync(user.Id, request.OtpCode);
            if (otp is null)
            {
                _logger.LogWarning("Email verification failed: Invalid/Expired OTP for User ID {UserId}", user.Id);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Invalid or expired OTP.",
                    Data = null
                };
            }

            otp.IsUsed = true;
            user.IsEmailVerified = true;

            await _otpRepository.SaveChangesAsync();
            await _userRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Email verified successfully.",
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during email verification for {Email}.", request.Email);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while verifying email.",
                Data = null
            };
        }
    }

    public async Task<ApiReponse> VerifyPhoneOtpAsync(VerifyPhoneOtpRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByPhoneNumberAsync(request.PhoneNumber);
            if (user is null)
            {
                _logger.LogWarning("Phone verification failed: User not found for phone number {PhoneNumber}", request.PhoneNumber);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "User not found.",
                    Data = null
                };
            }

            var otp = await _otpRepository.GetValidPhoneOtpAsync(user.Id, request.OtpCode);
            if (otp is null)
            {
                _logger.LogWarning("Phone verification failed: Invalid/Expired OTP for User ID {UserId}", user.Id);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Invalid or expired OTP.",
                    Data = null
                };
            }

            otp.IsUsed = true;
            user.IsPhoneVerified = true;

            await _otpRepository.SaveChangesAsync();
            await _userRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Phone number verified successfully.",
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during phone verification for {PhoneNumber}.", request.PhoneNumber);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while verifying phone number.",
                Data = null
            };
        }
    }

   public async Task<ApiReponse> ForgotPasswordAsync(ForgotPasswordRequest request)
{
    try
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("Forgot password requested for non-existent email: {Email}", request.Email);
            // Return success message to prevent user enumeration
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "If an account exists with this email, a password reset code has been sent.",
                Data = null
            };
        }

        var otpCode = new Random().Next(100000, 999999).ToString();
        var otp = new PasswordResetOtp
        {
            UserId = user.Id,
            Code = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _otpRepository.AddPasswordResetOtpAsync(otp);
        await _otpRepository.SaveChangesAsync();

        // 1. Generate the subject and body from the template service
        var (subject, body) = _emailTemplateService.GetPasswordResetTemplate(user.FullName, otpCode);

        // 2. Pass the template outputs into the email delivery service
        await _emailService.SendEmailAsync(user.Email, subject, body);

        return new ApiReponse
        {
            ResponseCode = ResponseCodes.Success,
            ResponseMessage = "If an account exists with this email, a password reset code has been sent.",
            Data = null
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred during forgot password request for {Email}.", request.Email);
        return new ApiReponse
        {
            ResponseCode = ResponseCodes.ServerError,
            ResponseMessage = "An unexpected error occurred while processing password reset request.",
            Data = null
        };
    }
}

    public async Task<ApiReponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user is null)
            {
                _logger.LogWarning("Reset password failed: User not found for email {Email}", request.Email);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.NotFound,
                    ResponseMessage = "User not found.",
                    Data = null
                };
            }

            var otp = await _otpRepository.GetValidPasswordResetOtpAsync(user.Id, request.OtpCode);
            if (otp is null)
            {
                _logger.LogWarning("Reset password failed: Invalid or expired OTP for User ID {UserId}", user.Id);
                return new ApiReponse
                {
                    ResponseCode = ResponseCodes.BadRequest,
                    ResponseMessage = "Invalid or expired OTP.",
                    Data = null
                };
            }

            otp.IsUsed = true;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _otpRepository.SaveChangesAsync();
            await _userRepository.SaveChangesAsync();

            return new ApiReponse
            {
                ResponseCode = ResponseCodes.Success,
                ResponseMessage = "Password has been reset successfully.",
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during password reset for {Email}.", request.Email);
            return new ApiReponse
            {
                ResponseCode = ResponseCodes.ServerError,
                ResponseMessage = "An unexpected error occurred while resetting password.",
                Data = null
            };
        }
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
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