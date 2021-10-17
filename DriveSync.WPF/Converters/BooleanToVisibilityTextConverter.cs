using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveSync
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToVisibilityTextConverter : IValueConverter
    {
        public static BooleanToVisibilityTextConverter Instance = new BooleanToVisibilityTextConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Hide equal files/folders" : "Show equal files/folders";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
