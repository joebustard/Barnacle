using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class SolidNameNode : SolidStatement
    {
        public SolidNameNode()
        {
            label = "SetName";
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
                            string name;

                            result = PullString(out name);
                            if (result && Script.ResultArtefacts[objectIndex] != null)
                            {
                                Script.ResultArtefacts[objectIndex].Name = name;
                                result = true;
                            }
                            else
                            {
                                Log.Instance().AddEntry($"Run Time Error : {label} expected string name");
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry($"Run Time Error : {label} unknown solid");
                        }
                    }
                }
            }
            return result;
        }
    }
}