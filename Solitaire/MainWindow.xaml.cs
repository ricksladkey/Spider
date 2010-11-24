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

            DataContext = new SpiderViewModel();
        }

        private bool mouseDown;
        private double xOffset;
        private double yOffset;

        private void element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            var position = e.GetPosition(mainCanvas);
            mouseDown = true;
            xOffset = position.X - Canvas.GetLeft(element);
            yOffset = position.Y - Canvas.GetTop(element);
            Mouse.Capture(element);
        }

        private void element_MouseMove(object sender, MouseEventArgs e)
        {
            var element = (UIElement)sender;
            if (mouseDown)
            {
                var position = e.GetPosition(mainCanvas);
                Canvas.SetLeft(element, position.X - xOffset);
                Canvas.SetTop(element, position.Y - yOffset);
            }
        }

        private void element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            Mouse.Capture(null);
            mouseDown = false;
        }
    }
}
