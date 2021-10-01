using Barnacle.Object3DLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeNode : ExpressionNode
    {
        private static string[] rotatedPrimitives =
      {
            "roof","cone","pyramid","roundroof","cap","polygon","rightangle","pointy"
        };

        private ExpressionNode typeExp;
        private ExpressionNode xExp;
        private ExpressionNode xSize;
        private ExpressionNode yExp;
        private ExpressionNode ySize;
        private ExpressionNode zExp;
        private ExpressionNode zSize;

        public MakeNode()
        {
        }

        public MakeNode(ExpressionNode it, ExpressionNode xExp, ExpressionNode yExp, ExpressionNode zExp, ExpressionNode xSize, ExpressionNode ySize, ExpressionNode zSize) : base(it)
        {
            this.typeExp = it;
            this.xExp = xExp;
            this.yExp = yExp;
            this.zExp = zExp;
            this.xSize = xSize;
            this.ySize = ySize;
            this.zSize = zSize;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (typeExp != null)
            {
                result = typeExp.Execute();
                if (result)
                {
                    result = false;
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            string shape = sti.StringValue;
                            double x = 0;
                            double y = 0;
                            double z = 0;
                            double sx = 0;
                            double sy = 0;
                            double sz = 0;
                            if (EvalExpression(xExp, ref x, "X") &&
                                EvalExpression(yExp, ref y, "Y") &&
                                EvalExpression(zExp, ref z, "Z") &&
                                EvalExpression(xSize, ref sx, "Size X") &&
                                EvalExpression(ySize, ref sy, "Size Y") &&
                                EvalExpression(zSize, ref sz, "Size Z")
                                )
                            {
                                result = true;
                                Color color = Colors.Beige;
                                Object3D obj = new Object3D();

                                obj.Name = "Solid";

                                obj.Scale = new Scale3D(20, 20, 20);

                                obj.Position = new Point3D(x, y, z);

                                obj.BuildPrimitive(shape);
                                obj.PrimType = "Mesh";
                                obj.ScaleMesh(sx, sy, sz);

                                for (int i = 0; i < rotatedPrimitives.GetLength(0); i++)
                                {
                                    if (rotatedPrimitives[i] == shape)
                                    {
                                        obj.SwapYZAxies();

                                        break;
                                    }
                                }
                                obj.CalcScale(false);
                                obj.Remesh();
                                Script.ResultArtefacts.Add(obj);
                                ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry("Make : string expected text as solidtype");
                        }
                    }
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
            String result = RichTextFormatter.KeyWord("Make") + "( ";
            result += typeExp.ToRichText() + ", ";
            result += xExp.ToRichText() + ", ";
            result += yExp.ToRichText() + ", ";
            result += zExp.ToRichText() + ", ";
            result += xSize.ToRichText() + ", ";
            result += ySize.ToRichText() + ", ";
            result += zSize.ToRichText();

            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Make( ";
            result += typeExp.ToString() + ", ";
            result += xExp.ToString() + ", ";
            result += yExp.ToString() + ", ";
            result += zExp.ToString() + ", ";
            result += xSize.ToString() + ", ";
            result += ySize.ToString() + ", ";
            result += zSize.ToString();

            result += " )";
            return result;
        }

        private bool EvalExpression(ExpressionNode exp, ref double x, string v)
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.ival)
                    {
                        x = sti.IntValue;
                        result = true;
                    }
                    else if (sti.MyType == StackItem.ItemType.dval)
                    {
                        x = sti.DoubleValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry("Make : " + v + " expression error");
            }
            return result;
        }
    }
}