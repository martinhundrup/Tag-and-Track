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

        // Keep a reference so we can control it outside the ctor
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

            // Unhook from old page if any
            if (_parentPage != null)
            {
                _parentPage.Appearing -= OnPageAppearing;
                _parentPage.Disappearing -= OnPageDisappearing;
            }

            // Find the new parent page
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
            // Turn scanning back on when the page becomes visible
            _scanner.IsDetecting = true;
        }

        private void OnPageDisappearing(object? sender, EventArgs e)
        {
            // Stop scanning when the page disappears or is popped
            _scanner.IsDetecting = false;
        }
    }
}
