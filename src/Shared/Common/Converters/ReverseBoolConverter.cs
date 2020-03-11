using System;
using System.Globalization;

namespace LiveScore.Common.Converters
{
    public class ReverseBoolConverter : ValueConverter<bool, bool>
    {
        public override bool Convert(bool source, Type targetType, object parameter, CultureInfo culture)
            => !source;
    }
}