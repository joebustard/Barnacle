using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakePillNode : ExpressionNode
    {
                private ExpressionNode flatLengthExp ;
        private ExpressionNode flatHeightExp ;
        private ExpressionNode edgeExp ;
        private ExpressionNode pillWidthExp ;


        public MakePillNode
            (
            ExpressionNode flatLength, ExpressionNode flatHeight, ExpressionNode edge, ExpressionNode pillWidth
            )
        {
                      this.flatLengthExp = flatLength ;
          this.flatHeightExp = flatHeight ;
          this.edgeExp = edge ;
          this.pillWidthExp = pillWidth ;

        }

        public MakePillNode
                (ExpressionCollection coll)
        {
                            this.flatLengthExp = coll.Get(0);
                this.flatHeightExp = coll.Get(1);
                this.edgeExp = coll.Get(2);
                this.pillWidthExp = coll.Get(3);

        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

                            double valFlatLength= 0;                double valFlatHeight= 0;                double valEdge= 0;                double valPillWidth= 0;

            if (
               EvalExpression(flatLengthExp, ref valFlatLength, "FlatLength", "MakePill")  &&
               EvalExpression(flatHeightExp, ref valFlatHeight, "FlatHeight", "MakePill")  &&
               EvalExpression(edgeExp, ref valEdge, "Edge", "MakePill")  &&
               EvalExpression(pillWidthExp, ref valPillWidth, "PillWidth", "MakePill") )
            {
                // check calculated values are in range
                bool inRange = true;
                
            if (valFlatLength < 1 || valFlatLength > 100 )
            {
                Log.Instance().AddEntry("MakePill : FlatLength value out of range (1..100)");
                inRange= false;
            }

            if (valFlatHeight < 1 || valFlatHeight > 100 )
            {
                Log.Instance().AddEntry("MakePill : FlatHeight value out of range (1..100)");
                inRange= false;
            }

            if (valEdge < 1 || valEdge > 100 )
            {
                Log.Instance().AddEntry("MakePill : Edge value out of range (1..100)");
                inRange= false;
            }

            if (valPillWidth < 2 || valPillWidth > 200 )
            {
                Log.Instance().AddEntry("MakePill : PillWidth value out of range (2..200)");
                inRange= false;
            }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Pill";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    PillMaker maker = new PillMaker(valFlatLength, valFlatHeight, valEdge, valPillWidth);

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
                    Log.Instance().AddEntry("MakePill : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakePill") + "( ";
            
        result += flatLengthExp.ToRichText()+", ";
        result += flatHeightExp.ToRichText()+", ";
        result += edgeExp.ToRichText()+", ";
        result += pillWidthExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakePill( ";
            
        result += flatLengthExp.ToString()+", ";
        result += flatHeightExp.ToString()+", ";
        result += edgeExp.ToString()+", ";
        result += pillWidthExp.ToString();
            result += " )";
            return result;
        }
    }
}
