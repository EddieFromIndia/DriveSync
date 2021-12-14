namespace DriveSync.Converters;

[ValueConversion(typeof(bool), typeof(string))]
public class EnabledToStringConverter : IValueConverter
{
    public static EnabledToStringConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "Add another backup location." : "Add the previous backup location first.";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
