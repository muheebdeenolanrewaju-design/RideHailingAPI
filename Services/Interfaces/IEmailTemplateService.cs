namespace RideHailingAPI.Services.Interfaces;

public interface IEmailTemplateService
{
    (string Subject, string Body) GetPasswordResetTemplate(string name, string otpCode);
    (string Subject, string Body) GetWelcomeEmailTemplate(string name);
    (string Subject, string Body) GetRideReceiptTemplate(string name, string rideRef, decimal amount);
}