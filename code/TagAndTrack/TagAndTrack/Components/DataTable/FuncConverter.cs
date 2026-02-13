using System;
using System.Collections.Generic;
using System.Globalization;

namespace TagAndTrack.Components
{
    internal class FuncConverter<TSource, TTarget> : IValueConverter
    {
        private readonly Func<TSource, TTarget> _convert;

        public FuncConverter(Func<TSource, TTarget> convert)
        {
            _convert = convert;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TSource typed)
                return _convert(typed);

            return default(TTarget);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
