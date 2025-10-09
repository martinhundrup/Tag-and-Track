namespace TagAndTrack
{
    public class AllSpecimensPage : TagAndTrackPage
    {
        protected new const string titleText = "View All Specimens";
        public AllSpecimensPage() { Initialize(); }

        protected override void Initialize()
        {
            Title = titleText;
        }
    }
}