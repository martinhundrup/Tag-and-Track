using System.Text;
using TagAndTrack.Backend;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class AllSpecimensPage : TagAndTrackPage
    {
        protected const string titleText = "View All Specimens";
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
            var items = ItemManager.GetItemsOfType(Item.ItemType.Specimen);
            var sb = new StringBuilder();
            List<SpecimenItem> list = new List<SpecimenItem>();
            foreach (var item in items)
            {
                list.Add((SpecimenItem)item);
            }
            var dt = new DataTableTemplate(dtHeader, sb.ToString());
            dt = new DataTableTemplate(list, false);

            var header = new HeaderTemplate(titleText);
            //var searchbar = new EntryTemplate(300, "Search");
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        header,
                        //searchbar,
                        dt
                    }
                }
            };
        }

        private static string ItemToCSVEntry(Item item)
        {
            var sb = new StringBuilder();
                sb.Append(item.ID).Append(',')
                  .Append(item.ArctosID).Append(',')
                  .Append(item.Name).Append(',')
                  .Append(item.Description).Append(',')
                  .Append(item.Status).AppendLine();
            return sb.ToString();
        }
    }
}