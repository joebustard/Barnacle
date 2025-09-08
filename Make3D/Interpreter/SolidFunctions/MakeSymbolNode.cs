using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeSymbolNode : ExpressionNode
    {
        private ExpressionNode symbolCodeExp;
        private ExpressionNode symbolFontExp;

        public MakeSymbolNode(ExpressionNode symbolCode, ExpressionNode symbolFont)
        {
            this.symbolCodeExp = symbolCode;
            this.symbolFontExp = symbolFont;
        }

        public MakeSymbolNode
                (ExpressionCollection coll)
        {
            this.symbolCodeExp = coll.Get(0);
            this.symbolFontExp = coll.Get(1);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            string valSymbolCode = "";
            string valFontName = "";
            double valLength = 25;
            if (EvalExpression(symbolCodeExp, ref valSymbolCode, "SymbolCode", "MakeSymbol") &&
                  EvalExpression(symbolFontExp, ref valFontName, "FontName", "MakeSymbol")
               )
            {
                result = true;

                Object3D obj = new Object3D();

                obj.Name = "Symbol";
                obj.PrimType = "Mesh";
                obj.Scale = new Scale3D(20, 20, 20);

                obj.Position = new Point3D(0, 0, 0);
                Point3DCollection tmp = new Point3DCollection();
                SymbolFontMaker maker = new SymbolFontMaker(valSymbolCode, valFontName, valLength);

                maker.Generate(tmp, obj.TriangleIndices);
                PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

                obj.CalcScale(false);
                obj.Remesh();
                obj.CalculateAbsoluteBounds();

                int id = Script.NextObjectId;
                Script.ResultArtefacts[id] = obj;
                ExecutionStack.Instance().PushSolid(id);
            }

            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeSymbol") + "( ";

            result += symbolCodeExp.ToRichText() + ", ";
            result += symbolFontExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeSymbol( ";
            result += symbolCodeExp.ToString() + ", ";
            result += symbolFontExp.ToString();
            result += " )";
            return result;
        }
    }
}