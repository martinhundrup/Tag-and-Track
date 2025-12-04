using Microsoft.Maui.Controls;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class ViewItemPage : TagAndTrackPage
    {
        protected new const string titleText = "View Item";
        public ViewItemPage() { Initialize(); }
        private LoanItem loanItem;

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                    if (Content is Layout root) ApplyThemeToLayout(root);
                }
            };
            var header = new HeaderTemplate(titleText);

            if (ScannedQRItem.lastScannedItem == null)
            {
                Content = BuildMessage("No item scanned.");
                return;
            }

            if (ScannedQRItem.lastScannedItem.StartsWith(Item.ItemType.Specimen.ToString()))
            {
                var specimen = ItemManager.GetItemByQRID(ScannedQRItem.lastScannedItem) as SpecimenItem;
                SpecimenView(specimen);
            }
            else if (ScannedQRItem.lastScannedItem.StartsWith(Item.ItemType.Loan.ToString()))
            {
                var loan = ItemManager.GetItemByQRID(ScannedQRItem.lastScannedItem) as LoanItem;
                LoanView(loan);
            }
            else
            {
                Content = BuildMessage("Unsupported item type.");
            }
        }

        protected void SpecimenView(SpecimenItem specimen)
        {
            if (specimen == null)
            {
                Content = BuildMessage("Specimen not found.");
                return;
            }

            var header = new HeaderTemplate(specimen.Name ?? "Specimen");
            /*
            var info = BuildInfoGrid();
            AddInfoRow(info, 0, "Type", specimen.Type.ToString());
            AddInfoRow(info, 1, "ID", specimen.ID.ToString());
            AddInfoRow(info, 2, "Arctos ID", specimen.ArctosID ?? "None");
            AddInfoRow(info, 3, "QR", specimen.QRID ?? "");
            AddInfoRow(info, 4, "Status", specimen.Status ? "Present" : "Checked out");
            AddInfoRow(info, 5, "Description", specimen.Description ?? "");
            */
            string status = specimen.Status ? "Present" : "Checked out";

            var data = new DataTableTemplate(string.Empty, $"Type,{specimen.Type.ToString()}\nID,{specimen.ID.ToString()}\nArctos ID,{specimen.ArctosID ?? "None"}\nQR,{specimen.QRID ?? ""}" +
                $"\nStatus,{status}\nDescription,{specimen.Description ?? ""}");

            var qr = new QrCodeView
            {
                Value = specimen.QRID,
                Size = 220,
                Padding = 4,
            };



            var pageData = new HorizontalStackLayout
            {
                Spacing = 32,
                Children = { data, qr }
            };

            var pageDataBorder = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                StrokeThickness = 1,
                Content = pageData
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    pageDataBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    pageDataBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };

            var root = new VerticalStackLayout
            {
                Spacing = 16,
                Padding = new Thickness(16),
                Children = { header, pageDataBorder }
            };
            ApplyThemeToLayout(root);

            // simple page that scrolls if needed
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = root
            };
        }

        protected void LoanView(LoanItem loan)
        {
            if (loan == null)
            {
                Content = BuildMessage("Loan not found.");
                return;
            }

            loanItem = loan;
            var header = new HeaderTemplate($"Loan {loan.ID}");

            /*
            var info = BuildInfoGrid();
            AddInfoRow(info, 0, "Type", loan.Type.ToString());
            AddInfoRow(info, 1, "ID", loan.ID.ToString());
            AddInfoRow(info, 2, "Arctos ID", loan.ArctosID ?? "None");
            AddInfoRow(info, 3, "Borrower", loan.Borrower ?? "None");
            AddInfoRow(info, 4, "Email", loan.Email ?? "None");
            AddInfoRow(info, 5, "Checked out", loan.DateCheckedOut == default ? "Unknown" : loan.DateCheckedOut.ToString("yyyy-MM-dd"));
            AddInfoRow(info, 6, "Due", loan.DateDue == default ? "Unknown" : loan.DateDue.ToString("yyyy-MM-dd"));
            AddInfoRow(info, 7, "Specimens", $"{loan.Specimens.Count}");*/

            string checkedOut = loan.DateCheckedOut == default ? "Unknown" : loan.DateCheckedOut.ToString("yyyy-MM-dd");
            string due = loan.DateDue == default ? "Unknown" : loan.DateDue.ToString("yyyy-MM-dd");

            var data = new DataTableTemplate(string.Empty, $"Type,{loan.Type.ToString()}\nID,{loan.ID.ToString()}\nArctos ID,{loan.ArctosID ?? "None"}\nBorrower,{loan.Borrower ?? "None"}" +
               $"\nEmail,{loan.Email ?? "None"}\nChecked out,{checkedOut}\nDue,{due}\nSpecimens,{loan.Specimens.Count.ToString()}");


            var qr = new QrCodeView
            {
                Value = loan.QRID,
                Size = 220,
                Padding = 4,
            };

            var checkinButton = new TagAndTrackButton("Check In", new Command(async () => await CheckInLoan()));

            var pageData = new HorizontalStackLayout
            {
                Spacing = 32,
                Children = { data, qr }
            };

            if (!loan.Status) // if checked out, add check in button
            {
                pageData.Children.Add(checkinButton);
            }

            var pageDataBorder = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                StrokeThickness = 1,
                Content = pageData
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    pageDataBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    pageDataBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };

            var listLabel = new Label
            {
                Text = "Specimens in loan",
                FontSize = 14,
                TextColor = CurrentTheme.Instance.Theme.Text
            };

            // Page layout: info at top, list below. Let the CollectionView own scrolling.
            var root = new Grid
            {
                RowSpacing = 16,
                Padding = new Thickness(16)
            };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            root.Children.Add(header);
            Grid.SetRow(header, 0);

            root.Children.Add(pageDataBorder);
            Grid.SetRow(pageDataBorder, 1);

            var dataView = new DataTableTemplate(loan.Specimens);

            var listContainer = new VerticalStackLayout
            {
                Spacing = 8,
                Children = { listLabel, dataView }
            };
            root.Children.Add(listContainer);
            Grid.SetRow(listContainer, 2);
            //root.Children.Add(qr);
            
            ApplyThemeToLayout(root);

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = root
            };
        }

        private async Task CheckInLoan()
        {
            if (loanItem == null) return;
            loanItem.Checkin();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlert("Loan Checked In", $"The loan and all its items have been checked in!", "OK");
            });

                Initialize();

        }

        private Grid BuildInfoGrid()
        {
            var info = new Grid
            {
                ColumnSpacing = 12,
                RowSpacing = 8
            };
            info.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            info.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            return info;
        }

        private void AddInfoRow(Grid grid, int rowIndex, string key, string value)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var keyLbl = new Label
            {
                Text = key,
                FontSize = 12,
                TextColor = CurrentTheme.Instance.Theme.Text
            }; 

            var valLbl = new Label
            {
                Text = value,
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                TextColor = CurrentTheme.Instance.Theme.Text
            };

            grid.Children.Add(keyLbl);
            Grid.SetRow(keyLbl, rowIndex);
            Grid.SetColumn(keyLbl, 0);

            grid.Children.Add(valLbl);
            Grid.SetRow(valLbl, rowIndex);
            Grid.SetColumn(valLbl, 1);
        }

        private View BuildMessage(string text)
        {
            var lbl = new Label
            {
                Text = text,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = CurrentTheme.Instance.Theme.Text
            };
            var layout = new Grid { Padding = new Thickness(16) };
            layout.Children.Add(lbl);
            ApplyThemeToLayout(layout);
            return layout;
        }

        private void ApplyThemeToLayout(Layout root)
        {
            root.BackgroundColor = CurrentTheme.Instance.Theme.Background;
            foreach (var c in root.Children)
            {
                if (c is Layout layout) ApplyThemeToLayout(layout);
                if (c is Label lbl) lbl.TextColor = CurrentTheme.Instance.Theme.Text;
            }
        }
    }
}
