using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class MakeDripTrayNode : ExpressionNode
    {
                private ExpressionNode topLengthExp ;
        private ExpressionNode topWidthExp ;
        private ExpressionNode baseLengthExp ;
        private ExpressionNode baseWidthExp ;
        private ExpressionNode trayHeightExp ;
        private ExpressionNode wallThicknessExp ;


        public MakeDripTrayNode
            (
            ExpressionNode topLength, ExpressionNode topWidth, ExpressionNode baseLength, ExpressionNode baseWidth, ExpressionNode trayHeight, ExpressionNode wallThickness
            )
        {
                      this.topLengthExp = topLength ;
          this.topWidthExp = topWidth ;
          this.baseLengthExp = baseLength ;
          this.baseWidthExp = baseWidth ;
          this.trayHeightExp = trayHeight ;
          this.wallThicknessExp = wallThickness ;

        }

        public MakeDripTrayNode
                (ExpressionCollection coll)
        {
                            this.topLengthExp = coll.Get(0);
                this.topWidthExp = coll.Get(1);
                this.baseLengthExp = coll.Get(2);
                this.baseWidthExp = coll.Get(3);
                this.trayHeightExp = coll.Get(4);
                this.wallThicknessExp = coll.Get(5);

        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

                            double valTopLength= 0;                double valTopWidth= 0;                double valBaseLength= 0;                double valBaseWidth= 0;                double valTrayHeight= 0;                double valWallThickness= 0;

            if (
               EvalExpression(topLengthExp, ref valTopLength, "TopLength", "MakeDripTray")  &&
               EvalExpression(topWidthExp, ref valTopWidth, "TopWidth", "MakeDripTray")  &&
               EvalExpression(baseLengthExp, ref valBaseLength, "BaseLength", "MakeDripTray")  &&
               EvalExpression(baseWidthExp, ref valBaseWidth, "BaseWidth", "MakeDripTray")  &&
               EvalExpression(trayHeightExp, ref valTrayHeight, "TrayHeight", "MakeDripTray")  &&
               EvalExpression(wallThicknessExp, ref valWallThickness, "WallThickness", "MakeDripTray") )
            {
                TrayMaker maker = new TrayMaker();
                // check calculated values are in range
                bool inRange = true;
                
            inRange = RangeCheck(maker, "TopLength", valTopLength) && inRange;
            inRange = RangeCheck(maker, "TopWidth", valTopWidth) && inRange;
            inRange = RangeCheck(maker, "BaseLength", valBaseLength) && inRange;
            inRange = RangeCheck(maker, "BaseWidth", valBaseWidth) && inRange;
            inRange = RangeCheck(maker, "TrayHeight", valTrayHeight) && inRange;
            inRange = RangeCheck(maker, "WallThickness", valWallThickness) && inRange;

                if (inRange)
                {
                    result = true;

                    Object3D obj = new Object3D();

                    obj.Name = "DripTray";
                    obj.PrimType = "Mesh";
                    obj.Scale = new Scale3D(20, 20, 20);

                    obj.Position = new Point3D(0, 0, 0);
                    Point3DCollection tmp = new Point3DCollection();
                    maker.SetValues(valTopLength, valTopWidth, valBaseLength, valBaseWidth, valTrayHeight, valWallThickness);

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
                    Log.Instance().AddEntry("MakeDripTray : Illegal value");
                }
            }

            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// display in the editor
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("MakeDripTray") + "( ";
            
        result += topLengthExp.ToRichText()+", ";
        result += topWidthExp.ToRichText()+", ";
        result += baseLengthExp.ToRichText()+", ";
        result += baseWidthExp.ToRichText()+", ";
        result += trayHeightExp.ToRichText()+", ";
        result += wallThicknessExp.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "MakeDripTray( ";
            
        result += topLengthExp.ToString()+", ";
        result += topWidthExp.ToString()+", ";
        result += baseLengthExp.ToString()+", ";
        result += baseWidthExp.ToString()+", ";
        result += trayHeightExp.ToString()+", ";
        result += wallThicknessExp.ToString();
            result += " )";
            return result;
        }

        private static bool RangeCheck(TrayMaker maker, string paramName, double val)
        {
            bool inRange = maker.CheckLimits(paramName, val);
            if (!inRange)
            {
                ParamLimit pl = maker.GetLimits(paramName);
                if (pl != null)
                {
                    Log.Instance().AddEntry($"MakeDripTray : {paramName} value {val} out of range ({pl.Low}..{pl.High}");
                }
                else
                {
                    Log.Instance().AddEntry($"MakeDripTray : Can't check parameter {paramName}");
                }
            }

            return inRange;
        }
    }
}
