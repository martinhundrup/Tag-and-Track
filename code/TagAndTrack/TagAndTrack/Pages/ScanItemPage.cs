using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class ScanItemPage : TagAndTrackPage
    {
        protected new const string titleText = "Scan Item";
        public ScanItemPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;

            Content = new ScanView();
        }
    }
}