using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace DriveSync;

[ValueConversion(typeof(string), typeof(string))]
public class PathToStringConverter : IValueConverter
{
    public static PathToStringConverter Instance = new PathToStringConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (parameter.ToString())
        {
            case "source":
                return string.IsNullOrEmpty(value?.ToString()) ? string.Empty : $"Exists in target with name: {new DirectoryInfo(value.ToString()).Name}";
            case "target":
                return string.IsNullOrEmpty(value?.ToString()) ? string.Empty : $"Exists in source with name: {new DirectoryInfo(value.ToString()).Name}";
            default:
                return string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
