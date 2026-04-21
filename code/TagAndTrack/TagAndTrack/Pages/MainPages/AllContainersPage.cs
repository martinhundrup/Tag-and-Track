using System.Collections.ObjectModel;
using System.ComponentModel;
using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class AllContainersPage : TagAndTrackPage
    {
        protected const string titleText = "All Containers";
        private CollectionView? containerCollection;
        private Label? statusLabel;
        private ActivityIndicator? loadingIndicator;
        private ObservableCollection<ContainerItem> containers = new();
        private List<PropertyChangedEventHandler> themeChangeHandlers = new List<PropertyChangedEventHandler>();

        public AllContainersPage()
        {
            DebugLogger.Log("AllContainersPage constructor called");
            Initialize();
        }

        protected override void Initialize()
        {
            DebugLogger.Log("AllContainersPage.Initialize() starting");
            
            Background = CurrentTheme.Instance.Theme.Background;

            PropertyChangedEventHandler themeChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    Background = CurrentTheme.Instance.Theme.Background;
            };
            CurrentTheme.Instance.PropertyChanged += themeChangedHandler;
            themeChangeHandlers.Add(themeChangedHandler);

            var header = new HeaderTemplate(titleText);

            var addButton = new TagAndTrackButton("Add Container", new Command(async () => await AddContainerAsync()), "plus.png");

            // The loading herald — displayed upon first arrival
            loadingIndicator = new ActivityIndicator
            {
                IsRunning = true,
                IsVisible = true,
                Color = Colors.CornflowerBlue,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 50,
                WidthRequest = 50
            };

            // A label to proclaim emptiness or misfortune
            statusLabel = new Label
            {
                Text = "Loading containers...",
                FontSize = 16,
                TextColor = CurrentTheme.Instance.Theme.Text,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = true
            };

            themeChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme) && statusLabel != null)
                    statusLabel.TextColor = CurrentTheme.Instance.Theme.Text;
            };

            CurrentTheme.Instance.PropertyChanged += themeChangedHandler;
            themeChangeHandlers.Add(themeChangedHandler);

            // A CollectionView for the containers — it governeth scrolling with grace upon all platforms
            containerCollection = new CollectionView
            {
                ItemsSource = containers,
                IsVisible = false,
                ItemTemplate = new DataTemplate(() => CreateContainerCardTemplate()),
                ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
                {
                    ItemSpacing = 10
                },
                Margin = new Thickness(20, 10)
            };

            // Employ the Grid arrangement, mirroring AllSpecimensPage whose ways have proven true
            var pageLayout = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }, // header
                    new RowDefinition { Height = GridLength.Auto }, // add button
                    new RowDefinition { Height = GridLength.Star }  // content area
                },
                Padding = new Thickness(0)
            };

            // The realm of content: the loading herald, the status proclamation, and the collection
            var contentArea = new Grid
            {
                Children = { loadingIndicator, statusLabel, containerCollection }
            };

            pageLayout.Children.Add(header);
            Grid.SetRow(header, 0);

            var buttonContainer = new VerticalStackLayout
            {
                Padding = new Thickness(20, 10),
                Children = { addButton }
            };
            pageLayout.Children.Add(buttonContainer);
            Grid.SetRow(buttonContainer, 1);

            pageLayout.Children.Add(contentArea);
            Grid.SetRow(contentArea, 2);

            Content = pageLayout;

            DebugLogger.Log("AllContainersPage.Initialize() complete");
        }

        protected override async void OnAppearing()
        {
            DebugLogger.Log("AllContainersPage.OnAppearing() called");
            base.OnAppearing();
            await LoadContainersAsync();
        }

        private async Task LoadContainersAsync()
        {
            DebugLogger.Log("AllContainersPage.LoadContainersAsync() starting");

            try
            {
                // Reveal the loading state upon the thread of the interface
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (loadingIndicator != null) loadingIndicator.IsVisible = true;
                    if (loadingIndicator != null) loadingIndicator.IsRunning = true;
                    if (statusLabel != null)
                    {
                        statusLabel.Text = "Loading containers...";
                        statusLabel.TextColor = CurrentTheme.Instance.Theme.Text;
                        statusLabel.IsVisible = true;
                    }
                    if (containerCollection != null) containerCollection.IsVisible = false;
                });

                // Summon forth the data from the depths of the database
                var containerList = await DbService.GetAllContainersAsync();
                DebugLogger.Log($"AllContainersPage: Retrieved {containerList.Count} containers from DB");

                // Refresh the visage upon the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    containers.Clear();
                    
                    if (containerList.Count == 0)
                    {
                        DebugLogger.Log("AllContainersPage: No containers found, showing empty message");
                        if (loadingIndicator != null)
                        {
                            loadingIndicator.IsRunning = false;
                            loadingIndicator.IsVisible = false;
                        }
                        if (statusLabel != null)
                        {
                            statusLabel.Text = "No containers yet. Add one to get started!";
                            statusLabel.IsVisible = true;
                        }
                        if (containerCollection != null) containerCollection.IsVisible = false;
                    }
                    else
                    {
                        DebugLogger.Log($"AllContainersPage: Adding {containerList.Count} containers to collection");
                        foreach (var container in containerList)
                        {
                            containers.Add(container);
                        }
                        
                        if (loadingIndicator != null)
                        {
                            loadingIndicator.IsRunning = false;
                            loadingIndicator.IsVisible = false;
                        }
                        if (statusLabel != null) statusLabel.IsVisible = false;
                        if (containerCollection != null) containerCollection.IsVisible = true;
                    }
                    
                    DebugLogger.Log("AllContainersPage.LoadContainersAsync() UI update complete");
                });
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"AllContainersPage.LoadContainersAsync() ERROR: {ex.GetType().Name}: {ex.Message}");
                DebugLogger.Log($"Stack trace: {ex.StackTrace}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (loadingIndicator != null)
                    {
                        loadingIndicator.IsRunning = false;
                        loadingIndicator.IsVisible = false;
                    }
                    if (statusLabel != null)
                    {
                        statusLabel.Text = $"Error loading containers: {ex.Message}";
                        statusLabel.TextColor = Colors.Red;
                        statusLabel.IsVisible = true;
                    }
                    if (containerCollection != null) containerCollection.IsVisible = false;
                });
            }
        }

        private View CreateContainerCardTemplate()
        {
            var nameLabel = new Label
            {
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = CurrentTheme.Instance.Theme.Text
            };
            nameLabel.SetBinding(Label.TextProperty, nameof(ContainerItem.Name));

            var descLabel = new Label
            {
                FontSize = 14,
                TextColor = CurrentTheme.Instance.Theme.Text
            };
            descLabel.SetBinding(Label.TextProperty, nameof(ContainerItem.Description));

            var countLabel = new Label
            {
                FontSize = 12,
                TextColor = CurrentTheme.Instance.Theme.Text
            };
            countLabel.SetBinding(Label.TextProperty, new Binding(nameof(ContainerItem.Specimens), 
                converter: new SpecimenCountConverter()));

            var viewButton = new Button
            {
                Text = "View",
                BackgroundColor = Colors.Crimson,
                TextColor = Colors.White,
                Padding = new Thickness(15, 8),
                CornerRadius = 8
            };
            viewButton.Clicked += async (s, e) =>
            {
                if ((s as Button)?.BindingContext is ContainerItem c)
                {
                    ScannedQRItem.lastScannedItem = c.QRID;
                    await Navigation.PushAsync(new ViewContainerPage((int)c.ID));
                }
            };

            var infoStack = new VerticalStackLayout
            {
                Spacing = 5,
                Children = { nameLabel, descLabel, countLabel }
            };

            var cardGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            cardGrid.Children.Add(infoStack);
            Grid.SetColumn(infoStack, 0);
            cardGrid.Children.Add(viewButton);
            Grid.SetColumn(viewButton, 1);

            var card = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                StrokeThickness = 1,
                Padding = new Thickness(15),
                Content = cardGrid
            };

            PropertyChangedEventHandler themeChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    card.Stroke = CurrentTheme.Instance.Theme.Borders;
                    card.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    nameLabel.TextColor = CurrentTheme.Instance.Theme.Text;
                    descLabel.TextColor = CurrentTheme.Instance.Theme.Text;
                    countLabel.TextColor = CurrentTheme.Instance.Theme.Text;
                }
            };
            CurrentTheme.Instance.PropertyChanged += themeChangedHandler;
            themeChangeHandlers.Add(themeChangedHandler);

            return card;
        }

        private async Task AddContainerAsync()
        {
            string? name = await DisplayPromptAsync("New Container", "Enter container name:");
            if (string.IsNullOrWhiteSpace(name)) return;

            string desc = await DisplayPromptAsync("New Container", "Enter description (optional):") ?? "";

            var container = new ContainerItem(name, desc);
            await DbService.AddContainerAsync(container);

            DebugLogger.Log($"Container added: {name}");
            await LoadContainersAsync();
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
            foreach (var handler in themeChangeHandlers)
            {
                CurrentTheme.Instance.PropertyChanged -= handler;
            }
            themeChangeHandlers.Clear();
        }

        // A converter to render the tally of specimens
        private class SpecimenCountConverter : IValueConverter
        {
            public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            {
                if (value is IReadOnlyList<SpecimenItem> specimens)
                    return $"Contains {specimens.Count} specimen(s)";
                return "Contains 0 specimen(s)";
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
