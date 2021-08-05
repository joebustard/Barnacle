using System;

namespace ScriptLanguage
{
    public class Symbol
    {
        private SymbolTable.SymbolType symbolType;

        private String name;

        public SymbolTable.SymbolType SymbolType
        {
            get { return symbolType; }
            set { symbolType = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        private int intValue;

        private double doubleValue;

        private String stringValue;

        private bool booleanValue;

        private int handleValue;
        private int solidValue;

        public int IntValue
        {
            get { return intValue; }
            set { intValue = value; }
        }

        public double DoubleValue
        {
            get { return doubleValue; }
            set { doubleValue = value; }
        }

        public String StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }

        public bool BooleanValue
        {
            get { return booleanValue; }
            set { booleanValue = value; }
        }

        public int HandleValue
        {
            get { return handleValue; }
            set { handleValue = value; }
        }

        public int SolidValue
        {
            get { return solidValue; }
            set { solidValue = value; }
        }
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

    }
}