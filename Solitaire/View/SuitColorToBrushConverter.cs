using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Spider.Engine;
using System.Windows.Media;

namespace Spider.Solitaire.View
{
    public class SuitColorToBrushConverter : IValueConverter
    {
        public Brush Black { get; set; }
        public Brush Red { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var suitColor = (SuitColor)value;
            switch (suitColor)
            {
                case SuitColor.Black:
                    return Black;
                case SuitColor.Red:
                    return Red;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
