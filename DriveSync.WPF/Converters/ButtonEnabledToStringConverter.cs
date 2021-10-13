using System;
using System.Globalization;
using System.Windows.Data;

namespace DriveSync
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class ButtonEnabledToStringConverter : IValueConverter
    {
        public static ButtonEnabledToStringConverter Instance = new ButtonEnabledToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return parameter.ToString();
            }
            else
            {
                return "Source or Target does not exist.";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
