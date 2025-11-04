using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

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

            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                {
                    scanView,
                    scanResultLabel,
                }
            };
        }

        private async void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            ScannedQRItem.lastScannedItem = args.Text;

            if (ItemManager.GetItemByQRID(args.Text) != null)
            {
                await Navigation.PushAsync(new ViewItemPage());
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => scanResultLabel.Text = $"Value {args.Text} not recognized!");
            }
        }
    }
}