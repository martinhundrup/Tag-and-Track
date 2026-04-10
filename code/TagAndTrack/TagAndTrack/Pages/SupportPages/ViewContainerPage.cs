using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class ViewContainerPage : TagAndTrackPage
    {
        private readonly int _containerId;
        private ContainerItem? _container;

        // Persistent UI elements updated by LoadContainerAsync
        private HeaderTemplate? _header;
        private Label? _descriptionLabel;
        private QrCodeView? _qrCode;
        private VerticalStackLayout? _addButtonContainer;
        private Grid? _specimensContainer;
        private ActivityIndicator? _loadingIndicator;
        private DataTable<SpecimenItem>? _dataTable;

        public ViewContainerPage(int containerId)
        {
            DebugLogger.Log($"ViewContainerPage constructor called with containerId={containerId}");
            _containerId = containerId;
            Initialize();
        }

        protected override void Initialize()
        {
            DebugLogger.Log("ViewContainerPage.Initialize() starting");
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    Background = CurrentTheme.Instance.Theme.Background;
            };

            _header = new HeaderTemplate("Loading...");

            _descriptionLabel = new Label
            {
                Text = "",
                FontSize = 14,
                TextColor = CurrentTheme.Instance.Theme.Text,
                Margin = new Thickness(20, 10),
                IsVisible = false
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme) && _descriptionLabel != null)
                    _descriptionLabel.TextColor = CurrentTheme.Instance.Theme.Text;
            };

            _qrCode = new QrCodeView
            {
                Value = "",
                Size = 150,
                Padding = 4,
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = false
            };

            _addButtonContainer = new VerticalStackLayout
            {
                IsVisible = false
            };

            _specimensContainer = new Grid
            {
                Padding = new Thickness(10),
                IsVisible = false
            };

            _loadingIndicator = new ActivityIndicator
            {
                IsRunning = true,
                IsVisible = true,
                Color = Colors.CornflowerBlue,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 50,
                WidthRequest = 50,
                Margin = new Thickness(0, 40)
            };

            var topSection = new VerticalStackLayout
            {
                Children =
                {
                    _header,
                    _loadingIndicator,
                    _descriptionLabel,
                    _qrCode,
                    _addButtonContainer
                }
            };

            var pageGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                }
            };

            pageGrid.Children.Add(topSection);
            Grid.SetRow(topSection, 0);

            pageGrid.Children.Add(_specimensContainer);
            Grid.SetRow(_specimensContainer, 1);

            Content = pageGrid;

            DebugLogger.Log("ViewContainerPage.Initialize() complete");
        }

        protected override async void OnAppearing()
        {
            DebugLogger.Log("ViewContainerPage.OnAppearing() called");
            base.OnAppearing();
            await LoadContainerAsync();
        }

        private async Task LoadContainerAsync()
        {
            DebugLogger.Log($"ViewContainerPage.LoadContainerAsync() called for containerId={_containerId}");

            try
            {
                _container = await DbService.GetContainerByIdAsync(_containerId);
                DebugLogger.Log($"ViewContainerPage: GetContainerByIdAsync returned {(_container == null ? "null" : _container.Name)}");

                if (_container == null)
                {
                    DebugLogger.Log("ViewContainerPage: Container not found, showing error");
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DisplayAlert("Error", "Container not found", "OK");
                        await Navigation.PopAsync();
                    });
                    return;
                }

                DebugLogger.Log($"ViewContainerPage: Building UI for container '{_container.Name}' with {_container.Specimens.Count} specimens");

                // All UI updates on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Hide loading, show content
                    if (_loadingIndicator != null)
                    {
                        _loadingIndicator.IsRunning = false;
                        _loadingIndicator.IsVisible = false;
                    }

                    // Update header - replace with new one showing container name
                    if (_header != null && Content is ScrollView sv && sv.Content is VerticalStackLayout vsl)
                    {
                        var idx = vsl.Children.IndexOf(_header);
                        if (idx >= 0)
                        {
                            var newHeader = new HeaderTemplate(_container.Name ?? "Container");
                            vsl.Children[idx] = newHeader;
                            _header = newHeader;
                        }
                    }

                    // Update description
                    if (_descriptionLabel != null)
                    {
                        _descriptionLabel.Text = _container.Description ?? "";
                        _descriptionLabel.IsVisible = true;
                    }

                    // Update QR code
                    if (_qrCode != null)
                    {
                        _qrCode.Value = _container.QRID ?? "";
                        _qrCode.IsVisible = true;
                    }

                    // Add button
                    if (_addButtonContainer != null)
                    {
                        _addButtonContainer.Children.Clear();
                        _addButtonContainer.Children.Add(
                            new TagAndTrackButton("Add Specimen to Container",
                                new Command(async () => await AddSpecimenAsync()), "plus.png"));
                        _addButtonContainer.IsVisible = true;
                    }

                    // Specimens title
                    if (_specimensContainer != null)
                    {
                        _specimensContainer.Children.Clear();
                        _specimensContainer.IsVisible = true;

                        if (_container.Specimens.Count == 0)
                        {
                            _specimensContainer.Children.Add(new Label
                            {
                                Text = "No specimens in this container",
                                FontSize = 14,
                                TextColor = CurrentTheme.Instance.Theme.Text,
                                HorizontalOptions = LayoutOptions.Center,
                                Margin = new Thickness(0, 20)
                            });
                        }
                        else
                        {
                            _dataTable = new DataTable<SpecimenItem>(_container.Specimens.ToList(), columns =>
                            {
                                columns.Add("ID", s => s.ID, 60);
                                columns.Add("Arctos ID", s => s.ArctosID, 100);
                                columns.Add("Name", s => s.Name);
                                columns.Add("Description", s => s.Description);
                                columns.AddIcon("Status", s =>
                                    s.Status ? "check.png" : "cross.png", width: 80);
                                columns.AddButton("View",
                                    s =>
                                    {
                                        ScannedQRItem.lastScannedItem = s.QRID;
                                        Navigation.PushAsync(new ViewItemPage());
                                    },
                                    "info.png", 60);
                                columns.AddButton("Remove",
                                    s => RemoveSpecimenAsync(s),
                                    "trash.png", 80);
                            });

                            _specimensContainer.Children.Add(_dataTable);
                        }
                    }

                    DebugLogger.Log("ViewContainerPage: UI update complete");
                });
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"ViewContainerPage.LoadContainerAsync() ERROR: {ex.GetType().Name}: {ex.Message}");
                DebugLogger.Log($"Stack trace: {ex.StackTrace}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_loadingIndicator != null)
                    {
                        _loadingIndicator.IsRunning = false;
                        _loadingIndicator.IsVisible = false;
                    }
                    if (_specimensContainer != null)
                    {
                        _specimensContainer.Children.Clear();
                        _specimensContainer.IsVisible = true;
                        _specimensContainer.Children.Add(new Label
                        {
                            Text = $"Error loading container: {ex.Message}",
                            FontSize = 16,
                            TextColor = Colors.Red,
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new Thickness(0, 20)
                        });
                    }
                });
            }
        }

        private async void RemoveSpecimenAsync(SpecimenItem specimen)
        {
            bool confirm = await DisplayAlert("Confirm", $"Remove {specimen.Name} from container?", "Yes", "No");
            if (confirm)
            {
                _container?.RemoveSpecimen(specimen);
                if (_container != null)
                {
                    await DbService.UpdateContainerSpecimensAsync(_containerId, _container.Specimens.ToList());
                }
                await LoadContainerAsync();
            }
        }

        private async Task AddSpecimenAsync()
        {
            // Get all specimens from DB
            var allSpecimens = await DbService.GetAllSpecimensAsync();

            // Filter out specimens already in container
            var existingIds = _container?.Specimens.Select(s => s.ID).ToHashSet() ?? new HashSet<ulong>();
            var available = allSpecimens.Where(s => !existingIds.Contains(s.ID)).ToList();

            if (available.Count == 0)
            {
                await DisplayAlert("Info", "No available specimens to add", "OK");
                return;
            }

            var tcs = new TaskCompletionSource<List<SpecimenItem>>();
            await Navigation.PushAsync(new SelectSpecimensPage(available, tcs));

            var selected = await tcs.Task;
            if (selected.Count > 0 && _container != null)
            {
                foreach (var specimen in selected)
                {
                    _container.AddSpecimen(specimen);
                }
                await DbService.UpdateContainerSpecimensAsync(_containerId, _container.Specimens.ToList());
                DebugLogger.Log($"{selected.Count} specimen(s) added to container {_container.Name}");
                await LoadContainerAsync();
            }
        }
    }
}
