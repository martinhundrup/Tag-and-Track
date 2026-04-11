using System.Diagnostics;
using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Employees;
using TagAndTrack.Backend.Items;
using TagAndTrack.Backend.Utils;
using TagAndTrack.Components;
using TagAndTrack.Pages;

namespace TagAndTrack
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Initialize();
        }

        private async void Initialize()
        {
            Title = null;
            Background = CurrentTheme.Instance.Theme.Background;

            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };

            // Build header with logged-in user info
            var employeeName = EmployeeManager.ActiveEmployee?.Name ?? "Unknown";
            var headerText = $"Home";

            var grid = new Grid
            {
                Padding = 20,
                RowSpacing = 15,
                ColumnSpacing = 15,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                }
            };

            var buttons = new[]
            {
                new TagAndTrackButton("Scan Item", new Command(async () => await Navigation.PushAsync(new ScanItemPage())), "qr_code.png"),
                new TagAndTrackButton("Start Loan", new Command(async () => await Navigation.PushAsync(new StartLoanPage())), "document.png"),
                new TagAndTrackButton("Loan History", new Command(async () => await Navigation.PushAsync(new LoanHistoryPage())), "history.png"),
                new TagAndTrackButton("All Specimens", new Command(async () => await Navigation.PushAsync(new AllSpecimensPage())), "fish.png"),
                new TagAndTrackButton("Containers", new Command(async () => await Navigation.PushAsync(new AllContainersPage())), "boxes.png"),
                new TagAndTrackButton("Add Item", new Command(async () => await Navigation.PushAsync(new AddItemPage())), "plus.png"),
                new TagAndTrackButton("Settings", new Command(async () => await Navigation.PushAsync(new SettingsPage())), "setting.png"),
                new TagAndTrackButton("Logout", new Command(async () => await LogoutAsync()), "logout.png"),
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                grid.Children.Add(buttons[i]);
                Grid.SetRow(buttons[i], row);
                Grid.SetColumn(buttons[i], col);
            }

            // Welcome message
            var welcomeLabel = new Label
            {
                Text = $"Logged in as: {employeeName}",
                FontSize = 14,
                TextColor = CurrentTheme.Instance.Theme.Text,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    welcomeLabel.TextColor = CurrentTheme.Instance.Theme.Text;
            };

            Content = new StackLayout()
            {
                Children = { new HeaderTemplate(headerText, true), welcomeLabel, grid }
            };
        }

        private async Task LogoutAsync()
        {
            EmployeeManager.SetActiveEmployee(null);
            await Shell.Current.GoToAsync("//LoginPage");
        }

    }
}
