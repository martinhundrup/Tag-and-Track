using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    
    public class AddItemPage : TagAndTrackPage
    {
        protected new const string titleText = "Add Item";
        public AddItemPage() { Initialize(); }

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

            var header = new HeaderTemplate(titleText);
            Content = new StackLayout
            {
                Children =
                {
                    header
                }
            };
        }
    }
}