using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace M12_GUI.Converter
{
    public class BoolToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (bool.TryParse(value.ToString(), out bool ret))
                {
                    if (ret)
                    {
                        return new SolidColorBrush(Colors.ForestGreen);
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.LightGray);
                    }
                }
                else
                {
                    return new SolidColorBrush(Colors.Red);
                }
            }
            else
            {
                return new SolidColorBrush(Colors.Red);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
