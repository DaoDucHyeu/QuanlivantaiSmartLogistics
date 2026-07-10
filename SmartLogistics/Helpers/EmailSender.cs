using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace SmartLogistics.Helpers
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || password == "DÁN_APP_PASSWORD_CỦA_BẠN_VÀO_ĐÂY")
            {
                // Fallback to console logger if no password is provided
                Console.WriteLine("===============================================================");
                Console.WriteLine($"[MOCK EMAIL SENDER] To: {email}");
                Console.WriteLine($"[MOCK EMAIL SENDER] Subject: {subject}");
                Console.WriteLine($"[MOCK EMAIL SENDER] Body:\n{htmlMessage}");
                Console.WriteLine("===============================================================");
                return;
            }

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username, "SmartLogistics System"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}
