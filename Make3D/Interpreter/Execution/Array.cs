using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    public class Array
    {
        private object[] elements;

        public Array()
        {
            elements = null;
            ItemType = StackItem.ItemType.noval;
        }

        public StackItem.ItemType ItemType { get; set; }

        public bool SetSize(int numElements)
        {
            bool res = true;
            elements = new object[numElements];
            return res;
        }
    }
}