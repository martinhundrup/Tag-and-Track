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

        public void AddIcon(string header, Func<T, string> selector, double width = 40)
        {
            Columns.Add(new DataTableColumn<T>
            {
                Header = header,
                IsIcon = true,
                IconSelector = selector,
                Width = width,
                IsFilterable = false
            });
        }

        public void AddCheckbox(string header, Action<T, bool> onToggled, double width = 50, Func<T, bool>? initialValue = null)
        {
            Columns.Add(new DataTableColumn<T>
            {
                Header = header,
                IsCheckbox = true,
                CheckboxAction = onToggled,
                CheckboxInitialValue = initialValue,
                Width = width,
                IsFilterable = false
            });
        }
    }
}
