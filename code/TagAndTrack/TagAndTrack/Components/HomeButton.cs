using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Home button component.
    /// TODO: Change Button to ImageButton once we have an appropriate (open source) home icon.
    /// </summary>
    internal class HomeButton : ImageButton
    {
        /// <summary>
        /// Creates a new instance of the <see cref="HomeButton"/> class.
        /// </summary>
        public HomeButton()
        {
            WidthRequest = 70;
            HeightRequest = 70;
            // Decree the command: upon being pressed, journey unto MainPage.
            Command = new Command(async() => await Shell.Current.GoToAsync("//MainPage"));

            Source = "building.png";
        }
    }
}
