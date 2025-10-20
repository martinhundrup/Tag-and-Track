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
        }
    }
}