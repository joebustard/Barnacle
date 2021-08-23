using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class SolidFloor : StatementNode
    {

        private String externalName;
        private String variableName;

        public SolidFloor()
        {
            variableName = "";
        }


        public String ExternalName
        {
            get { return externalName; }
            set { externalName = value; }
        }

        public String VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }


        public override bool Execute()
        {
            bool result = false;

            Symbol sym = SymbolTable.Instance().FindSymbol(variableName, SymbolTable.SymbolType.solidvariable);
            if (sym != null)
            {
                int objectIndex = sym.SolidValue;
                if (objectIndex >= 0 && objectIndex <= Script.ResultArtefacts.Count)
                {
                    Script.ResultArtefacts[objectIndex].MoveToFloor();
                    Script.ResultArtefacts[objectIndex].Remesh();
                    result = true;
                }
                else
                {
                    Log.Instance().AddEntry("Run Time Error : Floor solid name incorrect");
                }
            }
            return result;
        }

        public override String ToRichText()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("Floor ") + RichTextFormatter.VariableName(externalName)+" ;";
                if (HighLight)
                {
                    result = RichTextFormatter.Highlight(result);
                }
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + "Floor " + externalName +  " ;";
            }
            return result;
        }

       
    }
}