using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DriveSync
{
    [ValueConversion(typeof(string), typeof(SolidColorBrush))]
    public class StringToColorBrushConverter : IValueConverter
    {
        public static StringToColorBrushConverter Instance = new StringToColorBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value.ToString()) ?
                new SolidColorBrush() { Color = Color.FromArgb(0, 251, 247, 244) } :
                new SolidColorBrush() { Color = Color.FromArgb(255, 251, 247, 244) };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
