using System.ComponentModel;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Label template for better table styling.
    /// </summary>
    internal class LabelTemplate : Label
    {
        private PropertyChangedEventHandler handler;
        /// <summary>
        /// Creates a new instance of the <see cref="LabelTemplate"/> class.
        /// </summary>
        /// <param name="padding">The intended padding of the label.</param>
        /// <param name="text">The text that will be assigned.</param>
        public LabelTemplate(int padding, string text)
        {
            Text = text;
            TextColor = CurrentTheme.Instance.Theme.Text;
            HorizontalOptions = LayoutOptions.Start;
            VerticalOptions = LayoutOptions.Center;
            Padding = padding;

            handler = (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    TextColor = CurrentTheme.Instance.Theme.Text;
                }
            };

            CurrentTheme.Instance.PropertyChanged += handler;
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
            CurrentTheme.Instance.PropertyChanged -= handler;
        }
    }
}
