using System.Net;
using System.Net.Mail;
using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.Extensions.Options;

namespace Bc_exercise_and_healthy_nutrition.Services
{
    public class EmailSender
    {
        private readonly EmailSettings _settings;

        public EmailSender(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_settings.Email, _settings.Password),
                EnableSsl = true
            };

            var mail = new MailMessage(_settings.Email, to, subject, body);

            await client.SendMailAsync(mail);
        }
    }
}