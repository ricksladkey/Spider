using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Input;
using Spider.Solitaire.ViewModel;
using Spider.Engine;
using System.Windows.Controls;

namespace Spider.Solitaire.View
{
    public class MovePileBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty IsMovePileProperty =
            DependencyProperty.Register("IsMovePile", typeof(bool), typeof(MovePileBehavior), new PropertyMetadata(false, new PropertyChangedCallback(MovePileBehavior.OnIsMovePileChanged)));

        private static MovePileState State = null;

        public bool IsMovePile
        {
            get
            {
                return (bool)base.GetValue(IsMovePileProperty);
            }
            set
            {
                base.SetValue(IsMovePileProperty, value);
            }
        }

        private static void OnIsMovePileChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            MovePileBehavior behavior = sender as MovePileBehavior;
        }

        protected override void OnAttached()
        {
            if (State == null)
            {
                State = new MovePileState(Application.Current.MainWindow as MainWindow);
            }

            if (!this.IsMovePile)
            {
                AssociatedObject.MouseLeftButtonDown += (sender, e) => State.element_MouseDown(sender, e);
            }
            else
            {
                AssociatedObject.MouseLeftButtonUp += (sender, e) => State.element_MouseUp(sender, e);
                AssociatedObject.MouseMove += (sender, e) => State.element_MouseMove(sender, e);
            }
        }

        private class MovePileState
        {
            private Canvas mainCanvas;
            private FrameworkElement movePile;

            private bool mouseDown;
            private bool mouseDrag;
            private Point startPosition;
            private Vector offset;
            private object initialDataContext;

            public MovePileState(MainWindow mainWindow)
            {
                DataContext = mainWindow.DataContext;
                mainCanvas = mainWindow.mainCanvas;
                movePile = mainWindow.movePile;
            }

            public object DataContext { get; set; }

            public void element_MouseDown(object sender, MouseButtonEventArgs e)
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

            public void element_MouseMove(object sender, MouseEventArgs e)
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

            public void element_MouseUp(object sender, MouseButtonEventArgs e)
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
}
