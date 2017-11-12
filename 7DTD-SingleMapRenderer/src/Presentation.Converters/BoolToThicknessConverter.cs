using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace _7DTD_SingleMapRenderer.Presentation.Converters
{
    [ValueConversion(typeof(bool), typeof(Thickness), ParameterType = typeof(double))]
    public class BoolToThicknessConverter : IValueConverter
    {
        #region IValueConverter Member

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double thickness = 0.0;
            bool? val = value as bool?;
            if (val.Value)
            {
                double? para = parameter as double?;
                if (para.HasValue)
                {
                    thickness = para.Value;
                }
                else
                {
                    if (parameter is string)
                    {
                        if (!double.TryParse((string)parameter, System.Globalization.NumberStyles.Float, culture.NumberFormat, out thickness))
                        {
                            thickness = 1.0;
                        }
                    }
                }
            }
            return new Thickness(thickness);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
