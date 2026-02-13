using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class LoanHistoryPage : TagAndTrackPage
    {
        protected const string titleText = "View Loan History";
        public LoanHistoryPage() { Initialize(); }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };

            var allLoans = ItemManager.GetItemsOfType(Item.ItemType.Loan);
            List<LoanItem> loans = new List<LoanItem>();
            foreach( var item in allLoans )
            {
                loans.Add((LoanItem)item);
            }

            var dt = new DataTable<LoanItem>(loans, columns =>
            {
                columns.Add("ID", s => s.ID, 60);
                columns.Add("Name", s => s.Name);
                columns.Add("Borrower", s => s.Borrower);
                columns.Add("Checked Out", s => s.DateCheckedOut.ToShortDateString(), width: 100, filterable: false);
                columns.Add("Due Date", s => s.DateDue.ToShortDateString(), width: 100, filterable: false);
                columns.Add("Status", s => DateTime.Now > s.DateDue ? "Overdue" : "On Loan", width: 100, filterable: false);


                columns.AddButton("View Loan",
                s =>
                {
                    ScannedQRItem.lastScannedItem = s.QRID;
                    Navigation.PushAsync(new ViewItemPage());
                },
                "info_circle.svg", 80);

            });

            var header = new HeaderTemplate(titleText);

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

            pageLayout.Children.Add(dt);
            Grid.SetRow(dt, 1);

            Content = pageLayout;
        }
    }
}