namespace ChoSV.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendConfirmEmail(string toEmail, string userName, string token);
    }
}
