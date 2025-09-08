using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeDualProfileNode : ExpressionNode
    {
        private ExpressionNode frontExp;
        private ExpressionNode topExp;

        public MakeDualProfileNode
            (

            )
        {
        }

        public MakeDualProfileNode
                (ExpressionCollection coll)
        {
            this.frontExp = coll.Get(0);
            this.topExp = coll.Get(1);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            string valFront = "";
            string valTop = "";

            if (
                 EvalExpression(frontExp, ref valFront, "Front", "MakeDualProfileNode") &&
                 EvalExpression(topExp, ref valTop, "Top", "MakeDualProfileNode")
            )
            {
                // check calculated values are in range

                if (!String.IsNullOrEmpty(valFront) && !String.IsNullOrEmpty(valTop))
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "DualProfile";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    DualProfileMaker maker = new DualProfileMaker(valFront, valTop);

                    maker.Generate(tmp, obj.TriangleIndices);
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
                    Log.Instance().AddEntry("MakeDualProfile : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeDualProfile") + "( ";

            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeDualProfile( ";

            result += " )";
            return result;
        }
    }
}