using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class ContextMenuAction : INotifyPropertyChanged
    {
        private ICommand command;
        private string headerText;
        private string tooltip;

        public ContextMenuAction(String h = null, ICommand command = null, string tt = null)
        {
            headerText = h;
            this.MenuCommand = command;
            this.ToolTip = tt;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string HeaderText
        {
            get
            {
                return headerText;
            }
            set
            {
                if (headerText != value)
                {
                    headerText = value;
                    NotifyPropertyChanged("HeaderText");
                }
            }
        }

        public virtual bool IsSeparator
        {
            get
            {
                return false;
            }
        }

        public ICommand MenuCommand
        {
            get
            {
                return command;
            }
            set
            {
                if (command != value)
                {
                    command = value;
                    NotifyPropertyChanged("MenuCommand");
                }
            }
        }

        public string ToolTip
        {
            get
            {
                return tooltip;
            }
            set
            {
                if (tooltip != value)
                {
                    tooltip = value;
                    NotifyPropertyChanged("ToolTip");
                }
            }
        }

        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}