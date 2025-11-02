using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Label template for better table styling.
    /// </summary>
    internal class LabelTemplate : Label
    {
        /// <summary>
        /// Creates a new instance of the <see cref="LabelTemplate"/> class.
        /// </summary>
        /// <param name="padding">The intended padding of the label.</param>
        /// <param name="text">The text that will be assigned.</param>
        public LabelTemplate(int padding, string text)
        {
            Text = text;
            TextColor = CurrentTheme.Instance.Theme.Text;
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
            Padding = padding;

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
