using System;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Text;
using TagAndTrack.Backend;
using TagAndTrack.Backend.Items;
using TagAndTrack.Backend.Utils;
using TagAndTrack.Components;

namespace TagAndTrack.Pages.SupportPages
{
    public class FinalizeLoanPage : TagAndTrackPage
    {
        protected const string titleText = "Finalize Loan";

        private TextboxTemplate loanNameEntry;
        private TextboxTemplate loanDescriptionEntry;
        private TextboxTemplate clientNameEntry;
        private TextboxTemplate clientEmailEntry;
        private DatePicker dueDatePicker;
        private SignaturePadView signaturePad;
        private List<PropertyChangedEventHandler> themeChangeHandlers = new List<PropertyChangedEventHandler>();

        public FinalizeLoanPage()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            // Apply theme background and react to theme changes
            Background = CurrentTheme.Instance.Theme.Background;

            PropertyChangedEventHandler themeChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };

            CurrentTheme.Instance.PropertyChanged += themeChangedHandler;
            themeChangeHandlers.Add(themeChangedHandler);

            var header = new HeaderTemplate(titleText);

            var instructionLabel = new Label
            {
                Text = "Please enter client information:",
                Margin = new Thickness(24, 24, 24, 12),
                FontSize = 18
            };

            loanNameEntry = new TextboxTemplate(300, "Loan Name");
            loanDescriptionEntry = new TextboxTemplate(300, "Loan Description");
            clientNameEntry = new TextboxTemplate(300, "Client Name");
            clientEmailEntry = new TextboxTemplate(300, "Client Email");

            var dueDateLabel = new Label
            {
                Text = "Due Date:",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(24, 20, 24, 4),
                TextColor = CurrentTheme.Instance.Theme.Text
            };

            themeChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    dueDateLabel.TextColor = CurrentTheme.Instance.Theme.Text;
            };

            CurrentTheme.Instance.PropertyChanged += themeChangedHandler;
            themeChangeHandlers.Add(themeChangedHandler);

            dueDatePicker = new DatePicker
            {
                MinimumDate = DateTime.Today.AddDays(1),
                Date = DateTime.Today.AddDays(30),
                Format = "yyyy-MM-dd",
                HorizontalOptions = LayoutOptions.Fill,
                TextColor = CurrentTheme.Instance.Theme.Text,
                BackgroundColor = Colors.Transparent,
                HeightRequest = 40
            };

            var calendarIcon = new Label
            {
                Text = "📅",
                FontSize = 24,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };

            var datePickerRow = new HorizontalStackLayout
            {
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { calendarIcon, dueDatePicker }
            };

            var datePickerBorder = new Border
            {
                Stroke = CurrentTheme.Instance.Theme.Borders,
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(12, 4),
                Margin = new Thickness(24, 4, 24, 12),
                HorizontalOptions = LayoutOptions.Center,
                BackgroundColor = CurrentTheme.Instance.Theme.Background,
                MinimumWidthRequest = 250,
                Content = datePickerRow
            };

            themeChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    dueDatePicker.TextColor = CurrentTheme.Instance.Theme.Text;
                    datePickerBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                    datePickerBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                }
            };

            CurrentTheme.Instance.PropertyChanged += themeChangedHandler;
            themeChangeHandlers.Add(themeChangedHandler);

            // Signature pad for borrower handwritten signature
            var signatureLabel = new Label
            {
                Text = "Borrower Signature (sign below):",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(24, 20, 24, 4),
                TextColor = CurrentTheme.Instance.Theme.Text
            };

            themeChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    signatureLabel.TextColor = CurrentTheme.Instance.Theme.Text;
            };

            CurrentTheme.Instance.PropertyChanged += themeChangedHandler;
            themeChangeHandlers.Add(themeChangedHandler);

            signaturePad = new SignaturePadView
            {
                HeightRequest = 200,
                MinimumHeightRequest = 200,
                StrokeColor = Colors.Black,
                StrokeWidth = 3,
                BackgroundColor = Colors.White,
                HorizontalOptions = LayoutOptions.Fill,
            };

            var signatureBorder = new Border
            {
                Stroke = Colors.DarkGray,
                StrokeThickness = 2,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                Padding = 4,
                Margin = new Thickness(24, 4, 24, 4),
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.White,
                Content = signaturePad
            };

            var clearSignatureButton = new Button
            {
                Text = "Clear Signature",
                BackgroundColor = Colors.Crimson,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 4, 0, 12),
                CornerRadius = 8
            };
            clearSignatureButton.Clicked += (s, e) => signaturePad.Clear();


            var confirmButton = new TagAndTrackButton("Confirm Loan", new Command(async () => await ConfirmLoan()), "check.png");

            DataTable<SpecimenItem>? dt = null;

            dt = new DataTable<SpecimenItem>(LoanCreator.LoanItems, columns =>
            {
                columns.Add("ID", s => s.ID, 60);
                columns.Add("Arctos ID", s => s.ArctosID, 100);
                columns.Add("Name", s => s.Name);
                columns.Add("Description", s => s.Description);

                columns.AddButton("Remove from Loan",
                s =>
                {
                    LoanCreator.RemoveItem(s);
                    dt!.RemoveItem(s);
                },
                "trash.png", 80);

            }, showSearchBar: false);

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        header,
                        loanNameEntry,
                        loanDescriptionEntry,
                        clientNameEntry,
                        clientEmailEntry,
                        dueDateLabel,
                        datePickerBorder,
                        signatureLabel,
                        signatureBorder,
                        clearSignatureButton,
                        confirmButton,
                        dt
                    }
                }
            };
        }

        private DataTableTemplate CreateDT()
        {
            string dtHeader = "ID,Arctos ID,Name,Description,Remove item";

            return new DataTableTemplate(dtHeader, CreateDTCSV());
        }

        private string CreateDTCSV()
        {
            var sb = new StringBuilder(); // entries
            foreach (var entry in LoanCreator.LoanItems)
            {
                sb.Append(ItemToCSVEntry(entry));
            }
            return sb.ToString();
        }

        private static string ItemToCSVEntry(Item item)
        {
            var sb = new StringBuilder();
            sb.Append(item.ID).Append(',')
              .Append(item.ArctosID).Append(',')
              .Append(item.Name).Append(',')
              .Append(item.Description).Append(',')
              .Append(item.Status).AppendLine();
            return sb.ToString();
        }

        private static string CsvToHtmlTable(string csv)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body><table border='1' style='border-collapse: collapse;'>");

            sb.Append("<tr>")
                .Append("<th>ID</th>")
                .Append("<th>ArctosID</th>")
                .Append("<th>Name</th>")
                .Append("<th>Description</th>")
                .Append("</tr>");

            // Split rows by line breaks
            var rows = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var row in rows)
            {
                sb.Append("<tr>");
                var columns = row.Split(',');
                for (int i = 0; i < columns.Length - 1; i++)
                {
                    sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(columns[i])).Append("</td>");
                }
                sb.Append("</tr>");
            }

            sb.Append("</table></body></html>");
            return sb.ToString();
        }

        private async Task ConfirmLoan()
        {
            if (loanNameEntry.textbox.Text == "")
            {
                await Shell.Current.DisplayAlertAsync("Error", "Loan name must be entered.", "OK");
                return;
            }
            if (clientNameEntry.textbox.Text == "")
            {
                await Shell.Current.DisplayAlertAsync("Error", "Client name must be entered.", "OK");
                return;
            }
            if (clientEmailEntry.textbox.Text == "")
            {
                await Shell.Current.DisplayAlertAsync("Error", "Client email must be entered.", "OK");
                return;
            }

            // Capture signature as JSON stroke bytes (optional)
            byte[]? signatureBytes = null;
            try
            {
                if (!signaturePad.IsBlank)
                    signatureBytes = signaturePad.GetSignatureBytes();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("DEBUG: Signature Capture Error", ex.ToString(), "OK");
            }

            // Build the email body BEFORE persisting to DB
            var dueDate = dueDatePicker.Date ?? DateTime.Today.AddDays(30);

            var sb = new StringBuilder();
            sb.Append($"Loan Name: {loanNameEntry.textbox.Text}").AppendLine()
                .Append($"Loan Description: {loanDescriptionEntry.textbox.Text}").AppendLine()
                .Append($"Client Name: {clientNameEntry.textbox.Text}").AppendLine()
                .Append($"Date Checked Out: {DateTime.Now}").AppendLine()
                .Append($"Date Due: {dueDate}").AppendLine()
                .Append($"Items included in loan:").AppendLine();

            sb = sb.Replace("\n", "<br>");

            var htmlTable = CsvToHtmlTable(CreateDTCSV());
            var body = sb.ToString() + "<br>" + htmlTable;

            // Send email FIRST — only finalize to DB on success
            var (emailSuccess, emailError) = Emailer.Email(
                clientEmailEntry.textbox.Text,
                $"Tag and Track Loan Confirmed",
                body,
                signatureBytes);

            if (!emailSuccess)
            {
                await Shell.Current.DisplayAlertAsync("Email Error", $"Email failed to send. Loan was NOT created.\n\nDetails: {emailError}", "OK");
                return;
            }

            // Email succeeded — now persist to DB
            var loan = await LoanCreator.FinalizeLoanAsync(
                loanNameEntry.textbox.Text,
                loanDescriptionEntry.textbox.Text,
                clientNameEntry.textbox.Text,
                clientEmailEntry.textbox.Text,
                dueDate,
                signatureBytes);

            await Shell.Current.DisplayAlertAsync("Success!", "Loan registered and email sent!", "OK");
            await Shell.Current.GoToAsync("//MainPage");
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
            foreach(var handler in themeChangeHandlers)
            {
                CurrentTheme.Instance.PropertyChanged -= handler;
            }
        }
    }
}
