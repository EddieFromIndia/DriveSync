using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveSync;

[ValueConversion(typeof(WindowState), typeof(BitmapImage))]
public class WindowStateToImageConverter : IValueConverter
{
    public static WindowStateToImageConverter Instance = new WindowStateToImageConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string image = (WindowState)value == WindowState.Normal ? "maximize" : "restore";
        return new BitmapImage(new Uri($"pack://application:,,,/Assets/{image}.png"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
