namespace DriveSync;

[ValueConversion(typeof(ItemType), typeof(BitmapImage))]
public class ItemTypeToImageConverter : IValueConverter
{
    public static ItemTypeToImageConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string image = (ItemType)value switch
        {
            ItemType.Folder => "folder",
            ItemType.Android => "file-android",
            ItemType.Archive => "file-archive",
            ItemType.Audio => "file-audio",
            ItemType.Code => "file-code",
            ItemType.DiskImage => "file-disk-image",
            ItemType.Executable => "file-executable",
            ItemType.Font => "file-font",
            ItemType.Image => "file-image",
            ItemType.MarkupLanguage => "file-markup",
            ItemType.PDF => "file-pdf",
            ItemType.Presentation => "file-presentation",
            ItemType.Spreadsheet => "file-spreadsheet",
            ItemType.System => "file-system",
            ItemType.Text => "file-text",
            ItemType.Video => "file-video",
            _ => "file",
        };
        return new BitmapImage(new Uri($"pack://application:,,,/Assets/{image}.png"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
