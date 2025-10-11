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

            var scanView = new ScanView();
            scanView.ScanCaptured += ScanCaptured;
            Content = new ScanView();
        }

        private void ScanCaptured(object? sender, ScanCapturedEventArgs args)
        {
            Console.WriteLine($"scan captured: {args.Text}");
        }
    }
}