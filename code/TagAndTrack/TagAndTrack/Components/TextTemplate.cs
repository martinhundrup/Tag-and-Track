using System.ComponentModel;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Creates a template for the text we intend to use.
    /// </summary>
    public class TextTemplate : Label, IDisposable
    {
        PropertyChangedEventHandler handler;

        /// <summary>
        /// Creates a new instance of the <see cref="TextTemplate"/> class.
        /// </summary>
        public TextTemplate() 
        {
            Text = "";
            TextColor = CurrentTheme.Instance.Theme.Text;
            HorizontalTextAlignment = TextAlignment.Center;
           
            handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    TextColor = CurrentTheme.Instance.Theme.Text;
                }
            };
            CurrentTheme.Instance.PropertyChanged += handler;
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

            handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    TextColor = CurrentTheme.Instance.Theme.Text;
                }
            };
            CurrentTheme.Instance.PropertyChanged += handler;
        }

        public TextTemplate(string text, TextAlignment alignment, double textSize)
        {
            Text = text;
            TextColor = CurrentTheme.Instance.Theme.Text;
            HorizontalTextAlignment = alignment;
            FontSize = textSize;
            handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    TextColor = CurrentTheme.Instance.Theme.Text;
                }
            };
            CurrentTheme.Instance.PropertyChanged += handler;
        }

        /// <summary>
        /// Disposable method to clean up event subscriptions.
        /// </summary>
        public void Dispose()
        {
            CurrentTheme.Instance.PropertyChanged -= handler;
        }
    }
}
