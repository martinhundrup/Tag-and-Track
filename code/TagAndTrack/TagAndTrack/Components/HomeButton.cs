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
    internal class HomeButton : Button
    {
        /// <summary>
        /// Creates a new instance of the <see cref="HomeButton"/> class.
        /// </summary>
        public HomeButton()
        {
            WidthRequest = 70;
            HeightRequest = 70;
            // Set the command to navigate to MainPage when clicked.
            Command = new Command(async() => await Navigation.PushAsync(new MainPage()));

            BackgroundColor = Colors.Gray;
        }
    }
}
