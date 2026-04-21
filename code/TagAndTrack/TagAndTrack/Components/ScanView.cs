// ScanView, the keeper of the scanning eye
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

        // Retain a reference, that we might command it beyond the constructor's bounds
        private readonly CameraBarcodeReaderView _scanner;
        private Page? _parentPage;

        public ScanView()
        {
            _scanner = new CameraBarcodeReaderView
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

            _scanner.BarcodesDetected += (s, e) =>
            {
                var qr = e.Results?.FirstOrDefault();
                if (qr != null && !string.IsNullOrEmpty(qr.Value))
                    ScanCaptured?.Invoke(this, new ScanCapturedEventArgs(qr.Value));
            };

            Content = _scanner;
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            // Sever ties with the former page, if such a bond didst exist
            if (_parentPage != null)
            {
                _parentPage.Appearing -= OnPageAppearing;
                _parentPage.Disappearing -= OnPageDisappearing;
            }

            // Seek out the new parent page
            _parentPage = FindParentPage();

            if (_parentPage != null)
            {
                _parentPage.Appearing += OnPageAppearing;
                _parentPage.Disappearing += OnPageDisappearing;
            }
        }

        private Page? FindParentPage()
        {
            Element? parent = Parent;
            while (parent != null && parent is not Page)
            {
                parent = parent.Parent;
            }
            return parent as Page;
        }

        private void OnPageAppearing(object? sender, EventArgs e)
        {
            // Rouse the scanning forth anew when the page doth reveal itself
            _scanner.IsDetecting = true;
        }

        private void OnPageDisappearing(object? sender, EventArgs e)
        {
            // Cease the scanning when the page doth vanish or is cast away
            _scanner.IsDetecting = false;
        }
    }
}
