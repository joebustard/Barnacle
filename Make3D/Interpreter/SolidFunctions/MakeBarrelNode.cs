using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeBarrelNode : ExpressionNode
    {
        private ExpressionNode barrelHeightExp;
        private ExpressionNode middleRadiusExp;
        private ExpressionNode numberOrRibsExp;
        private ExpressionNode ribDepthExp;
        private ExpressionNode shellExp;
        private ExpressionNode shellThicknessExp;
        private ExpressionNode topRadiusExp;
        public MakeBarrelNode (
            ExpressionNode barrelHeight,
            ExpressionNode topRadius,
            ExpressionNode middleRadius,
            ExpressionNode numberOrRibs,
            ExpressionNode shell,
            ExpressionNode shellThickness,
            ExpressionNode ribDepth  )
        {
            this.barrelHeightExp = barrelHeight;
            this.topRadiusExp = topRadius;
            this.middleRadiusExp = middleRadius;
            this.numberOrRibsExp = numberOrRibs;
            this.shellExp = shell;
            this.shellThicknessExp = shellThickness;
            this.ribDepthExp = ribDepth;

        }

        public MakeBarrelNode(ExpressionCollection coll)
        {
            this.barrelHeightExp = coll.Get(0);
            this.topRadiusExp = coll.Get(1);
            this.middleRadiusExp = coll.Get(2);
            this.numberOrRibsExp = coll.Get(3);
            this.shellExp = coll.Get(4);
            this.shellThicknessExp = coll.Get(5);
            this.ribDepthExp = coll.Get(6);

        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            double valBarrelHeight = 0; 
            double valTopRadius = 0; 
            double valMiddleRadius = 0;             
            double valNumberOrRibs = 0; 
            bool valShell = false; 
            double valShellThickness = 0; 
            double valRibDepth = 0;

            if (
               EvalExpression(barrelHeightExp, ref valBarrelHeight, "BarrelHeight", "MakeBarrel") &&
               EvalExpression(topRadiusExp, ref valTopRadius, "TopRadius", "MakeBarrel") &&
               EvalExpression(middleRadiusExp, ref valMiddleRadius, "MiddleRadius", "MakeBarrel") &&
               EvalExpression(numberOrRibsExp, ref valNumberOrRibs, "NumberOrRibs", "MakeBarrel") &&
               EvalExpression(shellExp, ref valShell, "Shell", "MakeBarrel") &&
               EvalExpression(shellThicknessExp, ref valShellThickness, "ShellThickness", "MakeBarrel") &&
               EvalExpression(ribDepthExp, ref valRibDepth, "RibDepth", "MakeBarrel"))
            {
                // check calculated values are in range
                bool inRange = true;

                if (valBarrelHeight < 1 || valBarrelHeight > 200)
                {
                    Log.Instance().AddEntry("MakeBarrel : BarrelHeight value out of range (1..200)");
                    inRange = false;
                }

                if (valTopRadius < 2 || valTopRadius > 100)
                {
                    Log.Instance().AddEntry("MakeBarrel : TopRadius value out of range (2..100)");
                    inRange = false;
                }

                if (valMiddleRadius < 2 || valMiddleRadius > 100)
                {
                    Log.Instance().AddEntry("MakeBarrel : MiddleRadius value out of range (2..100)");
                    inRange = false;
                }


                if (valNumberOrRibs < 2 || valNumberOrRibs > 30)
                {
                    Log.Instance().AddEntry("MakeBarrel : NumberOrRibs value out of range (2..30)");
                    inRange = false;
                }

                if (valShellThickness < 1 || valShellThickness > 50)
                {
                    Log.Instance().AddEntry("MakeBarrel : ShellThickness value out of range (1..50)");
                    inRange = false;
                }

                if (valRibDepth < 0.1 || valRibDepth > 50)
                {
                    Log.Instance().AddEntry("MakeBarrel : RibDepth value out of range (0.1..50)");
                    inRange = false;
                }

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "Barrel";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);
                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    BarrelMaker maker = new BarrelMaker(valBarrelHeight, valTopRadius, valMiddleRadius, valNumberOrRibs, valShell, valShellThickness, valRibDepth);
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
                    Log.Instance().AddEntry("MakeBarrel : Illegal value");
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
            String result = RichTextFormatter.KeyWord("MakeBarrel") + "( ";

            result += barrelHeightExp.ToRichText() + ", ";
            result += topRadiusExp.ToRichText() + ", ";
            result += middleRadiusExp.ToRichText() + ", ";
            result += numberOrRibsExp.ToRichText() + ", ";
            result += shellExp.ToRichText() + ", ";
            result += shellThicknessExp.ToRichText() + ", ";
            result += ribDepthExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeBarrel( ";

            result += barrelHeightExp.ToString() + ", ";
            result += topRadiusExp.ToString() + ", ";
            result += middleRadiusExp.ToString() + ", ";
            result += numberOrRibsExp.ToString() + ", ";
            result += shellExp.ToString() + ", ";
            result += shellThicknessExp.ToString() + ", ";
            result += ribDepthExp.ToString();
            result += " )";
            return result;
        }
    }
}
