using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Spider.Solitaire.View
{
    public class OffsetSelector : DependencyObject
    {
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(OffsetSelector), new UIPropertyMetadata(typeof(object)));
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(double), typeof(OffsetSelector), new UIPropertyMetadata(0.0));

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public double Offset
        {
            get { return (double)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }
    }
}
