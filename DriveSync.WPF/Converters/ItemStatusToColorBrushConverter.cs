namespace DriveSync;

[ValueConversion(typeof(ItemStatus), typeof(SolidColorBrush))]
public class ItemStatusToColorBrushConverter : IValueConverter
{
    public static ItemStatusToColorBrushConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        SolidColorBrush brush = new();
        switch ((ItemStatus)value)
        {
            case ItemStatus.ExistsAndEqual:
                brush.Color = Color.FromArgb(255, 180, 180, 180);
                break;
            case ItemStatus.ExistsButDifferent:
                brush.Color = Color.FromArgb(255, 176, 137, 46);
                break;
            case ItemStatus.ExistsWithDifferentName:
                brush.Color = Color.FromArgb(255, 124, 87, 139);
                break;
            case ItemStatus.DoesNotExist:
                brush.Color = Color.FromArgb(255, 196, 81, 75);
                break;
            default:
                break;
        }

        return brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
