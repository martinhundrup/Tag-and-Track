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
        private VerticalStackLayout? _specimensLayout;
        private VerticalStackLayout? _addButtonContainer;
        private Label? _specimensTitle;
        private ActivityIndicator? _loadingIndicator;

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

            _specimensTitle = new Label
            {
                Text = "Specimens in this container:",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = CurrentTheme.Instance.Theme.Text,
                Margin = new Thickness(20, 20, 20, 5),
                IsVisible = false
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme) && _specimensTitle != null)
                    _specimensTitle.TextColor = CurrentTheme.Instance.Theme.Text;
            };

            _specimensLayout = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(10)
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

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        _header,
                        _loadingIndicator,
                        _descriptionLabel,
                        _qrCode,
                        _addButtonContainer,
                        _specimensTitle,
                        _specimensLayout
                    }
                }
            };

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
                                new Command(async () => await AddSpecimenAsync())));
                        _addButtonContainer.IsVisible = true;
                    }

                    // Specimens title
                    if (_specimensTitle != null)
                        _specimensTitle.IsVisible = true;

                    // Specimens list
                    if (_specimensLayout != null)
                    {
                        _specimensLayout.Children.Clear();

                        if (_container.Specimens.Count == 0)
                        {
                            _specimensLayout.Children.Add(new Label
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
                            foreach (var specimen in _container.Specimens)
                            {
                                _specimensLayout.Children.Add(CreateSpecimenRow(specimen));
                            }
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
                    if (_specimensLayout != null)
                    {
                        _specimensLayout.Children.Clear();
                        _specimensLayout.Children.Add(new Label
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

        private View CreateSpecimenRow(SpecimenItem specimen)
        {
            var nameLabel = new Label
            {
                Text = specimen.Name,
                FontSize = 16,
                TextColor = CurrentTheme.Instance.Theme.Text,
                VerticalOptions = LayoutOptions.Center
            };

            var removeButton = new Button
            {
                Text = "Remove",
                BackgroundColor = Colors.IndianRed,
                TextColor = Colors.White,
                Padding = new Thickness(10, 5),
                BindingContext = specimen
            };
            removeButton.Clicked += async (s, e) =>
            {
                if ((s as Button)?.BindingContext is SpecimenItem sp)
                {
                    bool confirm = await DisplayAlert("Confirm", $"Remove {sp.Name} from container?", "Yes", "No");
                    if (confirm)
                    {
                        _container?.RemoveSpecimen(sp);
                        if (_container != null)
                        {
                            await DbService.UpdateContainerSpecimensAsync(_containerId, _container.Specimens.ToList());
                        }
                        await LoadContainerAsync();
                    }
                }
            };

            var row = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                StrokeThickness = 1,
                Padding = new Thickness(10),
                Margin = new Thickness(10, 2),
                Content = new HorizontalStackLayout
                {
                    Spacing = 20,
                    Children = { nameLabel, removeButton }
                }
            };

            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    nameLabel.TextColor = CurrentTheme.Instance.Theme.Text;
                    row.Stroke = CurrentTheme.Instance.Theme.Borders;
                    row.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };

            return row;
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

            // Show picker
            var names = available.Select(s => s.Name ?? "Unknown").ToArray();
            string? result = await DisplayActionSheet("Select Specimen", "Cancel", null, names);

            if (string.IsNullOrEmpty(result) || result == "Cancel") return;

            var selected = available.FirstOrDefault(s => s.Name == result);
            if (selected != null && _container != null)
            {
                _container.AddSpecimen(selected);
                await DbService.UpdateContainerSpecimensAsync(_containerId, _container.Specimens.ToList());
                DebugLogger.Log($"Specimen {selected.Name} added to container {_container.Name}");
                await LoadContainerAsync();
            }
        }
    }
}
