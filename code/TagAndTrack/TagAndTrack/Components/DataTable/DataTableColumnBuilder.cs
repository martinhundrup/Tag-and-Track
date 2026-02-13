using System;
using System.Collections.Generic;
using System.Text;

namespace TagAndTrack.Components
{
    public class DataTableColumnBuilder<T>
    {
        public List<DataTableColumn<T>> Columns { get; } = new();

        public void Add(string header, Func<T, object?> selector, double width = -1, bool filterable = true)
        {
            Columns.Add(new DataTableColumn<T>
            {
                Header = header,
                ValueSelector = selector,
                Width = width,
                IsFilterable = filterable
            });
        }

        public void AddButton(string header, Action<T> action, string icon, double width = 60)
        {
            Columns.Add(new DataTableColumn<T>
            {
                Header = header,
                IsButton = true,
                ButtonAction = action,
                ButtonIcon = icon,
                Width = width,
                IsFilterable = false
            });
        }

        public void AddIcon(string header, Func<T, Icon> selector, double width = 40)
        {
            Columns.Add(new DataTableColumn<T>
            {
                Header = header,
                IsIcon = true,
                IconWithTintSelector = selector,
                Width = width,
                IsFilterable = false
            });
        }
    }
}
