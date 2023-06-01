using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.UserControls
{
    public class Symbol
    {
        private RelayCommand symbolCommand;
        public string Text { get; set; }

        public RelayCommand SymbolClicked
        {
            get { return symbolCommand; }
            set
            {
                symbolCommand = value;
            }
        }
    }
}