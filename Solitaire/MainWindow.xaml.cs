using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Spider.Solitaire.ViewModel;
using Spider.Engine;

namespace Spider.Solitaire
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool mouseDown;
        private bool mouseDrag;
        private Point startPosition;
        private Vector offset;
        private object initialDataContext;

        private void element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Utils.WriteLine("MouseDown: ClickCount = {0}", e.ClickCount);
            if (e.ClickCount == 2)
            {
                (DataContext as SpiderViewModel).AutoSelectCommand.Execute(initialDataContext);
                return;
            }
            mouseDown = true;
            mouseDrag = false;
            var element = (FrameworkElement)sender;
            startPosition = e.GetPosition(mainCanvas);
            GeneralTransform gt = element.TransformToVisual(mainCanvas);
            Vector margin = new Vector(3, 3);
            Point point = gt.Transform(new Point(0, 0)) - margin;
            Canvas.SetLeft(movePile, point.X);
            Canvas.SetTop(movePile, point.Y);
            offset = startPosition - point;
            initialDataContext = element.DataContext;
            (DataContext as SpiderViewModel).SelectCommand.Execute(initialDataContext);
            Mouse.Capture(movePile);
        }

        private void element_MouseMove(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (mouseDown)
            {
                var position = e.GetPosition(mainCanvas);
                Canvas.SetLeft(element, position.X - offset.X);
                Canvas.SetTop(element, position.Y - offset.Y);
                if (!mouseDrag)
                {
                    Vector drag = startPosition - position;
                    if (Math.Abs(drag.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(drag.Y) >= SystemParameters.MinimumVerticalDragDistance)
                    {
                        mouseDrag = true;
                    }
                }
            }
        }

        private void element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mouseDown = false;
            if (mouseDrag)
            {
                var element = Mouse.DirectlyOver as FrameworkElement;
                (DataContext as SpiderViewModel).SelectCommand.Execute(element.DataContext as CardViewModel);
            }
        }
    }
}
