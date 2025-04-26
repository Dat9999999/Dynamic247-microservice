using MailKit.Net.Smtp;
using MimeKit;

namespace NewsPage.helpers
{
    public class MailHelper
    {
        private readonly IConfiguration _configuration;
        private string _subject;
        private string _body;
        private readonly string _otpTemplate;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _otpTemplate = "<h1>Xác thực bắc buộc</h1><p>Mã OTP của bạn là: <strong>{0}</strong></p><p>Lưu ý mã có hiệu lực trong vòng 1 phút</p>";
        }

        public void ConfigEmail(string subject, string toEmail, string otp)
        {
            _subject = subject;
            _body = string.Format(_otpTemplate, otp);
            SendEmailAsync(toEmail).Wait();
        }

        public async Task SendEmailAsync(string toEmail)
        {
            // Kiểm tra toEmail
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new ArgumentException("Recipient email cannot be null or empty", nameof(toEmail));
            }

            // Kiểm tra định dạng email
            try
            {
                var toAddr = new System.Net.Mail.MailAddress(toEmail);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid recipient email format", toEmail);
            }

            // Lấy cấu hình email
            var emailSettings = _configuration.GetSection("EmailSettings");
            var senderEmail = emailSettings["SenderEmail"];
            var smtpServer = emailSettings["SmtpServer"];
            var port = emailSettings["Port"];
            var senderPassword = emailSettings["SenderPassword"];

            // Kiểm tra cấu hình email
            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(smtpServer) || 
                string.IsNullOrWhiteSpace(port) || string.IsNullOrWhiteSpace(senderPassword))
            {
                throw new InvalidOperationException("Email settings are missing or invalid in configuration");
            }

            // Kiểm tra định dạng sender email
            try
            {
                var fromAddr = new System.Net.Mail.MailAddress(senderEmail);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid sender email format in configuration");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Dynamic247 supporter", senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = _subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = _body };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, int.Parse(port), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendOtpEmailAsync( string subject,string toEmail, string otp)
        {
            _subject = subject;
            _body = string.Format(_otpTemplate, otp);
            await SendEmailAsync(toEmail);
        }

        public async Task SendEmailToMultipleRecipientsAsync(List<string> userEmails, string subject, string body)
        {
            _subject = subject;
            _body = body;
            
            foreach (string email in userEmails)
            {
                await SendEmailAsync(email);
            }
        }
    }
}