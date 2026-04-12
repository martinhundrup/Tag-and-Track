using Microsoft.Maui.Controls;
using System.ComponentModel;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class ViewItemPage : TagAndTrackPage
    {
        protected new const string titleText = "View Item";
        public ViewItemPage() { Initialize(); }
        private LoanItem loanItem;
        private List<PropertyChangedEventHandler> themeChangeHandlers = new List<PropertyChangedEventHandler>();

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;

            PropertyChangedEventHandler handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                    if (Content is Layout root) ApplyThemeToLayout(root);
                }
            };

            CurrentTheme.Instance.PropertyChanged += handler;
            themeChangeHandlers.Add(handler);

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

            PropertyChangedEventHandler handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    pageDataBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    pageDataBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };

            CurrentTheme.Instance.PropertyChanged += handler;
            themeChangeHandlers.Add(handler);

            // Placeholder for async-loaded sections
            var containersSection = new VerticalStackLayout { Spacing = 8 };
            var loanHistorySection = new VerticalStackLayout { Spacing = 8 };

            var deleteButton = new TagAndTrackButton("Delete Specimen", new Command(async () => await DeleteSpecimenAsync(specimen)), "trash.png");

            var root = new VerticalStackLayout
            {
                Spacing = 16,
                Padding = new Thickness(16),
                Children = { header, pageDataBorder, deleteButton, containersSection, loanHistorySection }
            };
            ApplyThemeToLayout(root);

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = root
            };

            // Load containers and loan history asynchronously
            _ = LoadSpecimenDetailsAsync(specimen, containersSection, loanHistorySection);
        }

        private async Task LoadSpecimenDetailsAsync(SpecimenItem specimen, VerticalStackLayout containersSection, VerticalStackLayout loanHistorySection)
        {
            var containersTask = DbService.GetContainersBySpecimenIdAsync((int)specimen.ID);
            var loansTask = DbService.GetLoansBySpecimenIdAsync((int)specimen.ID);
            await Task.WhenAll(containersTask, loansTask);

            var containers = containersTask.Result;
            var loans = loansTask.Result;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // --- Containers section ---
                var containerHeader = new Label
                {
                    Text = "Containers:",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = CurrentTheme.Instance.Theme.Text,
                    Margin = new Thickness(0, 10, 0, 4)
                };
                containersSection.Children.Add(containerHeader);

                if (containers.Count == 0)
                {
                    containersSection.Children.Add(new Label
                    {
                        Text = "Not in any containers",
                        FontSize = 14,
                        TextColor = CurrentTheme.Instance.Theme.Text
                    });
                }
                else
                {
                    foreach (var container in containers)
                    {
                        var row = new HorizontalStackLayout { Spacing = 10 };
                        row.Children.Add(new Label
                        {
                            Text = container.Name ?? "Container",
                            FontSize = 14,
                            TextColor = CurrentTheme.Instance.Theme.Text,
                            VerticalOptions = LayoutOptions.Center
                        });
                        var infoBtn = new ImageButton
                        {
                            Source = "info.png",
                            WidthRequest = 24,
                            HeightRequest = 24,
                            BackgroundColor = Colors.Transparent,
                            VerticalOptions = LayoutOptions.Center
                        };
                        int containerId = (int)container.ID;
                        infoBtn.Clicked += async (s, e) =>
                        {
                            await Navigation.PushAsync(new ViewContainerPage(containerId));
                        };
                        row.Children.Add(infoBtn);
                        containersSection.Children.Add(row);
                    }
                }

                // --- Loan history section ---
                var loanHeader = new Label
                {
                    Text = "Loan History:",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = CurrentTheme.Instance.Theme.Text,
                    Margin = new Thickness(0, 10, 0, 4)
                };
                loanHistorySection.Children.Add(loanHeader);

                if (loans.Count == 0)
                {
                    loanHistorySection.Children.Add(new Label
                    {
                        Text = "No loan history for this specimen",
                        FontSize = 14,
                        TextColor = CurrentTheme.Instance.Theme.Text
                    });
                }
                else
                {
                    var dt = new DataTable<LoanItem>(loans, columns =>
                    {
                        columns.Add("Loan ID", l => l.ID, 60);
                        columns.Add("Name", l => l.Name);
                        columns.Add("Borrower", l => l.Borrower);
                        columns.Add("Status", l => l.Status ? "Checked In" : (DateTime.Now > l.DateDue ? "Overdue" : "On Loan"), width: 100);

                        columns.AddButton("View Loan",
                        l =>
                        {
                            ScannedQRItem.lastScannedItem = l.QRID;
                            Navigation.PushAsync(new ViewItemPage());
                        },
                        "info.png", 80);
                    }, showSearchBar: false);
                    loanHistorySection.Children.Add(dt);
                }
            });
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

            var checkinButton = new TagAndTrackButton("Check In", new Command(async () => await CheckInLoan()), "check.png");

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

            PropertyChangedEventHandler handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    pageDataBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    pageDataBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };

            CurrentTheme.Instance.PropertyChanged += handler;
            themeChangeHandlers.Add(handler);

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

            root.Children.Add(header);
            Grid.SetRow(header, 0);

            root.Children.Add(pageDataBorder);
            Grid.SetRow(pageDataBorder, 1);

            // Signature display (if signature is on file)
            View? signatureSection = null;
            var sigDisplay = SignaturePadView.CreateSignatureDisplay(loan.SignatureImageBytes, 300, 120);
            if (sigDisplay != null)
            {
                var sigLabel = new Label
                {
                    Text = "Borrower Signature:",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = CurrentTheme.Instance.Theme.Text
                };
                var sigBorder = new Border
                {
                    Stroke = Colors.DarkGray,
                    StrokeThickness = 1,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
                    Padding = 4,
                    BackgroundColor = Colors.White,
                    Content = sigDisplay
                };
                signatureSection = new VerticalStackLayout
                {
                    Spacing = 4,
                    Children = { sigLabel, sigBorder }
                };
                root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                root.Children.Add(signatureSection);
                Grid.SetRow(signatureSection, 2);
            }

            var specimenGridRow = signatureSection != null ? 3 : 2;
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            var dt = new DataTable<SpecimenItem>(loan.Specimens, columns =>
            {
                columns.Add("ID", s => s.ID, 60);
                columns.Add("Name", s => s.Name);

                columns.AddButton("View Specimen",
                s =>
                {
                    ScannedQRItem.lastScannedItem = s.QRID;
                    Navigation.PushAsync(new ViewItemPage());
                },
                "info.png", 80);

            }, showSearchBar: false);
            var dataView = new DataTableTemplate(loan.Specimens);

            var listContainer = new VerticalStackLayout
            {
                Spacing = 8,
                Children = { listLabel, dt }
            };
            root.Children.Add(listContainer);
            Grid.SetRow(listContainer, specimenGridRow);
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
            
            // Update in-memory state
            loanItem.Checkin();

            // Persist loan to database (cast ulong ID to int for DB)
            await DbService.UpdateLoanAsync((int)loanItem.ID, true);

            // Persist all specimens to database
            foreach (var specimen in loanItem.Specimens)
            {
                await DbService.UpdateSpecimenAsync((int)specimen.ID, true);
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlert("Loan Checked In", $"The loan and all its items have been checked in!", "OK");
            });

                Initialize();

        }

        private async Task DeleteSpecimenAsync(SpecimenItem specimen)
        {
            bool inLoan = await DbService.IsSpecimenInAnyLoanAsync((int)specimen.ID);
            if (inLoan)
            {
                await DisplayAlert("Cannot Delete", "This specimen is referenced by one or more loans (active or past) and cannot be deleted.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Delete Specimen", $"Are you sure you want to permanently delete \"{specimen.Name}\"? This cannot be undone.", "Delete", "Cancel");
            if (!confirm) return;

            await DbService.DeleteSpecimenAsync((int)specimen.ID);
            await DisplayAlert("Deleted", "Specimen has been deleted.", "OK");
            await Navigation.PopAsync();
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
            foreach (var handler in themeChangeHandlers)
            {
                CurrentTheme.Instance.PropertyChanged -= handler;
            }
        }
    }
}
