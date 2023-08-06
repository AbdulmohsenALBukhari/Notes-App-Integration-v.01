using System.Net.Mail;
using System.Net;
using Notes_App_Integration_v._01.ModelViews;

namespace Notes_App_Integration_v._01.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var mail = "mr.7sooon.27@gmail.com";
            var pw = "qwertyuiop0921";
            var client = new SmtpClient("smtp.office365.com", 587)
            {
                EnableSsl = true,
                //    UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mail, pw)
            };

            return client.SendMailAsync(
                new MailMessage(from: mail,
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
