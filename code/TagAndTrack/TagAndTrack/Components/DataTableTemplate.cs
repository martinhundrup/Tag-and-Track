using TagAndTrack.Backend.Items;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Template class for data tables.
    /// </summary>
    public class DataTableTemplate : Grid
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DataTableTemplate"/> class.
        /// </summary>
        /// <param name="headers">The list of headers that there will be.</param>
        /// <param name="csvString">A csv string of data values (we can change this as we get more data interfacing).</param>
        public DataTableTemplate(string headerString, string csvString)
        {
            RowSpacing = 1;
            ColumnSpacing = 1;
            BackgroundColor = CurrentTheme.Instance.Theme.Background;
            HorizontalOptions = LayoutOptions.Center;

            string[] headers;
            if (string.IsNullOrEmpty(headerString))
            {
                headers = null;
            }
            else
            {
                headers = headerString.Split(new[] { ',' });
            }

            string[] rows = csvString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] data;


            int rowCount = rows.Length; // +1 for header row
            int columnCount = 0;
            if (headers != null)
            {
                columnCount = headers.Length;
            }
            else
            {                 // If no headers provided, infer column count from first row of data
                data = rows[0].Split(new[] { ',' }, StringSplitOptions.None);
                columnCount = data.Length;
            }

            // Define rows and columns.
            for (int i = 0; i < rowCount; i++)
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int j = 0; j < columnCount; j++)
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            if (headers != null)
            {


                for (int j = 0; j < columnCount; j++)
                {

                    var headerBorder = new Border
                    {
                        Stroke = CurrentTheme.Instance.Theme.Borders,
                        BackgroundColor = CurrentTheme.Instance.Theme.Background,
                        StrokeThickness = 1,
                        Content = new LabelTemplate(10, headers[j]),
                    };

                    this.Add(headerBorder, j, 0);

                    // Subscribe to theme changes.
                    CurrentTheme.Instance.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(CurrentTheme.Theme))
                        {
                            headerBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            headerBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        }
                    };
                }
            }
                

            // Add cells with borders
            for (int i = 0; i < rowCount; i++)
            {
                data = rows[i].Split(new[] { ',' }, StringSplitOptions.None);
                for (int j = 0; j < columnCount; j++)
                {
                    var border = new Border
                    {
                        Stroke = CurrentTheme.Instance.Theme.Borders,
                        BackgroundColor = CurrentTheme.Instance.Theme.Background,
                        StrokeThickness = 1,
                        Content = new LabelTemplate(10, data[j]),
                    };

                    this.Add(border, j, i + 1);

                    // Subscribe to theme changes.
                    CurrentTheme.Instance.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(CurrentTheme.Theme))
                        {
                            border.Stroke = CurrentTheme.Instance.Theme.Borders;
                            border.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Creates a data table template specifically for listing loan specimens.
        /// </summary>
        /// <param name="specimens">The specimens of the loan.</param>
        public DataTableTemplate(IReadOnlyList<SpecimenItem> specimens)
        {
            RowSpacing = 1;
            ColumnSpacing = 1;
            BackgroundColor = CurrentTheme.Instance.Theme.Background;

            // Define 3 ColumnDefinitions for ID, Name, and button
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            int row = 0;

            // Add header row.
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            // First column: ID header.
            var idHeaderBorder = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                StrokeThickness = 1,
                Content = new LabelTemplate(10, "ID"),
            };
            this.Add(idHeaderBorder, 0, row);
            // Subscribe to theme changes.
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    idHeaderBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    idHeaderBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };
            // Second column: Name header.
            var nameHeaderBorder = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                StrokeThickness = 1,
                Content = new LabelTemplate(10, "Name"),
            };
            this.Add(nameHeaderBorder, 1, row);
            // Subscribe to theme changes.
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    nameHeaderBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    nameHeaderBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };
            // Third column: Action header.
            var actionHeaderBorder = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                StrokeThickness = 1,
                Content = new LabelTemplate(10, "View"),
            };
            this.Add(actionHeaderBorder, 2, row);
            // Subscribe to theme changes.
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    actionHeaderBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    actionHeaderBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };

            row = 1;
            foreach (var specimen in specimens)
            {
                // Make a row that contains id, name, and status
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // First column: ID.
                var border = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, specimen.ID.ToString()),
                };
                this.Add(border, 0, row);
                // Subscribe to theme changes.
                CurrentTheme.Instance.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CurrentTheme.Theme))
                    {
                        border.Stroke = CurrentTheme.Instance.Theme.Borders;
                        border.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    }
                };

                // Second column: Name.
                border = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, specimen.Name),
                };
                this.Add(border, 1, row);
                // Subscribe to theme changes.
                CurrentTheme.Instance.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CurrentTheme.Theme))
                    {
                        border.Stroke = CurrentTheme.Instance.Theme.Borders;
                        border.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    }
                };

                // Third column: open buttons
                var openBtn = new Button
                {
                    Text = "Open",
                    FontSize = 12,
                    Padding = new Thickness(10, 4),
                    BindingContext = specimen,
                    BackgroundColor = Colors.Crimson
                };
                openBtn.Clicked += async (s, e) =>
                {
                    if ((s as Button)?.BindingContext is SpecimenItem sItem)
                    {
                        ScannedQRItem.lastScannedItem = sItem.QRID;
                        await Navigation.PushAsync(new Pages.ViewItemPage());
                    }
                };
                border = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = openBtn,
                };
                this.Add(border, 2, row);
                // Subscribe to theme changes.
                CurrentTheme.Instance.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CurrentTheme.Theme))
                    {
                        border.Stroke = CurrentTheme.Instance.Theme.Borders;
                        border.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    }
                };

                row++;
            }
        }
            
    }

}
