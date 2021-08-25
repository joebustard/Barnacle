using System;

namespace ScriptLanguage
{
    public class StackItem
    {
        private bool booleanValue;
        private double doubleValue;
        private int handleValue;
        private int intValue;
        private ItemType myType;
        private object objectValue;
        private int solidValue;
        private String stringValue;

        // Instance constructor
        public StackItem()
        {
            intValue = 0;
            doubleValue = 0.0;
            stringValue = "";
            booleanValue = false;
            handleValue = 0;
            solidValue = -1;
            objectValue = null;
            myType = ItemType.noval;
        }

        public enum ItemType
        {
            noval,
            ival,
            dval,
            sval,
            bval,
            hval,
            tval,
            sldval,
            arrayval
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

        public ItemType MyType
        {
            get { return myType; }
            set { myType = value; }
        }

        public object ObjectValue
        {
            get { return objectValue; }
            set { objectValue = value; }
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
    }
}