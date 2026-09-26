using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Sending email to [{To}] | Subject: {Subject}", to, subject);

            // Mock email dispatch (Replace with SMTP / SendGrid logic later)
            await Task.Delay(100); 

            _logger.LogInformation("EMAIL SENT SUCCESSFULLY TO [{To}] | Subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to [{To}] | Subject: {Subject}", to, subject);
            throw; // Rethrow so calling service can react if email delivery fails
        }
    }
}