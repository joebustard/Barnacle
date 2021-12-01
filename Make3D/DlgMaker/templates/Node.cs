using Barnacle.Object3DLib;
using MakerLib;
using System;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class Make<TOOLNAME>Node : ExpressionNode
    {
//NODEFIELDS

        public Make//TOOLNAMENode(//CONSTRUCTORPARAMETERS)
    {
        //COPYFIELDS
    }

/// Execute this node
/// returning false terminates the application
///
public override bool Execute()
{
    bool result = false;

    //EXECUTIONVALUEDECLARATIONS

    if (
//EVALEXPRESSIONS
       )
    {
        // check calculated values are in range
        bool inRange = true;
        //RANGECHECKS
        if (inRange)
        {
            result = true;

            Object3D obj = new Object3D();

            obj.Name = "//TOOLNAME";
            obj.PrimType = "Mesh";
            obj.Scale = new Scale3D(20, 20, 20);

            obj.Position = new Point3D(0, 0, 0);
            Point3DCollection tmp = new Point3DCollection();
            Maker<TOOLNAME> maker = new Maker<TOOLNAME>(//MAKERPARAMS);

            maker.Generate(tmp, obj.TriangleIndices);
            PointUtils.PointCollectionToP3D(tmp, obj.RelativeObjectVertices);

            obj.CalcScale(false);
            obj.Remesh();
            Script.ResultArtefacts.Add(obj);
            ExecutionStack.Instance().PushSolid((int)Script.ResultArtefacts.Count - 1);
        }
        else
        {
            Log.Instance().AddEntry("Make//TOOLNAME : Illegal value");
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
    String result = RichTextFormatter.KeyWord("Make//TOOLNAME") + "( ";
    //EXPRESSIONTORICHTEXT
    result += " )";
    return result;
}

public override String ToString()
{
    String result = "Make//TOOLNAME( ";
    //EXPRESSIONTOSTRING
    result += " )";
    return result;
}
}
}