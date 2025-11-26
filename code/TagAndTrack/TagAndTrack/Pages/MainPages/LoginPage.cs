using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class LoginPage : TagAndTrackPage
    {
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

            var header = new HeaderTemplate("Login");
            Content = new StackLayout
            {
                Children =
                {
                    header
                }
            };
        }
    }
}