namespace TagAndTrack.Pages
{
    public abstract class TagAndTrackPage : ContentPage
    {
        // Should this page have the default navigation back button?
        public bool HasBackButton
        {
            get;
            private set;
        }

        // Initialize sets up all content in the page and should be called in the page constructor.
        protected abstract void Initialize();
    }
}