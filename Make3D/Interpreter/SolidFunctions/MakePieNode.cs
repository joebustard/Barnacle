using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakePieNode : ExpressionNode
    {
                private ExpressionNode radiusExp ;
        private ExpressionNode thicknessExp ;
        private ExpressionNode sweepExp ;
        private ExpressionNode upperBevelExp ;
        private ExpressionNode lowerBevelExp ;


        public MakePieNode
            (
            ExpressionNode radius, ExpressionNode thickness, ExpressionNode sweep, ExpressionNode upperBevel, ExpressionNode lowerBevel
            )
        {
                      this.radiusExp = radius ;
          this.thicknessExp = thickness ;
          this.sweepExp = sweep ;
          this.upperBevelExp = upperBevel ;
          this.lowerBevelExp = lowerBevel ;

        }

        public MakePieNode
                (ExpressionCollection coll)
        {
                            this.radiusExp = coll.Get(0);
                this.thicknessExp = coll.Get(1);
                this.sweepExp = coll.Get(2);
                this.upperBevelExp = coll.Get(3);
                this.lowerBevelExp = coll.Get(4);

        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

                            double valRadius= 0;                double valThickness= 0;                double valSweep= 0;                double valUpperBevel= 0;                double valLowerBevel= 0;

            if (
               EvalExpression(radiusExp, ref valRadius, "Radius", "MakePie")  &&
               EvalExpression(thicknessExp, ref valThickness, "Thickness", "MakePie")  &&
               EvalExpression(sweepExp, ref valSweep, "Sweep", "MakePie")  &&
               EvalExpression(upperBevelExp, ref valUpperBevel, "UpperBevel", "MakePie")  &&
               EvalExpression(lowerBevelExp, ref valLowerBevel, "LowerBevel", "MakePie") )
            {
                // check calculated values are in range
                bool inRange = true;
                
            if (valRadius < 5 || valRadius > 200 )
            {
                Log.Instance().AddEntry("MakePie : Radius value out of range (5..200)");
                inRange= false;
            }

            if (valThickness < 1 || valThickness > 100 )
            {
                Log.Instance().AddEntry("MakePie : Thickness value out of range (1..100)");
                inRange= false;
            }

            if (valSweep < 1 || valSweep > 359 )
            {
                Log.Instance().AddEntry("MakePie : Sweep value out of range (1..359)");
                inRange= false;
            }

            if (valUpperBevel < 0 || valUpperBevel > 10 )
            {
                Log.Instance().AddEntry("MakePie : UpperBevel value out of range (0..10)");
                inRange= false;
            }

            if (valLowerBevel < 0 || valLowerBevel > 10 )
            {
                Log.Instance().AddEntry("MakePie : LowerBevel value out of range (0..10)");
                inRange= false;
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
                    PieMaker maker = new PieMaker(valRadius, valThickness, valSweep, valUpperBevel, valLowerBevel);

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
            
        result += radiusExp.ToRichText()+", ";
        result += thicknessExp.ToRichText()+", ";
        result += sweepExp.ToRichText()+", ";
        result += upperBevelExp.ToRichText()+", ";
        result += lowerBevelExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakePie( ";
            
        result += radiusExp.ToString()+", ";
        result += thicknessExp.ToString()+", ";
        result += sweepExp.ToString()+", ";
        result += upperBevelExp.ToString()+", ";
        result += lowerBevelExp.ToString();
            result += " )";
            return result;
        }
    }
}
