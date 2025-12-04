using System.Text;
using TagAndTrack.Backend.Items;
using TagAndTrack.Backend.Utils;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class ViewEditLoanItemsPage : TagAndTrackPage
    {
        protected const string titleText = "View/Edit Loan Items";
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

           /* string dtHeader = "ID,Arctos ID,Name,Description,Remove item";

            var sb = new StringBuilder(); // entries
            foreach (var entry in LoanCreator.LoanItems)
            {
                sb.Append(ItemToCSVEntry(entry));
            }

            var dt = new DataTableTemplate(dtHeader, sb.ToString());*/

            var dt = new DataTableTemplate(LoanCreator.LoanItems, true, false);

            var header = new HeaderTemplate(titleText);
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        header,
                        dt
                    }
                }
            };
        }

        private static string ItemToCSVEntry(Item item)
        {
            var sb = new StringBuilder();
            sb.Append(item.Name).Append(",")
                .Append(item.ArctosID).Append(",")
                .Append(item.Name).Append(",")
                .Append(item.Description).Append(",")
                .Append("TODO").AppendLine();
            return sb.ToString();
        }
    }
}