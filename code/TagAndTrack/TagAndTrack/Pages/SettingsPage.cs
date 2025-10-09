namespace TagAndTrack
{
    public class SettingsPage : TagAndTrackPage
    {
        protected new const string titleText = "Settings";
        public SettingsPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;
        }
    }
}