namespace TagAndTrack.Pages
{
    public abstract class TagAndTrackPage : ContentPage
    {
        // Doth this page bear the customary navigation back button?
        public bool HasBackButton
        {
            get;
            private set;
        }

        // Initialize doth arrange all content upon the page and ought be summoned within the constructor.
        protected abstract void Initialize();
    }
}