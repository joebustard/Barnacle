using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakePlateletNode : ExpressionNode
    {
        private ExpressionNode heightExp;
        private string label = "MakePlatelet";
        private ExpressionNode pointArrayExp;

        public MakePlateletNode()
        {
        }

        public MakePlateletNode(ExpressionNode p, ExpressionNode h) : base(h)
        {
            this.pointArrayExp = p;

            this.heightExp = h;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            double[] coords = null;
            double h = 0;

            if (EvalExpression(heightExp, ref h, "Height", "MakePlatelet") &&
                 EvalExpression(pointArrayExp, ref coords, "PointArray", "MakePlatelet"))
            {
                if (coords != null && coords.GetLength(0) >= 6 && h > 0)
                {
                    result = true;
                    List<System.Windows.Point> points = new List<System.Windows.Point>();

                    for (int i = 0; i < coords.GetLength(0); i += 2)
                    {
                        if (i + 1 < coords.GetLength(0))
                        {
                            points.Add(new System.Windows.Point(coords[i], coords[i + 1]));
                        }
                    }
                    Object3D obj = new Object3D();

                    obj.Name = "Platelet";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    PolyMaker maker = new PolyMaker(points, h);
                    Point3DCollection tmp = new Point3DCollection();
                    // maker.Generate(obj.RelativeObjectVertices, obj.TriangleIndices);
                    maker.Generate(tmp, obj.TriangleIndices);
                    PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);
                    obj.CalcScale(false);
                    obj.Remesh();
                    Script.ResultArtefacts.Add(obj);
                    ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                }
                else
                {
                    Log.Instance().AddEntry($"{label} : Illegal value");
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
            String result = RichTextFormatter.KeyWord($"{label}") + "( ";
            result += pointArrayExp.ToRichText() + ", ";
            result += heightExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";
            result += pointArrayExp.ToString() + ", ";
            result += heightExp.ToString();
            result += " )";
            return result;
        }
    }
}