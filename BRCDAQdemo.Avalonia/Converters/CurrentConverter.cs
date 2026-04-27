using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace BRCDAQdemo.Avalonia.Converters;

public sealed class CurrentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is double current
            ? current.ToString("#0mA", culture)
            : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
