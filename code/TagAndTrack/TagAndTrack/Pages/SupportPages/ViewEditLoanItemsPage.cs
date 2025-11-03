using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class ViewEditLoanItemsPage : TagAndTrackPage
    {
        protected new const string titleText = "Add Item";
        public ViewEditLoanItemsPage() { Initialize(); }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };
            Title = titleText;
        }
    }
}