using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeSquaredStadiumNode : ExpressionNode
    {
        private ExpressionNode endLength;
        private ExpressionNode gapExp;
        private ExpressionNode heightExp;
        private ExpressionNode overRunExp;
        private ExpressionNode radiusExp;

        public MakeSquaredStadiumNode()
        {
        }

        public MakeSquaredStadiumNode(ExpressionNode r1, ExpressionNode gp, ExpressionNode es, ExpressionNode h, ExpressionNode ov) : base(r1)
        {
            this.overRunExp = ov;
            this.radiusExp = r1;
            this.gapExp = gp;

            this.endLength = es;
            this.heightExp = h;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            try
            {
                double ov = 0;
                double r = 0;
                double el = 0;
                double g = 0;
                double h = 0;

                if (EvalExpression(radiusExp, ref r, "Radius", "MakeSquaredStadium") &&
                    EvalExpression(endLength, ref el, "EndSize", "MakeSquaredStadium") &&
                    EvalExpression(heightExp, ref h, "Height", "MakeSquaredStadium") &&
                    EvalExpression(overRunExp, ref ov, "OverRun", "MakeSquaredStadium") &&
                    EvalExpression(gapExp, ref g, "Gap", "MakeSquaredStadium")
                    )
                {
                    if (r > 0 && h > 0 && el > 0 && g >= 0 && ov >= 0)
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "SquaredStadium";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        SquaredStadiumMaker stadiumMaker = new SquaredStadiumMaker(r, g, el, h, ov);
                        Point3DCollection tmp = new Point3DCollection(); ;
                        stadiumMaker.Generate(tmp, obj.TriangleIndices);
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
                        Log.Instance().AddEntry("MakeSquaredStadium : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeSquaredStadium : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeSquaredStadium") + "( ";
            result += radiusExp.ToRichText() + ", ";
            result += gapExp.ToRichText() + ", ";
            result += endLength.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += overRunExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeSquaredStadium( ";
            result += radiusExp.ToString() + ", ";
            result += gapExp.ToString() + ", ";
            result += endLength.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += overRunExp.ToString();
            result += " )";
            return result;
        }
    }
}