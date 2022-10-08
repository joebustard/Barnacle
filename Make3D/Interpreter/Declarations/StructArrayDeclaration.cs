using System;

namespace ScriptLanguage
{
    internal class StructArrayDeclarationNode : ArrayDeclarationNode
    {
        private StructDefinition def;

        public StructArrayDeclarationNode()
        {
            DeclarationType = "structarray";
            def = null;
        }

        public StructDefinition Structure
        {
            get { return def; }
            set { def = value; }
        }

        public override bool Execute()
        {
            bool res = false;
            if (ActualSymbol != null)
            {
                int dim = 0;
                if (dimensions != null && EvalExpression(dimensions, ref dim, "Dimensions"))
                {
                    // tell the symbol how big the array should be
                    ActualSymbol.SetSize(dim);
                    // all arrays are arrays of objects. But for struct we what the array elemnt
                    // itself just to be a container for the struct values. Create them as place holder.s
                    for (int i = 0; i < dim; i++)
                    {
                        Array entry = new Array();
                        entry.SetSize(def.Fields.Count);
                        for (int field = 0; field < def.Fields.Count; field++)
                        {
                            SymbolTable.SymbolType ty = SymbolTable.Instance().GetFieldType(def.Fields[field].SymType);
                            switch (ty)
                            {
                                case SymbolTable.SymbolType.boolvariable:
                                    {
                                        entry.Set(field, false);
                                    }
                                    break;

                                case SymbolTable.SymbolType.intvariable:
                                    {
                                        entry.Set(field, (int)0);
                                    }
                                    break;

                                case SymbolTable.SymbolType.doublevariable:
                                    {
                                        entry.Set(field, (double)0.0);
                                    }
                                    break;

                                case SymbolTable.SymbolType.stringvariable:
                                    {
                                        entry.Set(field, (string)"");
                                    }
                                    break;

                                case SymbolTable.SymbolType.handlevariable:
                                    {
                                        entry.Set(field, (int)-1);
                                    }
                                    break;

                                case SymbolTable.SymbolType.solidvariable:
                                    {
                                        entry.Set(field, (int)-1);
                                    }
                                    break;
                            }
                        }
                        (ActualSymbol as ArraySymbol).Array.Set(i, entry);
                    }
                    res = true;
                }
                /* cant do initialisers on struct arrays.
                else
                {
                    if (initialisers != null && initialisers.Execute())
                    {
                        ActualSymbol.SetSize(initialisers.Count());
                        for (int i = 0; i < initialisers.Count(); i++)
                        {
                            PullInitialValues(ActualSymbol as ArraySymbol, i);
                        }
                        res = true;
                    }
                }
                */
            }
            return res;
        }

        public override String ToRichText()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord(def.StructName + " ") + "[";
                if (dimensions != null)
                {
                    result += dimensions.ToRichText();
                }
                result += "] " + RichTextFormatter.VariableName(VarName);
                if (initialisers != null && initialisers.Count() > 0)
                {
                    result += " = " + @"\par";
                    result += Indentor.Indentation() + @"\{\par ";
                    result += Indentor.Indentation() + Indentor.Indentation() + initialisers.ToRichText() + @"\par";
                    result += Indentor.Indentation() + @"\} ;\par";
                }
                else
                {
                    result += " ;";
                }
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
                result = Indentor.Indentation() + def.StructName + " [";
                if (dimensions != null)
                {
                    result += dimensions.ToString();
                }
                result += "] " + VarName;
                if (initialisers != null && initialisers.Count() > 0)
                {
                    result += "=\n";
                    result += Indentor.Indentation() + "{\n";
                    result += Indentor.Indentation() + initialisers.ToString();
                    result += "\n" + Indentor.Indentation() + "}";
                }
                result += " ;";
            }
            return result;
        }
    }
}