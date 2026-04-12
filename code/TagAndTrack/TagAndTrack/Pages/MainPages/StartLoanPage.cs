using System.ComponentModel;
using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Items;
using TagAndTrack.Backend.Utils;
using TagAndTrack.Components;
using TagAndTrack.Pages.SupportPages;

namespace TagAndTrack.Pages
{
    public class StartLoanPage : TagAndTrackPage, IDisposable
    {
        protected const string titleText = "Start Loan";
        private Label? scanResultLabel;
        private ScanView? scanView;
        private bool _listening;
        private bool _navigating;
        private PropertyChangedEventHandler _themeChange;

        public StartLoanPage() { LoanCreator.ClearLoan(); Initialize(); }


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
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = false
            });           
            Background = CurrentTheme.Instance.Theme.Background;

            _themeChange = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };
            CurrentTheme.Instance.PropertyChanged += _themeChange;

            var header = new HeaderTemplate(titleText);

            // See if this change affects anything
            scanView = new ScanView
            {
                WidthRequest = 600,
                HeightRequest = 600
            };

            scanResultLabel = new Label
            {
                Text = "Scan a specimen",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20)
            };

            scanView.ScanCaptured += ScanCaptured;


            var buttonLayout = new VerticalStackLayout
            {
                Spacing = 10,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new TagAndTrackButton("Manual Item Entry", new Command(async () => await ManualItemEntry()), "plus.png"),
                    new TagAndTrackButton("Cancel Loan", new Command(async () => await CancelLoan()), "cross.png"),
                    new TagAndTrackButton("View Items", new Command(async () => await ViewItems()), "view.png"),
                    new TagAndTrackButton("Finalize Loan", new Command(async () => await FinalizeLoan()), "check.png")
                }
            };

            // Scanner + buttons side-by-side so buttons stay visible in landscape
            var scannerRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 20,
                HorizontalOptions = LayoutOptions.Fill
            };
            scannerRow.Add(scanView, 0);
            scannerRow.Add(buttonLayout, 1);

            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                {
                    header,
                    scannerRow,
                    scanResultLabel,
                }
            };

        }
        private async void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (_navigating) return;
                _navigating = true;

                try
                {
                    var qr = args.Text?.Trim();
                    var item = ItemManager.GetItemByQRID(qr);

                    if (item == null)
                    {
                        await Shell.Current.DisplayAlert("Error", $"Value {qr} not recognized!", "OK");
                        return;
                    }

                    if (item is ContainerItem container)
                    {
                        ScannedQRItem.lastScannedItem = qr;

                        int added = LoanCreator.AddContainerItems(container);
                        DebugLogger.Log($"added {added} items from container {container.ID} to loan");
                        await Shell.Current.DisplayAlert(
                            "Container Scanned",
                            $"{added} item(s) from container {container.Name} added to loan.",
                            "OK");

                        if (scanResultLabel != null)
                            scanResultLabel.Text = $"{added} item(s) from container {container.Name} added to loan.";
                        return;
                    }

                    if (item is not SpecimenItem specimen)
                    {
                        await Shell.Current.DisplayAlert(
                            "Error",
                            $"Value {item.Name} not a specimen - only specimens can be added to loans!",
                            "OK");
                        return;
                    }

                    ScannedQRItem.lastScannedItem = qr;

                    var response = LoanCreator.AddItem(specimen);
                    if (response != null)
                    {
                        await Shell.Current.DisplayAlert("Error", response, "OK");
                        return;
                    }
                    else
                    {
                        DebugLogger.Log($"added {item.ID} to loan");
                        await Shell.Current.DisplayAlert("Sucess!", $"Item {item.Name} added to loan!", "OK");
                    }

                    if (scanResultLabel != null)
                        scanResultLabel.Text = $"Item {item.Name} added to loan!";
                }
                finally
                {
                    _navigating = false;
                }
            });
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
            DebugLogger.Log($"loan cleared and cancelled");
            await Shell.Current.Navigation.PopAsync();
        }

        private async Task ViewItems()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Navigation.PushAsync(new ViewEditLoanItemsPage()));
        }

        private async Task ManualItemEntry()
        {
            var allSpecimens = await DbService.GetAllSpecimensAsync();

            // Show all checked-in specimens; items already in loan will be pre-checked
            var existingIds = LoanCreator.LoanItems.Select(s => s.ID).ToHashSet();
            var available = allSpecimens.Where(s => s.Status || existingIds.Contains(s.ID)).ToList();

            if (available.Count == 0)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert("Info", "No available specimens to add.", "OK"));
                return;
            }

            var tcs = new TaskCompletionSource<List<SpecimenItem>>();
            await Navigation.PushAsync(new SelectSpecimensPage(available, tcs, preSelectedIds: existingIds));

            var selected = await tcs.Task;

            // Sync loan items to match the final selection:
            // remove unchecked items, add newly checked items
            var selectedIds = selected.Select(s => s.ID).ToHashSet();
            var toRemove = LoanCreator.LoanItems.Where(s => !selectedIds.Contains(s.ID)).ToList();
            foreach (var specimen in toRemove)
            {
                LoanCreator.RemoveItem(specimen);
            }
            foreach (var specimen in selected)
            {
                LoanCreator.AddItem(specimen); // AddItem ignores duplicates
            }

            if (scanResultLabel != null)
                scanResultLabel.Text = $"{LoanCreator.LoanItems.Count} specimen(s) in loan.";
        }

        private async Task FinalizeLoan()
        {
            if (LoanCreator.LoanItems.Count == 0)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert("Error", "At least one item must be added to the loan before finalizing.", "OK"));
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Navigation.PushAsync(new FinalizeLoanPage()));
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
            if (scanView != null)
            {
                scanView.ScanCaptured -= ScanCaptured;
            }
            CurrentTheme.Instance.PropertyChanged -= _themeChange;
        }
    }
}