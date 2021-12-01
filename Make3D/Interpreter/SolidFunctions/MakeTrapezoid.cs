using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeTrapezoidNode : ExpressionNode
    {
        private ExpressionNode bevelExp;
        private ExpressionNode bottomLengthExp;
        private ExpressionNode heightExp;
        private ExpressionNode topLengthExp;
        private ExpressionNode widthExp;

        public MakeTrapezoidNode()
        {
        }

        public MakeTrapezoidNode(ExpressionNode lt, ExpressionNode lb, ExpressionNode h, ExpressionNode w, ExpressionNode bev) : base(lt)
        {
            this.topLengthExp = lt;
            this.heightExp = h;
            this.widthExp = w;
            this.bottomLengthExp = lb;
            this.bevelExp = bev;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double lt = 0;
            double h = 0;

            double w = 0;
            double lb = 0;
            double b = 0;

            if (EvalExpression(bottomLengthExp, ref lb, "BottomLength", "MakeTrapezoid") &&
                EvalExpression(bevelExp, ref b, "Bevel", "MakeTrapezoid") &&
                EvalExpression(heightExp, ref h, "Height", "MakeTrapezoid") &&
                EvalExpression(widthExp, ref w, "Width", "MakeTrapezoid") &&
                EvalExpression(topLengthExp, ref lt, "TopLength", "MakeTrapezoid")
                )
            {
                if (lt > 0 && w > 0 && h > 0 && lb > 0 && b >= 0)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Trapezoid";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    TrapezoidMaker maker = new TrapezoidMaker(lt, lb, h, w, b);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeTrapezoid : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeTrapezoid") + "( ";
            result += topLengthExp.ToRichText() + ", ";
            result += bottomLengthExp.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += widthExp.ToRichText() + ", ";

            result += bevelExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeTrapezoid( ";

            result += topLengthExp.ToString() + ", ";
            result += bottomLengthExp.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += widthExp.ToString() + ", ";

            result += bevelExp.ToString();
            result += " )";
            return result;
        }
    }
}