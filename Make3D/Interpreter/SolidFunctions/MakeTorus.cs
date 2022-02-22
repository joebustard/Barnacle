using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeTorusNode : ExpressionNode
    {
        private ExpressionNode curveExp;
        private ExpressionNode heightExp;
        private ExpressionNode knobExp;
        private ExpressionNode mainRadiusExp;
        private ExpressionNode ringRadiusExp;

        //private ExpressionNode thicknessExp;
        private ExpressionNode vRadiusExp;

        public MakeTorusNode()
        {
        }

        public MakeTorusNode(ExpressionNode mr, ExpressionNode hr, ExpressionNode vr, ExpressionNode curve, ExpressionNode k, ExpressionNode h) : base(mr)
        {
            this.mainRadiusExp = mr;
            this.ringRadiusExp = hr;
            this.vRadiusExp = vr;

            this.curveExp = curve;
            this.knobExp = k;
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
            double mr = 0;
            double hr = 0;
            double vr = 0;
            int curve = 0;
            double knobs = 0;
            double h = 0;

            if (EvalExpression(mainRadiusExp, ref mr, "Radius", "MakeTorus") &&
                EvalExpression(ringRadiusExp, ref hr, "RingRadius", "MakeTorus") &&
                EvalExpression(vRadiusExp, ref vr, "VerticalRadius", "MakeTorus") &&
                 EvalExpression(curveExp, ref curve, "Curve", "MakeTorus") &&
                  EvalExpression(knobExp, ref knobs, "Knobs", "MakeTorus") &&
                  EvalExpression(heightExp, ref h, "Height", "MakeTorus")
                )
            {
                if (mr > 0 && h > 0 && hr >= 0 && vr >= 0 && curve >= 0 && curve < 4)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Torus";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    TorusMaker maker = new TorusMaker(mr, hr, vr, curve, knobs, h);
                    Point3DCollection tmp = new Point3DCollection();
                    //maker.Generate(obj.RelativeObjectVertices, obj.TriangleIndices);
                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);
                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry("MakeTorus : Illegal value");
                }
            }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeTorus : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeTorus") + "( ";

            result += mainRadiusExp.ToRichText() + ", ";
            result += ringRadiusExp.ToRichText() + ", ";
            result += vRadiusExp.ToRichText() + ", ";
            result += curveExp.ToRichText() + ", ";
            result += knobExp.ToRichText() + ", ";
            result += heightExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeTube( ";
            result += mainRadiusExp.ToString() + ", ";
            result += ringRadiusExp.ToString() + ", ";
            result += vRadiusExp.ToString() + ", ";
            result += curveExp.ToString() + ", ";
            result += knobExp.ToString() + ", ";
            result += heightExp.ToString();
            result += " )";
            return result;
        }
    }
}