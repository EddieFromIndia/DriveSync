using System;
using System.Globalization;
using System.Windows.Data;

namespace DriveSync;

[ValueConversion(typeof(bool), typeof(double))]
public class WindowActiveStateToOpacityConverter : IValueConverter
{
    public static WindowActiveStateToOpacityConverter Instance = new WindowActiveStateToOpacityConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? 1 : 0.7;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
