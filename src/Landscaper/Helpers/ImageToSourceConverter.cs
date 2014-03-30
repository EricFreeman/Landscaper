using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Landscaper.Helpers
{
    public class ImageToSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var image = value as Image;
            if (image != null)
            {
                return image.Source;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}