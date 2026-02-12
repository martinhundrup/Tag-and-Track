using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Creates a entry/search bar component.
    /// </summary>
    internal class EntryTemplate : Grid
    {
        /// <summary>
        /// The textbox used in the template.
        /// </summary>
        public TextboxTemplate textbox;

        /// <summary>
        /// The button used in the template.
        /// </summary>
        public Button button;

        // TODO: implement a command and make sure to take that into the constructor as well.
        /// <summary>
        /// Creates a new instance of the <see cref="EntryTemplate"/> class.
        /// </summary>
        /// <param name="width">The width of the textbox.</param>
        /// <param name="buttonText">The text for the button.</param>
        public EntryTemplate(double width, string buttonText)
        {
            HorizontalOptions = LayoutOptions.Center;
            ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            Margin = new Thickness(10, 5);
            textbox = new TextboxTemplate(width);
            
            button = new Button
            {
                Text = buttonText,
                BackgroundColor = Colors.Gray
                // No search functionality yet.
            };
            this.Add(textbox, 0, 0);
            this.Add(button, 1, 0);
        }

        public string Text
        {
            get => textbox.textbox.Text;
        }
    }
}
