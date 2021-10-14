﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DriveSync
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToImageConverter : IValueConverter
    {
        public static BooleanToImageConverter Instance = new BooleanToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string image = (bool)value ? "file" : "folder";
            return new BitmapImage(new Uri($"pack://application:,,,/Assets/{image}.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
