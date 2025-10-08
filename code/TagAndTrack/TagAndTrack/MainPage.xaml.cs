namespace TagAndTrack
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            Thing();
        }
        
        private async void Thing()
        {
            await Navigation.PushAsync(new TestPage());
        }
    }

}
