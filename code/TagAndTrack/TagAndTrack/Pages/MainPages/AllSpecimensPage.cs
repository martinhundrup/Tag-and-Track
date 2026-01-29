using System.Text;
using TagAndTrack.Backend;
using TagAndTrack.Backend.Data;
using TagAndTrack.Backend.Items;
using TagAndTrack.Components;

namespace TagAndTrack.Pages
{
    public class AllSpecimensPage : TagAndTrackPage
    {
        protected const string titleText = "View All Specimens";
        private VerticalStackLayout? contentLayout;

        public AllSpecimensPage()
        {
            DebugLogger.Log("AllSpecimensPage constructor called");
            Initialize();
        }

        protected override void Initialize()
        {
            DebugLogger.Log("AllSpecimensPage.Initialize() starting");
            Background = CurrentTheme.Instance.Theme.Background;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    Background = CurrentTheme.Instance.Theme.Background;
                }
            };

            var header = new HeaderTemplate(titleText);

            contentLayout = new VerticalStackLayout
            {
                Spacing = 5,
                Padding = new Thickness(10)
            };

            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        header,
                        contentLayout
                    }
                }
            };

            _ = LoadSpecimensAsync();
            DebugLogger.Log("AllSpecimensPage.Initialize() complete");
        }

        protected override void OnAppearing()
        {
            DebugLogger.Log("AllSpecimensPage.OnAppearing() called");
            base.OnAppearing();
            // Don't reload here - Initialize already loaded
            // _ = LoadSpecimensAsync();
        }

        private async Task LoadSpecimensAsync()
        {
            DebugLogger.Log("AllSpecimensPage.LoadSpecimensAsync() starting");
            if (contentLayout == null) return;

            contentLayout.Children.Clear();
            DebugLogger.Log("AllSpecimensPage: contentLayout cleared");

            var specimens = await DbService.GetAllSpecimensAsync();

            if (specimens.Count == 0)
            {
                contentLayout.Children.Add(new Label
                {
                    Text = "No specimens found",
                    FontSize = 16,
                    TextColor = CurrentTheme.Instance.Theme.Text,
                    HorizontalOptions = LayoutOptions.Center
                });
                return;
            }

            // Create a simple data table
            DebugLogger.Log($"AllSpecimensPage: Creating DataTableTemplate with {specimens.Count} specimens");
            var dt = new DataTableTemplate(specimens, false);
            contentLayout.Children.Add(dt);
            DebugLogger.Log("AllSpecimensPage.LoadSpecimensAsync() complete");
        }
    }
}