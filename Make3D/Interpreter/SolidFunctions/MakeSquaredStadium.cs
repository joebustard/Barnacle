﻿using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeSquaredStadiumNode : ExpressionNode
    {
        private ExpressionNode gapExp;
        private ExpressionNode heightExp;
        private ExpressionNode radiusExp;
        private ExpressionNode endLength;
        private ExpressionNode overRunExp;

        public MakeSquaredStadiumNode()
        {
        }

        public MakeSquaredStadiumNode(ExpressionNode r1, ExpressionNode gp, ExpressionNode es, ExpressionNode h, ExpressionNode ov) : base(r1)
        {
            this.overRunExp = ov;
            this.radiusExp = r1;
            this.gapExp = gp;

            this.endLength = es;
            this.heightExp = h;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double ov = 0;
            double r = 0;
            double el = 0;
            double g = 0;
            double h = 0;

            if (EvalExpression(radiusExp, ref r, "Radius") &&
                EvalExpression(endLength, ref el, "EndSize") &&
                EvalExpression(heightExp, ref h, "Height") &&
                EvalExpression(overRunExp, ref ov, "OverRun") &&
                EvalExpression(gapExp, ref g, "Gap")
                )
            {
                if (r > 0 && h > 0 && el > 0 && g >= 0 && ov >= 0)
                { 
                                
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "SquaredStadium";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    SquaredStadiumMaker stadiumMaker = new SquaredStadiumMaker(r,g,el,h,ov);
                    Point3DCollection tmp = new Point3DCollection(); ;
                    stadiumMaker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);
                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeStadium : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeSquaredStadium") + "( ";
            result += radiusExp.ToRichText() + ", ";
            result += gapExp.ToRichText() + ", ";
            result += endLength.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += overRunExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeSquaredStadium( ";
            result += radiusExp.ToString() + ", ";
            result += gapExp.ToString() + ", ";
            result += endLength.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += overRunExp.ToString();
            result += " )";
            return result;
        }

        private bool EvalExpression(ExpressionNode exp, ref double x, string v)
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
                    else if (sti.MyType == StackItem.ItemType.dval)
                    {
                        x = sti.DoubleValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry("MakeSquaredStadium : " + v + " expression error");
            }
            return result;
        }
    }
}