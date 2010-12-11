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
        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(Type), typeof(MoveObjectBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectCommandProperty =
            DependencyProperty.Register("SelectCommand", typeof(ICommand), typeof(MoveObjectBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty AutoSelectCommandProperty =
            DependencyProperty.Register("AutoSelectCommand", typeof(ICommand), typeof(MoveObjectBehavior), new PropertyMetadata(null));

        private static Dictionary<object, MoveObjectState> StateMap = new Dictionary<object, MoveObjectState>();

        public bool IsMoveObject
        {
            get
            {
                return (bool)base.GetValue(IsMoveObjectProperty);
            }
            set
            {
                base.SetValue(IsMoveObjectProperty, value);
            }
        }

        public Type TargetType
        {
            get
            {
                return (Type)base.GetValue(TargetTypeProperty);
            }
            set
            {
                base.SetValue(TargetTypeProperty, value);
            }
        }

        public ICommand SelectCommand
        {
            get
            {
                return (ICommand)base.GetValue(SelectCommandProperty);
            }
            set
            {
                base.SetValue(SelectCommandProperty, value);
            }
        }

        public ICommand AutoSelectCommand
        {
            get
            {
                return (ICommand)base.GetValue(AutoSelectCommandProperty);
            }
            set
            {
                base.SetValue(AutoSelectCommandProperty, value);
            }
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
            }

            AssociatedObject.MouseLeftButtonDown +=
                (sender, e) => { if (!IsMoveObject) StateMap[sender].element_MouseDown(sender, e); };
            AssociatedObject.MouseLeftButtonUp +=
                (sender, e) => { if (IsMoveObject) StateMap[sender].element_MouseUp(sender, e); };
            AssociatedObject.MouseMove +=
                (sender, e) => { if (IsMoveObject) StateMap[sender].element_MouseMove(sender, e); };
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
            private Point startPosition;
            private Vector offset;
            private object initialDataContext;

            public Canvas Canvas { get; set; }
            public MoveObjectBehavior MoveObjectBehavior { get; set; }

            public FrameworkElement MoveObject { get { return MoveObjectBehavior.AssociatedObject; } }

            public void element_MouseDown(object sender, MouseButtonEventArgs e)
            {
                if (e.ClickCount == 2)
                {
                    MoveObjectBehavior.AutoSelectCommand.Execute(initialDataContext);
                    return;
                }
                var element = (FrameworkElement)sender;
                initialDataContext = element.DataContext;
                if (!MoveObjectBehavior.SelectCommand.CanExecute(initialDataContext))
                {
                    return;
                }
                mouseDown = true;
                mouseDrag = false;
                startPosition = e.GetPosition(Canvas);
                var gt = element.TransformToVisual(Canvas);
                var point = gt.Transform(new Point(0, 0));
                Canvas.SetLeft(MoveObject, point.X);
                Canvas.SetTop(MoveObject, point.Y);
                offset = startPosition - point;
                MoveObjectBehavior.SelectCommand.Execute(initialDataContext);
                Mouse.Capture(MoveObject);
            }

            public void element_MouseMove(object sender, MouseEventArgs e)
            {
                var element = sender as FrameworkElement;
                if (mouseDown)
                {
                    var position = e.GetPosition(Canvas);
                    Canvas.SetLeft(element, position.X - offset.X);
                    Canvas.SetTop(element, position.Y - offset.Y);
                    if (!mouseDrag)
                    {
                        var drag = startPosition - position;
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
                    var dataContext = element != null ? element.DataContext : null;
                    var isDesiredType = dataContext != null && MoveObjectBehavior.TargetType.IsAssignableFrom(dataContext.GetType());
                    MoveObjectBehavior.SelectCommand.Execute(isDesiredType ? dataContext : null);
                }
            }
        }
    }
}
