using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using Markup.Programming.Core;

namespace Spider.Solitaire.View
{
    public class Confirm : Handler
    {
        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is ButtonBase)
                (AssociatedObject as ButtonBase).Click -= PromptAndExecuteCommand;
            else
                (AssociatedObject as UIElement).MouseLeftButtonDown -= PromptAndExecuteCommand;
        }

        void PromptAndExecuteCommand(object sender, RoutedEventArgs e)
        {
            if (!IsConfirm || MessageBoxResult.OK == MessageBox.Show(ConfirmMessage, ConfirmCaption, MessageBoxButton.OKCancel))
            {
                if (Command != null)
                {
                    Command.Execute(CommandParameter);
                }
            }
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(Confirm), null);
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }


        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(Confirm), null);
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty IsConfirmProperty = DependencyProperty.Register("Confirm", typeof(bool), typeof(Confirm), null);
        public bool IsConfirm
        {
            get { return (bool)GetValue(IsConfirmProperty); }
            set { SetValue(IsConfirmProperty, value); }
        }

        public static readonly DependencyProperty ConfirmCaptionProperty = DependencyProperty.Register("ConfirmCaption", typeof(string), typeof(Confirm), null);
        public string ConfirmCaption
        {
            get { return (string)GetValue(ConfirmCaptionProperty); }
            set { SetValue(ConfirmCaptionProperty, value); }
        }

        public static readonly DependencyProperty ConfirmMessageProperty = DependencyProperty.Register("ConfirmMessage", typeof(string), typeof(Confirm), null);
        public string ConfirmMessage
        {
            get { return (string)GetValue(ConfirmMessageProperty); }
            set { SetValue(ConfirmMessageProperty, value); }
        }

        protected override void OnActiveExecute(Markup.Programming.Core.Engine engine)
        {
            if (AssociatedObject is ButtonBase)
                (AssociatedObject as ButtonBase).Click += new RoutedEventHandler(PromptAndExecuteCommand);
            else
                (AssociatedObject as UIElement).MouseLeftButtonDown += new MouseButtonEventHandler(PromptAndExecuteCommand);
        }
    }
}

