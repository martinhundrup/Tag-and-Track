using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// The template for the textboxes the app will use.
    /// </summary>
    internal class TextboxTemplate : Entry
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ButtonTemplate"/> class.
        /// </summary>
        public TextboxTemplate(double width)
        {
            WidthRequest = width;
            HeightRequest = 40;
            BackgroundColor = Colors.Black ;
            TextColor = Colors.AntiqueWhite;
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing;
        }
    }
}
