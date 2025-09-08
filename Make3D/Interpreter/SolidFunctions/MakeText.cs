using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeTextNode : ExpressionNode
    {
        private ExpressionNode boldExp;
        private ExpressionNode fontNameExp;
        private ExpressionNode fontSizeExp;
        private ExpressionNode heightExp;
        private ExpressionNode italicExp;
        private ExpressionNode textExp;

        public MakeTextNode(ExpressionCollection coll)
        {
            this.textExp = coll.Get(0);
            this.fontNameExp = coll.Get(1);
            this.fontSizeExp = coll.Get(2);
            this.heightExp = coll.Get(3);
            this.boldExp = coll.Get(4);
            this.italicExp = coll.Get(5);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            try
            {
                string txt = "";
                string fname = "";
                double fontSize = 0;
                double h = 0;
                bool bold = false;
                bool italic = false;

                if (EvalExpression(textExp, ref txt, "Text", "MakeText") &&
                    EvalExpression(fontNameExp, ref fname, "FontName", "MakeText") &&
                    EvalExpression(fontSizeExp, ref fontSize, "FontSize", "MakeText") &&
                    EvalExpression(heightExp, ref h, "Height", "MakeText") &&
                    EvalExpression(boldExp, ref bold, "Bold", "MakeText") &&
                    EvalExpression(italicExp, ref italic, "Italic", "MakeText")
                    )
                {
                    if (fontSize > 0 && h > 0 && txt != "")
                    {
                        result = true;

                        Object3D obj = new Object3D();

                        obj.Name = "Text";
                        obj.PrimType = "Mesh";
                        obj.Scale = new Scale3D(20, 20, 20);

                        obj.Position = new Point3D(0, 0, 0);
                        TextMaker textMaker = new TextMaker(txt, fname, fontSize, h, true, bold, italic);
                        Point3DCollection tmp = new Point3DCollection(); ;
                        textMaker.Generate(tmp, obj.TriangleIndices);
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
                        Log.Instance().AddEntry("MakeText : Illegal value");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"MakeText : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeText") + "( ";
            result += textExp.ToRichText() + ", ";
            result += fontNameExp.ToRichText() + ", ";
            result += fontSizeExp.ToRichText() + ", ";
            result += heightExp.ToRichText() + ", ";
            result += boldExp.ToRichText() + ", ";
            result += italicExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeText( ";
            result += textExp.ToString() + ", ";
            result += fontNameExp.ToString() + ", ";
            result += fontSizeExp.ToString() + ", ";
            result += heightExp.ToString() + ", ";
            result += boldExp.ToString() + ", ";
            result += italicExp.ToString();
            result += " )";
            return result;
        }
    }
}