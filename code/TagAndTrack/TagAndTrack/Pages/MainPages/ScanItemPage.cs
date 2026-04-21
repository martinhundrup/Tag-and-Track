using Microsoft.Maui.Controls;
using System.ComponentModel;
using TagAndTrack.Backend;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class ScanItemPage : TagAndTrackPage, IDisposable
    {
        protected const string titleText = "Scan Item";
        private Label? scanResultLabel;
        private ScanView? scanView;
        private bool _listening;
        private bool _navigating;
        private PropertyChangedEventHandler handler;

        public ScanItemPage()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;

            handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    Background = CurrentTheme.Instance.Theme.Background;
            };

            CurrentTheme.Instance.PropertyChanged += handler;

            scanView = new ScanView
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

            var header = new HeaderTemplate(titleText);

            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                {
                    header,
                    scanView,
                    scanResultLabel
                }
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Subscribe and set the scanning forth when this page doth appear
            if (scanView != null && !_listening)
            {
                scanView.ScanCaptured += ScanCaptured;
                // Should thy ScanView bear the banner of control flags:
                // scanView.IsScanning = true;
                _listening = true;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Unsubscribe and still the scanning when this page doth retreat from sight
            if (scanView != null && _listening)
            {
                scanView.ScanCaptured -= ScanCaptured;
                // If such be supported:
                // scanView.IsScanning = false;
                _listening = false;
            }
        }

        private async void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            if (_navigating) return;
            _navigating = true;

            var qr = args.Text?.Trim();
            var item = ItemManager.GetItemByQRID(qr);

            if (item == null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (scanResultLabel != null)
                        scanResultLabel.Text = $"Value {qr} not recognized!";
                });
                _navigating = false;
                return;
            }
            
            // Halt the scanning ere we navigate hence
            OnDisappearing();

            ScannedQRItem.lastScannedItem = qr;

            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Navigation.PushAsync(new ViewItemPage()));

            _navigating = false;
        }

        protected override void OnParentChanged()
        {
            base.OnParentChanged();
            if(Parent == null)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            CurrentTheme.Instance.PropertyChanged -= handler;
        }
    }
}
