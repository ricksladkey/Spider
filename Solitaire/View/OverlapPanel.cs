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
              DependencyProperty.Register("Offset", typeof(double), typeof(OverlapPanel), new PropertyMetadata(0.0));
        public static readonly DependencyProperty OffsetSelectorsProperty =
            DependencyProperty.Register("OffsetSelectors", typeof(List<OffsetSelector>), typeof(OverlapPanel), new UIPropertyMetadata(new List<OffsetSelector>()));

        public double Offset
        {
            get { return (double)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public List<OffsetSelector> OffsetSelectors
        {
            get { return (List<OffsetSelector>)GetValue(OffsetSelectorsProperty); }
            set { SetValue(OffsetSelectorsProperty, value); }
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
                totalOffset += GetOffset(child);
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
                totalOffset += GetOffset(child);
            }

            return finalSize;
        }

        private double GetOffset(UIElement element)
        {
            var contentPresenter = element as ContentPresenter;
            if (contentPresenter != null && contentPresenter.DataContext != null)
            {
                Type type = contentPresenter.DataContext.GetType();
                foreach (var selector in OffsetSelectors)
                {
                    if (selector.Type.IsAssignableFrom(type))
                    {
                        return selector.Offset;
                    }
                }
            }
            return Offset;
        }
    }
}
