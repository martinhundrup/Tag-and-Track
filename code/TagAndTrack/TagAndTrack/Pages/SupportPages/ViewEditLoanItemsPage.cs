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


            DataTable<SpecimenItem>? dt = null;

            dt = new DataTable<SpecimenItem>(LoanCreator.LoanItems, columns =>
            {
                columns.Add("ID", s => s.ID, 60);
                columns.Add("Arctos ID", s => s.ArctosID, 100);
                columns.Add("Name", s => s.Name);
                columns.Add("Description", s => s.Description);

                columns.AddButton("Remove from Loan",
                s =>
                {
                    LoanCreator.RemoveItem(s);
                    dt!.RemoveItem(s);
                },
                "trash.svg", 80);

            }, showSearchBar: false);

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