using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Contains a template for the button and the text that will be underneath.
    /// </summary>
    public class ButtonTemplate : Button
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ButtonTemplate"/> class.
        /// </summary>
        public ButtonTemplate()
        {
            BackgroundColor = Colors.Crimson;
            WidthRequest = 80;
            HeightRequest = 80;
            CornerRadius = 40;
        }
    }
}
