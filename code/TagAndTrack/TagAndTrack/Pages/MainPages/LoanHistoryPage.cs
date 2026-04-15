using System.ComponentModel;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class LoanHistoryPage : TagAndTrackPage
    {
        protected const string titleText = "View Loan History";
        private List<LoanItem> _allLoans = new();
        private DataTable<LoanItem>? _dataTable;
        private string _currentFilter = "All";
        private Button? _allButton;
        private Button? _onLoanButton;
        private Button? _overdueButton;
        private Button? _checkedInButton;
        private PropertyChangedEventHandler handler;

        public LoanHistoryPage() { Initialize(); }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BuildContent();
        }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;

            handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };
            CurrentTheme.Instance.PropertyChanged += handler;
        }

        private string GetLoanStatus(LoanItem loan)
        {
            if (loan.Status) return "Checked In";
            return DateTime.Now > loan.DateDue ? "Overdue" : "On Loan";
        }

        private void ApplyStatusFilter(string filter)
        {
            _currentFilter = filter;

            var filtered = filter switch
            {
                "On Loan" => _allLoans.Where(l => GetLoanStatus(l) == "On Loan").ToList(),
                "Overdue" => _allLoans.Where(l => GetLoanStatus(l) == "Overdue").ToList(),
                "Checked In" => _allLoans.Where(l => GetLoanStatus(l) == "Checked In").ToList(),
                _ => _allLoans
            };

            _dataTable?.UpdateItems(filtered);
            UpdateFilterButtonStyles();
        }

        private void UpdateFilterButtonStyles()
        {
            var activeColor = Colors.Crimson;
            var inactiveColor = Colors.Gray;

            if (_allButton != null) _allButton.BackgroundColor = _currentFilter == "All" ? activeColor : inactiveColor;
            if (_onLoanButton != null) _onLoanButton.BackgroundColor = _currentFilter == "On Loan" ? activeColor : inactiveColor;
            if (_overdueButton != null) _overdueButton.BackgroundColor = _currentFilter == "Overdue" ? activeColor : inactiveColor;
            if (_checkedInButton != null) _checkedInButton.BackgroundColor = _currentFilter == "Checked In" ? activeColor : inactiveColor;
        }

        private void BuildContent()
        {
            var allItems = ItemManager.GetItemsOfType(Item.ItemType.Loan);
            _allLoans = new List<LoanItem>();
            foreach (var item in allItems)
            {
                _allLoans.Add((LoanItem)item);
            }

            // Filter buttons
            _allButton = new Button { Text = "All", TextColor = Colors.White, Padding = new Thickness(10, 5), BackgroundColor = Colors.Crimson, CornerRadius = 8 };
            _onLoanButton = new Button { Text = "On Loan", TextColor = Colors.White, Padding = new Thickness(10, 5), BackgroundColor = Colors.Gray, CornerRadius = 8 };
            _overdueButton = new Button { Text = "Overdue", TextColor = Colors.White, Padding = new Thickness(10, 5), BackgroundColor = Colors.Gray, CornerRadius = 8 };
            _checkedInButton = new Button { Text = "Checked In", TextColor = Colors.White, Padding = new Thickness(10, 5), BackgroundColor = Colors.Gray, CornerRadius = 8 };

            _allButton.Clicked += (s, e) => ApplyStatusFilter("All");
            _onLoanButton.Clicked += (s, e) => ApplyStatusFilter("On Loan");
            _overdueButton.Clicked += (s, e) => ApplyStatusFilter("Overdue");
            _checkedInButton.Clicked += (s, e) => ApplyStatusFilter("Checked In");

            _currentFilter = "All";
            UpdateFilterButtonStyles();

            var filterBar = new HorizontalStackLayout
            {
                Spacing = 10,
                Margin = new Thickness(10, 5),
                Children = { _allButton, _onLoanButton, _overdueButton, _checkedInButton }
            };

            _dataTable = new DataTable<LoanItem>(_allLoans, columns =>
            {
                columns.Add("ID", s => s.ID, 60);
                columns.Add("Name", s => s.Name);
                columns.Add("Borrower", s => s.Borrower);
                columns.Add("Checked Out", s => s.DateCheckedOut.ToShortDateString(), width: 100, filterable: false);
                columns.Add("Due Date", s => s.DateDue.ToShortDateString(), width: 100, filterable: false);
                columns.Add("Status", s => GetLoanStatus(s), width: 100);

                columns.AddButton("View Loan",
                s =>
                {
                    ScannedQRItem.lastScannedItem = s.QRID;
                    Navigation.PushAsync(new ViewItemPage());
                },
                "info.png", 80);

            });

            var header = new HeaderTemplate(titleText);

            var pageLayout = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }, // header
                    new RowDefinition { Height = GridLength.Auto }, // filter bar
                    new RowDefinition { Height = GridLength.Star }  // table
                }
            };

            pageLayout.Children.Add(header);
            Grid.SetRow(header, 0);

            pageLayout.Children.Add(filterBar);
            Grid.SetRow(filterBar, 1);

            pageLayout.Children.Add(_dataTable);
            Grid.SetRow(_dataTable, 2);

            Content = pageLayout;
        }

        protected override void OnParentChanged()
        {
            base.OnParentChanged();
            if(Parent == null)
            {
                Dispose();
            }
        }
        
        public void Dispose()
        {
            CurrentTheme.Instance.PropertyChanged -= handler;
        }
    }
}