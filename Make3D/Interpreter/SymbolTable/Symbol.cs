using System;

namespace ScriptLanguage
{
    public class Symbol
    {
        private bool booleanValue;
        private double doubleValue;
        private int handleValue;
        private int intValue;
        private String name;
        private int solidValue;
        private String stringValue;
        private SymbolTable.SymbolType symbolType;

        // Instance constructor
        public Symbol()
        {
            symbolType = 0;
            name = "";
            intValue = 0;
            doubleValue = 0;
            stringValue = "";
            booleanValue = false;
            solidValue = -1;
            handleValue = -1;
        }

        public bool BooleanValue
        {
            get { return booleanValue; }
            set { booleanValue = value; }
        }

        public double DoubleValue
        {
            get { return doubleValue; }
            set { doubleValue = value; }
        }

        public int HandleValue
        {
            get { return handleValue; }
            set { handleValue = value; }
        }

        public int IntValue
        {
            get { return intValue; }
            set { intValue = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public int SolidValue
        {
            get { return solidValue; }
            set { solidValue = value; }
        }

        public String StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }

        public SymbolTable.SymbolType SymbolType
        {
            get { return symbolType; }
            set { symbolType = value; }
        }

        public virtual void SetSize(int d)
        {
        }
    }
}