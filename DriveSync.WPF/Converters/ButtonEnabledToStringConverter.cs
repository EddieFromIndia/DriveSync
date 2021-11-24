namespace DriveSync.Converters;

[ValueConversion(typeof(bool), typeof(string))]
public class ButtonEnabledToStringConverter : IValueConverter
{
    public static ButtonEnabledToStringConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            return parameter.ToString();
        }
        else
        {
            return "Original or Backup does not exist, or refers to the same folder.";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
