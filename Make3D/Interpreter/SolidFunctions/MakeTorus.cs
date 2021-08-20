using Make3D.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeTorusNode : ExpressionNode
    {
        private ExpressionNode curveExp;
        private ExpressionNode heightExp;
        private ExpressionNode knobExp;
        private ExpressionNode mainRadiusExp;
        private ExpressionNode ringRadiusExp;

        //private ExpressionNode thicknessExp;
        private ExpressionNode vRadiusExp;

        public MakeTorusNode()
        {
        }

        public MakeTorusNode(ExpressionNode mr, ExpressionNode hr, ExpressionNode vr, ExpressionNode curve, ExpressionNode k, ExpressionNode h) : base(mr)
        {
            this.mainRadiusExp = mr;
            this.ringRadiusExp = hr;
            this.vRadiusExp = vr;

            this.curveExp = curve;
            this.knobExp = k;
            this.heightExp = h;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double mr = 0;
            double hr = 0;
            double vr = 0;
            int curve = 0;
            double knobs = 0;
            double h = 0;

            if (EvalExpression(mainRadiusExp, ref mr, "Radius") &&
                EvalExpression(ringRadiusExp, ref hr, "RingRadius") &&
                EvalExpression(vRadiusExp, ref vr, "VerticalRadius") &&
                 EvalExpression(curveExp, ref curve, "Curve") &&
                  EvalExpression(knobExp, ref knobs, "Knobs") &&
                  EvalExpression(heightExp, ref h, "Height")
                )
            {
                if (mr > 0 && h > 0 && hr >= 0 && vr >= 0 && curve >= 0 && curve < 4)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Torus";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    TorusMaker maker = new TorusMaker(mr, hr, vr, curve, knobs, h);
                    maker.Generate(obj.RelativeObjectVertices, obj.TriangleIndices);
                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeTorus : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeTorus") + "( ";

            result += mainRadiusExp.ToRichText() + ", ";
            result += ringRadiusExp.ToRichText() + ", ";
            result += vRadiusExp.ToRichText() + ", ";
            result += curveExp.ToRichText() + ", ";
            result += knobExp.ToRichText() + ", ";
            result += heightExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeTube( ";
            result += mainRadiusExp.ToString() + ", ";
            result += ringRadiusExp.ToString() + ", ";
            result += vRadiusExp.ToString() + ", ";
            result += curveExp.ToString() + ", ";
            result += knobExp.ToString() + ", ";
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
                Log.Instance().AddEntry("MakeTorus : " + v + " expression error");
            }
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
                        x = (int)sti.DoubleValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry("MakeTorus : " + v + " expression error");
            }
            return result;
        }
    }
}