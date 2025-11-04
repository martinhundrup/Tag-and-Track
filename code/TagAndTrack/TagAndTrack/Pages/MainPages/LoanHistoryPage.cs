using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class LoanHistoryPage : TagAndTrackPage
    {
        protected new const string titleText = "View Loan History";
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
            Title = titleText;

            string dtHeader = "ID,Name,Description,Borrower,Email,Date Checked Out,Date Due,Specimens,Status";
            var dt = new DataTableTemplate(dtHeader, ItemManager.GetLoansCSV());


            // Wrap the data table in a ScrollView so content is vertically scrollable
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = dt
            };
        }
    }
}