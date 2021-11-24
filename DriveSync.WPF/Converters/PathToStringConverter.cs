namespace DriveSync.Converters;

[ValueConversion(typeof(string), typeof(string))]
public class PathToStringConverter : IValueConverter
{
    public static PathToStringConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter.ToString() switch
        {
            "original" => string.IsNullOrEmpty(value?.ToString()) ? string.Empty : $"Exists in backup folder with name: {new DirectoryInfo(value.ToString()).Name}",
            "backup" => string.IsNullOrEmpty(value?.ToString()) ? string.Empty : $"Exists in original folder with name: {new DirectoryInfo(value.ToString()).Name}",
            _ => string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
