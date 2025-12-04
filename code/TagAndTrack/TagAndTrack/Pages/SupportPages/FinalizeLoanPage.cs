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

        private EntryTemplate loanNameEntry;
        private EntryTemplate loanDescriptionEntry;
        private EntryTemplate clientNameEntry;
        private EntryTemplate clientEmailEntry;

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

            loanNameEntry = new EntryTemplate(300, "Loan Name");
            loanDescriptionEntry = new EntryTemplate(300, "Loan Description");
            clientNameEntry = new EntryTemplate(300, "Client Name");
            clientEmailEntry = new EntryTemplate(300, "Client Email");
            //var dueDate = new EntryTemplate(300, "Client Email"); // TODO: date entry that complies with a DateTime


            var confirmButton = new TagAndTrackButton("Confirm Loan", new Command(async () => await ConfirmLoan()));


            var dt = CreateDT();

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

        private async Task ConfirmLoan()
        {
            if (loanNameEntry.Text == "")
            {
                await Shell.Current.DisplayAlert("Error", "Loan name must be entered.", "OK");
                return;
            }
            if (clientNameEntry.Text == "")
            {
                await Shell.Current.DisplayAlert("Error", "Client name must be entered.", "OK");
                return;
            }
            if (clientEmailEntry.Text == "")
            {
                await Shell.Current.DisplayAlert("Error", "Client email must be entered.", "OK");
                return;
            }

            var loan = LoanCreator.FinalizeLoan(loanNameEntry.Text, 
                loanDescriptionEntry.Text,
                clientNameEntry.Text,
                clientEmailEntry.Text,
                DateTime.MaxValue);

            var sb = new StringBuilder();
            sb.Append($"Loan Name: {loanNameEntry.Text}").AppendLine()
                .Append($"Loan ID: {loan.ID}").AppendLine()
                .Append($"Loan Description: {loanDescriptionEntry.Text}").AppendLine()
                .Append($"Client Name: {clientNameEntry.Text}").AppendLine()
                .Append($"Date Checked Out: {loan.DateCheckedOut.ToString()}").AppendLine()
                .Append($"Date Due: {loan.DateDue}").AppendLine()
                .Append($"Items included in loan:").AppendLine()
                .Append(CreateDTCSV()).AppendLine();

            var body = sb.ToString();

            var emailResult = Emailer.Email(clientEmailEntry.Text, $"Tag and Track Loan {loan.ID} Confirmed", body);

            if (!emailResult)
            {
                await Shell.Current.DisplayAlert("Error", "Email failed to send. Please double check email address.", "OK");
                // TODO: undo loan checkin
                return;
            }
            else
            {
                await Shell.Current.DisplayAlert("Success!", "Loan registered and email sent!", "OK");
                await Shell.Current.Navigation.PopToRootAsync();

            }
        }
    }
}
