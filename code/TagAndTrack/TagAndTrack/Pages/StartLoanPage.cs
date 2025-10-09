namespace TagAndTrack
{
    public class StartLoanPage : TagAndTrackPage
    {
        new protected const string titleText = "Start Loan";
        public StartLoanPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;
        }
    }    
}