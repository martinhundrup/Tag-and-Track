using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using SkiaSharp;

namespace TagAndTrack.Backend.Utils
{
    // responsible for sending emails to the user's email
    public static class Emailer
    {
        /// <summary>
        /// Sends an HTML-formatted email. If signatureData is provided,
        /// the borrower's handwritten signature is embedded as a PNG in the HTML body.
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

                // Embed signature PNG directly in HTML (no file attachment)
                if (signatureData != null && signatureData.Length > 0)
                {
                    var signaturePng = StrokesToPng(signatureData);
                    if (signaturePng != null)
                    {
                        var base64Png = Convert.ToBase64String(signaturePng);
                        var signatureBlock = "<br><b>Borrower Signature:</b><br>"
                                           + $"<div style='border:1px solid #ccc; padding:8px; display:inline-block; background:#fff;'>"
                                           + $"<img alt='Borrower signature' src='data:image/png;base64,{base64Png}' style='display:block; width:300px; height:auto;' />"
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
        /// Converts JSON stroke data (float[][][]) into a PNG image byte array.
        /// Returns null if data cannot be parsed.
        /// </summary>
        private static byte[]? StrokesToPng(byte[] signatureData)
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

                const int targetWidth = 1200;
                const int targetHeight = 440;
                const float pad = 20f;

                var scaleX = (targetWidth - pad * 2) / dataWidth;
                var scaleY = (targetHeight - pad * 2) / dataHeight;
                var scale = Math.Min(scaleX, scaleY);

                var usedWidth = dataWidth * scale;
                var usedHeight = dataHeight * scale;
                var offsetX = (targetWidth - usedWidth) / 2f;
                var offsetY = (targetHeight - usedHeight) / 2f;

                using var bitmap = new SKBitmap(targetWidth, targetHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.White);

                using (var borderPaint = new SKPaint
                {
                    Color = new SKColor(221, 221, 221),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2,
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(new SKRect(1, 1, targetWidth - 1, targetHeight - 1), borderPaint);
                }

                using var signaturePaint = new SKPaint
                {
                    Color = SKColors.Black,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 8,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round,
                    IsAntialias = true
                };

                foreach (var stroke in strokes)
                {
                    if (stroke.Length == 0) continue;

                    if (stroke.Length == 1)
                    {
                        var dotX = (stroke[0][0] - minX) * scale + offsetX;
                        var dotY = (stroke[0][1] - minY) * scale + offsetY;
                        canvas.DrawCircle(dotX, dotY, 4, signaturePaint);
                        continue;
                    }

                    using var path = new SKPath();
                    var startX = (stroke[0][0] - minX) * scale + offsetX;
                    var startY = (stroke[0][1] - minY) * scale + offsetY;
                    path.MoveTo(startX, startY);

                    for (int i = 1; i < stroke.Length; i++)
                    {
                        var x = (stroke[i][0] - minX) * scale + offsetX;
                        var y = (stroke[i][1] - minY) * scale + offsetY;
                        path.LineTo(x, y);
                    }

                    canvas.DrawPath(path, signaturePaint);
                }

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                return data.ToArray();
            }
            catch (Exception ex)
            {
                DebugLogger.Log("Failed to convert signature to PNG: " + ex.Message);
                return null;
            }
        }
    }
}