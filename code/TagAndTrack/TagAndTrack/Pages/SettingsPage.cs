namespace TagAndTrack
{
    public class SettingsPage : TagAndTrackPage
    {
        protected new const string titleText = "View Item";
        public SettingsPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;
        }
    }
}