using TagAndTrack.Components;
using Microsoft.Maui.Controls;

namespace TagAndTrack.Pages
{
    public class ScanItemPage : TagAndTrackPage
    {
        protected new const string titleText = "Scan Item";
        private Label? scanResultLabel;

        public ScanItemPage() { Initialize(); }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };
            Title = titleText;

            var scanView = new ScanView
            {
                WidthRequest = 512,
                HeightRequest = 512
            };

            scanResultLabel = new Label
            {
                Text = "Scan a QR code...",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20)
            };

            var qr = new QrCodeView
            {
                Value = "Specimen:3930587",
                Size = 512,
                Padding = 4,          // quiet zone in pixels
                Foreground = Colors.Black
            };


            scanView.ScanCaptured += ScanCaptured;

            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                {
                    scanView,
                    scanResultLabel,
                    qr,
                }
            };
        }

        private void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(() => scanResultLabel.Text = args.Text);
        }
    }
}