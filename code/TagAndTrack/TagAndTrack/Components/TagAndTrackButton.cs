using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Class for the circular button with text underneath it.
    /// </summary>
    public class TagAndTrackButton : VerticalStackLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagAndTrackButton"/> class with the specified text and command.
        /// </summary>
        /// <remarks>This constructor creates a button with a text label and associates it with the
        /// specified command. The button is centered horizontally and includes spacing between its elements.</remarks>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="command">The command to execute when the button is clicked.</param>
        public TagAndTrackButton(string text, Command command, string image = "")
        {
            Spacing = 5;
            HorizontalOptions = LayoutOptions.Center;
            ButtonTemplate button = new ButtonTemplate()
            {
                Command = command,
                Source = image,
            };
            TextTemplate buttonText = new TextTemplate(text);

            Children.Add(button);
            Children.Add(buttonText);
        }
    }
}
