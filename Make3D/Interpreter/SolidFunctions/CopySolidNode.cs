﻿using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class CopySolidNode : ExpressionNode
    {
        private ExpressionNode solid;

        public CopySolidNode()
        {
        }

        public CopySolidNode(ExpressionNode ls) : base(ls)
        {
            this.solid = ls;

            
        }

 

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            int ls = -1;

            if (EvalExpression(solid, ref ls))
            {
                Object3D src = Script.ResultArtefacts[ls];

                if (src != null)
                {
                    src.CalcScale(false);
                    Object3D clone = src.Clone();
                    clone.CalcScale(false);
                    clone.Remesh();
                    
                    Script.ResultArtefacts.Add(clone);
                    ExecutionStack.Instance().PushSolid(Script.ResultArtefacts.Count-1);
                    result = true;
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
            String result = RichTextFormatter.KeyWord("Copy") + "( ";

            result += solid.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Copy( ";
            result += solid.ToString();
            result += " )";
            return result;
        }

        private bool EvalExpression(ExpressionNode exp, ref int x)
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.ival)
                    {
                        x = sti.IntValue;
                        result = true;
                    }
                    if (sti.MyType == StackItem.ItemType.sldval)
                    {
                        x = sti.SolidValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry(" Copy :  expression error");
            }
            return result;
        }

      
    }
}