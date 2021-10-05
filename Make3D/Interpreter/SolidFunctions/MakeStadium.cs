using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeStadiumNode : ExpressionNode
    {
        private ExpressionNode gapExp;
        private ExpressionNode heightExp;
        private ExpressionNode radius1Exp;
        private ExpressionNode radius2Exp;
        private ExpressionNode shapeExp;

        public MakeStadiumNode()
        {
        }

        public MakeStadiumNode(ExpressionNode sh, ExpressionNode r1, ExpressionNode r2, ExpressionNode g, ExpressionNode h) : base(sh)
        {
            this.shapeExp = sh;
            this.radius1Exp = r1;
            this.gapExp = g;

            this.radius2Exp = r2;
            this.heightExp = h;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (shapeExp != null)
            {
                result = shapeExp.Execute();
                if (result)
                {
                    result = false;
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            string shape = sti.StringValue;
                            double r1 = 0;
                            double r2 = 0;
                            double g = 0;
                            double h = 0;

                            if (EvalExpression(radius1Exp, ref r1, "Radius1") &&
                                EvalExpression(radius2Exp, ref r2, "Radius2") &&
                                EvalExpression(heightExp, ref h, "Height") &&

                                EvalExpression(gapExp, ref g, "Gap")
                                )
                            {
                                if (r1 > 0 && r2 > 0 && g > 0 && h > 0 && (shape.ToLower() == "flat" || (shape.ToLower() == "sausage")))
                                {
                                    result = true;

                                    Object3D obj = new Object3D();

                                    obj.Name = "Stadium";
                                    obj.PrimType = "Mesh";
                                    obj.Scale = new Scale3D(20, 20, 20);

                                    obj.Position = new Point3D(0, 0, 0);
                                    StadiumMaker stadiumMaker = new StadiumMaker(shape, r1, r2, g, h);
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
                        }
                        else
                        {
                            Log.Instance().AddEntry("MakeStadium : flat or sausage string expected ");
                        }
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
            String result = RichTextFormatter.KeyWord("MakeStadium") + "( ";
            result += shapeExp.ToRichText() + ", ";
            result += radius1Exp.ToRichText() + ", ";
            result += radius2Exp.ToRichText() + ", ";
            result += gapExp.ToRichText() + ", ";
            result += heightExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeStadium( ";
            result += shapeExp.ToString() + ", ";
            result += radius1Exp.ToString() + ", ";
            result += radius2Exp.ToString() + ", ";
            result += gapExp.ToString() + ", ";
            result += heightExp.ToString();
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
                Log.Instance().AddEntry("MakeStadium : " + v + " expression error");
            }
            return result;
        }
    }
}