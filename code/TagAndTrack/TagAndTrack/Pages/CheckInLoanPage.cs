namespace TagAndTrack
{
    public class CheckInLoanPage : TagAndTrackPage
    {
        protected new const string titleText = "Check in a Loan";
        public CheckInLoanPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;
        }
    }
}