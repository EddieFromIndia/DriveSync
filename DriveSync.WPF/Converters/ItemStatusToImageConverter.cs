namespace DriveSync.Converters;

[ValueConversion(typeof(ItemStatus), typeof(BitmapImage))]
public class ItemStatusToImageConverter : IValueConverter
{
    public static ItemStatusToImageConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string image = string.Empty;
        switch (parameter.ToString())
        {
            case "original":
                image = (ItemStatus)value switch
                {
                    ItemStatus.ExistsButDifferent => "merge-right",
                    ItemStatus.ExistsWithDifferentName => "rename",
                    ItemStatus.DoesNotExist => "copy-right",
                    _ => "merge-right",
                };
                break;
            case "backup":
                image = (ItemStatus)value switch
                {
                    ItemStatus.ExistsButDifferent => "merge-left",
                    ItemStatus.ExistsWithDifferentName => "rename",
                    ItemStatus.DoesNotExist => "copy-left",
                    _ => "merge-left",
                };
                break;
        }

        return new BitmapImage(new Uri($"pack://application:,,,/Assets/{image}.png"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
