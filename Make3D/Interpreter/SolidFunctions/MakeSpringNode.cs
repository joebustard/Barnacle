using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeSpringNode : ExpressionNode
    {
                private ExpressionNode innerRadiusExp ;
        private ExpressionNode wireRadiusExp ;
        private ExpressionNode coilGapExp ;
        private ExpressionNode turnsExp ;
        private ExpressionNode facesPerTurnExp ;
        private ExpressionNode wireFacetsExp ;


        public MakeSpringNode
            (
            ExpressionNode innerRadius, ExpressionNode wireRadius, ExpressionNode coilGap, ExpressionNode turns, ExpressionNode facesPerTurn, ExpressionNode wireFacets
            )
        {
                      this.innerRadiusExp = innerRadius ;
          this.wireRadiusExp = wireRadius ;
          this.coilGapExp = coilGap ;
          this.turnsExp = turns ;
          this.facesPerTurnExp = facesPerTurn ;
          this.wireFacetsExp = wireFacets ;

        }

        public MakeSpringNode
                (ExpressionCollection coll)
        {
                            this.innerRadiusExp = coll.Get(0);
                this.wireRadiusExp = coll.Get(1);
                this.coilGapExp = coll.Get(2);
                this.turnsExp = coll.Get(3);
                this.facesPerTurnExp = coll.Get(4);
                this.wireFacetsExp = coll.Get(5);

        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

                            double valInnerRadius= 0;                double valWireRadius= 0;                double valCoilGap= 0;                double valTurns= 0;                double valFacesPerTurn= 0;                double valWireFacets= 0;

            if (
               EvalExpression(innerRadiusExp, ref valInnerRadius, "InnerRadius", "MakeSpring")  &&
               EvalExpression(wireRadiusExp, ref valWireRadius, "WireRadius", "MakeSpring")  &&
               EvalExpression(coilGapExp, ref valCoilGap, "CoilGap", "MakeSpring")  &&
               EvalExpression(turnsExp, ref valTurns, "Turns", "MakeSpring")  &&
               EvalExpression(facesPerTurnExp, ref valFacesPerTurn, "FacesPerTurn", "MakeSpring")  &&
               EvalExpression(wireFacetsExp, ref valWireFacets, "WireFacets", "MakeSpring") )
            {
                // check calculated values are in range
                bool inRange = true;
                
            if (valInnerRadius < 0 || valInnerRadius > 100 )
            {
                Log.Instance().AddEntry("MakeSpring : InnerRadius value out of range (0..100)");
                inRange= false;
            }

            if (valWireRadius < 0.1 || valWireRadius > 100 )
            {
                Log.Instance().AddEntry("MakeSpring : WireRadius value out of range (0.1..100)");
                inRange= false;
            }

            if (valCoilGap < 0 || valCoilGap > 100 )
            {
                Log.Instance().AddEntry("MakeSpring : CoilGap value out of range (0..100)");
                inRange= false;
            }

            if (valTurns < 1 || valTurns > 100 )
            {
                Log.Instance().AddEntry("MakeSpring : Turns value out of range (1..100)");
                inRange= false;
            }

            if (valFacesPerTurn < 10 || valFacesPerTurn > 360 )
            {
                Log.Instance().AddEntry("MakeSpring : FacesPerTurn value out of range (10..360)");
                inRange= false;
            }

            if (valWireFacets < 10 || valWireFacets > 360 )
            {
                Log.Instance().AddEntry("MakeSpring : WireFacets value out of range (10..360)");
                inRange= false;
            }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Spring";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    SpringMaker maker = new SpringMaker(valInnerRadius, valWireRadius, valCoilGap, valTurns, valFacesPerTurn, valWireFacets);

                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeSpring : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeSpring") + "( ";
            
        result += innerRadiusExp.ToRichText()+", ";
        result += wireRadiusExp.ToRichText()+", ";
        result += coilGapExp.ToRichText()+", ";
        result += turnsExp.ToRichText()+", ";
        result += facesPerTurnExp.ToRichText()+", ";
        result += wireFacetsExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeSpring( ";
            
        result += innerRadiusExp.ToString()+", ";
        result += wireRadiusExp.ToString()+", ";
        result += coilGapExp.ToString()+", ";
        result += turnsExp.ToString()+", ";
        result += facesPerTurnExp.ToString()+", ";
        result += wireFacetsExp.ToString();
            result += " )";
            return result;
        }
    }
}
