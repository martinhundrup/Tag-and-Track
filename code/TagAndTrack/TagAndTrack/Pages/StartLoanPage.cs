using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class StartLoanPage : TagAndTrackPage
    {
        new protected const string titleText = "Start Loan";
        public StartLoanPage() { Initialize(); }

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