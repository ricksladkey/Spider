using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Spider.Solitaire.View
{
    public class OverlapPanel : Panel
    {
        public static readonly DependencyProperty OffsetProperty =
              DependencyProperty.Register("Overlap", typeof(double), typeof(OverlapPanel)); 
        
        public OverlapPanel()
        {
            Offset = 0;
        }

        public double Offset
        {
            get
            {
                return (double)GetValue(OffsetProperty);
            }
            set
            {
                SetValue(OffsetProperty, value);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size resultSize = new Size(0, 0);

            double totalOffset = 0;
            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
                resultSize.Width = Math.Max(resultSize.Width, child.DesiredSize.Width);
                resultSize.Height = Math.Max(resultSize.Width, totalOffset + child.DesiredSize.Height);
                totalOffset += Offset;
            }

            resultSize.Width = double.IsPositiveInfinity(availableSize.Width) ?
                resultSize.Width : availableSize.Width;

            resultSize.Height = double.IsPositiveInfinity(availableSize.Height) ?
                resultSize.Height : availableSize.Height;

            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double totalOffset = 0;
            foreach (UIElement child in Children)
            {
                child.Arrange(new Rect(new Point(0, totalOffset), child.DesiredSize));
                totalOffset += Offset;
            }

            return finalSize;
        }
    }
}
