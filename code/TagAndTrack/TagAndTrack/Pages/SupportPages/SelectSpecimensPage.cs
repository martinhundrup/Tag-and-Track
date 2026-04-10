using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class SelectSpecimensPage : TagAndTrackPage
    {
        private readonly List<SpecimenItem> _available;
        private readonly HashSet<SpecimenItem> _selected = new();
        private readonly TaskCompletionSource<List<SpecimenItem>> _tcs;
        private readonly HashSet<ulong> _preSelectedIds;

        public SelectSpecimensPage(List<SpecimenItem> availableSpecimens, TaskCompletionSource<List<SpecimenItem>> tcs, IEnumerable<ulong>? preSelectedIds = null)
        {
            _available = availableSpecimens;
            _tcs = tcs;
            _preSelectedIds = preSelectedIds != null ? new HashSet<ulong>(preSelectedIds) : new HashSet<ulong>();

            // Seed _selected with pre-checked items
            foreach (var s in _available)
            {
                if (_preSelectedIds.Contains(s.ID))
                    _selected.Add(s);
            }

            Initialize();
        }

        protected override void Initialize()
        {
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                    Background = CurrentTheme.Instance.Theme.Background;
            };

            var header = new HeaderTemplate("Select Specimens");

            var dt = new DataTable<SpecimenItem>(_available, columns =>
            {
                columns.AddCheckbox("Select", (specimen, isChecked) =>
                {
                    if (isChecked)
                        _selected.Add(specimen);
                    else
                        _selected.Remove(specimen);
                }, 50, initialValue: s => _preSelectedIds.Contains(s.ID));

                columns.Add("ID", s => s.ID, 60);
                columns.Add("Arctos ID", s => s.ArctosID, 100);
                columns.Add("Name", s => s.Name);
                columns.Add("Description", s => s.Description);
                columns.AddIcon("Status", s =>
                    s.Status ? "check.png" : "cross.png", width: 80);
            });

            var addButton = new TagAndTrackButton("Add Selected", new Command(async () =>
            {
                _tcs.TrySetResult(_selected.ToList());
                await Navigation.PopAsync();
            }), "check.png");

            var cancelButton = new TagAndTrackButton("Cancel", new Command(async () =>
            {
                _tcs.TrySetResult(new List<SpecimenItem>());
                await Navigation.PopAsync();
            }), "cross.png");

            var buttonBar = new HorizontalStackLayout
            {
                Spacing = 20,
                HorizontalOptions = LayoutOptions.Center,
                Children = { cancelButton, addButton }
            };

            var pageLayout = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                },
                Padding = new Thickness(10)
            };

            pageLayout.Children.Add(header);
            Grid.SetRow(header, 0);

            pageLayout.Children.Add(buttonBar);
            Grid.SetRow(buttonBar, 1);

            pageLayout.Children.Add(dt);
            Grid.SetRow(dt, 2);

            Content = pageLayout;
        }
    }
}
