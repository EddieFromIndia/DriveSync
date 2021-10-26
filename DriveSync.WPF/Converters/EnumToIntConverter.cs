using DriveSync.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveSync
{
    [ValueConversion(typeof(ResolveMethods), typeof(int))]
    public class EnumToIntConverter : IValueConverter
    {
        public static EnumToIntConverter Instance = new EnumToIntConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(ResolveMethods)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
