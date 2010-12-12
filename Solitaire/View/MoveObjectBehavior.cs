using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Spider.Solitaire.View
{
    public class MoveObjectBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty IsMoveObjectProperty =
            DependencyProperty.Register("IsMoveObject", typeof(bool), typeof(MoveObjectBehavior), new PropertyMetadata(false));
        public static readonly DependencyProperty FromSelectedProperty =
            DependencyProperty.Register("FromSelected", typeof(bool), typeof(MoveObjectBehavior), new UIPropertyMetadata(false));
        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(Type), typeof(MoveObjectBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectCommandProperty =
            DependencyProperty.Register("SelectCommand", typeof(ICommand), typeof(MoveObjectBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty AutoSelectCommandProperty =
            DependencyProperty.Register("AutoSelectCommand", typeof(ICommand), typeof(MoveObjectBehavior), new PropertyMetadata(null));

        private static Dictionary<object, MoveObjectState> StateMap = new Dictionary<object, MoveObjectState>();

        public bool IsMoveObject
        {
            get { return (bool)base.GetValue(IsMoveObjectProperty); }
            set { base.SetValue(IsMoveObjectProperty, value); }
        }

        public bool FromSelected
        {
            get { return (bool)GetValue(FromSelectedProperty); }
            set { SetValue(FromSelectedProperty, value); }
        }

        public Type TargetType
        {
            get { return (Type)base.GetValue(TargetTypeProperty); }
            set { base.SetValue(TargetTypeProperty, value); }
        }

        public ICommand SelectCommand
        {
            get { return (ICommand)base.GetValue(SelectCommandProperty); }
            set { base.SetValue(SelectCommandProperty, value); }
        }

        public ICommand AutoSelectCommand
        {
            get { return (ICommand)base.GetValue(AutoSelectCommandProperty); }
            set { base.SetValue(AutoSelectCommandProperty, value); } 
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            var canvas = FindParent<Canvas>(AssociatedObject);
            var state = StateMap.Values.Where(value => value.Canvas == canvas).FirstOrDefault();
            if (state == null)
            {
                state = new MoveObjectState { Canvas = canvas };
            }
            StateMap[AssociatedObject] = state;
            if (IsMoveObject)
            {
                state.MoveObjectBehavior = this;
                AssociatedObject.Visibility = Visibility.Collapsed;
            }

            AssociatedObject.MouseLeftButtonDown +=
                (sender, e) => { if (!IsMoveObject) StateMap[sender].element_MouseDown(sender, e); };
            AssociatedObject.MouseLeftButtonUp +=
                (sender, e) => { StateMap[sender].element_MouseUp(sender, e); };
            AssociatedObject.MouseMove +=
                (sender, e) => { StateMap[sender].element_MouseMove(sender, e); };
        }

        private T FindParent<T>(DependencyObject element)
            where T : DependencyObject
        {
            while (true)
            {
                element = VisualTreeHelper.GetParent(element);
                if (element == null)
                {
                    return null;
                }
                if (element is T)
                {
                    return element as T;
                }
            }
        }

        private class MoveObjectState
        {
            private bool mouseDown;
            private bool mouseDrag;
            private int clickCount;
            private Point startPositionElement;
            private Point startPositionMouse;
            private Vector offsetElementToMouse;
            private Point fromPosition;
            private object initialDataContext;

            public Canvas Canvas { get; set; }
            public MoveObjectBehavior MoveObjectBehavior { get; set; }

            public FrameworkElement MoveObject { get { return MoveObjectBehavior.AssociatedObject; } }
            public bool FromSelected { get { return MoveObjectBehavior.FromSelected; } }

            public void element_MouseDown(object sender, MouseButtonEventArgs e)
            {
                var element = (FrameworkElement)sender;
                initialDataContext = element.DataContext;
                if (!MoveObjectBehavior.SelectCommand.CanExecute(initialDataContext))
                {
                    return;
                }
                mouseDown = true;
                mouseDrag = false;
                clickCount = e.ClickCount;
                startPositionElement = element.TransformToVisual(Canvas).Transform(new Point(0, 0));
                startPositionMouse = e.GetPosition(Canvas);
                offsetElementToMouse = startPositionMouse - startPositionElement;
                if (!FromSelected)
                {
                    fromPosition = startPositionElement;
                }
            }

            public void element_MouseMove(object sender, MouseEventArgs e)
            {
                var element = sender as FrameworkElement;
                if (mouseDown)
                {
                    var position = e.GetPosition(Canvas);
                    if (!mouseDrag)
                    {
                        var drag = startPositionMouse - position;
                        if (Math.Abs(drag.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                            Math.Abs(drag.Y) >= SystemParameters.MinimumVerticalDragDistance)
                        {
                            mouseDrag = true;
                            MoveObjectBehavior.SelectCommand.Execute(initialDataContext);
                            MoveObject.Visibility = Visibility.Visible;
                            MoveObject.IsHitTestVisible = false;
                            Mouse.Capture(MoveObject);
                        }
                    }
                    if (mouseDrag)
                    {
                        Canvas.SetLeft(MoveObject, position.X - offsetElementToMouse.X);
                        Canvas.SetTop(MoveObject, position.Y - offsetElementToMouse.Y);
                    }
                }
            }

            public void element_MouseUp(object sender, MouseButtonEventArgs e)
            {
                Mouse.Capture(null);
                if (mouseDrag)
                {
                    var position = e.GetPosition(Canvas);
                    var element = Mouse.DirectlyOver as FrameworkElement;
                    var dataContext = element != null ? element.DataContext : null;
                    var isDesiredType = dataContext != null && MoveObjectBehavior.TargetType.IsAssignableFrom(dataContext.GetType());
                    MoveObjectBehavior.SelectCommand.Execute(isDesiredType ? dataContext : null);
                    MoveObject.Visibility = Visibility.Collapsed;
                    MoveObject.IsHitTestVisible = true;
                }
                else if (mouseDown)
                {
                    if (clickCount == 1)
                    {
                        if (!FromSelected)
                        {
                            MoveObject.Visibility = Visibility.Visible;
                            Canvas.SetLeft(MoveObject, startPositionElement.X);
                            Canvas.SetTop(MoveObject, startPositionElement.Y);
                        }
                        else
                        {
                            MoveObject.Visibility = Visibility.Collapsed;
                            var element = sender as FrameworkElement;
                            var toPosition = element.TransformToVisual(Canvas).Transform(new Point(0, 25));
                            AnimateMove(fromPosition, toPosition);
                        }
                        MoveObjectBehavior.SelectCommand.Execute(initialDataContext);
                    }
                    else if (clickCount == 2)
                    {
                        MoveObjectBehavior.AutoSelectCommand.Execute(initialDataContext);
                    }
                }
                mouseDown = false;
                mouseDrag = false;
            }

            private void AnimateMove(Point from, Point to)
            {
            }
        }
    }
}
