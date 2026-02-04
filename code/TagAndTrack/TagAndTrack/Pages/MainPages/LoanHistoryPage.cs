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

            //string dtHeader = "ID,Name,Description,Borrower,Email,Date Checked Out,Date Due,Specimens,Status";
            //var dt = new DataTableTemplate(dtHeader, ItemManager.GetLoansCSV());

            var allLoans = ItemManager.GetItemsOfType(Item.ItemType.Loan);
            List<LoanItem> loans = new List<LoanItem>();
            foreach( var item in allLoans )
            {
                loans.Add((LoanItem)item);
            }

            var dt = new DataTableTemplate(loans);
            var header = new HeaderTemplate(titleText);

            var searchbar = new EntryTemplate(300, "Search");

            // Wrap the data table in a ScrollView so content is vertically scrollable
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout()
                {
                    Children =
                    {
                        header,
                        searchbar,
                        dt
                    }
                }
            };
        }
    }
}