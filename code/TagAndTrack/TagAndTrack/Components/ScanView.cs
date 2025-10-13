// ScanView.cs
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace TagAndTrack.Components
{
    public class ScanCapturedEventArgs : EventArgs
    {
        public ScanCapturedEventArgs(string text)
        {
            Text = text;
        }
        public string Text { get; }
    }

    public class ScanView : ContentView
    {
        public event EventHandler<ScanCapturedEventArgs>? ScanCaptured;

        public ScanView()
        {
            var scanner = new CameraBarcodeReaderView()
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                IsDetecting = true,
                Options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormat.QrCode,
                    AutoRotate = true,
                    Multiple = false,
                    TryHarder = true
                }
            };

            scanner.BarcodesDetected += (s, e) =>
            {
                var qr = e.Results?.FirstOrDefault();
                if (qr != null && !string.IsNullOrEmpty(qr.Value))
                    ScanCaptured?.Invoke(this, new ScanCapturedEventArgs(qr.Value));
            };

            Content = scanner;
        }
    }
}
