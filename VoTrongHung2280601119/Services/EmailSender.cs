using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System.Diagnostics;

namespace VoTrongHung2280601119.Services // Đảm bảo namespace này khớp với project của bạn
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Debug.WriteLine($"Simulated Email Send:");
            Debug.WriteLine($"To: {email}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Message: {htmlMessage}");
            return Task.CompletedTask;
        }
    }
}