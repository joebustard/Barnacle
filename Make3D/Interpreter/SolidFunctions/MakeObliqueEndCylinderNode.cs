using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeObliqueEndCylinderNode : ExpressionNode
    {
                private ExpressionNode radiusExp ;
        private ExpressionNode mainHeightExp ;
        private ExpressionNode cutHeightExp ;
        private ExpressionNode cutStyleExp ;
        private ExpressionNode cutPointsExp ;


        public MakeObliqueEndCylinderNode
            (
            ExpressionNode radius, ExpressionNode mainHeight, ExpressionNode cutHeight, ExpressionNode cutStyle, ExpressionNode cutPoints
            )
        {
                      this.radiusExp = radius ;
          this.mainHeightExp = mainHeight ;
          this.cutHeightExp = cutHeight ;
          this.cutStyleExp = cutStyle ;
          this.cutPointsExp = cutPoints ;

        }

        public MakeObliqueEndCylinderNode
                (ExpressionCollection coll)
        {
                            this.radiusExp = coll.Get(0);
                this.mainHeightExp = coll.Get(1);
                this.cutHeightExp = coll.Get(2);
                this.cutStyleExp = coll.Get(3);
                this.cutPointsExp = coll.Get(4);

        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

                            double valRadius= 0;                double valMainHeight= 0;                double valCutHeight= 0;                double valCutStyle= 0;                double valCutPoints= 0;

            if (
               EvalExpression(radiusExp, ref valRadius, "Radius", "MakeObliqueEndCylinder")  &&
               EvalExpression(mainHeightExp, ref valMainHeight, "MainHeight", "MakeObliqueEndCylinder")  &&
               EvalExpression(cutHeightExp, ref valCutHeight, "CutHeight", "MakeObliqueEndCylinder")  &&
               EvalExpression(cutStyleExp, ref valCutStyle, "CutStyle", "MakeObliqueEndCylinder")  &&
               EvalExpression(cutPointsExp, ref valCutPoints, "CutPoints", "MakeObliqueEndCylinder") )
            {
                // check calculated values are in range
                bool inRange = true;
                
            if (valRadius < 0.5 || valRadius > 200 )
            {
                Log.Instance().AddEntry("MakeObliqueEndCylinder : Radius value out of range (0.5..200)");
                inRange= false;
            }

            if (valMainHeight < 0.1 || valMainHeight > 200 )
            {
                Log.Instance().AddEntry("MakeObliqueEndCylinder : MainHeight value out of range (0.1..200)");
                inRange= false;
            }

            if (valCutHeight < 0.1 || valCutHeight > 200 )
            {
                Log.Instance().AddEntry("MakeObliqueEndCylinder : CutHeight value out of range (0.1..200)");
                inRange= false;
            }

            if (valCutStyle < 0 || valCutStyle > 10 )
            {
                Log.Instance().AddEntry("MakeObliqueEndCylinder : CutStyle value out of range (0..10)");
                inRange= false;
            }

            if (valCutPoints < 1 || valCutPoints > 10 )
            {
                Log.Instance().AddEntry("MakeObliqueEndCylinder : CutPoints value out of range (1..10)");
                inRange= false;
            }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "ObliqueEndCylinder";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    ObliqueEndCylinderMaker maker = new ObliqueEndCylinderMaker(valRadius, valMainHeight, valCutHeight, valCutStyle, valCutPoints);

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
                    Log.Instance().AddEntry("MakeObliqueEndCylinder : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeObliqueEndCylinder") + "( ";
            
        result += radiusExp.ToRichText()+", ";
        result += mainHeightExp.ToRichText()+", ";
        result += cutHeightExp.ToRichText()+", ";
        result += cutStyleExp.ToRichText()+", ";
        result += cutPointsExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeObliqueEndCylinder( ";
            
        result += radiusExp.ToString()+", ";
        result += mainHeightExp.ToString()+", ";
        result += cutHeightExp.ToString()+", ";
        result += cutStyleExp.ToString()+", ";
        result += cutPointsExp.ToString();
            result += " )";
            return result;
        }
    }
}
