using System.Net;
using System.Net.Mail;

namespace TagAndTrack.Backend.Utils
{
    // responsible for sending emails to the user's email
    public static class Emailer
    {

        public static bool Email(string email)
        {
            try
            {
                // Set up the email details
                string fromEmail = "EncryptionDoNotReply@gmail.com"; // Sender email address
                string toEmail = email; // Recipient email address
                string subject = "Test Email";
 
                string body = $"Hello, your code is {pin}"; // ** TODO **

                string smtpServer = "smtp.gmail.com"; // Gmail SMTP server
                int smtpPort = 587; // Gmail uses port 587 for TLS
                string? emailPassword = Environment.GetEnvironmentVariable(""); // ** TODO **

                // Create the email message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail);
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;

                // Set up the SMTP client
                SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(fromEmail, emailPassword),
                    EnableSsl = true // Enable SSL for secure connection
                };

                // Send the email
                smtpClient.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                return false;
            }
        }
        
    }
}