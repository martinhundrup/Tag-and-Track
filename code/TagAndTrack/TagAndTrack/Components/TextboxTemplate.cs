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
    internal class TextboxTemplate : Border
    {
        /// <summary>
        /// The textbox used in the template.
        /// </summary>
        public Entry textbox = new Entry();

        /// <summary>
        /// Creates a new instance of the <see cref="ButtonTemplate"/> class.
        /// </summary>
        public TextboxTemplate(double width)
        {
            textbox.WidthRequest = width;
            this.WidthRequest = width + 5;
            textbox.HeightRequest = 40;
            this.HeightRequest = 45;
            textbox.BackgroundColor = CurrentTheme.Instance.Theme.Background;
            textbox.TextColor = CurrentTheme.Instance.Theme.Text;
            textbox.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;

            this.Stroke = CurrentTheme.Instance.Theme.Borders;
            this.StrokeThickness = 1;
            this.BackgroundColor = Colors.Transparent;


            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    textbox.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    textbox.TextColor = CurrentTheme.Instance.Theme.Text;
                    this.Stroke = CurrentTheme.Instance.Theme.Borders;
                }
            };

            this.Content = textbox;
        }
    }
}
