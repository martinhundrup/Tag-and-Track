using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TagAndTrack.Components
{
    public class DataTable<T> : ContentView, IDisposable
    {
        private readonly ObservableCollection<T> _allItems;
        private readonly ObservableCollection<T> _filteredItems;

        private readonly SearchBar? searchBar;
        private readonly Border? searchBorder;
        private readonly bool _showSearchBar;

        private EventHandler<TextChangedEventArgs>? _searchHandler;


        private readonly List<DataTableColumn<T>> columns;

        public DataTable(IEnumerable<T> items, Action<DataTableColumnBuilder<T>> config, bool showSearchBar = true)
        {
            _allItems = new ObservableCollection<T>(items);
            _filteredItems = new ObservableCollection<T>(items);
            _showSearchBar = showSearchBar;

            // Erect the columns
            var builder = new DataTableColumnBuilder<T>();
            config(builder);
            columns = builder.Columns;

            if (_showSearchBar)
            {
                // The instrument of inquiry
                searchBar = new SearchBar
                {
                    Placeholder = "Search...",
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    TextColor = CurrentTheme.Instance.Theme.Text
                };
                _searchHandler = (s, e) => ApplyFilter(e.NewTextValue);
                searchBar.TextChanged += _searchHandler;

                searchBorder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    StrokeThickness = 1,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    Padding = new Thickness(1),
                    Margin = new Thickness(10, 10, 10, 0),
                    Content = searchBar
                };

                // Attend to the theme's transformations
                CurrentTheme.Instance.PropertyChanged += ThemeChanged;
            }

            // The header, crowning the table
            var header = new Grid
            {
                BackgroundColor = Colors.LightGray,
                Padding = 6
            };

            foreach (var col in columns)
            {
                header.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = col.Width > 0 ? col.Width : GridLength.Star
                });
            }

            for (int i = 0; i < columns.Count; i++)
            {
                header.Add(new Label
                {
                    Text = columns[i].Header,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black
                }, i, 0);
            }

            // The table itself, bearer of all records
            var table = new CollectionView
            {
                ItemsSource = _filteredItems,
                ItemTemplate = new DataTemplate(() =>
                {
                    var row = new Grid
                    {
                        Padding = 6,
                        BackgroundColor = Colors.AliceBlue,
                        Margin = new Thickness(0, 0, 0, 1)
                    };

                    foreach (var col in columns)
                    {
                        row.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = col.Width > 0 ? col.Width : GridLength.Star
                        });
                    }

                    for (int i = 0; i < columns.Count; i++)
                    {
                        var col = columns[i];

                        if (col.IsButton)
                        {
                            var btn = new ImageButton
                            {
                                Source = col.ButtonIcon,
                                BackgroundColor = Colors.Transparent,
                                Padding = 0,
                                WidthRequest = 24,
                                HeightRequest = 24,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                Aspect = Aspect.AspectFit
                            };

                            btn.Clicked += (s, e) =>
                            {
                                if (btn.BindingContext is T item)
                                    col.ButtonAction?.Invoke(item);
                            };

                            row.Add(btn, i, 0);
                        }
                        else if (col.IsIcon)
                        {
                            var imgBtn = new ImageButton
                            {
                                BackgroundColor = Colors.Transparent,
                                Padding = 0,
                                WidthRequest = 24,
                                HeightRequest = 24,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                Aspect = Aspect.AspectFit,
                                IsEnabled = false // behaves like an image
                            };

                            imgBtn.SetBinding(ImageButton.SourceProperty, new Binding
                            {
                                Path = ".",
                                Converter = new FuncConverter<T, ImageSource>(item =>
                                    col.IconSelector!(item)
                                )
                            });

                            row.Add(imgBtn, i, 0);


                        }
                        else if (col.IsCheckbox)
                        {
                            var cb = new CheckBox
                            {
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                Color = Colors.Crimson
                            };

                            if (col.CheckboxInitialValue != null)
                            {
                                cb.BindingContextChanged += (s, e) =>
                                {
                                    if (cb.BindingContext is T initItem)
                                        cb.IsChecked = col.CheckboxInitialValue(initItem);
                                };
                            }

                            cb.CheckedChanged += (s, e) =>
                            {
                                if (cb.BindingContext is T item)
                                    col.CheckboxAction?.Invoke(item, e.Value);
                            };

                            row.Add(cb, i, 0);
                        }
                        else
                        {
                            var label = new Label { TextColor = Colors.Black };

                            label.SetBinding(Label.TextProperty, new Binding
                            {
                                Path = ".",
                                Converter = new FuncConverter<T, string>(item =>
                                    col.ValueSelector(item)?.ToString() ?? "")
                            });

                            row.Add(label, i, 0);
                        }
                    }

                    return row;
                })
            };

            // The arrangement of all within
            int row = 0;
            var layout = new Grid();

            if (_showSearchBar)
            {
                layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // search
            }

            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // header
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }); // data



            if (_showSearchBar)
            {
                layout.Add(searchBorder);
                Grid.SetRow(searchBorder!, row++);

            }

            layout.Add(header);
            Grid.SetRow(header, row++);

            layout.Add(table);
            Grid.SetRow(table, row);


            Content = layout;
        }

        private void ThemeChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentTheme.Theme))
            {
                if (searchBar != null) searchBar.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                if (searchBar != null) searchBar.TextColor = CurrentTheme.Instance.Theme.Text;
                if (searchBorder != null) searchBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                if (searchBorder != null) searchBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
            }
        }

        private void ApplyFilter(string query)
        {
            query = query?.ToLower() ?? "";

            _filteredItems.Clear();

            foreach (var item in _allItems)
            {
                bool match = columns.Any(col =>
                {
                    if (!col.IsFilterable) return false;
                    var value = col.ValueSelector(item)?.ToString()?.ToLower();
                    return value?.Contains(query) ?? false;
                });

                if (match)
                    _filteredItems.Add(item);
            }
        }

        public void RemoveItem(T item)
        {
            _allItems.Remove(item);
            _filteredItems.Remove(item);
        }

        public void UpdateItems(IEnumerable<T> newItems)
        {
            _allItems.Clear();
            _filteredItems.Clear();
            foreach (var item in newItems)
            {
                _allItems.Add(item);
                _filteredItems.Add(item);
            }
        }

        public void Dispose()
        {
            if (_showSearchBar)
            {
                CurrentTheme.Instance.PropertyChanged -= ThemeChanged;
                searchBar?.TextChanged -= _searchHandler;
            }
        }

        protected override void OnParentChanged()
        {
            base.OnParentChanged();
            if (Parent == null)
            {
                Dispose();
            }
        }
    }
}