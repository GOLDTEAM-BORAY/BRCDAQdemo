using System;
using System.Globalization;
using System.Windows.Data;

namespace BRCDAQdemo.WPF.Converter
{
    [ValueConversion(typeof(double), typeof(string))]
    public class CurrentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double))
                throw new InvalidOperationException("The target must be a double");
            return ((double)value).ToString("#0mA");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
