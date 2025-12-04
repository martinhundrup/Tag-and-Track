using System.Data.Common;
using System.Net;
using System.Net.Mail;

namespace TagAndTrack.Backend.Utils
{
    // responsible for sending emails to the user's email
    public static class Emailer
    {

        public static bool Email(string email, string subject, string body)
        {
            try
            {
                DebugLogger.Log($"attempting to send email to {email}");
                
                const string fromEmail = "TagAndTrackWSU@gmail.com"; // must match the Gmail account the app password is for
                const string appPassword = "";        // 16-char app password, no spaces

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail);
                    mail.To.Add(email);
                    mail.Subject = subject;
                    mail.Body = body;

                    using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                        // critical bits for Gmail auth
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(fromEmail, appPassword);

                        smtpClient.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.Log("Error sending email: " + ex.Message);
                if (ex.InnerException != null)
                {
                    DebugLogger.Log("Inner exception: " + ex.InnerException.Message);
                }
                return false;
            }
        }
    }
}