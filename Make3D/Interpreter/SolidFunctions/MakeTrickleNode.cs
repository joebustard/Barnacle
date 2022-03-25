using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeTrickleNode : ExpressionNode
    {
        private ExpressionNode radiusExp;
        private ExpressionNode sideExp;
        private ExpressionNode thicknessExp;

        public MakeTrickleNode(ExpressionNode radius, ExpressionNode side, ExpressionNode thickness)
        {
            this.radiusExp = radius;
            this.sideExp = side;
            this.thicknessExp = thickness;
        }

        public MakeTrickleNode(ExpressionCollection coll)
        {
            this.radiusExp = coll.Get(0);
            this.sideExp = coll.Get(1);
            this.thicknessExp = coll.Get(2);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valRadius = 0; double valSide = 0; double valThickness = 0;

            if (EvalExpression(radiusExp, ref valRadius, "Radius", "MakeTrickle") &&
                EvalExpression(sideExp, ref valSide, "Side", "MakeTrickle") &&
                EvalExpression(thicknessExp, ref valThickness, "Thickness", "MakeTrickle"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valRadius < 1 || valRadius > 100)
                {
                    Log.Instance().AddEntry("MakeTrickle : Radius value out of range (1..100)");
                    inRange = false;
                }

                if (valSide < 1 || valSide > 200)
                {
                    Log.Instance().AddEntry("MakeTrickle : Side value out of range (1..200)");
                    inRange = false;
                }

                if (valThickness < 0.1 || valThickness > 100)
                {
                    Log.Instance().AddEntry("MakeTrickle : Thickness value out of range (0.1..100)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Trickle";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    TrickleMaker maker = new TrickleMaker(valRadius, valSide, valThickness);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeTrickle : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeTrickle") + "( ";

            result += radiusExp.ToRichText() + ", ";
            result += sideExp.ToRichText() + ", ";
            result += thicknessExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeTrickle( ";

            result += radiusExp.ToString() + ", ";
            result += sideExp.ToString() + ", ";
            result += thicknessExp.ToString();
            result += " )";
            return result;
        }
    }
}