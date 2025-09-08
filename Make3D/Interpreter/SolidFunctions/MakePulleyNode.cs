using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakePulleyNode : ExpressionNode
    {
        private ExpressionNode axleBoreRadiusExp;
        private ExpressionNode extraRimRadiusExp;
        private ExpressionNode extraRimThicknessExp;
        private ExpressionNode grooveDepthExp;
        private ExpressionNode mainRadiusExp;
        private ExpressionNode mainThicknessExp;

        public MakePulleyNode(ExpressionNode mainRadius, ExpressionNode mainThickness, ExpressionNode extraRimRadius, ExpressionNode extraRimThickness, ExpressionNode grooveDepth, ExpressionNode axleBoreRadius)
        {
            this.mainRadiusExp = mainRadius;
            this.mainThicknessExp = mainThickness;
            this.extraRimRadiusExp = extraRimRadius;
            this.extraRimThicknessExp = extraRimThickness;
            this.grooveDepthExp = grooveDepth;
            this.axleBoreRadiusExp = axleBoreRadius;
        }

        public MakePulleyNode(ExpressionCollection exprs)
        {
            if (exprs != null && exprs.Count() == 6)
            {
                this.mainRadiusExp = exprs.Get(0);
                this.mainThicknessExp = exprs.Get(1);
                this.extraRimRadiusExp = exprs.Get(2);
                this.extraRimThicknessExp = exprs.Get(3);
                this.grooveDepthExp = exprs.Get(4);
                this.axleBoreRadiusExp = exprs.Get(5);
            }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                double valMainRadius = 0;
                double valMainThickness = 0;
                double valExtraRimRadius = 0;
                double valExtraRimThickness = 0;
                double valGrooveDepth = 0;
                double valAxleBoreRadius = 0;

                if (
                       EvalExpression(mainRadiusExp, ref valMainRadius, "MainRadius", "MakePulley") &&
                       EvalExpression(mainThicknessExp, ref valMainThickness, "MainThickness", "MakePulley") &&
                       EvalExpression(extraRimRadiusExp, ref valExtraRimRadius, "ExtraRimRadius", "MakePulley") &&
                       EvalExpression(extraRimThicknessExp, ref valExtraRimThickness, "ExtraRimThickness", "MakePulley") &&
                       EvalExpression(grooveDepthExp, ref valGrooveDepth, "GrooveDepth", "MakePulley") &&
                       EvalExpression(axleBoreRadiusExp, ref valAxleBoreRadius, "AxleBoreRadius", "MakePulley")
                   )
                {
                    // add range check
                    bool inRange = true;

                    if (valMainRadius < 1 || valMainRadius > 200)
                    {
                        Log.Instance().AddEntry("MakePulley : MainRadius value out of range (1..200)");
                        inRange = false;
                    }

                    if (valMainThickness < 1 || valMainThickness > 100)
                    {
                        Log.Instance().AddEntry("MakePulley : MainThickness value out of range (1..100)");
                        inRange = false;
                    }

                    if (valExtraRimRadius < 0 || valExtraRimRadius > 20)
                    {
                        Log.Instance().AddEntry("MakePulley : ExtraRimRadius value out of range (0..20)");
                        inRange = false;
                    }

                    if (valExtraRimThickness < 0 || valExtraRimThickness > 20)
                    {
                        Log.Instance().AddEntry("MakePulley : ExtraRimThickness value out of range (0..20)");
                        inRange = false;
                    }

                    if (valGrooveDepth < 1 || valGrooveDepth > 50)
                    {
                        Log.Instance().AddEntry("MakePulley : GrooveDepth value out of range (1..50)");
                        inRange = false;
                    }

                    if (valAxleBoreRadius < 0 || valAxleBoreRadius > 100)
                    {
                        Log.Instance().AddEntry("MakePulley : AxleBoreRadius value out of range (0..100)");
                        inRange = false;
                    }

                    if (inRange)
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "Pulley";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        Point3DCollection tmp = new Point3DCollection();
                        PulleyMaker maker = new PulleyMaker(valMainRadius, valMainThickness, valExtraRimRadius, valExtraRimThickness, valGrooveDepth, valAxleBoreRadius);

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
                        Log.Instance().AddEntry("MakePulley : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakePulley : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakePulley") + "( ";

            result += mainRadiusExp.ToRichText() + ", ";
            result += mainThicknessExp.ToRichText() + ", ";
            result += extraRimRadiusExp.ToRichText() + ", ";
            result += extraRimThicknessExp.ToRichText() + ", ";
            result += grooveDepthExp.ToRichText() + ", ";
            result += axleBoreRadiusExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakePulley( ";

            result += mainRadiusExp.ToString() + ", ";
            result += mainThicknessExp.ToString() + ", ";
            result += extraRimRadiusExp.ToString() + ", ";
            result += extraRimThicknessExp.ToString() + ", ";
            result += grooveDepthExp.ToString() + ", ";
            result += axleBoreRadiusExp.ToString();
            result += " )";
            return result;
        }
    }
}