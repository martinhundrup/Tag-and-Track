namespace TagAndTrack
{
    public class MainPage : TagAndTrackPage
    {
        protected new const string titleText = "Tag and Track";
        public MainPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Children =
                {
                    new Button // scan item page
                    {
                        Text = "Scan Item",
                        Command = new Command(async () => await Navigation.PushAsync(new ScanItemPage()))
                    },
                    new Button // start loan page
                    {
                        Text = "Start Loan",
                        Command = new Command(async () => await Navigation.PushAsync(new StartLoanPage()))
                    },
                    new Button // check-in loan page
                    {
                        Text = "Check-in Loan",
                        Command = new Command(async () => await Navigation.PushAsync(new CheckInLoanPage()))
                    },
                    new Button // loan history page
                    {
                        Text = "Loan History",
                        Command = new Command(async () => await Navigation.PushAsync(new LoanHistoryPage()))
                    },
                    new Button // all specimens page
                    {
                        Text = "All Specimens",
                        Command = new Command(async () => await Navigation.PushAsync(new AllSpecimensPage()))
                    },
                    new Button // add item page
                    {
                        Text = "Add Item",
                        Command = new Command(async () => await Navigation.PushAsync(new AddItemPage()))
                    },
                    new Button // Login page
                    {
                        Text = "Login",
                        Command = new Command(async () => await Navigation.PushAsync(new LoginPage()))
                    },
                    new Button // Settings page
                    {
                        Text = "Settings",
                        Command = new Command(async () => await Navigation.PushAsync(new SettingsPage()))
                    },
                }
            };
        }
    }
}