using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveSync;

[ValueConversion(typeof(bool), typeof(string))]
public class BooleanToTextConverter : IValueConverter
{
    public static BooleanToTextConverter Instance = new BooleanToTextConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (parameter.ToString())
        {
            case "folder-empty":
                return (bool)value ? "Hide empty folders" : "Show empty folders";
            case "visibility":
                return (bool)value ? "Hide equal files/folders" : "Show equal files/folders";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
