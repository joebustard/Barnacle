using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeImagePlaqueNode : ExpressionNode
    {
        private ExpressionNode plagueThicknessExp;
        private ExpressionNode plaqueImagePathExp;

        public MakeImagePlaqueNode(ExpressionNode plagueThickness, ExpressionNode plaqueImagePath
            )
        {
            this.plagueThicknessExp = plagueThickness;
            this.plaqueImagePathExp = plaqueImagePath;
        }

        public MakeImagePlaqueNode
                (ExpressionCollection coll)
        {
            this.plagueThicknessExp = coll.Get(0);
            this.plaqueImagePathExp = coll.Get(1);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valPlagueThickness = 0; string valPlaqueImagePath = "";

            if (
               EvalExpression(plagueThicknessExp, ref valPlagueThickness, "PlagueThickness", "MakeImagePlaque") &&
               EvalExpression(plaqueImagePathExp, ref valPlaqueImagePath, "PlaqueImagePath", "MakeImagePlaque"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valPlagueThickness < 0.5 || valPlagueThickness > 100)
                {
                    Log.Instance().AddEntry("MakeImagePlaque : PlagueThickness value out of range (0.5..100)");
                    inRange = false;
                }

                if (String.IsNullOrEmpty(valPlaqueImagePath))
                {
                    Log.Instance().AddEntry("MakeImagePlaque : PlaqueImagePath path must be set");

                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "ImagePlaque";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    ImagePlaqueMaker maker = new ImagePlaqueMaker(valPlagueThickness, valPlaqueImagePath, false, 0);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    int id = Script.NextObjectId;
                    Script.ResultArtefacts[id] = obj;
                    ExecutionStack.Instance().PushSolid(id);
                }
                else
                {
                    Log.Instance().AddEntry("MakeImagePlaque : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeImagePlaque") + "( ";

            result += plagueThicknessExp.ToRichText() + ", ";
            result += plaqueImagePathExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeImagePlaque( ";

            result += plagueThicknessExp.ToString() + ", ";
            result += plaqueImagePathExp.ToString();
            result += " )";
            return result;
        }
    }
}