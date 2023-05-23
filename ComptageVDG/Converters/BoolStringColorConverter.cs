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
    public class BoolStringColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is bool visible ))
                return "#89f09f";

            return (visible ? "#89f09f" : "#f58e87"); 
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string val && val == "#89f09f"))
                return true;

            return (val == "#89f09f" ? true : false);
        }
    }
}
