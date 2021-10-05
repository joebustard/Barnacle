using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeHollowNode : ExpressionNode
    {
        private ExpressionNode heightExp;
        private string label = "MakeHollow";
        private ExpressionNode pointArrayExp;
        private ExpressionNode wallThicknessExp;

        public MakeHollowNode()
        {
        }

        public MakeHollowNode(ExpressionNode p, ExpressionNode h, ExpressionNode wth) : base(h)
        {
            this.pointArrayExp = p;

            this.heightExp = h;
            this.wallThicknessExp = wth;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            double[] coords = null;
            double h = 0;
            double wt = 0;
            if (EvalExpression(heightExp, ref h, "Height") &&
                EvalExpression(wallThicknessExp, ref wt, "WallThickness") &&
                 EvalExpression(pointArrayExp, ref coords, "PointArray"))
            {
                if (coords != null && coords.GetLength(0) >= 6 && h > 0)
                {
                    result = true;
                    List<System.Windows.Point> points = new List<System.Windows.Point>();

                    for (int i = 0; i < coords.GetLength(0); i += 2)
                    {
                        points.Add(new System.Windows.Point(coords[i], coords[i + 1]));
                    }
                    Object3D obj = new Object3D();

                    obj.Name = "Platelet";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    HollowPolyMaker maker = new HollowPolyMaker(points, h, wt);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);
                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry($"{label} : Illegal value");
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
            String result = RichTextFormatter.KeyWord($"{label}") + "( ";
            result += pointArrayExp.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += wallThicknessExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";
            result += pointArrayExp.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += wallThicknessExp.ToString();
            result += " )";
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
                Log.Instance().AddEntry($"{label} : " + v + " expression error");
            }
            return result;
        }

        private bool EvalExpression(ExpressionNode exp, ref double[] x, string v)
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.arrayval)
                    {
                        if ((sti.ObjectValue as ArraySymbol).SymbolType == SymbolTable.SymbolType.doublearrayvariable)
                        {
                            object[] tmp = (sti.ObjectValue as ArraySymbol).Array.GetAll();
                            x = new double[tmp.Length];
                            for (int i = 0; i < tmp.Length; i++)
                            {
                                x[i] = (double)tmp[i];
                            }
                            result = true;
                        }
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry("MakeTube : " + v + " expression error");
            }
            return result;
        }
    }
}