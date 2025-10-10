using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Creates a template for the text we intend to use.
    /// </summary>
    public class TextTemplate : Label
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TextTemplate"/> class.
        /// </summary>
        public TextTemplate() 
        {
            Text = "Test";
            TextColor = Colors.AntiqueWhite;
            HorizontalTextAlignment = TextAlignment.Center;
        }
    }
}
