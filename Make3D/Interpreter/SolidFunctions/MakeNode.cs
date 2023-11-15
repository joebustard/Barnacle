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

        private static string[] shapenames =
               {
                "box",
                "sphere",
                "cylinder",
                "roof",
                "roundroof",
                "cone",
                "pyramid",
                "pyramid2",
                "torus",
                "cap",
                "polygon",
                "tube",
                "rightangle",
                "pointy",
                "octahedron"
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
            try
            {
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
                                string shape = sti.StringValue.ToLower();
                                if (ValidShape(shape))
                                {
                                    bool isSwapper = false;
                                    for (int i = 0; i < rotatedPrimitives.GetLength(0); i++)
                                    {
                                        if (rotatedPrimitives[i] == shape)
                                        {
                                            isSwapper = true;

                                            break;
                                        }
                                    }
                                    double x = 0;
                                    double y = 0;
                                    double z = 0;
                                    double sx = 0;
                                    double sy = 0;
                                    double sz = 0;
                                    if (EvalExpression(xExp, ref x, "X", "Make") &&
                                        EvalExpression(yExp, ref y, "Y", "Make") &&
                                        EvalExpression(zExp, ref z, "Z", "Make") &&
                                        EvalExpression(xSize, ref sx, "Size X", "Make") &&
                                        EvalExpression(ySize, ref sy, "Size Y", "Make") &&
                                        EvalExpression(zSize, ref sz, "Size Z", "Make")
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
                                        if (!isSwapper)
                                        {
                                            obj.ScaleMesh(sx, sy, sz);
                                        }
                                        else
                                        {
                                            obj.ScaleMesh(sx, sz, sy);

                                            obj.SwapYZAxies();
                                        }

                                        obj.CalcScale(false);
                                        obj.Remesh();
                                        int id = Script.NextObjectId;
                                        Script.ResultArtefacts[id] = obj;
                                        ExecutionStack.Instance().PushSolid(id);
                                    }
                                }
                                else
                                {
                                    Log.Instance().AddEntry($"Make : solid shape {shape} not recognised");
                                    Log.Instance().AddEntry("Make : valid shapes are : box, sphere, cylinder, roof, roundroof, cone, pyramid, pyramid2, torus, cap, polygon, tube, rightangle, pointy, octahedron");
                                }
                            }
                            else
                            {
                                Log.Instance().AddEntry("Make : string expected text as solidtype");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"Make : {ex.Message}");
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

        private bool ValidShape(string shape)
        {
            bool res = false;
            for (int i = 0; i < shapenames.Length; i++)
            {
                if (shapenames[i] == shape)
                {
                    res = true;
                    break;
                }
            }

            return res;
        }
    }
}