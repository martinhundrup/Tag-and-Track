using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics.Text;

namespace TagAndTrack.Components
{
    /// <summary>
    /// The header template for the pages.
    /// </summary>
    public class HeaderTemplate : ContentView
    {
        /// <summary>
        /// Creates a new instance of the <see cref="HeaderTemplate"/> class.
        /// </summary>
        /// <param name="text">The text for the header.</param>
        /// <param name="isImage">Whether or not we should use a logo or a home button, defaults to false.</param>
        public HeaderTemplate(string text, bool isImage = false)
        {
            // Create the grid
            var grid = new Grid
            {
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
            },
                RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }
            },
                Padding = new Thickness(10, 5)
            };

            // Header text.
            var label = new TextTemplate(text, TextAlignment.Start, 50);
            grid.Add(label, 0, 0);

            // Right button.
            if(isImage)
            {
                var right = new Image()
                {
                    Source = "wsulogo.png",
                    HeightRequest = 70,
                    WidthRequest = 70,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };
                grid.Add(right, 2, 0);
            }
            else
            {
                var right = new HomeButton();
                grid.Add(right, 2, 0);
            }  
          
            // Adjust grid to have 2 columns (text and button)
            grid.ColumnDefinitions.Clear();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Wrap in a border for bottom stroke
            var border = new BoxView
            {
                HeightRequest = 1.5,
                Color = CurrentTheme.Instance.Theme.Text,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.End,
            };

            CurrentTheme.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTheme.Theme))
                {
                    border.Color = CurrentTheme.Instance.Theme.Text;
                }
            };

            var stack = new StackLayout
            {
                Spacing = 0,
                Children = { grid, border }
            };

            Content = stack;
        }
    }
}
