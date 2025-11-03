using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class AllSpecimensPage : TagAndTrackPage
    {
        protected new const string titleText = "View All Specimens";
        public AllSpecimensPage() { Initialize(); }

        private const string csvContent = @"0,ARC-000000,Salmon Specimen,Cleaned bone specimen,true
1,ARC-000001,Lion Claw,Preserved specimen stored in fluid jar,true
2,ARC-000002,Bear Femur,Egg sample in container,false
3,ARC-000003,Heron Egg,Fluid jar sample with tag,true
4,ARC-000004,Toad Sample,Full skeleton display,false
5,ARC-000005,Owl Wing,Tissue sample for genetics,true
6,ARC-000006,Beaver Skull,Cleaned bone specimen,true
7,ARC-000007,Lion Claw,Cleaned bone specimen,true
8,ARC-000008,Toad Sample,Loose bone fragment,false";

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

            List<string> dtHeader = new List<string>();
            dtHeader.Add("ID");
            dtHeader.Add("Arctos ID");
            dtHeader.Add("Name");
            dtHeader.Add("Description");
            dtHeader.Add("Status");

            var dt = new DataTableTemplate(5, 5, dtHeader, csvContent);

            // Wrap the data table in a ScrollView so content is vertically scrollable
            Content = dt;
            // {
            //     Orientation = ScrollOrientation.Vertical,
            //     Content = dt
            // };
        }
    }
}