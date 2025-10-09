namespace TagAndTrack
{
    public class LoginPage : TagAndTrackPage
    {
        protected new const string titleText = "Login";
        public LoginPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;
        }
    }
}