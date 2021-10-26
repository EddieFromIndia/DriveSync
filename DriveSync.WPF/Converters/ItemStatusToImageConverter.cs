using DriveSync.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveSync
{
    [ValueConversion(typeof(ItemStatus), typeof(BitmapImage))]
    public class ItemStatusToImageConverter : IValueConverter
    {
        public static ItemStatusToImageConverter Instance = new ItemStatusToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string image = string.Empty;
            switch (parameter.ToString())
            {
                case "source":
                    switch ((ItemStatus)value)
                    {
                        case ItemStatus.ExistsButDifferent:
                            image = "merge-right";
                            break;
                        case ItemStatus.ExistsElsewhere:
                            // Not Implemented
                            break;
                        case ItemStatus.DoesNotExist:
                            image = "copy-right";
                            break;
                        default:
                            image = "merge-right";
                            break;
                    }
                    break;
                case "target":
                    switch ((ItemStatus)value)
                    {
                        case ItemStatus.ExistsButDifferent:
                            image = "merge-left";
                            break;
                        case ItemStatus.ExistsElsewhere:
                            // Not Implemented
                            break;
                        case ItemStatus.DoesNotExist:
                            image = "copy-left";
                            break;
                        default:
                            image = "merge-left";
                            break;
                    }
                    break;
            }

            return new BitmapImage(new Uri($"pack://application:,,,/Assets/{image}.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
