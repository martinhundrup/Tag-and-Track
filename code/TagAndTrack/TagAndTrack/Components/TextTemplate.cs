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
            Text = "";
            TextColor = Colors.AntiqueWhite;
            HorizontalTextAlignment = TextAlignment.Center;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TextTemplate"/> class and initializes its text.
        /// <param>text is the text to initialize the text to.</param>
        /// </summary>
        public TextTemplate(string text)
        {
            Text = text;
            TextColor = Colors.AntiqueWhite;
            HorizontalTextAlignment = TextAlignment.Center;
        }
    }
}
