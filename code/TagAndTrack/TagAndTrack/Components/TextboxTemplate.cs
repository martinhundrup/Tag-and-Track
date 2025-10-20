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
        private CurrentTheme theme = new CurrentTheme();

        /// <summary>
        /// Creates a new instance of the <see cref="ButtonTemplate"/> class.
        /// </summary>
        public TextboxTemplate(double width)
        {
            WidthRequest = width;
            HeightRequest = 40;
            BackgroundColor = CurrentTheme.Instance.Theme.Background;
            TextColor = CurrentTheme.Instance.Theme.Text;
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing;

            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    TextColor = CurrentTheme.Instance.Theme.Text;
                }
            };
        }
    }
}
