namespace DriveSync;

[ValueConversion(typeof(WindowState), typeof(BitmapImage))]
public class WindowStateToImageConverter : IValueConverter
{
    public static WindowStateToImageConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string image = null;
        if (Properties.Settings.Default.Theme == "Light")
        {
            switch (parameter.ToString())
            {
                case "close":
                    image = "close-black";
                    break;
                case "maximize":
                    image = (WindowState)value == WindowState.Normal ? "maximize-black" : "restore-black";
                    break;
                case "minimize":
                    image = "minimize-black";
                    break;
            }
        }
        else
        {
            switch (parameter.ToString())
            {
                case "close":
                    image = "close";
                    break;
                case "maximize":
                    image = (WindowState)value == WindowState.Normal ? "maximize" : "restore";
                    break;
                case "minimize":
                    image = "minimize";
                    break;
            }
        }

        return new BitmapImage(new Uri($"pack://application:,,,/Assets/{image}.png"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
