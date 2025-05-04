using System;
using System.Collections.Generic;
using static CSGLib.BooleanModeller;

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
        private static SymbolTable Singleton;

        // Instance constructor
        public SymbolTable()
        {
            Symbols = new List<Symbol>();
        }

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
            boolarrayvariable,
            intarrayvariable,
            doublearrayvariable,
            stringarrayvariable,
            handlearrayvariable,
            solidarrayvariable,
            structarrayvariable,
            structname
        }

        public static SymbolTable Instance()
        {
            if (Singleton == null)
            {
                Singleton = new SymbolTable();
            }
            return Singleton;
        }

        public Symbol AddArraySymbol(string strName, SymbolType symbolType)
        {
            ArraySymbol sym = new ArraySymbol();
            sym.Name = strName;
            sym.ItemType = symbolType;
            sym.SymbolType = symbolType;
            Symbols.Add(sym);
            return sym;
        }

        public Symbol AddStructArraySymbol(string strName, StructDefinition def)
        {
            StructArraySymbol sym = new StructArraySymbol();
            sym.Name = strName;
            sym.Structure = def;
            sym.SymbolType = SymbolType.structarrayvariable;
            Symbols.Add(sym);
            return sym;
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

        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("Symbol Table Dump");
            foreach (Symbol sym in Symbols)
            {
                sym.Dump();
            }
        }

        public ArraySymbol FindArraySymbol(string strName)
        {
            Symbol result = null;
            foreach (Symbol sym in Symbols)
            {
                if ((sym.Name.ToLower() == strName.ToLower()) &&
                    (sym is ArraySymbol))
                {
                    result = sym;
                    break;
                }
            }
            return (ArraySymbol)result;
        }

        public ArraySymbol FindArraySymbol(string parent, string name)
        {
            Symbol result = null;
            string local = parent + name;
            // locals first
            foreach (Symbol sym in Symbols)
            {
                if ((sym.Name.ToLower() == local.ToLower()) &&
                    (sym is ArraySymbol))
                {
                    result = sym;
                    break;
                }
            }
            if (result == null)
            {
                foreach (Symbol sym in Symbols)
                {
                    if ((sym.Name.ToLower() == name.ToLower()) &&
                        (sym is ArraySymbol))
                    {
                        result = sym;
                        break;
                    }
                }
            }
            return (ArraySymbol)result;
        }

        public StructArraySymbol FindStructArraySymbol(string parent, string name)
        {
            Symbol result = null;
            string local = parent + name;
            // locals first
            foreach (Symbol sym in Symbols)
            {
                if ((sym.Name.ToLower() == local.ToLower()) &&
                    (sym is StructArraySymbol))
                {
                    result = sym;
                    break;
                }
            }
            if (result == null)
            {
                foreach (Symbol sym in Symbols)
                {
                    if ((sym.Name.ToLower() == name.ToLower()) &&
                        (sym is StructArraySymbol))
                    {
                        result = sym;
                        break;
                    }
                }
            }
            return (StructArraySymbol)result;
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

        public SymbolTable.SymbolType GetFieldType(string s)
        {
            SymbolTable.SymbolType ty = SymbolTable.SymbolType.unknown;
            switch (s)
            {
                case "Bool": { ty = SymbolTable.SymbolType.boolvariable; break; }
                case "Int": { ty = SymbolTable.SymbolType.intvariable; break; }
                case "Double": { ty = SymbolTable.SymbolType.doublevariable; break; }
                case "String": { ty = SymbolTable.SymbolType.stringvariable; break; }
                case "Handle": { ty = SymbolTable.SymbolType.handlevariable; break; }
                case "Solid": { ty = SymbolTable.SymbolType.solidvariable; break; }
            }
            return ty;
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