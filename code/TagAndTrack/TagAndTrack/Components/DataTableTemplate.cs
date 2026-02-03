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

        /// <summary>
        /// Creates a data table template specifically for listing loan specimens, with either a remove button or a button to show info.
        /// </summary>
        /// <param name="specimens">The list of specimens.</param>
        /// <param name="removeButton">Whether or not we are using a remove button.</param>
        public DataTableTemplate(List<SpecimenItem> specimens, bool removeButton, bool finalize = true)
        {
            RowSpacing = 1;
            ColumnSpacing = 1;
            BackgroundColor = CurrentTheme.Instance.Theme.Background;
            HorizontalOptions = LayoutOptions.Center;
            // Define 5 columns
            for (int i = 0; i < 5; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            if (!removeButton)
            {
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                // Create the header with: ID, Arctos IS, Name, Description, Status, View Info
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var headers = new string[] { "ID", "Arctos ID", "Name", "Description", "Status", "View Info" };
                for (int j = 0; j < headers.Length; j++)
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
            else
            {
                // Create the header with: ID, Arctos ID, Name, Description, Remove Item
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var headers = new string[] { "ID", "Arctos ID", "Name", "Description", "Remove Item" };
                for (int j = 0; j < headers.Length; j++)
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



            int row = 1;

            foreach(SpecimenItem spceimen in specimens)
            {
                // Add an entry in the row for ID, Arctos ID, name, and description

                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                // Make  border containing label for ID
                var idborder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, spceimen.ID.ToString()),
                };

                // Make border containing label for Arctos ID
                var arctosborder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, spceimen.ArctosID),
                };

                // Make border containing label for Name
                var nameborder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, spceimen.Name),
                };

                // Make border containing label for Description
                var descborder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, spceimen.Description),
                };

                if(removeButton)
                {
                    // Make border containing button for removing item
                    var removeBtn = new Button
                    {
                        Text = "Remove",
                        FontSize = 12,
                        Padding = new Thickness(10, 4),
                        BindingContext = spceimen,
                        BackgroundColor = Colors.Crimson
                    };
                    removeBtn.Clicked += (s, e) =>
                    {
                        if ((s as Button)?.BindingContext is SpecimenItem sItem)
                        {
                            Backend.Utils.LoanCreator.RemoveItem(sItem);
                            // Refresh page
                            Application.Current.MainPage.Navigation.PopAsync();
                            
                            if(finalize)
                            {
                                Application.Current.MainPage.Navigation.PushAsync(new Pages.SupportPages.FinalizeLoanPage());
                            
                            }
                            else
                            {
                                Application.Current.MainPage.Navigation.PushAsync(new Pages.ViewEditLoanItemsPage());
                            }
                        }
                    };

                    var removeborder = new Border
                    {
                        Stroke = CurrentTheme.Instance.Theme.Borders,
                        BackgroundColor = CurrentTheme.Instance.Theme.Background,
                        StrokeThickness = 1,
                        Content = removeBtn,
                    };

                    // Subscribe all to theme changes
                    CurrentTheme.Instance.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(CurrentTheme.Theme))
                        {
                            idborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            idborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                            arctosborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            arctosborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                            nameborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            nameborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                            descborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            descborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                            removeborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            removeborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        }
                    };
                    // Add all to grid.
                    this.Add(idborder, 0, row);
                    this.Add(arctosborder, 1, row);
                    this.Add(nameborder, 2, row);
                    this.Add(descborder, 3, row);
                    this.Add(removeborder, 4, row);
                }
                else
                {
                    // Make border containg status
                    
                    var statusborder = new Border
                    {
                        Stroke = CurrentTheme.Instance.Theme.Borders,
                        BackgroundColor = CurrentTheme.Instance.Theme.Background,
                        StrokeThickness = 1,
                        Content = new LabelTemplate(10, spceimen.Status.ToString()),
                    };

                    // Make border containing button for viewing info
                    var infoBtn = new Button
                    {
                        Text = "View",
                        FontSize = 12,
                        Padding = new Thickness(10, 4),
                        BindingContext = spceimen,
                        BackgroundColor = Colors.Crimson
                    };

                    infoBtn.Clicked += async (s, e) =>
                    {
                        if ((s as Button)?.BindingContext is SpecimenItem sItem)
                        {
                            ScannedQRItem.lastScannedItem = sItem.QRID;
                            await Navigation.PushAsync(new Pages.ViewItemPage());
                        }
                    };

                    var infoborder = new Border
                    {
                        Stroke = CurrentTheme.Instance.Theme.Borders,
                        BackgroundColor = CurrentTheme.Instance.Theme.Background,
                        StrokeThickness = 1,
                        Content = infoBtn,
                    };

                    // Subscribe all to theme changes
                    CurrentTheme.Instance.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(CurrentTheme.Theme))
                        {
                            idborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            idborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                            arctosborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            arctosborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                            nameborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            nameborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                            descborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            descborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                            statusborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            statusborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                            infoborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                            infoborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        }
                    };

                    // Add all to grid.
                    this.Add(idborder, 0, row);
                    this.Add(arctosborder, 1, row);
                    this.Add(nameborder, 2, row);
                    this.Add(descborder, 3, row);
                    this.Add(statusborder, 4, row);
                    this.Add(infoborder, 5, row);
                }
                row++;
            }
        }

        /// <summary>
        /// Makes a data table for loans
        /// </summary>
        /// <param name="loanItems">The loans</param>
        public DataTableTemplate(List<LoanItem> loanItems)
        {
            RowSpacing = 1;
            ColumnSpacing = 1;
            BackgroundColor = CurrentTheme.Instance.Theme.Background;
            HorizontalOptions = LayoutOptions.Center;

            // Create columns for ID, name, borrower, date checked out, date due, status, and the view button (removed specimens column)
            for (int i = 0; i < 7; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            // Add a row definition for the header row
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string[] headers = { "ID", "Name", "Borrower", "Checked Out", "Due Date", "Status", "View" };
            // Make a row with the headers
            for (int j = 0; j < headers.Length; j++)
            {
                var headerBorder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, headers[j]),
                };
                // row 0 is reserved for headers
                this.Add(headerBorder, j, 0);

                CurrentTheme.Instance.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CurrentTheme.Theme))
                    {
                        headerBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                        headerBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    }
                };
            }

            // Start data rows at 1 (since 0 is header)
            int row = 1;

            foreach (LoanItem loan in loanItems)
            {
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var idborder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, loan.ID.ToString()),
                };

                var nameBorder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, loan.Name),
                };

                var borrowerBorder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, loan.Borrower),
                };

                var checkedOutDateBorder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, loan.DateCheckedOut.ToString("yyyy-MM-dd")),
                };

                var dueDateBorder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, loan.DateDue.ToString("yyyy-MM-dd")),
                };

                var statusBorder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, loan.Status ? "Returned" : "On Loan"),
                };

                var viewBtn = new Button
                {
                    Text = "View",
                    FontSize = 12,
                    Padding = new Thickness(10, 4),
                    BindingContext = loan,
                    BackgroundColor = Colors.Crimson
                };
                viewBtn.Clicked += async (s, e) =>
                {
                    if ((s as Button)?.BindingContext is LoanItem lItem)
                    {
                        ScannedQRItem.lastScannedItem = lItem.QRID;
                        await Navigation.PushAsync(new Pages.ViewItemPage());
                    }
                };

                var buttonBorder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = viewBtn,
                };

                CurrentTheme.Instance.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CurrentTheme.Theme))
                    {
                        idborder.Stroke = CurrentTheme.Instance.Theme.Borders;
                        idborder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        nameBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                        nameBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        borrowerBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                        borrowerBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        checkedOutDateBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                        checkedOutDateBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        dueDateBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                        dueDateBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        statusBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                        statusBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        buttonBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                        buttonBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    }
                };

                this.Add(idborder, 0, row);
                this.Add(nameBorder, 1, row);
                this.Add(borrowerBorder, 2, row);
                this.Add(checkedOutDateBorder, 3, row);
                this.Add(dueDateBorder, 4, row);
                this.Add(statusBorder, 5, row);
                this.Add(buttonBorder, 6, row);

                row++;
            }
        }

    }

}
