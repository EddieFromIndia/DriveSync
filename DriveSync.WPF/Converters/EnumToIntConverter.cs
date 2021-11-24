namespace DriveSync.Converters;

[ValueConversion(typeof(ResolveMethods), typeof(int))]
public class EnumToIntConverter : IValueConverter
{
    public static EnumToIntConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)(ResolveMethods)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (ResolveMethods)(int)value;
    }
}
