﻿using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeParallelogramNode : ExpressionNode
    {
        private ExpressionNode angleExp;
        private ExpressionNode bevelExp;
        private ExpressionNode heightExp;
        private ExpressionNode lengthExp;
        private ExpressionNode widthExp;

        public MakeParallelogramNode()
        {
        }

        public MakeParallelogramNode(ExpressionNode l, ExpressionNode h, ExpressionNode w, ExpressionNode a, ExpressionNode bev) : base(l)
        {
            this.lengthExp = l;
            this.heightExp = h;
            this.widthExp = w;
            this.angleExp = a;
            this.bevelExp = bev;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                double l = 0;
                double h = 0;

                double w = 0;
                double a = 0;
                double b = 0;

                if (EvalExpression(angleExp, ref a, "Angle", "MakeParallelogram") &&
                    EvalExpression(bevelExp, ref b, "Bevel", "MakeParallelogram") &&
                    EvalExpression(heightExp, ref h, "Height", "MakeParallelogram") &&
                    EvalExpression(widthExp, ref w, "Width", "MakeParallelogram") &&
                    EvalExpression(lengthExp, ref l, "Length", "MakeParallelogram")
                    )
                {
                    if (l > 0 && w > 0 && h > 0 && a > 0 && a < 90 && b >= 0)
                    {
                        result = true;
                        a = 90 - a;
                        Object3D obj = new Object3D();

                        obj.Name = "Parallelogram";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        Point3DCollection tmp = new Point3DCollection();
                        PGramMaker maker = new PGramMaker(l, h, w, a, b);

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
                        Log.Instance().AddEntry("MakeParallelogram : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeParallelogram : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeParallelogram") + "( ";
            result += lengthExp.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += widthExp.ToRichText() + ", ";
            result += angleExp.ToRichText() + ", ";
            result += bevelExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeParallelogram( ";

            result += lengthExp.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += widthExp.ToString() + ", ";
            result += angleExp.ToString() + ", ";
            result += bevelExp.ToString();
            result += " )";
            return result;
        }
    }
}