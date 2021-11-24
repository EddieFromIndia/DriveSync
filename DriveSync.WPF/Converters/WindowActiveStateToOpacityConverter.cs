namespace DriveSync.Converters;

[ValueConversion(typeof(bool), typeof(double))]
public class WindowActiveStateToOpacityConverter : IValueConverter
{
    public static WindowActiveStateToOpacityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? 1 : 0.7;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
