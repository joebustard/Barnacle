using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeCurvedFunnelNode : ExpressionNode
    {
        private ExpressionNode factorAExp;
        private ExpressionNode heightExp;
        private ExpressionNode radiusExp;
        private ExpressionNode wallThicknessExp;

        public MakeCurvedFunnelNode
            (
            ExpressionNode radius, ExpressionNode factorA, ExpressionNode wallThickness, ExpressionNode height
            )
        {
            this.radiusExp = radius;
            this.factorAExp = factorA;
            this.wallThicknessExp = wallThickness;
            this.heightExp = height;
        }

        public MakeCurvedFunnelNode
                (ExpressionCollection coll)
        {
            this.radiusExp = coll.Get(0);
            this.factorAExp = coll.Get(1);
            this.wallThicknessExp = coll.Get(2);
            this.heightExp = coll.Get(3);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valRadius = 0; double valFactorA = 0; double valWallThickness = 0; double valHeight = 0;

            if (
                       EvalExpression(radiusExp, ref valRadius, "Radius", "MakeCurvedFunnel") &&
                       EvalExpression(factorAExp, ref valFactorA, "FactorA", "MakeCurvedFunnel") &&
                       EvalExpression(wallThicknessExp, ref valWallThickness, "WallThickness", "MakeCurvedFunnel") &&
                       EvalExpression(heightExp, ref valHeight, "Height", "MakeCurvedFunnel"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valRadius < 1 || valRadius > 100)
                {
                    Log.Instance().AddEntry("MakeCurvedFunnel : Radius value out of range (1..100)");
                    inRange = false;
                }

                if (valFactorA < 0.1 || valFactorA > 10)
                {
                    Log.Instance().AddEntry("MakeCurvedFunnel : FactorA value out of range (0.1..10)");
                    inRange = false;
                }

                if (valWallThickness < 1 || valWallThickness > 10)
                {
                    Log.Instance().AddEntry("MakeCurvedFunnel : WallThickness value out of range (1..10)");
                    inRange = false;
                }

                if (valHeight < 5 || valHeight > 100)
                {
                    Log.Instance().AddEntry("MakeCurvedFunnel : Height value out of range (5..100)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "CurvedFunnel";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    CurvedFunnelMaker maker = new CurvedFunnelMaker(valRadius, valFactorA, valWallThickness, valHeight);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    obj.CalculateAbsoluteBounds();
                    int id = Script.NextObjectId;
                    Script.ResultArtefacts[id] = obj;
                    ExecutionStack.Instance().PushSolid(id);
                }
                else
                {
                    Log.Instance().AddEntry("MakeCurvedFunnel : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeCurvedFunnel") + "( ";

            result += radiusExp.ToRichText() + ", ";
            result += factorAExp.ToRichText() + ", ";
            result += wallThicknessExp.ToRichText() + ", ";
            result += heightExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeCurvedFunnel( ";

            result += radiusExp.ToString() + ", ";
            result += factorAExp.ToString() + ", ";
            result += wallThicknessExp.ToString() + ", ";
            result += heightExp.ToString();
            result += " )";
            return result;
        }
    }
}