using TagAndTrack.Backend;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class AllSpecimensPage : TagAndTrackPage
    {
        protected new const string titleText = "View All Specimens";
        public AllSpecimensPage() { Initialize(); }

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

            string dtHeader = "ID,Arctos ID,Name,Description,Status";
            var dt = new DataTableTemplate(dtHeader, ItemManager.GetItemsOfType(Item.ItemType.Specimen));

            var header = new HeaderTemplate(titleText);
            var searchbar = new EntryTemplate(300, "Search");
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        header,
                        searchbar,
                        dt
                    }
                }
            };
        }
    }
}