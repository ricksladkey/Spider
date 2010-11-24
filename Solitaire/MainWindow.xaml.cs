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

        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(mainCanvas);
            mouseDown = true;
            xOffset = position.X - Canvas.GetLeft(canvas1);
            yOffset = position.Y - Canvas.GetTop(canvas1);
            Mouse.Capture(canvas1);
        }

        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                var position = e.GetPosition(mainCanvas);
                Canvas.SetLeft(canvas1, position.X - xOffset);
                Canvas.SetTop(canvas1, position.Y - yOffset);
            }
        }

        private void canvas1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mouseDown = false;
        }
    }
}
