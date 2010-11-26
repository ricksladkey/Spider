using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Spider.Solitaire.ViewModel;

namespace Spider.Solitaire.View
{
    public class OverlapPanel : Panel
    {
        public static readonly DependencyProperty DownOffsetProperty =
              DependencyProperty.Register("DownOffset", typeof(double), typeof(OverlapPanel), new PropertyMetadata(0.0));

        public static readonly DependencyProperty UpOffsetProperty =
              DependencyProperty.Register("UpOffset", typeof(double), typeof(OverlapPanel), new PropertyMetadata(0.0));

        public OverlapPanel()
        {
        }

        public double DownOffset
        {
            get
            {
                return (double)GetValue(DownOffsetProperty);
            }
            set
            {
                SetValue(DownOffsetProperty, value);
            }
        }

        public double UpOffset
        {
            get
            {
                return (double)GetValue(UpOffsetProperty);
            }
            set
            {
                SetValue(UpOffsetProperty, value);
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
            if (contentPresenter != null && contentPresenter.DataContext is DownCardViewModel)
            {
                return DownOffset;
            }
            return UpOffset;
        }
    }
}
