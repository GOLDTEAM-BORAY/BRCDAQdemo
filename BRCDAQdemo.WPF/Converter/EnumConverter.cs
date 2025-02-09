using System;
using System.Globalization;
using System.Windows.Data;

namespace BRCDAQdemo.WPF.Converter
{
    [ValueConversion(typeof(object), typeof(object))]
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(parameter) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
