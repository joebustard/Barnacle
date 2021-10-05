using Barnacle.LineLib;
using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakePathNode : ExpressionNode
    {
        private ExpressionNode heightExp;
        private string label = "MakePath";
        private ExpressionNode pathTextExp;

        public MakePathNode()
        {
        }

        public MakePathNode(ExpressionNode p, ExpressionNode h) : base(h)
        {
            this.pathTextExp = p;

            this.heightExp = h;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            string pathText = "";
            double h = 0;

            if (EvalExpression(heightExp, ref h, "Height") &&
                 EvalExpression(pathTextExp, ref pathText, "PathText"))
            {
                if (pathText != "")
                {
                    FlexiPath flexiPath = new FlexiPath();
                    if (flexiPath.FromTextPath(pathText))
                    {
                        List<System.Windows.Point> points = flexiPath.DisplayPoints();

                        if (points.Count >= 3 && h > 0)
                        {
                            result = true;

                            Object3D obj = new Object3D();

                            obj.Name = "Platelet";
                            obj.PrimType = "Mesh";
                            obj.Scale = new Scale3D(20, 20, 20);

                            obj.Position = new Point3D(0, 0, 0);
                            PolyMaker maker = new PolyMaker(points, h);
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
                            Log.Instance().AddEntry($"{label} : Not enough points to create object");
                        }
                    }
                    else
                    {
                        Log.Instance().AddEntry($"{label} : Illegal value");
                    }
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
            result += pathTextExp.ToRichText() + ", ";
            result += heightExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";
            result += pathTextExp.ToString() + ", ";
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
                Log.Instance().AddEntry($"{label} : " + v + " expression error");
            }
            return result;
        }

        private bool EvalExpression(ExpressionNode exp, ref string x, string v)
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.sval)
                    {
                        x = sti.StringValue;
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
    }
}