using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeSquirkleNode : ExpressionNode
    {
        private ExpressionNode lengthExp;
        private ExpressionNode heightExp;
        private ExpressionNode depthExp;

        private ExpressionNode tlExp;
        private ExpressionNode trExp;
        private ExpressionNode blExp;
        private ExpressionNode brExp;

        public MakeSquirkleNode()
        {
        }

        public MakeSquirkleNode(ExpressionNode tl, ExpressionNode tr, ExpressionNode bl, ExpressionNode br, ExpressionNode l, ExpressionNode h, ExpressionNode d) : base(tl)
        {
            this.tlExp = tl;
            this.trExp = tr;
            this.blExp = bl;
            this.brExp = br;

            this.lengthExp = l;

            this.heightExp = h;
            this.depthExp = d;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            int tlc = 0;
            int trc = 0;
            int blc = 0;
            int brc = 0;
            double length = 0.0;
            double height = 0;
            double depth = 0;

            if (EvalExpression(tlExp, ref tlc, "Topleft") &&
                EvalExpression(trExp, ref trc, "Topright") &&
                EvalExpression(blExp, ref blc, "Bottomleft") &&
                EvalExpression(brExp, ref brc, "BottomRight") &&

                EvalExpression(heightExp, ref length, "Length") &&
                EvalExpression(lengthExp, ref height, "Height") &&
                EvalExpression(depthExp, ref depth, "Width")
                )
            {
                if ( CheckCode(tlc, "Topleft") &&
                   CheckCode(trc, "Topright") &&
                   CheckCode(blc, "Bottomleft") &&
                   CheckCode(brc, "BottomRight")  && length > 0 && height > 0 && depth > 0)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Squirkle";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    SquirkleMaker maker = new SquirkleMaker(tlc, trc, blc, brc,  length, height, depth);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeSquirkle : Illegal value");
                }
            }

            return result;
        }

        private bool CheckCode(int c, string v)
        {
            bool res = true;
            if ( c<0 || c>2)
            {
                res = false;
                Log.Instance().AddEntry("MakeSquirkle : Illegal corner value :"+v+" should be 0,1 or 2");
            }
            return res;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeSquirkle") + "( ";

            result += tlExp.ToRichText() + ", ";
            result += trExp.ToRichText() + ", ";
            result += blExp.ToRichText() + ", ";
            result += brExp.ToRichText() + ", ";
            result += lengthExp.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += depthExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeSquirkle( ";

            result += tlExp.ToString() + ", ";
            result += trExp.ToString() + ", ";
            result += blExp.ToString() + ", ";
            result += brExp.ToString() + ", ";
            result += lengthExp.ToString() + ", ";
            result += heightExp.ToString()+", ";
            result += depthExp.ToString();
            result += " )";
            return result;
        }

        private bool EvalExpression(ExpressionNode exp, ref int x, string v)
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
                        x = (int) sti.DoubleValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry("MakeSquirkle : " + v + " expression error");
            }
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
                Log.Instance().AddEntry("MakeSquirkle : " + v + " expression error");
            }
            return result;
        }

        private bool EvalExpression(ExpressionNode exp, ref bool x, string v)
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.bval)
                    {
                        x = sti.BooleanValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry("MakeSquirkle : " + v + " expression error");
            }
            return result;
        }
    }
}