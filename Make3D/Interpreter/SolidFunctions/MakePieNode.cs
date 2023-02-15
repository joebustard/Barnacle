using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakePieNode : ExpressionNode
    {
        private ExpressionNode radiusExp;
        private ExpressionNode centreThicknessExp;
        private ExpressionNode edgeThicknessExp;
        private ExpressionNode sweepExp;

        public MakePieNode( ExpressionNode radius,
                            ExpressionNode centreThickness,
        ExpressionNode edgeThickness, 
        ExpressionNode sweep)
        {
            this.radiusExp = radius;
            this.centreThicknessExp = edgeThickness;
            this.edgeThicknessExp = edgeThickness;
            this.sweepExp = sweep;

        }

        public MakePieNode
                (ExpressionCollection coll)
        {
            this.radiusExp = coll.Get(0);
            this.centreThicknessExp = coll.Get(1);
            this.edgeThicknessExp = coll.Get(2);
            this.sweepExp = coll.Get(3);                        
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valRadius = 0; 
            double valCentreThickness = 0;
            double valEdgeThickness = 0;
            double valSweep = 0; 


            if (
               EvalExpression(radiusExp, ref valRadius, "Radius", "MakePie") &&
               EvalExpression(edgeThicknessExp, ref valEdgeThickness, "Edge Thickness", "MakePie") &&
               EvalExpression(sweepExp, ref valSweep, "Sweep", "MakePie") &&
               EvalExpression(centreThicknessExp, ref valCentreThickness, "CentreThick", "MakePie"))
            
            {
                // check calculated values are in range
                bool inRange = true;

                if (valRadius < 5 || valRadius > 200)
                {
                    Log.Instance().AddEntry("MakePie : Radius value out of range (5..200)");
                    inRange = false;
                }
                if (valCentreThickness < 0.1 || valCentreThickness > 100)
                {
                    Log.Instance().AddEntry("MakePie : Centre Thickness value out of range (0.1..100)");
                    inRange = false;
                }
                if (valEdgeThickness < 1 || valEdgeThickness > 100)
                {
                    Log.Instance().AddEntry("MakePie : Edge Thickness value out of range (1..100)");
                    inRange = false;
                }

                if (valSweep < 1 || valSweep > 359)
                {
                    Log.Instance().AddEntry("MakePie : Sweep value out of range (1..359)");
                    inRange = false;
                }


                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Pie";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    PieMaker maker = new PieMaker(valRadius, valCentreThickness,valEdgeThickness, valSweep);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakePie : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakePie") + "( ";

            result += radiusExp.ToRichText() + ", ";
            result += centreThicknessExp.ToRichText() + ", ";
            result += edgeThicknessExp.ToRichText() + ", ";
            result += sweepExp.ToRichText() ;
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakePie( ";

            result += radiusExp.ToString() + ", ";
            result += centreThicknessExp.ToString() + ", ";
            result += edgeThicknessExp.ToString() + ", ";
            result += sweepExp.ToString();

            result += " )";
            return result;
        }
    }
}