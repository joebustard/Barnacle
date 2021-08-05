using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class OrNode : ExpressionNode
    {
        private ExpressionNode _LeftNode;
        private ExpressionNode _RightNode;

        // Instance constructor
        public OrNode()
        {
            _LeftNode = null;
            _RightNode = null;
        }

        public ExpressionNode LeftNode
        {
            get { return _LeftNode; }
            set { _LeftNode = value; }
        }

        public ExpressionNode RightNode
        {
            get { return _RightNode; }
            set { _RightNode = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (_LeftNode != null)
            {
                if (_LeftNode.Execute())
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti.MyType == StackItem.ItemType.bval)
                    {
                        bool bLeft = sti.BooleanValue;
                        if (_RightNode != null)
                        {
                            if (_RightNode.Execute())
                            {
                                sti = ExecutionStack.Instance().Pull();
                                if (sti.MyType == StackItem.ItemType.bval)
                                {
                                    bool bRight = sti.BooleanValue;

                                    ExecutionStack.Instance().Push((bool)(bLeft || bRight));
                                    result = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Instance().AddEntry("Run Time Error || expected boolean");
                    }
                }
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = "";
            if (_LeftNode != null)
            {
                result = _LeftNode.ToRichText();
                result += RichTextFormatter.Operator(" || ");
                if (_RightNode != null)
                {
                    result += _RightNode.ToRichText();
                }
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (_LeftNode != null)
            {
                result = _LeftNode.ToString();
                result += " || ";
                if (_RightNode != null)
                {
                    result += _RightNode.ToString();
                }
            }
            return result;
        }
    }
}