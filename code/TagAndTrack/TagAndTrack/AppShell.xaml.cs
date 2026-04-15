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

            // Bestow the main page unto the shell's dominion, that it may be reached by the "//MainPage" route.
            // Thus shall the navigation stack be swept clean when one journeys unto MainPage.
            Items.Add(new ShellContent
            {
                Route = "MainPage",
                ContentTemplate = new DataTemplate(typeof(MainPage))
            });

        }
    }
}
