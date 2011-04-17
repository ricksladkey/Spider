using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using Markup.Programming.Core;

namespace Spider.Solitaire.View
{
    public class MoveObjectBehavior : Handler
    {
        public static readonly DependencyProperty IsMoveObjectProperty =
            DependencyProperty.Register("IsMoveObject", typeof(bool), typeof(MoveObjectBehavior), new PropertyMetadata(false));
        public static readonly DependencyProperty FromSelectedProperty =
            DependencyProperty.Register("FromSelected", typeof(bool), typeof(MoveObjectBehavior), new UIPropertyMetadata(false));
        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(Type), typeof(MoveObjectBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty MoveSelectCommandProperty =
            DependencyProperty.Register("MoveSelectCommand", typeof(ICommand), typeof(MoveObjectBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty AutoSelectCommandProperty =
            DependencyProperty.Register("AutoSelectCommand", typeof(ICommand), typeof(MoveObjectBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty StoryboardProperty =
            DependencyProperty.Register("Storyboard", typeof(Storyboard), typeof(MoveObjectBehavior), new UIPropertyMetadata(null));
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(Point), typeof(MoveObjectBehavior), new UIPropertyMetadata(new Point()));
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(Point), typeof(MoveObjectBehavior), new UIPropertyMetadata(new Point()));
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(MoveObjectBehavior), new UIPropertyMetadata(new Duration(new TimeSpan(0, 0, 1))));
        public static readonly DependencyProperty ScaledDurationProperty =
            DependencyProperty.Register("ScaledDuration", typeof(Duration), typeof(MoveObjectBehavior), new UIPropertyMetadata(new Duration(new TimeSpan(0, 0, 1))));

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

        public ICommand MoveSelectCommand
        {
            get { return (ICommand)base.GetValue(MoveSelectCommandProperty); }
            set { base.SetValue(MoveSelectCommandProperty, value); }
        }

        public ICommand AutoSelectCommand
        {
            get { return (ICommand)base.GetValue(AutoSelectCommandProperty); }
            set { base.SetValue(AutoSelectCommandProperty, value); } 
        }

        public Storyboard Storyboard
        {
            get { return (Storyboard)GetValue(StoryboardProperty); }
            set { SetValue(StoryboardProperty, value); }
        }

        public Point From
        {
            get { return (Point)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public Point To
        {
            get { return (Point)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public Duration ScaledDuration
        {
            get { return (Duration)GetValue(ScaledDurationProperty); }
            set { SetValue(ScaledDurationProperty, value); }
        }

        public FrameworkElement MoveObject { get { return AssociatedObject as FrameworkElement; } }

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
            public MoveObjectBehavior MoveObjectBehavior { get; private set; }

            public FrameworkElement MoveObject { get { return MoveObjectBehavior.MoveObject; } }
            public bool FromSelected { get { return MoveObjectBehavior.FromSelected; } }

            public void SetParent(MoveObjectBehavior moveObjectBehavoir)
            {
                MoveObjectBehavior = moveObjectBehavoir;
            }

            public void element_MouseDown(object sender, MouseButtonEventArgs e)
            {
                var element = (FrameworkElement)sender;
                if (element != MoveObject)
                {
                    if (!MoveObjectBehavior.MoveSelectCommand.CanExecute(element.DataContext) &&
                        !MoveObjectBehavior.AutoSelectCommand.CanExecute(element.DataContext))
                    {
                        return;
                    }
                    initialDataContext = element.DataContext;
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
                            if (FromSelected)
                            {
                                MoveObjectBehavior.MoveSelectCommand.Execute(null);
                            }
                            MoveObjectBehavior.MoveSelectCommand.Execute(initialDataContext);
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
                    MoveObjectBehavior.MoveSelectCommand.Execute(isDesiredType ? dataContext : null);
                    MoveObject.Visibility = Visibility.Collapsed;
                    MoveObject.IsHitTestVisible = true;
                }
                else if (mouseDown)
                {
                    if (clickCount == 1)
                    {
                        if (MoveObjectBehavior.MoveSelectCommand.CanExecute(initialDataContext))
                        {
                            if (!FromSelected)
                            {
                                Canvas.SetLeft(MoveObject, startPositionElement.X);
                                Canvas.SetTop(MoveObject, startPositionElement.Y);
                                MoveObject.Visibility = Visibility.Visible;
                                MoveObjectBehavior.MoveSelectCommand.Execute(initialDataContext);
                            }
                            else
                            {
                                var element = sender as FrameworkElement;
                                var toPosition = element.TransformToVisual(Canvas).Transform(new Point(0, 25));
                                AnimateMove(fromPosition, toPosition, () =>
                                    {
                                        MoveObject.Visibility = Visibility.Collapsed;
                                        MoveObjectBehavior.MoveSelectCommand.Execute(initialDataContext);
                                    });
                            }
                        }
                        else if (MoveObjectBehavior.AutoSelectCommand.CanExecute(initialDataContext))
                        {
                            MoveObjectBehavior.AutoSelectCommand.Execute(initialDataContext);
                        }
                    }
                    else if (clickCount == 2)
                    {
                        if (MoveObjectBehavior.AutoSelectCommand.CanExecute(initialDataContext))
                        {
                            MoveObject.Visibility = Visibility.Collapsed;
                            MoveObjectBehavior.AutoSelectCommand.Execute(initialDataContext);
                        }
                    }
                }
                mouseDown = false;
                mouseDrag = false;
            }

            private void AnimateMove(Point from, Point to, Action onStoryboardCompleted)
            {
                if (MoveObjectBehavior.Storyboard != null)
                {
                    MoveObjectBehavior.From = from;
                    MoveObjectBehavior.To = to;
                    var scaledDuration = (from - to).Length / Canvas.RenderSize.Width * MoveObjectBehavior.Duration.TimeSpan.TotalSeconds;
                    MoveObjectBehavior.ScaledDuration = new Duration(TimeSpan.FromSeconds(scaledDuration));

                    var sbp = new StoryboardPlayer(MoveObjectBehavior.Storyboard);
                    var ui = TaskScheduler.FromCurrentSynchronizationContext();
                    sbp.PlayStoryboardAsync().ContinueWith(task => onStoryboardCompleted(), ui);
                }
                else
                {
                    onStoryboardCompleted();
                }
            }
        }

        protected override void OnActiveExecute(Markup.Programming.Core.Engine engine)
        {
            var canvas = FindParent<Canvas>(AssociatedObject);
            var state = StateMap.Values.Where(value => value.Canvas == canvas).FirstOrDefault();
            if (state == null)
            {
                state = new MoveObjectState { Canvas = canvas };
            }
            StateMap[AssociatedObject] = state;
            if (IsMoveObject)
            {
                state.SetParent(this);
                MoveObject.Visibility = Visibility.Collapsed;
            }

            MoveObject.PreviewMouseLeftButtonDown +=
                (sender, e) => StateMap[sender].element_MouseDown(sender, e);
            MoveObject.PreviewMouseMove +=
                (sender, e) => StateMap[sender].element_MouseMove(sender, e);
            MoveObject.PreviewMouseLeftButtonUp +=
                (sender, e) => StateMap[sender].element_MouseUp(sender, e);
        }
    }
}
