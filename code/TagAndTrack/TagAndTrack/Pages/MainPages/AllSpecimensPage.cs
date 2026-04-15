using System.ComponentModel;
using System.Text;
using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class AllSpecimensPage : TagAndTrackPage, IDisposable
    {
        protected const string titleText = "View All Specimens";
        private Grid? contentLayout;
        private List<SpecimenItem> _allSpecimens = new();
        private DataTable<SpecimenItem>? _dataTable;
        private string _currentFilter = "All";
        private Button? _allButton;
        private Button? _checkedInButton;
        private Button? _checkedOutButton;
        private PropertyChangedEventHandler handler;

        public AllSpecimensPage()
        {
            DebugLogger.Log("AllSpecimensPage constructor called");
            Initialize();
        }

        protected override void Initialize()
        {
            DebugLogger.Log("AllSpecimensPage.Initialize() starting");
            Background = CurrentTheme.Instance.Theme.Background;
            handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };
            CurrentTheme.Instance.PropertyChanged += handler;

            var header = new HeaderTemplate(titleText);

            contentLayout = new Grid
            {
                Padding = new Thickness(10)
            };

            var pageLayout = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }, // header
                    new RowDefinition { Height = GridLength.Star }  // table
                }
            };

            pageLayout.Children.Add(header);
            Grid.SetRow(header, 0);

            pageLayout.Children.Add(contentLayout);
            Grid.SetRow(contentLayout, 1);

            Content = pageLayout;

            _ = LoadSpecimensAsync();
            DebugLogger.Log("AllSpecimensPage.Initialize() complete");
        }

        protected override void OnAppearing()
        {
            DebugLogger.Log("AllSpecimensPage.OnAppearing() called");
            base.OnAppearing();
            _ = LoadSpecimensAsync();
        }

        private void ApplyStatusFilter(string filter)
        {
            _currentFilter = filter;

            var filtered = filter switch
            {
                "Checked In" => _allSpecimens.Where(s => s.Status).ToList(),
                "Checked Out" => _allSpecimens.Where(s => !s.Status).ToList(),
                _ => _allSpecimens
            };

            _dataTable?.UpdateItems(filtered);
            UpdateFilterButtonStyles();
        }

        private void UpdateFilterButtonStyles()
        {
            var activeColor = Colors.Crimson;
            var inactiveColor = Colors.Gray;

            if (_allButton != null) _allButton.BackgroundColor = _currentFilter == "All" ? activeColor : inactiveColor;
            if (_checkedInButton != null) _checkedInButton.BackgroundColor = _currentFilter == "Checked In" ? activeColor : inactiveColor;
            if (_checkedOutButton != null) _checkedOutButton.BackgroundColor = _currentFilter == "Checked Out" ? activeColor : inactiveColor;
        }

        private async Task LoadSpecimensAsync()
        {
            DebugLogger.Log("AllSpecimensPage.LoadSpecimensAsync() starting");
            if (contentLayout == null) return;

            contentLayout.Children.Clear();
            DebugLogger.Log("AllSpecimensPage: contentLayout cleared");

            _allSpecimens = await DbService.GetAllSpecimensAsync();

            if (_allSpecimens.Count == 0)
            {
                contentLayout.Children.Add(new Label
                {
                    Text = "No specimens found",
                    FontSize = 16,
                    TextColor = CurrentTheme.Instance.Theme.Text,
                    HorizontalOptions = LayoutOptions.Center
                });
                return;
            }

            // Filter buttons
            _allButton = new Button { Text = "All", TextColor = Colors.White, Padding = new Thickness(10, 5), BackgroundColor = Colors.Crimson, CornerRadius = 8 };
            _checkedInButton = new Button { Text = "Checked In", TextColor = Colors.White, Padding = new Thickness(10, 5), BackgroundColor = Colors.Gray, CornerRadius = 8 };
            _checkedOutButton = new Button { Text = "Checked Out", TextColor = Colors.White, Padding = new Thickness(10, 5), BackgroundColor = Colors.Gray, CornerRadius = 8 };

            _allButton.Clicked += (s, e) => ApplyStatusFilter("All");
            _checkedInButton.Clicked += (s, e) => ApplyStatusFilter("Checked In");
            _checkedOutButton.Clicked += (s, e) => ApplyStatusFilter("Checked Out");

            UpdateFilterButtonStyles();

            var filterBar = new HorizontalStackLayout
            {
                Spacing = 10,
                Margin = new Thickness(10, 5),
                Children = { _allButton, _checkedInButton, _checkedOutButton }
            };

            // Create a simple data table
            DebugLogger.Log($"AllSpecimensPage: Creating DataTableTemplate with {_allSpecimens.Count} specimens");
            _dataTable = new DataTable<SpecimenItem>(_allSpecimens, columns =>
            {
                columns.Add("ID", s => s.ID, 60);
                columns.Add("Arctos ID", s => s.ArctosID, 100);
                columns.Add("Name", s => s.Name);
                columns.Add("Description", s => s.Description);
                columns.AddIcon("Status", s =>
                    s.Status
                        ? "check.png"
                        : "cross.png",
                    width: 80);

                columns.AddButton("View Specimen",
                s =>
                {
                    ScannedQRItem.lastScannedItem = s.QRID;
                    Navigation.PushAsync(new ViewItemPage());
                },
                "info.png", 80);

            });

            contentLayout.RowDefinitions.Clear();
            contentLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            contentLayout.Children.Add(filterBar);
            Grid.SetRow(filterBar, 0);

            contentLayout.Children.Add(_dataTable);
            Grid.SetRow(_dataTable, 1);

            DebugLogger.Log("AllSpecimensPage.LoadSpecimensAsync() complete");
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