using ChoSV.Configurations;
using ChoSV.Services.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace ChoSV.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        public EmailService(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MailMessage
            {
                From = new MailAddress(_emailConfig.From),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            email.To.Add(toEmail);

            using var smtp = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.Port)
            {
                Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password),
                EnableSsl = true
            };
            await smtp.SendMailAsync(email);
        }
        public async Task SendConfirmEmailAsync(string toEmail, string userName, string token)
        {
            var encodedToken = HttpUtility.UrlEncode(token);
            var encodedEmail = HttpUtility.UrlEncode(toEmail);
            var confirmationLink = $"http://localhost:5173/emailConfirmed?token={encodedToken}&email={encodedEmail}";

            await SendEmailAsync(toEmail, "Xác nhận email của bạn – ChoSV",
                    $"<div style='font-family: Time New Roman; font-size: 18px; color: black;'>Xin chào {userName},  " +
                    "<br><br>Cảm ơn bạn đã đăng ký tài khoản tại ChoSV. Vui lòng xác nhận email của bạn bằng cách nhấp vào đường dẫn dưới đây:" +
                    $"<br><br><a href='{confirmationLink}'>Xác nhận email</a>" +
                    "<br><br>Nếu bạn không thực hiện, xin hãy bỏ qua email này." +
                    "<br><br>Trân trọng,  <br>ChoSV." +
                    "</div>");
        }

        public async Task SendForgotPasswordEmailAsync(string toEmail, string userName, string token)
        {
            var encodedToken = HttpUtility.UrlEncode(token);
            var encodedEmail = HttpUtility.UrlEncode(toEmail);
            var forgotPasswordLink = $"http://localhost:5173/resetPassword?token={encodedToken}&email={encodedEmail}";
            await SendEmailAsync(toEmail, "Đặt lại mật khẩu của bạn – ChoSV",
                     $"<div style='font-family: Time New Roman; font-size: 18px; color: black;'>Xin chào {userName},  " +
                    "<br><br>Vui lòng đặt lại mật khẩu của bạn bằng cách nhấp vào đường dẫn dưới đây:" +
                    $"<br><br><a href='{forgotPasswordLink}'>Đổi lại mật khẩu</a>" +
                    "<br><br>Nếu bạn không thực hiện, xin hãy bỏ qua email này." +
                    "<br><br>Trân trọng,  <br>ChoSV." +
                    "</div>");
        }

        public async Task SendChangedPasswordEmailAsync(string toEmail, string userName)
        {
            await SendEmailAsync(toEmail, "Mật khẩu của bạn đã được cập nhật – ChoSV",
                        $"<div style='font-family: Time New Roman; font-size: 18px; color: black;'>Xin chào {userName},  " +
                        "<br><br>Mật khẩu của bạn đã được thay đổi thành công. Nếu bạn đã thực hiện thay đổi này, không cần làm gì thêm.  " +
                        "<br><br>Nếu bạn không thực hiện, vui lòng liên hệ ngay với chúng tôi tại chosvvn24@gmail.com để bảo vệ tài khoản của bạn.  " +
                        "<br><br>Trân trọng,  <br>ChoSV." +
                        "</div>");
        }
    }
}
