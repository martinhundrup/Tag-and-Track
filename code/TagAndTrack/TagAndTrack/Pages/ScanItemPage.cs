using TagAndTrack.Components;
using Microsoft.Maui.Controls;

namespace TagAndTrack.Pages
{
    public class ScanItemPage : TagAndTrackPage
    {
        protected new const string titleText = "Scan Item";
        private Label scanResultLabel;

        public ScanItemPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;

            var scanView = new ScanView
            {
                WidthRequest = 300,
                HeightRequest = 300
            };

            scanResultLabel = new Label
            {
                Text = "Scan a QR code...",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20)
            };

            scanView.ScanCaptured += ScanCaptured;

            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                {
                    scanView,
                    scanResultLabel
                }
            };
        }

        private void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(() => scanResultLabel.Text = args.Text);
        }
    }
}