using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class CheckInLoanPage : TagAndTrackPage
    {
        protected new const string titleText = "Check in a Loan";
        public CheckInLoanPage() { Initialize(); }

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