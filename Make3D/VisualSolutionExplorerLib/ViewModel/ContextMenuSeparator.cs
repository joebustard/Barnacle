using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualSolutionExplorer
{
    internal class ContextMenuSeparator : ContextMenuAction
    {
        public override bool IsSeparator
        {
            get { return true; }
        }
    }
}