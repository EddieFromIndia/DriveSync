namespace DriveSync;

[ValueConversion(typeof(string), typeof(SolidColorBrush))]
public class StringToColorBrushConverter : IValueConverter
{
    public static StringToColorBrushConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Settings.Default.Theme switch
        {
            "Light" => string.IsNullOrEmpty(value.ToString()) ?
                        new SolidColorBrush() { Color = Color.FromArgb(0, 255, 255, 255) } :
                        new SolidColorBrush() { Color = Color.FromArgb(255, 255, 255, 255) },
            "Dark" => string.IsNullOrEmpty(value.ToString()) ?
                        new SolidColorBrush() { Color = Color.FromArgb(0, 32, 32, 32) } :
                        new SolidColorBrush() { Color = Color.FromArgb(255, 32, 32, 32) },
            _ => string.IsNullOrEmpty(value.ToString()) ?
                        new SolidColorBrush() { Color = Color.FromArgb(0, 255, 255, 255) } :
                        new SolidColorBrush() { Color = Color.FromArgb(255, 255, 255, 255) },
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
