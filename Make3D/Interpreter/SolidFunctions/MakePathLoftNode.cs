using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakePathLoftNode : ExpressionNode
    {
        private ExpressionNode loftHeightExp;
        private ExpressionNode loftThicknessExp;

        public MakePathLoftNode
            (
            ExpressionNode loftHeight,
            ExpressionNode loftThickness
            )
        {
            this.loftHeightExp = loftHeight;
            this.loftHeightExp = loftThickness;
        }

        public MakePathLoftNode
                (ExpressionCollection coll)
        {
            this.loftHeightExp = coll.Get(0);
            this.loftThicknessExp = coll.Get(1);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valLoftHeight = 0;
            double valLoftThickness = 0;

            if (EvalExpression(loftHeightExp, ref valLoftHeight, "LoftHeight", "MakePathLoft") &&
                 EvalExpression(loftThicknessExp, ref valLoftThickness, "LoftThickness", "MakePathLoft"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valLoftHeight < 0.1 || valLoftHeight > 200)
                {
                    Log.Instance().AddEntry("MakePathLoft : LoftHeight value out of range (0.1..200)");
                    inRange = false;
                }

                if (valLoftThickness < 0.1 || valLoftThickness > 200)
                {
                    Log.Instance().AddEntry("MakePathLoft : LoftThickness value out of range (0.1..200)");
                    inRange = false;
                }
                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "PathLoft";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    PathLoftMaker maker = new PathLoftMaker(valLoftHeight, valLoftThickness);

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
                    Log.Instance().AddEntry("MakePathLoft : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakePathLoft") + "( ";

            result += loftHeightExp.ToRichText();
            result += ", ";
            result += loftHeightExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakePathLoft( ";

            result += loftHeightExp.ToString();
            result += ", ";
            result += loftHeightExp.ToString();
            result += " )";
            return result;
        }
    }
}