using DriveSync.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveSync
{
    [ValueConversion(typeof(ItemType), typeof(BitmapImage))]
    public class ItemTypeToImageConverter : IValueConverter
    {
        public static ItemTypeToImageConverter Instance = new ItemTypeToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string image;
            switch ((ItemType)value)
            {
                case ItemType.Folder:
                    image = "folder";
                    break;
                case ItemType.Android:
                    image = "file-android";
                    break;
                case ItemType.Archive:
                    image = "file-archive";
                    break;
                case ItemType.Audio:
                    image = "file-audio";
                    break;
                case ItemType.Code:
                    image = "file-code";
                    break;
                case ItemType.DiskImage:
                    image = "file-disk-image";
                    break;
                case ItemType.Executable:
                    image = "file-executable";
                    break;
                case ItemType.Font:
                    image = "file-font";
                    break;
                case ItemType.Image:
                    image = "file-image";
                    break;
                case ItemType.PDF:
                    image = "file-pdf";
                    break;
                case ItemType.Presentation:
                    image = "file-presentation";
                    break;
                case ItemType.Spreadsheet:
                    image = "file-spreadsheet";
                    break;
                case ItemType.System:
                    image = "file-system";
                    break;
                case ItemType.Text:
                    image = "file-text";
                    break;
                case ItemType.Video:
                    image = "file-video";
                    break;
                case ItemType.Webpage:
                    image = "file-webpage";
                    break;
                default:
                    image = "file";
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
