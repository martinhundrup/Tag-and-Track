using System.Collections.ObjectModel;
using TagAndTrack.Backend.Items;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Class for a data table that will contain specimens.
    /// </summary>
    public class SpecimenDataTable : ContentView, IDisposable
    {
        /// <summary>
        /// All specimens loaded from the database, used for filtering.
        /// </summary>
        private readonly ObservableCollection<SpecimenItem> _allSpecimens;

        /// <summary>
        /// The specimens displayed in the table, which may be filtered.
        /// </summary>
        private readonly ObservableCollection<SpecimenItem> _filteredSpecimens;

        /// <summary>
        /// The search bar used to filter specimens in the table.
        /// </summary>
        private SearchBar searchBar;

        /// <summary>
        /// The border around the search bar.
        /// </summary>
        private Border searchBorder;

        /// <summary>
        /// Creates new instance of the <see cref="SpecimenDataTable"/> class.
        /// </summary>
        /// <param name="specimens">The list of specimens to go in the table.</param>
        public SpecimenDataTable(List<SpecimenItem> specimens)
        {
            _allSpecimens = new ObservableCollection<SpecimenItem>(specimens);
            _filteredSpecimens = new ObservableCollection<SpecimenItem>(specimens);

            searchBar = new SearchBar
            {
                Placeholder = "Search specimens...",
                Margin = new Thickness(10, 10, 10, 0),
                BackgroundColor = Colors.Transparent,
                TextColor = CurrentTheme.Instance.Theme.Text
            };
            
            searchBar.TextChanged += (s, e) =>
            {
                ApplyFilter(e.NewTextValue);
            };

            searchBorder = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                StrokeThickness = 1,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                Padding = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 0),
                Content = searchBar
            };

            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    searchBar.TextColor = CurrentTheme.Instance.Theme.Text;
                    searchBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    searchBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };


            // Sticky header
            var header = new Grid
            {
                BackgroundColor = Colors.LightGray,
                Padding = 6,
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = 60 },
                new ColumnDefinition { Width = 100 },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 100 },
                new ColumnDefinition { Width = 80 }
            }
            };

            header.Add(new Label { Text = "ID", FontAttributes = FontAttributes.Bold, TextColor = Colors.Black }, 0, 0);
            header.Add(new Label { Text = "Arctos ID", FontAttributes = FontAttributes.Bold, TextColor = Colors.Black }, 1, 0);
            header.Add(new Label { Text = "Name", FontAttributes = FontAttributes.Bold, TextColor = Colors.Black }, 2, 0);
            header.Add(new Label { Text = "Description", FontAttributes = FontAttributes.Bold, TextColor = Colors.Black }, 3, 0);
            header.Add(new Label { Text = "Status", FontAttributes = FontAttributes.Bold, TextColor = Colors.Black }, 4, 0);
            header.Add(new Label { Text = "Action", FontAttributes = FontAttributes.Bold, TextColor = Colors.Black }, 5, 0);

            // Table body
            var table = new CollectionView
            {
                ItemsSource = _filteredSpecimens,
                ItemTemplate = new DataTemplate(() =>
                {
                    var row = new Grid
                    {
                        Padding = 6,
                        ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = 60 },
                        new ColumnDefinition { Width = 100 },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = 100 },
                        new ColumnDefinition { Width = 80 }
                    },
                        BackgroundColor = Colors.AliceBlue,
                        Margin = new Thickness(0, 0, 0, 1)
                    };

                    var id = new Label();
                    id.SetBinding(Label.TextProperty, "ID");
                    id.TextColor = Colors.Black;

                    var arctos = new Label();
                    arctos.SetBinding(Label.TextProperty, "ArctosID");
                    arctos.TextColor = Colors.Black;

                    var name = new Label();
                    name.SetBinding(Label.TextProperty, "Name");
                    name.TextColor = Colors.Black;

                    var desc = new Label();
                    desc.SetBinding(Label.TextProperty, "Description");
                    desc.TextColor = Colors.Black;

                    var status = new Label();
                    status.SetBinding(Label.TextProperty, "Status");
                    status.TextColor = Colors.Black;

                    var btn = new Button
                    {
                        // Image will only show its tint on iOS.
                        ImageSource = "info_circle.svg",
                        TextColor = Colors.Black,
                        BackgroundColor = Colors.Transparent,
                        BorderWidth = 0,
                        Padding = 4,
                        WidthRequest = 24,
                        HeightRequest = 24,
                    };

                    btn.Clicked += async (s, e) =>
                    {
                        if ((s as Button)?.BindingContext is SpecimenItem sItem)
                        {
                            ScannedQRItem.lastScannedItem = sItem.QRID;
                            await Navigation.PushAsync(new Pages.ViewItemPage());
                        }
                    };

                    row.Add(id, 0, 0);
                    row.Add(arctos, 1, 0);
                    row.Add(name, 2, 0);
                    row.Add(desc, 3, 0);
                    row.Add(status, 4, 0);
                    row.Add(btn, 5, 0);

                    return row;
                })
            };

            var layout = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }, // search
                    new RowDefinition { Height = GridLength.Auto }, // header
                    new RowDefinition { Height = GridLength.Star }  // table
                }
            };

            // Add children
            layout.Children.Add(searchBorder);
            layout.Children.Add(header);
            layout.Children.Add(table);

            // Position them
            Grid.SetRow(searchBar, 0);
            Grid.SetColumn(searchBar, 0);

            Grid.SetRow(header, 1);
            Grid.SetColumn(header, 0);

            Grid.SetRow(table, 2);
            Grid.SetColumn(table, 0);

            Content = layout;
        }

        private void ApplyFilter(string query)
        {
            query = query?.ToLower() ?? "";

            _filteredSpecimens.Clear();

            foreach (SpecimenItem s in _allSpecimens)
            {
                
                if (s.ID.ToString().Contains(query) ||
                    (s.ArctosID?.ToString().ToLower().Contains(query) ?? false) ||
                    (s.Name?.ToLower().Contains(query) ?? false) ||
                    (s.Description?.ToLower().Contains(query) ?? false))
                {
                    _filteredSpecimens.Add(s);
                }
            }
        }

        /// <summary>
        /// Disposes of any subsctiptions made by the component to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            // Unsubscribe from theme changes.
            CurrentTheme.Instance.PropertyChanged -= (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    searchBar.TextColor = CurrentTheme.Instance.Theme.Text;
                    searchBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    searchBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };

            searchBar.TextChanged -= (s, e) =>
            {
                ApplyFilter(e.NewTextValue);
            };
        }
    }
}
