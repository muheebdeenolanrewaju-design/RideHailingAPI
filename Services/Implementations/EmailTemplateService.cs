using RideHailingAPI.Services.Interfaces;

namespace RideHailingAPI.Services.Implementations;

public class EmailTemplateService : IEmailTemplateService
{
    public (string Subject, string Body) GetPasswordResetTemplate(string name, string otpCode)
    {
        string subject = "Reset Your Password";
        string body = $"""
                       <!DOCTYPE html>
                       <html>
                       <body style="font-family: Arial, sans-serif; color: #333;">
                           <h2>Hello {name},</h2>
                           <p>You requested a password reset. Use the OTP code below to complete the process:</p>
                           <div style="background: #f4f4f4; padding: 10px 20px; font-size: 24px; font-weight: bold; letter-spacing: 4px; text-align: center; width: 200px;">
                               {otpCode}
                           </div>
                           <p>This code expires in 10 minutes. If you did not request this, please ignore this email.</p>
                       </body>
                       </html>
                       """;

        return (subject, body);
    }

    public (string Subject, string Body) GetWelcomeEmailTemplate(string name)
    {
        string subject = "Welcome to RideHailingApp!";
        string body = $"""
                       <html>
                       <body>
                           <h2>Welcome, {name}!</h2>
                           <p>Thanks for registering with us. We're excited to have you on board!</p>
                       </body>
                       </html>
                       """;

        return (subject, body);
    }

    public (string Subject, string Body) GetRideReceiptTemplate(string name, string rideRef, decimal amount)
    {
        string subject = $"Your Receipt for Ride #{rideRef}";
        string body = $"""
                       <html>
                       <body>
                           <h2>Ride Completed</h2>
                           <p>Hi {name}, thank you for riding with us.</p>
                           <p><strong>Total Paid:</strong> ₦{amount:N2}</p>
                       </body>
                       </html>
                       """;

        return (subject, body);
    }
}