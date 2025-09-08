using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeTexturedDiskNode : ExpressionNode
    {
        private ExpressionNode radiusExp;
        private ExpressionNode sweepExp;
        private ExpressionNode textureDepthExp;
        private ExpressionNode textureExp;
        private ExpressionNode textureResolutionExp;
        private ExpressionNode tubeHeightExp;

        public MakeTexturedDiskNode
            (
            ExpressionNode tubeHeight, ExpressionNode radius, ExpressionNode solid, ExpressionNode sweep, ExpressionNode texture, ExpressionNode textureDepth, ExpressionNode textureResolution
            )
        {
            this.tubeHeightExp = tubeHeight;
            this.radiusExp = radius;

            this.sweepExp = sweep;
            this.textureExp = texture;
            this.textureDepthExp = textureDepth;
            this.textureResolutionExp = textureResolution;
        }

        public MakeTexturedDiskNode
                (ExpressionCollection coll)
        {
            this.tubeHeightExp = coll.Get(0);
            this.radiusExp = coll.Get(1);

            this.sweepExp = coll.Get(2);
            this.textureExp = coll.Get(3);
            this.textureDepthExp = coll.Get(4);
            this.textureResolutionExp = coll.Get(5);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valTubeHeight = 0;
            double valRadius = 0;
            double valSweep = 0;
            string valTexture = "";
            double valTextureDepth = 0;
            double valTextureResolution = 0;

            if (
               EvalExpression(tubeHeightExp, ref valTubeHeight, "Height", "MakeTexturedDisk") &&
               EvalExpression(radiusExp, ref valRadius, "Radius", "MakeTexturedDisk") &&
               EvalExpression(sweepExp, ref valSweep, "Sweep", "MakeTexturedDisk") &&
               EvalExpression(textureExp, ref valTexture, "Texture", "MakeTexturedDisk") &&
               EvalExpression(textureDepthExp, ref valTextureDepth, "TextureDepth", "MakeTexturedDisk") &&
               EvalExpression(textureResolutionExp, ref valTextureResolution, "TextureResolution", "MakeTexturedDisk"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valTubeHeight < 1 || valTubeHeight > 200)
                {
                    Log.Instance().AddEntry("MakeTexturedDisk : TubeHeight value out of range (1..200)");
                    inRange = false;
                }

                if (valRadius < 1 || valRadius > 200)
                {
                    Log.Instance().AddEntry("MakeTexturedDisk : Radius value out of range (1..200)");
                    inRange = false;
                }

                if (valSweep < 1 || valSweep > 360)
                {
                    Log.Instance().AddEntry("MakeTexturedDisk : Sweep value out of range (1..360)");
                    inRange = false;
                }

                if (valTextureDepth < 0.1 || valTextureDepth > 10)
                {
                    Log.Instance().AddEntry("MakeTexturedDisk : TextureDepth value out of range (0.1..10)");
                    inRange = false;
                }

                if (valTextureResolution < 0.1 || valTextureResolution > 1)
                {
                    Log.Instance().AddEntry("MakeTexturedDisk : TextureResolution value out of range (0.1..1)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "TexturedDisk";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    TexturedDiskMaker maker = new TexturedDiskMaker(valTubeHeight, valRadius, valSweep, valTexture, valTextureDepth, valTextureResolution);

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
                    Log.Instance().AddEntry("MakeTexturedDisk : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeTexturedDisk") + "( ";

            result += tubeHeightExp.ToRichText() + ", ";
            result += radiusExp.ToRichText() + ", ";
            result += sweepExp.ToRichText() + ", ";
            result += textureExp.ToRichText() + ", ";
            result += textureDepthExp.ToRichText() + ", ";
            result += textureResolutionExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeTexturedDisk( ";

            result += tubeHeightExp.ToString() + ", ";
            result += radiusExp.ToString() + ", ";
            result += sweepExp.ToString() + ", ";
            result += textureExp.ToString() + ", ";
            result += textureDepthExp.ToString() + ", ";
            result += textureResolutionExp.ToString();
            result += " )";
            return result;
        }
    }
}