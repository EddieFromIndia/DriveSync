using DriveSync.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DriveSync
{
    [ValueConversion(typeof(ItemStatus), typeof(string))]
    public class ItemStatusToStringConverter : IValueConverter
    {
        public static ItemStatusToStringConverter Instance = new ItemStatusToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string message = "Resolve";
            switch (parameter.ToString())
            {
                case "source":
                    switch ((ItemStatus)value)
                    {
                        case ItemStatus.ExistsButDifferent:
                            message = "Merge with Target";
                            break;
                        case ItemStatus.ExistsElsewhere:
                            // Not Implemented
                            break;
                        case ItemStatus.DoesNotExist:
                            message = "Copy to Target";
                            break;
                    }
                    break;
                case "target":
                    switch ((ItemStatus)value)
                    {
                        case ItemStatus.ExistsButDifferent:
                            message = "Merge with Source";
                            break;
                        case ItemStatus.ExistsElsewhere:
                            // Not Implemented
                            break;
                        case ItemStatus.DoesNotExist:
                            message = "Copy to Source";
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
}
