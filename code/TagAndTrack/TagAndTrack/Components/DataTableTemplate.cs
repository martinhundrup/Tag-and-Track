using Microsoft.Maui.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagAndTrack.Components
{
    /// <summary>
    /// Template class for data tables.
    /// </summary>
    public class DataTableTemplate : Grid
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DataTableTemplate"/> class.
        /// </summary>
        /// <param name="headers">The list of headers that there will be.</param>
        /// <param name="csvString">A csv string of data values (we can change this as we get more data interfacing).</param>
        /// TODO: Add the headers and refine how it looks cause it'sugly rn.
        public DataTableTemplate(double width, double height, List<string> headers, string csvString)
        {
            RowSpacing = 1;
            ColumnSpacing = 1;
            BackgroundColor = CurrentTheme.Instance.Theme.Background;
            WidthRequest = width;
            HeightRequest = height;

            int columnCount = headers.Count;
            string[] rows = csvString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] data;


            int rowCount = rows.Length; // +1 for header row

            // Define rows and columns.
            for (int i = 0; i < rowCount; i++)
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int j = 0; j < columnCount; j++)
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            for (int j = 0; j < columnCount; j++)
            {
                var headerBorder = new Border
                {
                    Stroke = CurrentTheme.Instance.Theme.Borders,
                    BackgroundColor = CurrentTheme.Instance.Theme.Background,
                    StrokeThickness = 1,
                    Content = new LabelTemplate(10, headers[j]),
                };

                this.Add(headerBorder, j, 0);

                // Subscribe to theme changes.
                CurrentTheme.Instance.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CurrentTheme.Theme))
                    {
                        headerBorder.Stroke = CurrentTheme.Instance.Theme.Borders;
                        headerBorder.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                    }
                };
            }
                

                // Add cells with borders
                for (int i = 0; i < rowCount; i++)
            {
                data = rows[i].Split(new[] { ',' }, StringSplitOptions.None);
                for (int j = 0; j < columnCount; j++)
                {
                    var border = new Border
                    {
                        Stroke = CurrentTheme.Instance.Theme.Borders,
                        BackgroundColor = CurrentTheme.Instance.Theme.Background,
                        StrokeThickness = 1,
                        Content = new LabelTemplate(10, data[j]),
                    };

                    this.Add(border, j, i + 1);

                    // Subscribe to theme changes.
                    CurrentTheme.Instance.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(CurrentTheme.Theme))
                        {
                            border.Stroke = CurrentTheme.Instance.Theme.Borders;
                            border.BackgroundColor = CurrentTheme.Instance.Theme.Background;
                        }
                    };
                }
            }

            /*
             Grid grid = new Grid
{
    RowSpacing = 1,
    ColumnSpacing = 1,
    BackgroundColor = Colors.Black // for visible spacing
};

int rows = data.GetLength(0);
int cols = data.GetLength(1);

// Define rows and columns
for (int i = 0; i < rows; i++)
    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

for (int j = 0; j < cols; j++)
    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

// Add cells with borders
for (int i = 0; i < rows; i++)
{
    for (int j = 0; j < cols; j++)
    {
        var border = new Border
        {
            Stroke = Colors.Gray,
            StrokeThickness = 1,
            Content = new Label
            {
                Text = data[i, j],
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = 10
            }
        };

        grid.Add(border, j, i);
    }
}
             */
        }
    }
}
