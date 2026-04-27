using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace BRCDAQdemo.Avalonia.Converters;

public sealed class SampleRateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is double rate
            ? (Math.Truncate(rate * 1000.0) / 1000.0 / 1000.0).ToString("#0.000K", culture)
            : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
