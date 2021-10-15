using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveSync
{
    [ValueConversion(typeof(bool), typeof(BitmapImage))]
    public class BooleanToVisibilityImageConverter : IValueConverter
    {
        public static BooleanToVisibilityImageConverter Instance = new BooleanToVisibilityImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string image = (bool)value ? "visible" : "invisible";
            return new BitmapImage(new Uri($"pack://application:,,,/Assets/{image}.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
