﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    public class ArraySymbol : Symbol
    {
        private SymbolTable.SymbolType itemType;

        public Array Array { get; set; }

        public SymbolTable.SymbolType ItemType
        {
            get { return itemType; }
            set { itemType = value; }
        }

        public override void SetSize(int d)
        {
            Array = new Array();
            Array.SetSize(d);
        }
    }
}