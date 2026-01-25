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
        private VerticalStackLayout? specimensLayout;

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

            specimensLayout = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(10)
            };

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new HeaderTemplate("Loading..."),
                        specimensLayout
                    }
                }
            };
            // Load data in OnAppearing() only to prevent race conditions
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadContainerAsync();
        }

        private async Task LoadContainerAsync()
        {
            DebugLogger.Log($"ViewContainerPage.LoadContainerAsync() called for containerId={_containerId}");
            _container = await DbService.GetContainerByIdAsync(_containerId);
            DebugLogger.Log($"ViewContainerPage: GetContainerByIdAsync returned {(_container == null ? "null" : _container.Name)}");

            if (_container == null)
            {
                DebugLogger.Log("ViewContainerPage: Container not found, showing error");
                await DisplayAlert("Error", "Container not found", "OK");
                await Navigation.PopAsync();
                return;
            }

            DebugLogger.Log($"ViewContainerPage: Building UI for container '{_container.Name}' with {_container.Specimens.Count} specimens");

            var header = new HeaderTemplate(_container.Name ?? "Container");

            var infoLabel = new Label
            {
                Text = _container.Description,
                FontSize = 14,
                TextColor = CurrentTheme.Instance.Theme.Text,
                Margin = new Thickness(20, 10)
            };

            var qr = new QrCodeView
            {
                Value = _container.QRID,
                Size = 150,
                Padding = 4,
                HorizontalOptions = LayoutOptions.Center
            };

            var addSpecimenButton = new TagAndTrackButton("Add Specimen to Container", new Command(async () => await AddSpecimenAsync()));

            specimensLayout!.Children.Clear();

            if (_container.Specimens.Count == 0)
            {
                specimensLayout.Children.Add(new Label
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
                    specimensLayout.Children.Add(CreateSpecimenRow(specimen));
                }
            }

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        header,
                        infoLabel,
                        qr,
                        addSpecimenButton,
                        new Label
                        {
                            Text = "Specimens in this container:",
                            FontSize = 16,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = CurrentTheme.Instance.Theme.Text,
                            Margin = new Thickness(20, 20, 20, 5)
                        },
                        specimensLayout
                    }
                }
            };
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
            string result = await DisplayActionSheet("Select Specimen", "Cancel", null, names);

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
