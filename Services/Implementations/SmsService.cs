using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Services.Implementations;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation("Sending SMS to [{PhoneNumber}]", phoneNumber);

            // Mock SMS dispatch (Replace with Twilio / Termii logic later)
            await Task.Delay(100);

            _logger.LogInformation("SMS SENT SUCCESSFULLY TO [{PhoneNumber}]", phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to [{PhoneNumber}]", phoneNumber);
            throw;
        }
    }
}