using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeTexturedTubeNode : ExpressionNode
    {
        private ExpressionNode tubeHeightExp;
        private ExpressionNode innerRadiusExp;
        private ExpressionNode thicknessExp;
        private ExpressionNode sweepExp;
        private ExpressionNode textureExp;
        private ExpressionNode textureDepthExp;
        private ExpressionNode textureResolutionExp;


        public MakeTexturedTubeNode
            (
            ExpressionNode tubeHeight, ExpressionNode innerRadius, ExpressionNode thickness,  ExpressionNode sweep, ExpressionNode texture, ExpressionNode textureDepth, ExpressionNode textureResolution
            )
        {
            this.tubeHeightExp = tubeHeight;
            this.innerRadiusExp = innerRadius;
            this.thicknessExp = thickness;
            this.sweepExp = sweep;
            this.textureExp = texture;
            this.textureDepthExp = textureDepth;
            this.textureResolutionExp = textureResolution;

        }

        public MakeTexturedTubeNode
                (ExpressionCollection coll)
        {
            this.tubeHeightExp = coll.Get(0);
            this.innerRadiusExp = coll.Get(1);
            this.thicknessExp = coll.Get(2);
            this.sweepExp = coll.Get(4);
            this.textureExp = coll.Get(5);
            this.textureDepthExp = coll.Get(6);
            this.textureResolutionExp = coll.Get(7);

        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valTubeHeight = 0; 
            double valInnerRadius = 0; 
            double valThickness = 0; 
            double valSweep = 0; 
            string valTexture = ""; 
            double valTextureDepth = 0; 
            double valTextureResolution = 0;

            if (
               EvalExpression(tubeHeightExp, ref valTubeHeight, "TubeHeight", "MakeTexturedTube") &&
               EvalExpression(innerRadiusExp, ref valInnerRadius, "InnerRadius", "MakeTexturedTube") &&
               EvalExpression(thicknessExp, ref valThickness, "Thickness", "MakeTexturedTube") &&
               EvalExpression(sweepExp, ref valSweep, "Sweep", "MakeTexturedTube") &&
               EvalExpression(textureExp, ref valTexture, "Texture", "MakeTexturedTube") &&
               EvalExpression(textureDepthExp, ref valTextureDepth, "TextureDepth", "MakeTexturedTube") &&
               EvalExpression(textureResolutionExp, ref valTextureResolution, "TextureResolution", "MakeTexturedTube"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valTubeHeight < 1 || valTubeHeight > 200)
                {
                    Log.Instance().AddEntry("MakeTexturedTube : TubeHeight value out of range (1..200)");
                    inRange = false;
                }

                if (valInnerRadius < 1 || valInnerRadius > 200)
                {
                    Log.Instance().AddEntry("MakeTexturedTube : InnerRadius value out of range (1..200)");
                    inRange = false;
                }

                if (valThickness < 1 || valThickness > 50)
                {
                    Log.Instance().AddEntry("MakeTexturedTube : Thickness value out of range (1..50)");
                    inRange = false;
                }



                if (valSweep < 1 || valSweep > 360)
                {
                    Log.Instance().AddEntry("MakeTexturedTube : Sweep value out of range (1..360)");
                    inRange = false;
                }

                if (valTextureDepth < 0.1 || valTextureDepth > 10)
                {
                    Log.Instance().AddEntry("MakeTexturedTube : TextureDepth value out of range (0.1..10)");
                    inRange = false;
                }

                if (valTextureResolution < 0.1 || valTextureResolution > 1)
                {
                    Log.Instance().AddEntry("MakeTexturedTube : TextureResolution value out of range (0.1..1)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "TexturedTube";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    TexturedTubeMaker maker = new TexturedTubeMaker(valTubeHeight, valInnerRadius, valThickness, valSweep, valTexture, valTextureDepth, valTextureResolution);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeTexturedTube : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeTexturedTube") + "( ";

            result += tubeHeightExp.ToRichText() + ", ";
            result += innerRadiusExp.ToRichText() + ", ";
            result += thicknessExp.ToRichText() + ", ";            
            result += sweepExp.ToRichText() + ", ";
            result += textureExp.ToRichText() + ", ";
            result += textureDepthExp.ToRichText() + ", ";
            result += textureResolutionExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeTexturedTube( ";

            result += tubeHeightExp.ToString() + ", ";
            result += innerRadiusExp.ToString() + ", ";
            result += thicknessExp.ToString() + ", ";            
            result += sweepExp.ToString() + ", ";
            result += textureExp.ToString() + ", ";
            result += textureDepthExp.ToString() + ", ";
            result += textureResolutionExp.ToString();
            result += " )";
            return result;
        }
    }
}
