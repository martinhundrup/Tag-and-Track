namespace TagAndTrack
{
    public abstract class TagAndTrackPage : ContentPage
    {
        protected const string titleText = "Tag and Track";

        // Should this page have the default navigation back button?
        public bool HasBackButton
        {
            get;
            private set;
        }

        public string TitleText => titleText;


        // Initialize sets up all content in the page and should be called in the page constructor.
        protected abstract void Initialize();
    }
}