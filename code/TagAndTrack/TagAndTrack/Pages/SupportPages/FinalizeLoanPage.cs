using System;
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
        private SignaturePadView signaturePad;

        public FinalizeLoanPage()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            // Apply theme background and react to theme changes
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };

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
            //var dueDate = new EntryTemplate(300, "Client Email"); // TODO: date entry that complies with a DateTime

            // Signature pad for borrower handwritten signature
            var signatureLabel = new Label
            {
                Text = "Borrower Signature (sign below):",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(24, 20, 24, 4),
                TextColor = CurrentTheme.Instance.Theme.Text
            };
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    signatureLabel.TextColor = CurrentTheme.Instance.Theme.Text;
            };

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
                BackgroundColor = Colors.Gray,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 4, 0, 12)
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
            if (signaturePad.IsBlank)
            {
                await Shell.Current.DisplayAlertAsync("Error", "Borrower signature is required.", "OK");
                return;
            }

            // Capture signature as JSON stroke bytes
            byte[]? signatureBytes = null;
            try
            {
                signatureBytes = signaturePad.GetSignatureBytes();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("DEBUG: Signature Capture Error", ex.ToString(), "OK");
            }

            var loan = await LoanCreator.FinalizeLoanAsync(loanNameEntry.textbox.Text, 
                loanDescriptionEntry.textbox.Text,
                clientNameEntry.textbox.Text,
                clientEmailEntry.textbox.Text,
                DateTime.MaxValue,
                signatureBytes);

            var sb = new StringBuilder();
            sb.Append($"Loan Name: {loanNameEntry.textbox.Text}").AppendLine()
                .Append($"Loan ID: {loan.ID}").AppendLine()
                .Append($"Loan Description: {loanDescriptionEntry.textbox.Text}").AppendLine()
                .Append($"Client Name: {clientNameEntry.textbox.Text}").AppendLine()
                .Append($"Date Checked Out: {loan.DateCheckedOut.ToString()}").AppendLine()
                .Append($"Date Due: {loan.DateDue}").AppendLine()
                .Append($"Items included in loan:").AppendLine();
            //.Append(CreateDTCSV()).AppendLine();

            sb = sb.Replace("\n", "<br>");

            var htmlTable = CsvToHtmlTable(CreateDTCSV());

            var body = sb.ToString() + "<br>" + htmlTable;

            var (emailSuccess, emailError) = Emailer.Email(clientEmailEntry.textbox.Text, $"Tag and Track Loan {loan.ID} Confirmed", body, signatureBytes);

            if (!emailSuccess)
            {
                await Shell.Current.DisplayAlertAsync("Email Error", $"Email failed to send.\n\nDetails: {emailError}", "OK");
                // TODO: undo loan checkin
                return;
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Success!", "Loan registered and email sent!", "OK");
                await Shell.Current.GoToAsync("//MainPage");
            }
        }
    }
}
