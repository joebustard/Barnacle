﻿using System.Windows.Media;

namespace ScriptLanguage
{
    internal class SolidColourNode : SolidStatement
    {
        public SolidColourNode()
        {
            label = "SetColour";
            expressions = new ExpressionCollection();
        }

        public override bool Execute()
        {
            bool result = false;
            if (expressions != null)
            {
                result = expressions.Execute();
                if (result)
                {
                    int objectIndex;
                    if (!PullSolid(out objectIndex))
                    {
                        Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect");
                    }
                    else
                    {
                        if (objectIndex >= 0 && objectIndex <= Script.ResultArtefacts.Count && Script.ResultArtefacts[objectIndex] != null)
                        {
                            byte A;
                            byte R;
                            byte G;
                            byte B;
                            result = PullByte(out A);
                            if (result)
                            {
                                result = PullByte(out R);
                                if (result)
                                {
                                    result = PullByte(out G);
                                    if (result)
                                    {
                                        result = PullByte(out B);
                                        if (result)
                                        {
                                            Script.ResultArtefacts[objectIndex].Color = Color.FromArgb(A, R, G, B);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}