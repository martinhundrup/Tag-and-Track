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
            TextColor = CurrentTheme.Instance.Theme.Text;
            HorizontalTextAlignment = TextAlignment.Center;

            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    TextColor = CurrentTheme.Instance.Theme.Text;
                }
            };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TextTemplate"/> class and initializes its text.
        /// <param>text is the text to initialize the text to.</param>
        /// </summary>
        public TextTemplate(string text)
        {
            Text = text;
            TextColor = CurrentTheme.Instance.Theme.Text;
            HorizontalTextAlignment = TextAlignment.Center;

            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    TextColor = CurrentTheme.Instance.Theme.Text;
                }
            };
        }

        public TextTemplate(string text, TextAlignment alignment, double textSize)
        {
            Text = text;
            TextColor = CurrentTheme.Instance.Theme.Text;
            HorizontalTextAlignment = alignment;
            FontSize = textSize;
            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    TextColor = CurrentTheme.Instance.Theme.Text;
                }
            };
        }
    }
}
