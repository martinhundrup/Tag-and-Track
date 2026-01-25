using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class AllContainersPage : TagAndTrackPage
    {
        protected const string titleText = "All Containers";
        private VerticalStackLayout? contentLayout;

        public AllContainersPage() { Initialize(); }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    Background = CurrentTheme.Instance.Theme.Background;
            };

            var header = new HeaderTemplate(titleText);

            contentLayout = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(20)
            };

            var addButton = new TagAndTrackButton("Add Container", new Command(async () => await AddContainerAsync()));

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children = { header, addButton, contentLayout }
                }
            };
            // Load data in OnAppearing() only to prevent duplicates
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadContainersAsync();
        }

        private async Task LoadContainersAsync()
        {
            if (contentLayout == null) return;

            contentLayout.Children.Clear();

            var containers = await DbService.GetAllContainersAsync();

            if (containers.Count == 0)
            {
                contentLayout.Children.Add(new Label
                {
                    Text = "No containers yet. Add one to get started!",
                    FontSize = 16,
                    TextColor = CurrentTheme.Instance.Theme.Text,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20)
                });
                return;
            }

            foreach (var container in containers)
            {
                var containerCard = CreateContainerCard(container);
                contentLayout.Children.Add(containerCard);
            }
        }

        private View CreateContainerCard(ContainerItem container)
        {
            var nameLabel = new Label
            {
                Text = container.Name,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = CurrentTheme.Instance.Theme.Text
            };

            var descLabel = new Label
            {
                Text = container.Description,
                FontSize = 14,
                TextColor = CurrentTheme.Instance.Theme.Text
            };

            var countLabel = new Label
            {
                Text = $"Contains {container.Specimens.Count} specimen(s)",
                FontSize = 12,
                TextColor = CurrentTheme.Instance.Theme.Text
            };

            var viewButton = new Button
            {
                Text = "View",
                BackgroundColor = Colors.CornflowerBlue,
                TextColor = Colors.White,
                Padding = new Thickness(15, 8),
                BindingContext = container
            };
            viewButton.Clicked += async (s, e) =>
            {
                if ((s as Button)?.BindingContext is ContainerItem c)
                {
                    ScannedQRItem.lastScannedItem = c.QRID;
                    await Navigation.PushAsync(new ViewContainerPage((int)c.ID));
                }
            };

            var card = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                StrokeThickness = 1,
                Padding = new Thickness(15),
                Content = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    Children =
                    {
                        new VerticalStackLayout
                        {
                            Spacing = 5,
                            Children = { nameLabel, descLabel, countLabel }
                        },
                        viewButton
                    }
                }
            };

            var cardGrid = (Grid)card.Content;
            Grid.SetColumn(cardGrid.Children[1] as View, 1);

            CurrentTheme.Instance.PropertyChanged += (s, e) =>
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

            return card;
        }

        private async Task AddContainerAsync()
        {
            string name = await DisplayPromptAsync("New Container", "Enter container name:");
            if (string.IsNullOrWhiteSpace(name)) return;

            string desc = await DisplayPromptAsync("New Container", "Enter description (optional):") ?? "";

            var container = new ContainerItem(name, desc);
            await DbService.AddContainerAsync(container);

            DebugLogger.Log($"Container added: {name}");
            await LoadContainersAsync();
        }
    }
}
