using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace TagAndTrack.Backend.Utils
{
    // responsible for sending emails to the user's email
    public static class Emailer
    {
        /// <summary>
        /// Sends an HTML-formatted email. If signatureData is provided,
        /// the borrower's handwritten signature is rendered inline as SVG.
        /// Returns (success, errorMessage). errorMessage is null on success.
        /// </summary>
        public static (bool Success, string? Error) Email(string email, string subject, string body, byte[]? signatureData = null)
        {
            try
            {
                DebugLogger.Log($"attempting to send email to {email}");

                const string fromEmail = "TagAndTrackWSU@gmail.com"; // must match the Gmail account the app password is for
                // ⚠️ SET THIS BEFORE BUILDING — intentionally blank in git for security
                const string appPassword = "";        // 16-char Google app password, no spaces

                // Append signature SVG to the existing HTML body if available
                var finalBody = body;
                if (signatureData != null && signatureData.Length > 0)
                {
                    var svgMarkup = StrokesToSvg(signatureData);
                    if (svgMarkup != null)
                    {
                        finalBody += "<br><b>Borrower Signature:</b><br>"
                                   + $"<div style='border:1px solid #ccc; padding:8px; display:inline-block; background:#fff;'>{svgMarkup}</div>";
                    }
                    else
                    {
                        finalBody += "<br><p><em>Borrower signature is on file.</em></p>";
                    }
                }

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, "Tag and Track");
                    mail.To.Add(email);
                    mail.Subject = subject;
                    mail.Body = finalBody;
                    mail.IsBodyHtml = true;

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

                return (true, null);
            }
            catch (Exception ex)
            {
                DebugLogger.Log("Error sending email: " + ex.Message);
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    DebugLogger.Log("Inner exception: " + ex.InnerException.Message);
                    errorMsg += $"\nInner: {ex.InnerException.Message}";
                }
                return (false, errorMsg);
            }
        }

        /// <summary>
        /// Converts JSON stroke data (float[][][]) into an inline SVG element.
        /// Returns null if data cannot be parsed.
        /// </summary>
        private static string? StrokesToSvg(byte[] signatureData)
        {
            try
            {
                var json = Encoding.UTF8.GetString(signatureData);
                var strokes = JsonSerializer.Deserialize<float[][][]>(json);
                if (strokes == null || strokes.Length == 0) return null;

                // Find bounding box
                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;
                foreach (var stroke in strokes)
                    foreach (var pt in stroke)
                    {
                        if (pt[0] < minX) minX = pt[0];
                        if (pt[1] < minY) minY = pt[1];
                        if (pt[0] > maxX) maxX = pt[0];
                        if (pt[1] > maxY) maxY = pt[1];
                    }

                float pad = 10f;
                float width = maxX - minX + pad * 2;
                float height = maxY - minY + pad * 2;

                var svgSb = new StringBuilder();
                svgSb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{width:F0}' height='{height:F0}' viewBox='0 0 {width:F1} {height:F1}'>");

                foreach (var stroke in strokes)
                {
                    if (stroke.Length < 2) continue;
                    svgSb.Append("<polyline points='");
                    foreach (var pt in stroke)
                    {
                        svgSb.Append($"{pt[0] - minX + pad:F1},{pt[1] - minY + pad:F1} ");
                    }
                    svgSb.Append("' fill='none' stroke='black' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/>");
                }

                svgSb.Append("</svg>");
                return svgSb.ToString();
            }
            catch (Exception ex)
            {
                DebugLogger.Log("Failed to convert signature to SVG: " + ex.Message);
                return null;
            }
        }
    }
}