namespace TagAndTrack
{
    
    public class AddItemPage : TagAndTrackPage
    {
        protected new const string titleText = "Add Item";
        public AddItemPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;
        }
    }
}