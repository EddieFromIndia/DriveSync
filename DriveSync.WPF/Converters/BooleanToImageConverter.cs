using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveSync
{
    [ValueConversion(typeof(bool), typeof(BitmapImage))]
    public class BooleanToImageConverter : IValueConverter
    {
        public static BooleanToImageConverter Instance = new BooleanToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string image = string.Empty;
            switch (parameter.ToString())
            {
                case "link":
                    image = (bool)value ? "link" : "unlink";
                    break;
                case "back":
                    image = (bool)value ? "arrow-up" : "arrow-up-disabled";
                    break;
                case "visibility":
                    image = (bool)value ? "visible" : "invisible";
                    break;
                case "file-folder":
                    image = (bool)value ? "file" : "folder";
                    break;
            }

            return new BitmapImage(new Uri($"pack://application:,,,/Assets/{image}.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
