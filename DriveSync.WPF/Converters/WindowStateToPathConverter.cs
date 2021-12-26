namespace DriveSync.Converters;

[ValueConversion(typeof(WindowState), typeof(string))]
public class WindowStateToPathConverter : IValueConverter
{
    public static WindowStateToPathConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (WindowState)value == WindowState.Normal ? "M0,0l32,0 0,32 -32,0 0,-32 2,2 0,28 28,0 0,-28 -28,0z"
            : "M1.9999998,8.009992L1.9999998,29.99999 24,29.99999 24,8.009992z M6.9999855,2L6.9999855,6.0099912 26,6.0099912 26,25.004003 29.999985,25.004003 29.999985,2z M4.9999855,0L31.999985,0 31.999985,27.004003 26,27.004003 26,31.99999 0,31.99999 0,6.0099912 4.9999855,6.0099912z";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
