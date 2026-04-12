using TagAndTrack.Components;
using System.ComponentModel;

namespace TagAndTrack.Pages
{
    public class CheckInLoanPage : TagAndTrackPage, IDisposable
    {
        protected const string titleText = "Check in a Loan";
        private Label? scanResultLabel;
        private ScanView? scanView;
        private bool _listening;
        private bool _navigating;
        private PropertyChangedEventHandler handler;
        public CheckInLoanPage() { Initialize(); }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // subscribe/start scanning when visible
            if (scanView != null && !_listening)
            {
                scanView.ScanCaptured += ScanCaptured;
                // if your ScanView supports control flags:
                // scanView.IsScanning = true;
                _listening = true;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // unsubscribe/stop scanning when hidden
            if (scanView != null && _listening)
            {
                scanView.ScanCaptured -= ScanCaptured;
                // if supported:
                // scanView.IsScanning = false;
                _listening = false;
            }
        }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;

            handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };
            CurrentTheme.Instance.PropertyChanged += handler;

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

        private async void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {

            });
        }

        protected override void OnParentChanged()
        {
            base.OnParentChanged();
            if (Parent == null)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (scanView != null && _listening)
            {
                scanView.ScanCaptured -= ScanCaptured;
                _listening = false;
            }
            CurrentTheme.Instance.PropertyChanged -= handler;
        }
    }
}