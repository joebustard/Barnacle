//******************************************************************************
//
// Project:
//
// Module:
//
//           (c) copyright Parkeon 2016
//
// File:
//
// Description:
//
//*******************************************************************************

using System;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class RelayCommand : ICommand
    {
        private Func<object, bool> canExecute;
        private Action<object> execute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}