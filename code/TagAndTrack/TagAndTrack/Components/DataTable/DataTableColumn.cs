using System;
using System.Collections.Generic;
using System.Text;

namespace TagAndTrack.Components
{
    public class DataTableColumn<T>
    {
        public string Header { get; set; }
        public Func<T, object?> ValueSelector { get; set; }
        public double Width { get; set; } = GridLength.Star.Value;
        public bool IsButton { get; set; }
        public Action<T>? ButtonAction { get; set; }
        public string? ButtonIcon { get; set; }
        public bool IsIcon { get; set; }
        public Func<T, string>? IconSelector { get; set; }
        public bool IsFilterable { get; set; } = true;

    }
}
