using System;
using System.Globalization;
using System.Windows.Data;

namespace Gosu.MsTestRunner.UI.Converters
{
    public class ReducedNumericValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleParameter = System.Convert.ToDouble(parameter);
            return System.Convert.ToDouble(value) - doubleParameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleParameter = System.Convert.ToDouble(parameter);
            return System.Convert.ToDouble(value) + doubleParameter;
        }
    }
}