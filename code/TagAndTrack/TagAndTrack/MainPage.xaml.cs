using System.Diagnostics;
using TagAndTrack.Backend;
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
            Initialize();
        }

        private void Initialize()
        {
            Title = null;
            Background = CurrentTheme.Instance.Theme.Background;
            DebugLogger.Init();
            DebugLogger.Log(DebugLogger.GetLogFilePath());
            DebugLogger.Log("App started");
            //System.Diagnostics.Process.Start("explorer.exe", Path.GetDirectoryName(DebugLogger.GetLogFilePath())); // uncoomment to open log file location windows ONLY

            ItemManager.LoadAllDebugItems();
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };
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
                    new RowDefinition { Height = GridLength.Auto },
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
                new TagAndTrackButton("Settings", new Command(async () => await Navigation.PushAsync(new SettingsPage()))),
                new TagAndTrackButton("Light/Dark Mode", new Command(() => CurrentTheme.Instance.SwitchTheme())),
                new TagAndTrackButton("Specimen:1", new Command(async () => foo())),
                new TagAndTrackButton("Loan:1", new Command(async () => bar())),
                new TagAndTrackButton("Send Email", new Command(async () => TestEmail())),
                new TagAndTrackButton("Open Debug Logs", new Command(async () => OpenDebugLogs())),

            };

            for (int i = 0; i < buttons.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                grid.Children.Add(buttons[i]);
                Grid.SetRow(buttons[i], row);
                Grid.SetColumn(buttons[i], col);
            }

            Content = new StackLayout()
            {
                Children = { new HeaderTemplate("Home", true), grid }
            };


        } // end initialize()

        private async void foo()
        {
            ScannedQRItem.lastScannedItem = "Specimen:1";
            await Navigation.PushAsync(new ViewItemPage());
        }
        private async void bar()
        {
            ScannedQRItem.lastScannedItem = "Loan:1";
            await Navigation.PushAsync(new ViewItemPage());
        }

        private void TestEmail()
        {
            DebugLogger.Log("attempting to send email to hundrupm@gmail.com");
            var response = Emailer.Email("hundrupm@gmail.com", "test", "hello world!");
            DebugLogger.Log($"email response: {response}");
        }

        private void OpenDebugLogs()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = DebugLogger.GetLogFilePath(),
                UseShellExecute = true
            });
        }
    }
}
