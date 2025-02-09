using System;
using System.Globalization;
using System.Windows.Data;

namespace BRCDAQdemo.WPF.Converter
{
    [ValueConversion(typeof(double), typeof(string))]
    public class SampleRateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double))
                throw new InvalidOperationException("The target must be a double");
            return (Math.Truncate((double)value * 1000.0) / 1000.0 / 1000.0).ToString("#0.000K");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
