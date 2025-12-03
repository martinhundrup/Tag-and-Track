using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class CheckInLoanPage : TagAndTrackPage
    {
        protected const string titleText = "Check in a Loan";
        private Label? scanResultLabel;
        public CheckInLoanPage() { Initialize(); }

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

            var header = new HeaderTemplate(titleText);

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

            Content = new StackLayout
            {
                Children =
                {
                    header,
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