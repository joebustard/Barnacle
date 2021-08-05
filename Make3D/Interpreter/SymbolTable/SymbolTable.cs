using System;
using System.Collections.Generic;

namespace ScriptLanguage
{
    // this class is a little confusing, its a singleton
    // that you can have more than one of!
    //
    // When running we only want one general symbol table for the whole program
    // but its turns out its useful for structs and classes to have their own
    // local symbol table too.
    public class SymbolTable
    {
        public List<Symbol> Symbols;
        static private SymbolTable Singleton;

        public enum SymbolType
        {
            unknown,
            intvariable,
            doublevariable,
            boolvariable,
            stringvariable,
            procedurename,
            functionname,
            handlevariable,
            solidvariable,
            structname
        }

        // Instance constructor
        public SymbolTable()
        {
            Symbols = new List<Symbol>();
        }

        public static SymbolTable Instance()
        {
            if (Singleton == null)
            {
                Singleton = new SymbolTable();
            }
            return Singleton;
        }

        public Symbol AddStructSymbol(StructSymbol sym)
        {
            Symbols.Add(sym);
            return sym;
        }

        public Symbol AddStructSymbol(String name, StructDefinition def)
        {
            StructSymbol sym = new StructSymbol();
            sym.Name = name;
            sym.SymbolType = SymbolType.structname;
            sym.Definition = def;
            sym.SetFields();
            Symbols.Add(sym);
            return sym;
        }

        public Symbol AddSymbol(string strName, SymbolType symbolType)
        {
            Symbol sym = new Symbol();
            sym.Name = strName;
            sym.SymbolType = symbolType;
            Symbols.Add(sym);
            return sym;
        }

        public void Clear()
        {
            Symbols.Clear();
        }

        public SymbolType FindSymbol(string strName)
        {
            SymbolType result = SymbolType.unknown;
            foreach (Symbol sym in Symbols)
            {
                if (sym.Name.ToLower() == strName.ToLower())
                {
                    result = sym.SymbolType;
                    break;
                }
            }
            return result;
        }

        public Symbol FindSymbol(string strName, SymbolType type)
        {
            Symbol result = null;
            foreach (Symbol sym in Symbols)
            {
                if ((sym.Name.ToLower() == strName.ToLower()) &&
                    (sym.SymbolType == type))
                {
                    result = sym;
                    break;
                }
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToString()
        {
            String result = "";
            return result;
        }
    }
}