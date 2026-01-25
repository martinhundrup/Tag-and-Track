using TagAndTrack.Pages;

namespace TagAndTrack
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("AllContainersPage", typeof(AllContainersPage));
            Routing.RegisterRoute("ViewContainerPage", typeof(ViewContainerPage));
        }
    }
}
