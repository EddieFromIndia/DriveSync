namespace DriveSync.Converters;

[ValueConversion(typeof(ItemStatus), typeof(string))]
public class ItemStatusToStringConverter : IValueConverter
{
    public static ItemStatusToStringConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string message = "Resolve";
        switch (parameter.ToString())
        {
            case "original":
                switch ((ItemStatus)value)
                {
                    case ItemStatus.ExistsButDifferent:
                        message = "Merge with Backup";
                        break;
                    case ItemStatus.ExistsWithDifferentName:
                        message = "Rename to match Backup";
                        break;
                    case ItemStatus.DoesNotExist:
                        message = "Copy to Backup folder";
                        break;
                }
                break;
            case "backup":
                switch ((ItemStatus)value)
                {
                    case ItemStatus.ExistsButDifferent:
                        message = "Merge with Original";
                        break;
                    case ItemStatus.ExistsWithDifferentName:
                        message = "Rename to match Original";
                        break;
                    case ItemStatus.DoesNotExist:
                        message = "Copy to Original folder";
                        break;
                }
                break;
        }

        return message;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
