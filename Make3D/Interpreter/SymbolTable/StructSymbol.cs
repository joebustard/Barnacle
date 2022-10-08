namespace ScriptLanguage
{
    public class StructSymbol : Symbol
    {
        public StructDefinition Definition;

        // the symbol or a struct contains a symbol for each field in the struct definition
        public SymbolTable FieldValues;

        public StructSymbol()
        {
            FieldValues = new SymbolTable();
        }

        internal void SetFields()
        {
            if (Definition != null)
            {
                foreach (StructDefinition.StructField f in Definition.Fields)
                {
                    SymbolTable.SymbolType ty = SymbolTable.SymbolType.unknown;
                    switch (f.SymType)
                    {
                        case "Bool": { ty = SymbolTable.SymbolType.boolvariable; break; }
                        case "Int": { ty = SymbolTable.SymbolType.intvariable; break; }
                        case "Double": { ty = SymbolTable.SymbolType.doublevariable; break; }
                        case "String": { ty = SymbolTable.SymbolType.stringvariable; break; }
                        case "Handle": { ty = SymbolTable.SymbolType.handlevariable; break; }
                        case "Solid": { ty = SymbolTable.SymbolType.solidvariable; break; }
                    }

                    FieldValues.AddSymbol(f.FieldName, ty);
                }
            }
        }
    }
}