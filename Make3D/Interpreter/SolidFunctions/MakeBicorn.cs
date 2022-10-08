using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeBicornNode : ExpressionNode
    {
        private ExpressionNode doubleupExp;
        private ExpressionNode heightExp;
        private ExpressionNode radius1Exp;
        private ExpressionNode radius2Exp;

        public MakeBicornNode()
        {
        }

        public MakeBicornNode(ExpressionNode r1, ExpressionNode r2, ExpressionNode h, ExpressionNode g) : base(r1)
        {
            this.radius1Exp = r1;
            this.doubleupExp = g;

            this.radius2Exp = r2;
            this.heightExp = h;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                double r1 = 0;
                double r2 = 0;
                bool g = false;
                double h = 0;

                if (EvalExpression(radius1Exp, ref r1, "Radius1") &&
                    EvalExpression(radius2Exp, ref r2, "Radius2") &&
                    EvalExpression(heightExp, ref h, "Height") &&
                    EvalExpression(doubleupExp, ref g, "DoubleUp")
                    )
                {
                    if (r1 > 0 && r2 > 0 && h > 0)
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "Bicorn";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        Point3DCollection tmp = new Point3DCollection();
                        BicornMaker BicornMaker = new BicornMaker(r1, r2, h, g);

                        BicornMaker.Generate(tmp, obj.TriangleIndices);
                        PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                        obj.CalcScale(false);
                        obj.Remesh();
                        Script.ResultArtefacts.Add(obj);
                        ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                    }
                    else
                    {
                        Log.Instance().AddEntry("MakeBicorn : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeBicorn : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeBicorn") + "( ";

            result += radius1Exp.ToRichText() + ", ";
            result += radius2Exp.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += doubleupExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeBicorn( ";

            result += radius1Exp.ToString() + ", ";
            result += radius2Exp.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += doubleupExp.ToString();
            result += " )";
            return result;
        }
    }
}