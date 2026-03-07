using TagAndTrack.Pages;

namespace TagAndTrack
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("AllContainersPage", typeof(AllContainersPage));
            Routing.RegisterRoute("ViewContainerPage", typeof(ViewContainerPage));

            // Add main page to shell structure so that it can be navigated to with "//MainPage" route.
            // This allows the navigation stack to be cleared if navigating to MainPage.
            Items.Add(new ShellContent
            {
                Route = "MainPage",
                ContentTemplate = new DataTemplate(typeof(MainPage))
            });

        }
    }
}
