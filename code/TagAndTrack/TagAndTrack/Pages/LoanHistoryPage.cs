namespace TagAndTrack
{
    public class LoanHistoryPage : TagAndTrackPage
    {
        protected new const string titleText = "View Loan History";
        public LoanHistoryPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;
        }
    }
}