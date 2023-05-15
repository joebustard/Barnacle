using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeSquirkleNode : ExpressionNode
    {
        private ExpressionNode blExp;
        private ExpressionNode brExp;
        private ExpressionNode depthExp;
        private ExpressionNode heightExp;
        private ExpressionNode lengthExp;
        private ExpressionNode tlExp;
        private ExpressionNode trExp;

        public MakeSquirkleNode()
        {
        }

        public MakeSquirkleNode(ExpressionNode tl, ExpressionNode tr, ExpressionNode bl, ExpressionNode br, ExpressionNode l, ExpressionNode h, ExpressionNode d) : base(tl)
        {
            this.tlExp = tl;
            this.trExp = tr;
            this.blExp = bl;
            this.brExp = br;

            this.lengthExp = l;

            this.heightExp = h;
            this.depthExp = d;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                int tlc = 0;
                int trc = 0;
                int blc = 0;
                int brc = 0;
                double length = 0.0;
                double height = 0;
                double depth = 0;

                if (EvalExpression(tlExp, ref tlc, "Topleft", "MakeSquirkle") &&
                    EvalExpression(trExp, ref trc, "Topright", "MakeSquirkle") &&
                    EvalExpression(blExp, ref blc, "Bottomleft", "MakeSquirkle") &&
                    EvalExpression(brExp, ref brc, "BottomRight", "MakeSquirkle") &&

                    EvalExpression(heightExp, ref length, "Length", "MakeSquirkle") &&
                    EvalExpression(lengthExp, ref height, "Height", "MakeSquirkle") &&
                    EvalExpression(depthExp, ref depth, "Width", "MakeSquirkle")
                    )
                {
                    if (CheckCode(tlc, "Topleft") &&
                       CheckCode(trc, "Topright") &&
                       CheckCode(blc, "Bottomleft") &&
                       CheckCode(brc, "BottomRight") && length > 0 && height > 0 && depth > 0)
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "Squirkle";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        Point3DCollection tmp = new Point3DCollection();
                        SquirkleMaker maker = new SquirkleMaker(tlc, trc, blc, brc, length, height, depth);

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
                        Log.Instance().AddEntry("MakeSquirkle : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeSquirkle : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeSquirkle") + "( ";

            result += tlExp.ToRichText() + ", ";
            result += trExp.ToRichText() + ", ";
            result += blExp.ToRichText() + ", ";
            result += brExp.ToRichText() + ", ";
            result += lengthExp.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += depthExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeSquirkle( ";

            result += tlExp.ToString() + ", ";
            result += trExp.ToString() + ", ";
            result += blExp.ToString() + ", ";
            result += brExp.ToString() + ", ";
            result += lengthExp.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += depthExp.ToString();
            result += " )";
            return result;
        }

        private bool CheckCode(int c, string v)
        {
            bool res = true;
            if (c < 0 || c > 2)
            {
                res = false;
                Log.Instance().AddEntry("MakeSquirkle : Illegal corner value :" + v + " should be 0,1 or 2");
            }
            return res;
        }
    }
}