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

        public int Length
        {
            get
            {
                if (elements == null)
                {
                    return 0;
                }
                else
                {
                    return elements.Length;
                }
            }
        }

        public bool SetSize(int numElements)
        {
            bool res = true;
            elements = new object[numElements];
            return res;
        }

        internal object Get(int arrayIndex)
        {
            return elements[arrayIndex];
        }

        internal object[] GetAll()
        {
            return elements;
        }

        internal void Set(int arrayIndex, object obj)
        {
            elements[arrayIndex] = obj;
        }
    }
}