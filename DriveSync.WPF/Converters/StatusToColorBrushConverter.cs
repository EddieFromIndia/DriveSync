using DriveSync.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DriveSync
{
    [ValueConversion(typeof(ItemStatus), typeof(SolidColorBrush))]
    public class StatusToColorBrushConverter : IValueConverter
    {
        public static StatusToColorBrushConverter Instance = new StatusToColorBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = new SolidColorBrush();
            switch ((ItemStatus)value)
            {
                case ItemStatus.ExistsAndEqual:
                    brush.Color = Color.FromArgb(255, 180, 180, 180);
                    break;
                case ItemStatus.ExistsButDifferent:
                    brush.Color = Color.FromArgb(255, 176, 137, 46);
                    break;
                case ItemStatus.ExistsElsewhere:
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
}
