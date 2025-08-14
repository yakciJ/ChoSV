namespace ChoSV.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendConfirmEmailAsync(string toEmail, string userName, string token);
        Task SendForgotPasswordEmailAsync(string toEmail, string userName, string token);
        Task SendChangedPasswordEmailAsync(string toEmail, string userName);
        // thêm delete email khi người dùng xóa tài khoản.
    }
}
