using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class LoginPage : TagAndTrackPage
    {
        protected new const string titleText = "Login";
        public LoginPage() { Initialize(); }

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