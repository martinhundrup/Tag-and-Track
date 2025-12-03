using TagAndTrack.Backend.Items;
using TagAndTrack.Backend.Utils;
using TagAndTrack.Components;
using TagAndTrack.Pages.SupportPages;

namespace TagAndTrack.Pages
{
    public class StartLoanPage : TagAndTrackPage
    {
        protected const string titleText = "Start Loan";
        private Label? scanResultLabel;
        private bool _navigating;

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
                Text = "Scan a specimen",
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
                //Spacing = 20,
                Children =
                {
                    header,
                    scanView,
                    scanResultLabel,
                    buttonLayout,
                }
            };

            LoanCreator.ClearLoan();
        }
        private async void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            if (_navigating) return;
            _navigating = true;

            var qr = args.Text?.Trim();
            var item = ItemManager.GetItemByQRID(qr);

            if (item == null)
            {
                await Shell.Current.DisplayAlert("Error", $"Value {qr} not recognized!", "OK");
                _navigating = false;
                return;
            }

            if (item is not SpecimenItem)
            {
                await Shell.Current.DisplayAlert("Error", $"Value {qr} not a specimen - only specimens can be added to loans!", "OK");
                _navigating = false;
                return;
            }

            ScannedQRItem.lastScannedItem = qr;

            var response = LoanCreator.AddItem(item as SpecimenItem);
            if (response != null) 
            {
                await Shell.Current.DisplayAlert("Error", response, "OK");
                _navigating = false;
                return;
            }

            // success
            scanResultLabel.Text = $"Item {item.Name} added to loan!";
        }

        private async Task CancelLoan()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Cancelled", "Loan cancelled.", "OK");
                }
            });

            LoanCreator.ClearLoan();
        }

        private async Task ViewItems()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Navigation.PushAsync(new ViewEditLoanItemsPage()));
        }

        private async Task FinalizeLoan()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Navigation.PushAsync(new FinalizeLoanPage()));
        }
    }
}