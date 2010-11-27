using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Spider.Solitaire.View
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandBehavior
    {
        //<Button Margin="30" Content="Button" Name="btn"
        //cc:CommandBehavior.RoutedEvent="Button.MouseEnter"
        //cc:CommandBehavior.Command="ApplicationCommands.Undo"/>

        private static Dictionary<UIElement, RoutedEventHandler> handlerTable = new Dictionary<UIElement, RoutedEventHandler>();

        public static ICommand GetCommand(UIElement obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(UIElement obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static Object GetCommandParameter(UIElement obj)
        {
            return obj.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(UIElement obj, Object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }

        public static RoutedEvent GetRoutedEvent(DependencyObject obj)
        {
            return (RoutedEvent)obj.GetValue(RoutedEventProperty);
        }

        public static void SetRoutedEvent(DependencyObject obj, RoutedEvent value)
        {
            obj.SetValue(RoutedEventProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(Object),
            typeof(CommandBehavior),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(CommandBehavior),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback((sender, e) =>
            {
                UIElement element = sender as UIElement;
                ICommand oldCommand = e.OldValue as ICommand;
                ICommand newCommand = e.NewValue as ICommand;
                RoutedEvent routedEvent = element.GetValue(RoutedEventProperty) as RoutedEvent;
                Object commandParameter = element.GetValue(CommandParameterProperty);

                UnwireupCommand(element, routedEvent, oldCommand);
                WireupAndInvokeCommand(element, routedEvent, newCommand, commandParameter);
            })));

        public static readonly DependencyProperty RoutedEventProperty = DependencyProperty.RegisterAttached(
            "RoutedEvent",
            typeof(RoutedEvent),
            typeof(CommandBehavior),
            new FrameworkPropertyMetadata(
                null,
                new PropertyChangedCallback((sender, e) =>
                {
                    UIElement element = sender as UIElement;
                    RoutedEvent oldEvent = e.OldValue as RoutedEvent;
                    RoutedEvent newEvent = e.NewValue as RoutedEvent;
                    ICommand command = element.GetValue(CommandProperty) as ICommand;
                    Object commandParameter = element.GetValue(CommandParameterProperty);

                    UnwireupCommand(element, oldEvent, command);
                    WireupAndInvokeCommand(element, newEvent, command, commandParameter);
                })));

        private static void WireupAndInvokeCommand(UIElement element, RoutedEvent routedEvent, ICommand command, Object commandParameter)
        {
            if (routedEvent != null && element != null && command != null)
            {
                RoutedEventHandler InvokeCommandHandler = new RoutedEventHandler(delegate
                {
                    command.Execute(commandParameter);
                });

                handlerTable.Add(element, InvokeCommandHandler);
                element.AddHandler(routedEvent, InvokeCommandHandler);
            }
        }

        private static void UnwireupCommand(UIElement element, RoutedEvent routedEvent, ICommand command)
        {
            if (routedEvent != null && element != null && command != null)
            {
                RoutedEventHandler handler = handlerTable[element];
                if (handler != null)
                {
                    element.RemoveHandler(routedEvent, handler);
                    handlerTable.Remove(element);
                }
            }
        }
    }
}
