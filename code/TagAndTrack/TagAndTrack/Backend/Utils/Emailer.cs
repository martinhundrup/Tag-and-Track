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
        /// the borrower's handwritten signature is embedded as inline SVG in the HTML body.
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

                var finalBody = body;

                // Embed signature as inline SVG in HTML
                if (signatureData != null && signatureData.Length > 0)
                {
                    var signatureSvg = StrokesToSvg(signatureData);
                    if (signatureSvg != null)
                    {
                        var signatureBlock = "<br><b>Borrower Signature:</b><br>"
                                           + $"<div style='border:1px solid #ccc; padding:8px; display:inline-block; background:#fff;'>"
                                           + signatureSvg
                                           + "</div>";
                        finalBody = InsertBeforeBodyClose(finalBody, signatureBlock);
                    }
                    else
                    {
                        finalBody = InsertBeforeBodyClose(finalBody, "<br><p><em>Borrower signature is on file.</em></p>");
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
        /// Inserts HTML content right before </body> if present; otherwise appends.
        /// </summary>
        private static string InsertBeforeBodyClose(string html, string content)
        {
            var bodyCloseIndex = html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (bodyCloseIndex >= 0)
            {
                return html.Insert(bodyCloseIndex, content);
            }
            return html + content;
        }

        /// <summary>
        /// Converts JSON stroke data (float[][][]) into an inline SVG string.
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
                var pointCount = 0;
                foreach (var stroke in strokes)
                    foreach (var pt in stroke)
                    {
                        pointCount++;
                        if (pt[0] < minX) minX = pt[0];
                        if (pt[1] < minY) minY = pt[1];
                        if (pt[0] > maxX) maxX = pt[0];
                        if (pt[1] > maxY) maxY = pt[1];
                    }

                if (pointCount == 0)
                    return null;

                var dataWidth = Math.Max(1f, maxX - minX);
                var dataHeight = Math.Max(1f, maxY - minY);

                const float targetWidth = 1200f;
                const float targetHeight = 440f;
                const float pad = 20f;

                var scaleX = (targetWidth - pad * 2) / dataWidth;
                var scaleY = (targetHeight - pad * 2) / dataHeight;
                var scale = Math.Min(scaleX, scaleY);

                var usedWidth = dataWidth * scale;
                var usedHeight = dataHeight * scale;
                var offsetX = (targetWidth - usedWidth) / 2f;
                var offsetY = (targetHeight - usedHeight) / 2f;

                var svg = new StringBuilder();
                svg.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='300' height='110' viewBox='0 0 {targetWidth:F0} {targetHeight:F0}' preserveAspectRatio='xMidYMid meet' style='display:block;'>");
                svg.Append($"<rect x='1' y='1' width='{targetWidth - 2:F0}' height='{targetHeight - 2:F0}' fill='white' stroke='#dddddd' stroke-width='2' />");

                foreach (var stroke in strokes)
                {
                    if (stroke.Length == 0) continue;

                    if (stroke.Length == 1)
                    {
                        var dotX = (stroke[0][0] - minX) * scale + offsetX;
                        var dotY = (stroke[0][1] - minY) * scale + offsetY;
                        svg.Append($"<circle cx='{dotX:F1}' cy='{dotY:F1}' r='4' fill='black' />");
                        continue;
                    }

                    svg.Append("<polyline points='");
                    for (int i = 0; i < stroke.Length; i++)
                    {
                        var x = (stroke[i][0] - minX) * scale + offsetX;
                        var y = (stroke[i][1] - minY) * scale + offsetY;
                        svg.Append($"{x:F1},{y:F1} ");
                    }
                    svg.Append("' fill='none' stroke='black' stroke-width='8' stroke-linecap='round' stroke-linejoin='round' />");
                }

                svg.Append("</svg>");
                return svg.ToString();
            }
            catch (Exception ex)
            {
                DebugLogger.Log("Failed to convert signature to SVG: " + ex.Message);
                return null;
            }
        }
    }
}