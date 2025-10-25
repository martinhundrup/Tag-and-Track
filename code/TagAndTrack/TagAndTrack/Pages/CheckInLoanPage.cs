using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class CheckInLoanPage : TagAndTrackPage
    {
        protected new const string titleText = "Check in a Loan";
        private Label? scanResultLabel;
        public CheckInLoanPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;

            var scanView = new ScanView
            {
                WidthRequest = 800,
                HeightRequest = 800
            };

            scanResultLabel = new Label
            {
                Text = "Looking for a QR code...",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20)
            };

            scanView.ScanCaptured += ScanCaptured;
        }

        private void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(() => scanResultLabel.Text = args.Text);
        }
    }
}