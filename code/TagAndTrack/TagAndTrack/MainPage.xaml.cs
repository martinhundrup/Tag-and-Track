using TagAndTrack.Pages;
using TagAndTrack.Components;

namespace TagAndTrack
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            Title = "Tag and Track";
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
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            var buttons = new[]
            {
                new TagAndTrackButton("Scan Item", new Command(async () => await Navigation.PushAsync(new ScanItemPage()))),
                new TagAndTrackButton("Start Loan", new Command(async () => await Navigation.PushAsync(new StartLoanPage()))),
                new TagAndTrackButton("Check-in Loan", new Command(async () => await Navigation.PushAsync(new CheckInLoanPage()))),
                new TagAndTrackButton("Loan History", new Command(async () => await Navigation.PushAsync(new LoanHistoryPage()))),
                new TagAndTrackButton("All Specimens", new Command(async () => await Navigation.PushAsync(new AllSpecimensPage()))),
                new TagAndTrackButton("Add Item", new Command(async () => await Navigation.PushAsync(new AddItemPage()))),
                new TagAndTrackButton("Login", new Command(async () => await Navigation.PushAsync(new LoginPage()))),
                new TagAndTrackButton("Settings", new Command(async () => await Navigation.PushAsync(new SettingsPage())))
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                grid.Children.Add(buttons[i]);
                Grid.SetRow(buttons[i], row);
                Grid.SetColumn(buttons[i], col);
            }

            Content = grid;
        } // end initialize()
    }
}
