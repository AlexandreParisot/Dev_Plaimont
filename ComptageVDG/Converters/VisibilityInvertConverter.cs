using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ComptageVDG.Converters
{
    public class VisibilityInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is bool visible ))
                return Visibility.Collapsed;

            return (!visible ? Visibility.Visible : Visibility.Collapsed); 
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility val))
                return false;

            return (val == Visibility.Visible ? false : true);
        }
    }
}
