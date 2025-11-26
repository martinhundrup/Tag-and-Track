using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class StartLoanPage : TagAndTrackPage
    {
        new protected const string titleText = "Start Loan";
        private Label? scanResultLabel;
        public StartLoanPage() { Initialize(); }

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

            var buttonLayout = new HorizontalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new TagAndTrackButton("Cancel Loan", new Command(async () => await CancelLoan())),
                    new TagAndTrackButton("View Items", new Command(async () => await ViewItems())),
                    new TagAndTrackButton("Finalize Loan", new Command(async () => await FinalizeLoan()))
                }
            };

            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                {
                    header,
                    scanView,
                    scanResultLabel,
                    buttonLayout,
                }
            };
        }
        private void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(() => scanResultLabel.Text = args.Text);
        }

        private async Task CancelLoan()
        {
            // Run any cancellation logic here (stop scan, clear fields, etc.)
            // For now show a confirmation alert on the UI thread.
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Cancelled", "Loan cancelled.", "OK");
                }
            });
        }

        private async Task ViewItems()
        {
            // TEMPORARY
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Cancelled", "Loan cancelled.", "OK");
                }
            });
        }

        private async Task FinalizeLoan()
        {
            // Run any cancellation logic here (stop scan, clear fields, etc.)
            // For now show a confirmation alert on the UI thread.
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Cancelled", "Loan cancelled.", "OK");
                }
            });
        }
    }
}