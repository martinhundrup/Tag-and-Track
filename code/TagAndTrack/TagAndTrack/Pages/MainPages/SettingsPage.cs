using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class SettingsPage : TagAndTrackPage
    {
        protected new const string titleText = "Settings";
        public SettingsPage() { Initialize(); }

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