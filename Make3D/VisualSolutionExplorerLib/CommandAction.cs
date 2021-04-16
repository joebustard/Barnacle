using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class CommandAction : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty CommandParameterProperty =
         DependencyProperty.Register("CommandParameter", typeof(object), typeof(CommandAction),
         new PropertyMetadata(null, OnCommandParameterChanged));

        public static readonly DependencyProperty CommandProperty =
                 DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandAction),
         new PropertyMetadata(null, OnCommandChanged));

        private IDisposable canExecuteChanged;

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            ICommand command = Command;
            if (command != null)
            {
                command.Execute(CommandParameter);
            }
        }

        private static void OnCommandChanged(DependencyObject sender,
                                DependencyPropertyChangedEventArgs e)
        {
            CommandAction ev = sender as CommandAction;
            if (ev != null)
            {
                if (ev.canExecuteChanged != null)
                {
                    ev.canExecuteChanged.Dispose();
                }

                ICommand command = e.NewValue as ICommand;
                if (command != null)
                {
                    ev.canExecuteChanged = Observable.FromEventPattern(
                        x => command.CanExecuteChanged += x,
                        x => command.CanExecuteChanged -= x).Subscribe
                        (_ => ev.SynchronizeElementState());
                }
            }
        }

        private static void OnCommandParameterChanged(DependencyObject sender,
                 DependencyPropertyChangedEventArgs e)
        {
            CommandAction ev = sender as CommandAction;
            if (ev != null)
            {
                ev.SynchronizeElementState();
            }
        }

        private void SynchronizeElementState()
        {
            ICommand command = Command;
            if (command != null)
            {
                FrameworkElement associatedObject = AssociatedObject as FrameworkElement;
                if (associatedObject != null)
                {
                    associatedObject.IsEnabled = command.CanExecute(CommandParameter);
                }
            }
        }
    }
}