namespace DriveSync.Converters;

[ValueConversion(typeof(bool), typeof(string))]
public class BooleanToTextConverter : IValueConverter
{
    public static BooleanToTextConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter.ToString() switch
        {
            "folder-empty" => (bool)value ? "Hide empty folders" : "Show empty folders",
            "visibility" => (bool)value ? "Hide equal files/folders" : "Show equal files/folders",
            _ => string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
