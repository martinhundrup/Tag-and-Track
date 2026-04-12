using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// The template for the textboxes the app will use.
    /// </summary>
    internal class TextboxTemplate : Border, IDisposable
    {
        /// <summary>
        /// The textbox used in the template.
        /// </summary>
        public Entry textbox = new Entry();

        private PropertyChangedEventHandler themeChangedHandler;

        /// <summary>
        /// Creates a new instance of the <see cref="ButtonTemplate"/> class.
        /// </summary>
        public TextboxTemplate(double width, string previewText = "")
        {
            textbox.WidthRequest = width;
            this.WidthRequest = width + 5;
            textbox.HeightRequest = 40;
            this.HeightRequest = 45;
            textbox.BackgroundColor = CurrentTheme.Instance.Theme.Background;
            textbox.TextColor = CurrentTheme.Instance.Theme.Text;
            textbox.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;
            textbox.Placeholder = previewText;

            this.Stroke = CurrentTheme.Instance.Theme.Borders;
            this.StrokeThickness = 1;
            this.BackgroundColor = Colors.Transparent;


            themeChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    textbox.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    textbox.TextColor = CurrentTheme.Instance.Theme.Text;
                    this.Stroke = CurrentTheme.Instance.Theme.Borders;
                }
            };
            CurrentTheme.Instance.PropertyChanged += themeChangedHandler;

            this.Content = textbox;
        }

        protected override void OnParentChanged()
        {
            base.OnParentChanged();
            if (Parent == null)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            // Unsubscribe from the event to prevent memory leaks.
            CurrentTheme.Instance.PropertyChanged -= themeChangedHandler;
        }
    }
}
