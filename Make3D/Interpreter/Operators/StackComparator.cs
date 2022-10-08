using System;

namespace ScriptLanguage
{
    internal class StackComparator
    {
        public static bool Compare(StackItem it1, StackItem it2, out int result)
        {
            bool valid = false;
            result = 0;
            if (it1 != null && it2 != null)
            {
                if (it1.MyType == it2.MyType)
                {
                    valid = true;
                    CompareSameType(it1, it2, out result);
                }
                else
                {
                    valid = CompareDifferentType(it1, it2, out result);
                }
            }
            return valid;
        }

        private static bool CompareDifferentType(StackItem it1, StackItem it2, out int result)
        {
            bool valid = false;
            result = 0;

            switch (it1.MyType)
            {
                case StackItem.ItemType.dval:
                    {
                        // can compare against int
                        if (it2.MyType == StackItem.ItemType.ival)
                        {
                            valid = true;
                            double v1 = it1.DoubleValue;
                            double v2 = (double)it2.IntValue;
                            if (v1 == v2)
                            {
                                result = 0;
                            }
                            else
                            {
                                if (v1 < v2)
                                {
                                    result = -1;
                                }
                                else
                                {
                                    result = 1;
                                }
                            }
                        }
                    }
                    break;

                case StackItem.ItemType.ival:
                    {
                        // can compare against double or solid
                        if (it2.MyType == StackItem.ItemType.dval)
                        {
                            valid = true;
                            double v1 = (double)it1.IntValue;
                            double v2 = it2.DoubleValue;
                            if (v1 == v2)
                            {
                                result = 0;
                            }
                            else
                            {
                                if (v1 < v2)
                                {
                                    result = -1;
                                }
                                else
                                {
                                    result = 1;
                                }
                            }
                        }

                        if (it2.MyType == StackItem.ItemType.sldval)
                        {
                            valid = true;
                            int v1 = it1.IntValue;
                            int v2 = it2.SolidValue;
                            if (v1 == v2)
                            {
                                result = 0;
                            }
                            else
                            {
                                if (v1 < v2)
                                {
                                    result = -1;
                                }
                                else
                                {
                                    result = 1;
                                }
                            }
                        }
                    }
                    break;

                case StackItem.ItemType.sldval:
                    {
                        // can compare against int
                        if (it2.MyType == StackItem.ItemType.ival)
                        {
                            valid = true;
                            int v1 = it1.SolidValue;
                            int v2 = it2.IntValue;
                            if (v1 == v2)
                            {
                                result = 0;
                            }
                            else
                            {
                                if (v1 < v2)
                                {
                                    result = -1;
                                }
                                else
                                {
                                    result = 1;
                                }
                            }
                        }
                    }
                    break;
            }

            return valid;
        }

        private static void CompareSameType(StackItem it1, StackItem it2, out int result)
        {
            result = 0;
            switch (it1.MyType)
            {
                case StackItem.ItemType.bval:
                    {
                        if (it1.BooleanValue == it2.BooleanValue)
                        {
                            result = 0;
                        }
                        else
                        {
                            result = -1;
                        }
                    }
                    break;

                case StackItem.ItemType.dval:
                    {
                        if (it1.DoubleValue == it2.DoubleValue)
                        {
                            result = 0;
                        }
                        else
                        {
                            if (it1.DoubleValue < it2.DoubleValue)
                            {
                                result = -1;
                            }
                            else
                            {
                                result = 1;
                            }
                        }
                    }
                    break;

                case StackItem.ItemType.hval:
                    {
                        if (it1.HandleValue == it2.HandleValue)
                        {
                            result = 0;
                        }
                        else
                        {
                            if (it1.HandleValue < it2.HandleValue)
                            {
                                result = -1;
                            }
                            else
                            {
                                result = 1;
                            }
                        }
                    }
                    break;

                case StackItem.ItemType.ival:
                    {
                        if (it1.IntValue == it2.IntValue)
                        {
                            result = 0;
                        }
                        else
                        {
                            if (it1.IntValue < it2.IntValue)
                            {
                                result = -1;
                            }
                            else
                            {
                                result = 1;
                            }
                        }
                    }
                    break;

                case StackItem.ItemType.sldval:
                    {
                        if (it1.SolidValue == it2.SolidValue)
                        {
                            result = 0;
                        }
                        else
                        {
                            if (it1.SolidValue < it2.SolidValue)
                            {
                                result = -1;
                            }
                            else
                            {
                                result = 1;
                            }
                        }
                    }
                    break;

                case StackItem.ItemType.sval:
                    {
                        result = String.Compare(it1.StringValue, it2.StringValue);
                    }
                    break;
            }
        }
    }
}