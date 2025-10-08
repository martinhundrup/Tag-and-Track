namespace TagAndTrack
{
    public class TestPage : ContentPage
    {
        int count = 0;

        public TestPage()
        {
            Title = "Test Page";

            var label = new Label
            {
                Text = "Specimen Information",
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center
            };

            var button = new Button
            {
                Text = "Scan Specimen"
            };
            button.Clicked += async (s, e) =>
            {
                await DisplayAlert("Scan", "Launching camera for scanning...", "OK");
            };

            Content = new VerticalStackLayout
            {
                Padding = 20,
                Children = { label, button }
            };
        }
    }

}