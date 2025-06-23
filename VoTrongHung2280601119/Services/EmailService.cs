using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using VoTrongHung2280601119.Models; // Thay bằng namespace chứa MailSettings của bạn

namespace VoTrongHung2280601119.Services // Thay bằng namespace của bạn
{
    // Quan trọng: Kế thừa từ IEmailSender của Identity
    public class EmailService : IEmailSender
    {
        private readonly VoTrongHung2280601119.Models.MailSettings _mailSettings; // Fully qualify the type
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<VoTrongHung2280601119.Models.MailSettings> mailSettings, ILogger<EmailService> logger) // Fully qualify the type
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));
                mimeMessage.To.Add(MailboxAddress.Parse(email));
                mimeMessage.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlMessage };
                mimeMessage.Body = builder.ToMessageBody();

                using (var smtp = new SmtpClient())
                {
                    // Kết nối tới server SMTP của Gmail
                    await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);

                    // Xác thực bằng email và Mật khẩu ứng dụng
                    await smtp.AuthenticateAsync(_mailSettings.Email, _mailSettings.AppPassword);

                    // Gửi email
                    await smtp.SendAsync(mimeMessage);

                    _logger.LogInformation($"Email đã được gửi thành công đến {email}");

                    // Ngắt kết nối
                    await smtp.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Gửi email đến {email} thất bại.");
                // Ném lại lỗi để hệ thống biết đã có vấn đề xảy ra
                throw;
            }
        }
    }
}