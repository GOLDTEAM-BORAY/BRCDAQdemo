using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace BRCDAQdemo.Avalonia.Converters;

public sealed class EnumConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.Equals(parameter) ?? false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.Equals(true) == true ? parameter! : BindingOperations.DoNothing;
    }
}
