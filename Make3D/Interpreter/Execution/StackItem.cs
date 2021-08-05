using System;

namespace ScriptLanguage
{
    public class StackItem
    {
        private bool _BooleanValue;
        private double _DoubleValue;
        private int _HandleValue;
        private int _IntValue;
        private ItemType _myType;
        private String _StringValue;
        private int solidValue;

        public enum ItemType
        {
            noval,
            ival,
            dval,
            sval,
            bval,
            hval,
            tval,
            sldval
        }

        public bool BooleanValue
        {
            get { return _BooleanValue; }
            set { _BooleanValue = value; }
        }

        public double DoubleValue
        {
            get { return _DoubleValue; }
            set { _DoubleValue = value; }
        }

        public int HandleValue
        {
            get { return _HandleValue; }
            set { _HandleValue = value; }
        }

        public int IntValue
        {
            get { return _IntValue; }
            set { _IntValue = value; }
        }

        public ItemType MyType
        {
            get { return _myType; }
            set { _myType = value; }
        }

        public int SolidValue
        {
            get { return solidValue; }
            set { solidValue = value; }
        }

        public String StringValue
        {
            get { return _StringValue; }
            set { _StringValue = value; }
        }

        // Instance constructor
        public StackItem()
        {
            _IntValue = 0;
            _DoubleValue = 0.0;
            _StringValue = "";
            _BooleanValue = false;
            _HandleValue = 0;
            solidValue = -1;
            _myType = ItemType.noval;
        }
    }
}