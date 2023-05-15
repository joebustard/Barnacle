using System;

namespace ScriptLanguage
{
    public class Interpreter
    {
        private String[] functionNames;
        private String[] keywords;
        private string lastError;
        private Tokeniser tokeniser;

        // Instance constructor
        public Interpreter()
        {
            tokeniser = new Tokeniser();
            keywords = new String[]
            {
                "alignleft",
                "alignright",
                "aligntop",
                "alignbottom",
                "alignfront",
                "alignback",
                "aligncentre",
                "break",
                "bool",
                "delete",
                "do",
                "double",
                "else",
                "exit",
                "floor",
                "function",
                "for",
                "if",
                "include",
                "int",
                "move",
                "procedure",
                "print",
                "rotate",
                "resize",
                "return",
                "rmove",
                "run",
                "setcolour",
                "setname",
                "solid",
                "stackleft",
                "stackright",
                "stackabove",
                "stackbelow",
                "stackfront",
                "stackbehind",
                "string",
                "struct",
                "while",
                "writeline",
            };

            functionNames = new String[]
            {
                "abs",
                "atan",
                "cos",
                "copy",
                "cutout",
                "degrees",
                "difference",
                "dim",
                "height",
                "inputstring",
                "insertpart",
                "intersection",
                "len",
                "length",
                "make",
                "makebicorn",
                "makebrickwall",
                "makebricktower",
                "makehollow",
                "makedualprofile",
                "makerailwheel",
                "makepath",
                "makeparallelogram",
                "makeparabolicdish",
                "makepathloft",
                "makepill",
                "makeplatelet",
                "makeplankwall",
                "makepie",
                "makepulley",
                "makereuleaux",
                "makeroofridge",
                "makeshapedbrickwall",
                "makespring",
                "makestadium",
                "makesquaredstadium",
                "makesquirkle",
                "makestonewall",
                "maketext",
                "maketiledroof",
                "maketorus",
                "maketexturedtube",
                "maketrapezoid",
                "maketrickle",
                "maketube",
                "makewagonwheel",
                "makecurvedfunnel",
                "now",
                "pcname",
                "rad",
                "rnd",
                "replaceall",
                "sin",
                "sqrt",
                "str",
                "substring",
                "tan",
                "trim",
                "trimleft",
                "trimright",
                "union",
                "unionall",
                "width",
                "val",
                "validsolid"
            };
        }

        private ExpressionNode ParseMakePieFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakePie";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 4;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakePieNode mn = new MakePieNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakePillFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakePill";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 4;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakePillNode mn = new MakePillNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        public bool Load(Script script, string FilePath)
        {
            bool result = false;
            lastError = "";
            if (script != null)
            {
                if (FilePath != "")
                {
                    tokeniser.Initialise();
                    string originalScriptFolder = System.IO.Path.GetDirectoryName(FilePath);
                    tokeniser.SetRunningFolder(originalScriptFolder);
                    if (tokeniser.SetSource(FilePath))
                    {
                        result = ParseSource(script);
                    }
                }
            }
            return result;
        }

        public bool LoadFromText(Script script, string text, string originalfilePath)
        {
            bool result = false;
            lastError = "";
            SymbolTable.Instance().Clear();
            ExecutionStack.Instance().Clear();
            CProcedureCache.Instance().Clear();
            FunctionCache.Instance().Clear();
            StructDefinitiontTable.Instance().Clear();
            if (script != null)
            {
                if (text != "")
                {
                    string originalScriptFolder = "";
                    if (originalfilePath != "")
                    {
                        originalScriptFolder = System.IO.Path.GetDirectoryName(originalfilePath);
                    }
                    tokeniser.Initialise();
                    tokeniser.SetRunningFolder(originalScriptFolder);
                    tokeniser.SetText(text);
                    result = ParseSource(script);
                }
            }
            return result;
        }

        private bool CheckForComma(bool report = true)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Comma)
                {
                    result = true;
                }
                else
                {
                    if (report)
                    {
                        ReportSyntaxError("Expected comma");
                    }
                }
            }
            return result;
        }

        private bool CheckForComma(string s)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Comma)
                {
                    result = true;
                }
                else
                {
                    ReportSyntaxError($"{s} expected comma");
                }
            }
            return result;
        }

        private bool CheckForInitialiser(CompoundNode parentNode, String parentName, string Identifier, String strExternalName, DeclarationNode dec)
        {
            //
            // This function checks if there is an optional initialiser
            // It is OK for there not to be one
            //
            bool Result = true;
            if (dec != null)
            {
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (tokenType == Tokeniser.TokenType.Assignment)
                    {
                        ExpressionNode exp = ParseExpressionNode(parentName);
                        if (exp != null)
                        {
                            dec.Initialiser = exp;
                        }
                        else
                        {
                            ReportSyntaxError("Illegal initialiser");
                            Result = false;
                        }
                    }
                    else
                    {
                        tokeniser.PutTokenBack();
                    }
                }
            }
            return Result;
        }

        private bool CheckForSemiColon(bool report = true)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.SemiColon)
                {
                    result = true;
                }
                else
                {
                    if (report)
                    {
                        ReportSyntaxError("Expected ;");
                    }
                }
            }
            return result;
        }

        private bool FetchToken(out string token, out Tokeniser.TokenType tokenType)
        {
            bool result = false;
            token = "";
            tokenType = Tokeniser.TokenType.None;
            //
            // This will get  tokens from the input stream until one that doesnt look like a comment is found
            //
            bool bDone = false;
            while (bDone == false)
            {
                //
                // try to get next token. If there are none then we are done
                //
                if (tokeniser.GetToken(out token, out tokenType) == false)
                {
                    bDone = true;
                }
                else
                {
                    //
                    // We got a token
                    // if its a comment then skip to end of line
                    if (tokenType == Tokeniser.TokenType.Comment)
                    {
                        tokeniser.SkipToEndOfLine();
                    }
                    else
                    {
                        if ((tokenType == Tokeniser.TokenType.OpenBlockComment))
                        {
                            while ((tokeniser.GetToken(out token, out tokenType) == true) &&
                                    (tokenType != Tokeniser.TokenType.CloseBlockComment))
                            {
                            }
                        }
                        else
                        {
                            result = true;
                            bDone = true;
                        }
                    }
                }
            }
            return result;
        }

        private SingleParameterFunction GetFunctionNode<T>(String parentName) where T : new()
        {
            var myObj = new T();
            ExpressionNode argumentExpression = ParseExpressionNode(parentName);
            if (argumentExpression != null)
            {
                (myObj as SingleParameterFunction).Expression = argumentExpression;
            }
            return myObj as SingleParameterFunction;
        }

        private bool HexToInt(string Source, out int Result)
        {
            bool bOK = true;
            Result = 0;
            Source = Source.ToLower();
            char[] Digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            for (int i = 0; i < Source.Length; i++)
            {
                char c = Source[i];
                bool bFound = false;
                for (int j = 0; j < Digits.GetLength(0); j++)
                {
                    if (c == Digits[j])
                    {
                        Result = Result * 16 + j;
                        bFound = true;
                        break;
                    }
                }
                if (bFound == false)
                {
                    ReportSyntaxError("Bad hex digit " + c);
                    bOK = false;
                    break;
                }
            }
            return bOK;
        }

        private bool IsIntrinsicFunction(string Identifier)
        {
            bool result = false;
            Identifier = Identifier.ToLower();
            for (int i = 0; i < functionNames.GetLength(0); i++)
            {
                if (functionNames[i] == Identifier)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private bool IsKeyword(string Identifier)
        {
            bool result = false;
            for (int i = 0; i < keywords.GetLength(0); i++)
            {
                if (keywords[i] == Identifier)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private bool IsUserFunction(string token)
        {
            bool IsKnownFunction = false;
            if (FunctionCache.Instance().FindFunction(token) != null)
            {
                IsKnownFunction = true;
            }
            return IsKnownFunction;
        }

        private bool ParseAlignStatement(CompoundNode parentNode, String parentName, string id)
        {
            bool result = false;
            SolidAlignmentNode sal = new SolidAlignmentNode();
            sal.OrientationMode = SolidAlignmentNode.Mode.Align;
            switch (id)
            {
                case "alignleft":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Left;
                    }
                    break;

                case "alignright":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Right;
                    }
                    break;

                case "aligntop":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Top;
                    }
                    break;

                case "alignbottom":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Bottom;
                    }
                    break;

                case "alignfront":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Front;
                    }
                    break;

                case "alignback":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Back;
                    }
                    break;

                case "aligncentre":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Centre;
                    }
                    break;
            }

            result = ParseSolidStatement(parentNode, parentName, sal.label, 2, sal);
            return result;
        }

        private ExpressionNode ParseArithmeticExpression(String parentName)
        {
            ExpressionNode exp = null;
            exp = ParseMultiplicativeExpression(parentName);
            if (exp != null)
            {
                bool bDone = false;
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                while (!bDone)
                {
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.Addition)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseMultiplicativeExpression(parentName);
                            if (RightNode != null)
                            {
                                AdditionNode Addexp = new AdditionNode();
                                Addexp.LeftNode = LeftNode;
                                Addexp.RightNode = RightNode;
                                Addexp.IsInLibrary = tokeniser.InIncludeFile();
                                exp = Addexp;
                            }
                        }
                        else
                            if (tokenType == Tokeniser.TokenType.Subtraction)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseMultiplicativeExpression(parentName);
                            if (RightNode != null)
                            {
                                CSubtractionNode Subexp = new CSubtractionNode();
                                Subexp.LeftNode = LeftNode;
                                Subexp.RightNode = RightNode;
                                exp = Subexp;
                            }
                        }
                        else
                        {
                            tokeniser.PutTokenBack();
                            bDone = true;
                        }
                    }
                }
            }
            return exp;
        }

        private void ParseArrayDeclaration(CompoundNode parentNode, string parentName, ref bool result, ref string token, ref Tokeniser.TokenType tokenType, SymbolTable.SymbolType arrayType)
        {
            ExpressionNode exp = ParseExpressionNode(parentName);
            if (exp == null)
            {
                tokeniser.PutTokenBack();
            }

            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "]")
                {
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.Identifier)
                        {
                            string id = token.ToLower();
                            if (IsIntrinsicFunction(id) || IsKeyword(id) || IsUserFunction(id))
                            {
                                ReportSyntaxError("can't use keyword/function {token} in declaration");
                            }
                            else
                            {
                                //
                                // To avoid local variable names clashing with other variables
                                // we prefix them with the parent [procedures name
                                //
                                String strVarName = token;
                                token = parentName + token;
                                if (parentNode.FindSymbol(token) != SymbolTable.SymbolType.unknown)
                                {
                                    ReportSyntaxError("Duplicate variable name " + strVarName);
                                }
                                else
                                {
                                    Symbol sym = parentNode.AddArraySymbol(token, arrayType);

                                    ArrayDeclarationNode node = new ArrayDeclarationNode();
                                    node.VarName = strVarName;
                                    node.ItemType = arrayType;
                                    node.Dimensions = exp;
                                    node.ActualSymbol = sym;
                                    node.IsInLibrary = tokeniser.InIncludeFile();
                                    parentNode.AddStatement(node);
                                    if (FetchToken(out token, out tokenType) == true)
                                    {
                                        if (token == "=")
                                        {
                                            if (FetchToken(out token, out tokenType) == true)
                                            {
                                                if (token == "{")
                                                {
                                                    bool bDone = false;
                                                    do
                                                    {
                                                        //
                                                        // See if it there is a nice expression
                                                        //
                                                        ExpressionNode iexp = ParseExpressionNode(parentName);
                                                        if (iexp != null)
                                                        {
                                                            node.AddInitialiserExpression(iexp);

                                                            //
                                                            // If there is a comma there should another expression
                                                            //
                                                            if (FetchToken(out token, out tokenType) == true)
                                                            {
                                                                if (tokenType == Tokeniser.TokenType.Comma)
                                                                {
                                                                    bDone = false;
                                                                }
                                                                else if (tokenType == Tokeniser.TokenType.CloseCurly)
                                                                {
                                                                    bDone = true;
                                                                }
                                                                else
                                                                {
                                                                    ReportSyntaxError("Unexpected token processing array initialiser Expected ) found " + token);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                ReportSyntaxError("Unexpected end of file ");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            bDone = true;
                                                        }
                                                    } while (bDone == false);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            tokeniser.PutTokenBack();
                                        }

                                        result = CheckForSemiColon();
                                    }
                                }
                            }
                        }
                        else
                            ReportSyntaxError("Expected variable name , found " + token);
                    }
                }
                else
                {
                    ReportSyntaxError("Expected ], found " + token);
                }
            }
            else
            {
                ReportSyntaxError("Expected ]");
            }
        }

        private bool ParseArrayParam(SymbolTable.SymbolType symbolType, ProcedureNode proc, string strFunctionName)
        {
            bool result = false;
            // convert the type into equivalent array
            switch (symbolType)
            {
                case SymbolTable.SymbolType.boolvariable:
                    {
                        symbolType = SymbolTable.SymbolType.boolarrayvariable;
                    }
                    break;

                case SymbolTable.SymbolType.intvariable:
                    {
                        symbolType = SymbolTable.SymbolType.intarrayvariable;
                    }
                    break;

                case SymbolTable.SymbolType.doublevariable:
                    {
                        symbolType = SymbolTable.SymbolType.doublearrayvariable;
                    }
                    break;

                case SymbolTable.SymbolType.stringvariable:
                    {
                        symbolType = SymbolTable.SymbolType.stringarrayvariable;
                    }
                    break;

                case SymbolTable.SymbolType.solidvariable:
                    {
                        symbolType = SymbolTable.SymbolType.solidarrayvariable;
                    }
                    break;

                case SymbolTable.SymbolType.handlevariable:
                    {
                        symbolType = SymbolTable.SymbolType.handlearrayvariable;
                    }
                    break;
            }
            String token = "";
            // we already know the type so look for a name
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "]")
                {
                    if (FetchToken(out token, out tokenType) && tokenType == Tokeniser.TokenType.Identifier)
                    {
                        String strParamName = strFunctionName + token;
                        String ExternalName = token;
                        // if its not a duplicate then add it to the symbol table
                        if (SymbolTable.Instance().FindSymbol(strParamName) == SymbolTable.SymbolType.unknown)
                        {
                            Symbol Symbol = SymbolTable.Instance().AddArraySymbol(strParamName, symbolType);
                            proc.AddParameter(strParamName, symbolType);

                            result = true;
                        }
                        else
                        {
                            ReportSyntaxError("Duplicate parameter name");
                        }
                    }
                    else
                    {
                        ReportSyntaxError("Expected parameter name, found " + token);
                    }
                }
                else
                {
                    ReportSyntaxError("]");
                }
            }
            return result;
        }

        private ExpressionNode ParseArraySymbolForCall(string externalName, string parentName, ArraySymbol arrSymbol)
        {
            ArraySymbolForCallNode exp = new ArraySymbolForCallNode();
            exp.ExternalName = externalName;
            exp.Symbol = arrSymbol;
            exp.Name = parentName + externalName;
            return exp;
        }

        private ExpressionNode ParseArrayVariableNode(String name, String parentName)
        {
            ExpressionNode exp = null;

            ExpressionNode indexExp = ParseExpressionNode(parentName);
            if (indexExp != null)
            {
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (token != "]")
                    {
                        ReportSyntaxError("Expected ], found " + token);
                    }
                    else
                    {
                        //
                        // Check for local variable
                        //
                        String externalName = name;
                        String internalName = parentName + name;
                        StructArraySymbol sarssym = SymbolTable.Instance().FindStructArraySymbol(parentName, name);

                        if (sarssym != null)
                        {
                            exp = ParseStructArrayVarNode(indexExp, sarssym, externalName, internalName);
                        }
                        else
                        {
                            ArraySymbol arsym = SymbolTable.Instance().FindArraySymbol(parentName, name);
                            if (arsym != null)
                            {
                                ArrayVariableNode vn = new ArrayVariableNode();
                                vn.Name = internalName;
                                vn.ExternalName = externalName;
                                vn.Symbol = arsym;
                                vn.IndexExpression = indexExp;
                                vn.IsInLibrary = tokeniser.InIncludeFile();
                                exp = vn;
                            }
                            else
                            {
                                //
                                // Try to match up with a global
                                //
                                ArraySymbol glsym = SymbolTable.Instance().FindArraySymbol(internalName);
                                if (glsym != null)
                                {
                                    ArrayVariableNode vn = new ArrayVariableNode();
                                    vn.Name = internalName;
                                    vn.Symbol = glsym;
                                    vn.ExternalName = externalName;
                                    vn.IndexExpression = indexExp;
                                    exp = vn;
                                }
                                else
                                {
                                    ReportSyntaxError("Unidentified variable " + name);
                                }
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private bool ParseAssignment(String identifier, CompoundNode parentNode, String parentName)
        {
            bool result = false;
            String externalName = identifier;
            string internalName = identifier;
            bool foundSym = false;

            if (parentNode.FindSymbol(parentName + identifier) != SymbolTable.SymbolType.unknown)
            {
                internalName = parentName + identifier;
                foundSym = true;
            }
            else
            {
                if (parentNode.FindSymbol(identifier) != SymbolTable.SymbolType.unknown)
                {
                    internalName = identifier;
                    foundSym = true;
                }
            }
            if (!foundSym)
            {
                ReportSyntaxError("Undefined variable " + externalName);
            }
            else
            {
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (tokenType != Tokeniser.TokenType.Assignment &&
                    tokenType != Tokeniser.TokenType.PlusEqual &&
                    tokenType != Tokeniser.TokenType.MinusEqual &&
                    tokenType != Tokeniser.TokenType.TimesEqual &&
                    tokenType != Tokeniser.TokenType.DivideEqual)
                    {
                        ReportSyntaxError("Expected = or += or -= or *= or /=, found " + token);
                    }
                    else
                    {
                        string opcode = token;
                        ExpressionNode exp = ParseExpressionNode(parentName);
                        if (exp != null)
                        {
                            result = CheckForSemiColon();
                            if (result)
                            {
                                if (opcode == "=")
                                {
                                    AssignmentNode asn = new AssignmentNode();
                                    asn.VariableName = internalName;
                                    asn.ExternalName = externalName;
                                    asn.ExpressionNode = exp;
                                    parentNode.AddStatement(asn);
                                }
                                else
                                {
                                    AssignOpNode asn = new AssignOpNode();
                                    asn.VariableName = internalName;
                                    asn.ExternalName = externalName;
                                    asn.OpCode = opcode;
                                    asn.ExpressionNode = exp;
                                    parentNode.AddStatement(asn);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private bool ParseAssignmentToArrayElement(String identifier, CompoundNode parentNode, String parentName)
        {
            bool result = false;
            String externalName = identifier;
            string internalName = identifier;

            bool foundSym = false;

            if (parentNode.FindSymbol(externalName) != SymbolTable.SymbolType.unknown)
            {
                internalName = externalName;
                foundSym = true;
            }
            else
            {
                if (parentNode.FindSymbol(parentName + identifier) != SymbolTable.SymbolType.unknown)
                {
                    internalName = parentName + identifier;
                    foundSym = true;
                }
            }
            if (!foundSym)
            {
                ReportSyntaxError("Undefined variable " + identifier);
            }
            else
            {
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (token != "[")
                    {
                        ReportSyntaxError("Expected [, found " + token);
                    }
                    else
                    {
                        ExpressionNode indexexp = ParseExpressionNode(parentName);
                        if (indexexp != null)
                        {
                            if (FetchToken(out token, out tokenType) == true)
                            {
                                if (token != "]")
                                {
                                    ReportSyntaxError("Expected ], found " + token);
                                }
                                else
                                {
                                    if (FetchToken(out token, out tokenType) == true)
                                    {
                                        if (tokenType != Tokeniser.TokenType.Assignment)
                                        {
                                            ReportSyntaxError("Expected =");
                                        }
                                        else
                                        {
                                            ExpressionNode exp = ParseExpressionNode(parentName);
                                            if (exp != null)
                                            {
                                                result = CheckForSemiColon();
                                                if (result)
                                                {
                                                    AssignToArrayElement asn = new AssignToArrayElement();
                                                    asn.VariableName = internalName;
                                                    asn.ExternalName = externalName;
                                                    asn.ValueExpression = exp;
                                                    asn.IndexExpression = indexexp;
                                                    parentNode.AddStatement(asn);
                                                }
                                            }
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

        private bool ParseAssignmentToStruct(String identifier, CompoundNode parentNode, String parentName)
        {
            bool result = false;
            String externalVarName = identifier;
            identifier = parentName + identifier;                   //

            if (parentNode.FindSymbol(identifier) == SymbolTable.SymbolType.unknown)
            {
                ReportSyntaxError("Undefined variable " + externalVarName);
            }
            else
            {
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                if (FetchToken(out token, out tokenType) == true)
                {
                    //there are two differnt ways of assigning to a struct
                    //one is whole struct to whole struct
                    // the other is field to field
                    // if have "name=" assume its struct to struct
                    // if its "name.name=" assume its a field
                    if (tokenType == Tokeniser.TokenType.Assignment)
                    {
                        // whole struct
                        result = ParseWholeStructAssignment(externalVarName, parentNode, parentName);
                    }
                    else
                    if (tokenType == Tokeniser.TokenType.Dot)
                    {
                        // field
                        result = ParseStructFieldAssignment(externalVarName, parentNode, parentName);
                    }
                    else
                    {
                        ReportSyntaxError("Syntax Error");
                    }
                }
            }

            return result;
        }

        private bool ParseAssignmentToStructArrayElement(string identifier, CompoundNode parentNode, string parentName)
        {
            bool result = false;
            String externalName = identifier;
            string internalName = identifier;

            bool foundSym = false;

            if (parentNode.FindSymbol(externalName) != SymbolTable.SymbolType.unknown)
            {
                internalName = externalName;
                foundSym = true;
            }
            else
            {
                if (parentNode.FindSymbol(parentName + identifier) != SymbolTable.SymbolType.unknown)
                {
                    internalName = parentName + identifier;
                    foundSym = true;
                }
            }
            if (!foundSym)
            {
                ReportSyntaxError("Undefined variable " + identifier);
            }
            else
            {
                StructArraySymbol actualSym = SymbolTable.Instance().FindStructArraySymbol(parentName, externalName);
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (token != "[")
                    {
                        ReportSyntaxError("Expected [, found " + token);
                    }
                    else
                    {
                        ExpressionNode indexexp = ParseExpressionNode(parentName);
                        if (indexexp != null)
                        {
                            if (FetchToken(out token, out tokenType) == true)
                            {
                                if (token != "]")
                                {
                                    ReportSyntaxError("Expected ], found " + token);
                                }
                                else
                                {
                                    if (FetchToken(out token, out tokenType) == true)
                                    {
                                        if (tokenType != Tokeniser.TokenType.Assignment)
                                        {
                                            ReportSyntaxError("Expected =, found " + token);
                                        }
                                        else
                                        {
                                            ExpressionNode exp = ParseExpressionNode(parentName);
                                            if (exp != null)
                                            {
                                                result = CheckForSemiColon();
                                                if (result)
                                                {
                                                    AssignStructToArrayElement asn = new AssignStructToArrayElement();
                                                    asn.VariableName = internalName;
                                                    asn.ExternalName = externalName;
                                                    asn.ValueExpression = exp;
                                                    asn.IndexExpression = indexexp;
                                                    asn.ActualSymbol = actualSym;
                                                    parentNode.AddStatement(asn);
                                                }
                                            }
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

        private bool ParseBody(CProgramNode tn)
        {
            bool result = false;

            //
            // Fetch the first non comment token
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;

            //
            // Look for the opening bracket of the body
            //
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.OpenCurly)
                {
                    CompoundNode CompNode = new CompoundNode();
                    CompNode.IsTestBody = true;
                    //
                    // We have to pass in a procedure name to help separate local variables
                    // FOr the top level compound node we just pass in ""
                    //
                    if (ParseCompoundNode(CompNode, "", true))
                    {
                        tn.SetChild(CompNode);
                        result = true;
                    }
                }
                else
                {
                    ReportSyntaxError("Expected {  found " + token);
                }
            }

            return result;
        }

        private void ParseBoolArrayDeclaration(CompoundNode parentNode, string parentName, ref bool result, ref string token, Tokeniser.TokenType tokenType)
        {
            ParseArrayDeclaration(parentNode, parentName, ref result, ref token, ref tokenType, SymbolTable.SymbolType.boolarrayvariable);
        }

        private bool ParseBoolStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;

            //
            // See if there is an identifier following the type
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "[")
                {
                    // array
                    ParseArrayDeclaration(parentNode, parentName, ref result, ref token, ref tokenType, SymbolTable.SymbolType.boolarrayvariable);
                }
                else
                {
                    ParseSingleBoolDeclaration(parentNode, parentName, ref result, ref token, tokenType);
                }
            }

            return result;
        }

        private bool ParseBreakStatement(CompoundNode parentNode)
        {
            bool result = CheckForSemiColon();
            if (result)
            {
                BreakNode en = new BreakNode();
                en.IsInLibrary = tokeniser.InIncludeFile();
                parentNode.AddStatement(en);
            }

            return result;
        }

        private bool ParseChainScriptStatement(CompoundNode parentNode, string parentName)
        {
            bool result = false;
            ExpressionNode exp = ParseExpressionNode(parentName);
            if (exp != null)
            {
                result = CheckForSemiColon();
                if (result)
                {
                    CChainScriptNode chain = new CChainScriptNode();
                    chain.Expression = exp;
                    chain.IsInLibrary = tokeniser.InIncludeFile();
                    parentNode.AddStatement(chain);
                }
            }

            return result;
        }

        private bool ParseCloseFileStatement(CompoundNode parentNode, string parentName)
        {
            bool result = false;
            ExpressionNode exp = ParseExpressionNode(parentName);
            if (exp != null)
            {
                result = CheckForSemiColon();
                if (result)
                {
                    CCloseFileNode crfile = new CCloseFileNode();
                    crfile.Expression = exp;
                    crfile.IsInLibrary = tokeniser.InIncludeFile();
                    parentNode.AddStatement(crfile);
                }
            }

            return result;
        }

        private ExpressionNode ParseComparativeExpression(String parentName)
        {
            ExpressionNode exp = null;
            exp = ParseArithmeticExpression(parentName);
            if (exp != null)
            {
                bool bDone = false;
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                while (!bDone)
                {
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.Assignment)
                        {
                            ReportSyntaxError("= found in logical expression, do you mean '==' ?");
                            bDone = true;
                        }
                        else
                            if (tokenType == Tokeniser.TokenType.Equality)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseArithmeticExpression(parentName);
                            if (RightNode != null)
                            {
                                EqualityNode Andexp = new EqualityNode();
                                Andexp.LeftNode = LeftNode;
                                Andexp.RightNode = RightNode;
                                Andexp.IsInLibrary = tokeniser.InIncludeFile();
                                exp = Andexp;
                            }
                        }
                        else
                                if (tokenType == Tokeniser.TokenType.InEquality)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseArithmeticExpression(parentName);
                            if (RightNode != null)
                            {
                                CInEqualityNode Orexp = new CInEqualityNode();
                                Orexp.LeftNode = LeftNode;
                                Orexp.RightNode = RightNode;
                                exp = Orexp;
                            }
                        }
                        else
                                    if (tokenType == Tokeniser.TokenType.LessThan)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseArithmeticExpression(parentName);
                            if (RightNode != null)
                            {
                                LessThanNode Orexp = new LessThanNode();
                                Orexp.LeftNode = LeftNode;
                                Orexp.RightNode = RightNode;
                                exp = Orexp;
                            }
                        }
                        else
                                        if (tokenType == Tokeniser.TokenType.LessThanOrEqual)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseArithmeticExpression(parentName);
                            if (RightNode != null)
                            {
                                LessThanOrEqualToNode Orexp = new LessThanOrEqualToNode();
                                Orexp.LeftNode = LeftNode;
                                Orexp.RightNode = RightNode;
                                exp = Orexp;
                            }
                        }
                        else
                                            if (tokenType == Tokeniser.TokenType.GreaterThan)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseArithmeticExpression(parentName);
                            if (RightNode != null)
                            {
                                CGreaterThanNode Orexp = new CGreaterThanNode();
                                Orexp.LeftNode = LeftNode;
                                Orexp.RightNode = RightNode;
                                exp = Orexp;
                            }
                        }
                        else
                                                if (tokenType == Tokeniser.TokenType.GreaterThanOrEqual)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseArithmeticExpression(parentName);
                            if (RightNode != null)
                            {
                                CGreaterThanOrEqualToNode Orexp = new CGreaterThanOrEqualToNode();
                                Orexp.LeftNode = LeftNode;
                                Orexp.RightNode = RightNode;
                                exp = Orexp;
                            }
                        }
                        else
                        {
                            tokeniser.PutTokenBack();
                            bDone = true;
                        }
                    }
                }
            }
            return exp;
        }

        private bool ParseCompoundNode(CompoundNode CompNode, String parentName, bool IsTestBody)
        {
            //
            // The opening curly should have processed already
            // Just parse any statements until we see a close bracket.
            bool result = true;

            //
            // Fetch the first non comment token
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;

            bool bDone = false;

            do
            {
                //
                // Look for the closing bracket of the body
                //
                if (tokeniser.GetToken(out token, out tokenType) == true)
                {
                    // System.Diagnostics.Debug.WriteLine(token);
                    if (tokenType == Tokeniser.TokenType.CloseCurly)
                    {
                        bDone = true;
                    }
                    else
                    {
                        //
                        // Put the token back so someone else can deal with it.
                        //
                        tokeniser.PutTokenBack();

                        //
                        // Try parsing a statement
                        //
                        result = ParseStatement(CompNode, parentName, IsTestBody);
                    }
                }
                else
                {
                    ReportSyntaxError("Unexpected end of file processing compound statement " + parentName + " Missing }?");
                    result = false;
                }
            } while ((!bDone) && (tokenType != Tokeniser.TokenType.None) && (result == true));
            return result;
        }

        private ExpressionNode ParseCopyFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode leftSolid = ParseExpressionNode(parentName);
            if (leftSolid != null)
            {
                CopySolidNode mn = new CopySolidNode(leftSolid);
                exp = mn;
                mn.IsInLibrary = tokeniser.InIncludeFile();
            }
            return exp;
        }

        private ExpressionNode ParseCutoutFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode leftSolid = ParseExpressionNode(parentName);
            if (leftSolid != null)
            {
                if (CheckForComma())
                {
                    ExpressionNode rightSolid = ParseExpressionNode(parentName);
                    if (rightSolid != null)
                    {
                        CSGNode mn = new CSGNode(leftSolid, rightSolid, "groupcut");
                        exp = mn;
                        mn.IsInLibrary = tokeniser.InIncludeFile();
                    }
                }
            }
            return exp;
        }

        private bool ParseDeleteStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            string label = "Delete";
            SolidDeleteNode asn = new SolidDeleteNode();
            result = ParseSolidStatement(parentNode, parentName, label, 1, asn);
            return result;
        }

        private ExpressionNode ParseDifferenceFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode leftSolid = ParseExpressionNode(parentName);
            if (leftSolid != null)
            {
                if (CheckForComma())
                {
                    ExpressionNode rightSolid = ParseExpressionNode(parentName);
                    if (rightSolid != null)
                    {
                        CSGNode mn = new CSGNode(leftSolid, rightSolid, "groupdifference");
                        exp = mn;
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseDimFunction(string parentName)
        {
            ExpressionNode exp = null;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                ArraySymbol sym = SymbolTable.Instance().FindArraySymbol(parentName + token);
                if (sym != null)
                {
                    DimNode dm = new DimNode();
                    dm.Symbol = sym;
                    dm.ArrayName = token;
                    dm.IsInLibrary = tokeniser.InIncludeFile();
                    exp = dm;
                }
                else
                {
                    ReportSyntaxError("Dim expected array name");
                }
            }
            return exp;
        }

        private bool ParseDoubleStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;

            //
            // See if there is an identifier following the double
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "[")
                {
                    // array
                    ParseArrayDeclaration(parentNode, parentName, ref result, ref token, ref tokenType, SymbolTable.SymbolType.doublearrayvariable);
                }
                else
                {
                    DoubleDeclarationNode node = new DoubleDeclarationNode();
                    result = ParseSingleDeclaration(parentNode, parentName, ref token, tokenType, node, SymbolTable.SymbolType.doublevariable, "Double");
                }
            }
            return result;
        }

        private bool ParseExitStatement(CompoundNode parentNode)
        {
            bool result = CheckForSemiColon();
            if (result)
            {
                ExitNode en = new ExitNode();
                en.IsInLibrary = tokeniser.InIncludeFile();
                parentNode.AddStatement(en);
            }

            return result;
        }

        private ExpressionNode ParseExpressionForCallNode(string parentName)
        {
            ExpressionNode exp = null;
            // get token
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)

            {
                string internalName = parentName + token;
                string externalName = token;
                Symbol sym = SymbolTable.Instance().FindSymbol(internalName, SymbolTable.SymbolType.structname);
                if (sym != null)
                {
                    exp = ParseStructSymbolForCall(token, parentName, sym as StructSymbol);
                }
                else
                {
                    sym = SymbolTable.Instance().FindArraySymbol(internalName);
                    if (sym != null)
                    {
                        if (FetchToken(out token, out tokenType) == true)

                        {
                            // is it trying to pass a whole array or just an element
                            if (token != "[")
                            {
                                tokeniser.PutTokenBack();
                                exp = ParseArraySymbolForCall(externalName, parentName, sym as ArraySymbol);
                            }
                            else
                            {
                                exp = ParseArrayVariableNode(externalName, parentName);
                            }
                        }
                    }
                    else
                    {
                        tokeniser.PutTokenBack();
                        exp = ParseExpressionNode(parentName);
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseExpressionNode(String parentName)
        {
            ExpressionNode exp = null;
            exp = ParseLogicalExpression(parentName);
            return exp;
        }

        private ExpressionNode ParseFactor(String parentName)
        {
            ExpressionNode exp = null;

            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    //
                    // We have a name, is it a built in function or a variable
                    //
                    if (IsIntrinsicFunction(token) == true)
                    {
                        exp = ParseIntrinsicFunction(token, parentName);
                    }
                    else
                    if (IsUserFunction(token) == true)
                    {
                        exp = ParseUserFunctionCall(token, parentName);
                    }
                    else
                    {
                        string vname = token;
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            // if its a variable name followed by a dot its probably a field
                            if (tokenType == Tokeniser.TokenType.Dot)
                            {
                                exp = ParseFieldNode(vname, parentName);
                            }
                            else
                            if (tokenType == Tokeniser.TokenType.OpenSquare)
                            {
                                exp = ParseArrayVariableNode(vname, parentName);
                            }
                            else
                            {
                                // not a field
                                tokeniser.PutTokenBack();
                                exp = ParseVariableNode(vname, parentName);
                            }
                        }
                    }
                }
                else if (tokenType == Tokeniser.TokenType.Number)
                {
                    exp = ParseNumericConstant(token);
                }
                else if (tokenType == Tokeniser.TokenType.HexNumber)
                {
                    exp = ParseHexNumericConstant(token);
                }
                else if (tokenType == Tokeniser.TokenType.QuotedString)
                {
                    exp = ParseStringConstant(token);
                }
                else if (tokenType == Tokeniser.TokenType.Subtraction)
                {
                    exp = ParseUnaryMinus(parentName);
                }
                else if (tokenType == Tokeniser.TokenType.OpenBracket)
                {
                    exp = ParseParenthesis(parentName);
                }
                else if (tokenType == Tokeniser.TokenType.False)
                {
                    exp = ParseFalse();
                }
                else if (tokenType == Tokeniser.TokenType.True)
                {
                    exp = ParseTrue();
                }
            }

            return exp;
        }

        private ExpressionNode ParseFalse()
        {
            ExpressionNode exp = new FalseConstantNode();
            exp.IsInLibrary = tokeniser.InIncludeFile();
            return exp;
        }

        private ExpressionNode ParseFieldNode(string vname, string parentName)
        {
            ExpressionNode exp = null;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    //
                    // Check for local variable
                    //
                    String ExternalName = vname;
                    String strTarget = parentName + vname;
                    SymbolTable.SymbolType typ = SymbolTable.Instance().FindSymbol(strTarget);
                    if (typ != SymbolTable.SymbolType.unknown)
                    {
                        FieldNode vn = new FieldNode();
                        vn.ObjectSymbolName = strTarget;
                        vn.ExternalObjectName = ExternalName;
                        vn.FieldName = token;
                        vn.Symbol = SymbolTable.Instance().FindSymbol(strTarget, typ);
                        vn.IsInLibrary = tokeniser.InIncludeFile();
                        exp = vn;
                    }
                    else
                    {
                        //
                        // Try to match up with a global
                        //
                        typ = SymbolTable.Instance().FindSymbol(vname);
                        if (typ != SymbolTable.SymbolType.unknown)
                        {
                            FieldNode vn = new FieldNode();
                            vn.ObjectSymbolName = strTarget;
                            vn.Symbol = SymbolTable.Instance().FindSymbol(vname, typ);
                            vn.ExternalObjectName = ExternalName;
                            vn.FieldName = token;
                            exp = vn;
                        }
                        else
                        {
                            ReportSyntaxError("Unidentified variable " + vname);
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseFileExistsFunction(string parentName)
        {
            return GetFunctionNode<FileExistsNode>(parentName);
        }

        private bool ParseFloorStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            string label = "Floor";
            SolidFloorNode asn = new SolidFloorNode();
            result = ParseSolidStatement(parentNode, parentName, label, 1, asn);
            return result;
        }

        private bool ParseForStatement(CompoundNode parentNode, string parentName)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            //
            // Fetch token which should be a name
            //
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    String localName = token;
                    String strVariableName = parentName + token;                   //
                                                                                   //
                                                                                   // Make sure it has already been declared
                                                                                   //
                    if (SymbolTable.Instance().FindSymbol(strVariableName) != SymbolTable.SymbolType.unknown)
                    {
                        //
                        // next thing should be =
                        //
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            if (tokenType == Tokeniser.TokenType.Assignment)
                            {
                                ExpressionNode StartExpression = ParseExpressionNode(parentName);
                                if (StartExpression != null)
                                {
                                    //
                                    // Next thing should be "TO"
                                    //
                                    if (FetchToken(out token, out tokenType) == true)
                                    {
                                        if (token == "to")
                                        {
                                            ExpressionNode EndExpression = ParseExpressionNode(parentName);
                                            if (EndExpression != null)
                                            {
                                                //
                                                // Now it gets complicated
                                                // if the next word is Step then there is a step expression
                                                // if its { there is no step
                                                //
                                                if (FetchToken(out token, out tokenType) == true)
                                                {
                                                    if (tokenType == Tokeniser.TokenType.OpenCurly)
                                                    {
                                                        CompoundNode comp = new CompoundNode();
                                                        if (ParseCompoundNode(comp, parentName, false))
                                                        {
                                                            CForNode fnode = new CForNode();
                                                            fnode.VariableName = strVariableName;
                                                            fnode.LocalName = localName;
                                                            fnode.StartExpression = StartExpression;
                                                            fnode.EndExpression = EndExpression;
                                                            fnode.Body = comp;
                                                            fnode.StepExpression = null;
                                                            parentNode.AddStatement(fnode);
                                                            result = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (token == "step")
                                                        {
                                                            ExpressionNode StepExpression = ParseExpressionNode(parentName);
                                                            if (StepExpression != null)
                                                            {
                                                                if (FetchToken(out token, out tokenType) == true)
                                                                {
                                                                    if (tokenType == Tokeniser.TokenType.OpenCurly)
                                                                    {
                                                                        CompoundNode comp = new CompoundNode();
                                                                        if (ParseCompoundNode(comp, parentName, false))
                                                                        {
                                                                            CForNode fnode = new CForNode();
                                                                            fnode.VariableName = strVariableName;
                                                                            fnode.LocalName = localName;
                                                                            fnode.StartExpression = StartExpression;
                                                                            fnode.EndExpression = EndExpression;
                                                                            fnode.Body = comp;
                                                                            fnode.StepExpression = StepExpression;
                                                                            parentNode.AddStatement(fnode);

                                                                            result = true;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ReportSyntaxError("Syntax error in for");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ReportSyntaxError("For expected to, found " + token);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ReportSyntaxError("For expected = found " + token);
                            }
                        }
                    }
                    else
                    {
                        ReportSyntaxError("For Loop expected variable name found " + token);
                    }
                }
            }

            return result;
        }

        private bool ParseFunctionDeclarationStatement(CompoundNode parentNode, string parentName)
        {
            bool result = false;
            bool bInLibrary = tokeniser.InIncludeFile();
            string returnTypeName = "";
            //
            // Format is
            // Function <type> <Name>( [Parameter List] );
            //
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            String token;
            //
            // Fetch token which should be a name
            //
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    string typeId = token;
                    //
                    // This token must be a type
                    // ie int, double,bool,string,handle,testtarget
                    //
                    bool isArray = false;
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (token == "[")
                        {
                            isArray = true;
                            if (!FetchToken(out token, out tokenType) || token != "]")
                            {
                            }
                        }
                        else
                        {
                            tokeniser.PutTokenBack();
                        }
                        SymbolTable.SymbolType FunctionType = SymbolTable.SymbolType.unknown;
                        returnTypeName = TitleCase(typeId);
                        switch (returnTypeName)
                        {
                            case "Int":
                                {
                                    FunctionType = isArray ? SymbolTable.SymbolType.intarrayvariable : SymbolTable.SymbolType.intvariable;
                                }
                                break;

                            case "Double":
                                {
                                    FunctionType = isArray ? SymbolTable.SymbolType.doublearrayvariable : SymbolTable.SymbolType.doublevariable;
                                }
                                break;

                            case "String":
                                {
                                    FunctionType = isArray ? SymbolTable.SymbolType.stringarrayvariable : SymbolTable.SymbolType.stringvariable;
                                }
                                break;

                            case "Bool":
                                {
                                    FunctionType = isArray ? SymbolTable.SymbolType.boolarrayvariable : SymbolTable.SymbolType.boolvariable;
                                }
                                break;

                            case "Solid":
                                {
                                    FunctionType = isArray ? SymbolTable.SymbolType.solidarrayvariable : SymbolTable.SymbolType.solidvariable;
                                }
                                break;

                            case "Handle":
                                {
                                    FunctionType = isArray ? SymbolTable.SymbolType.handlearrayvariable : SymbolTable.SymbolType.handlevariable;
                                }
                                break;
                        }

                        if (FunctionType == SymbolTable.SymbolType.unknown)
                        {
                            //Have a look to see if its a struct
                            string tmp = TitleCase(typeId);
                            StructDefinition def = StructDefinitiontTable.Instance().FindStruct(tmp);
                            if (def != null)
                            {
                                FunctionType = SymbolTable.SymbolType.structname;
                            }
                        }

                        if (FunctionType == SymbolTable.SymbolType.unknown)
                        {
                            ReportSyntaxError("Function declaration expected return type, found " + typeId);
                        }
                        else
                        {
                            if (FetchToken(out token, out tokenType) == true)
                            {
                                if (tokenType == Tokeniser.TokenType.Identifier)
                                {
                                    string id = token.ToLower();
                                    if (IsKeyword(id) || IsIntrinsicFunction(id))
                                    {
                                        ReportSyntaxError($"cant use keyword/function {token} in declaration");
                                    }
                                    else
                                    {
                                        String FunctionName = token;
                                        //
                                        // Should be an open bracket
                                        //
                                        if (FetchToken(out token, out tokenType) == true)
                                        {
                                            if (tokenType == Tokeniser.TokenType.OpenBracket)
                                            {
                                                CFunctionNode proc = new CFunctionNode();
                                                if (ParseProcedureParameters(proc, FunctionName))
                                                {
                                                    if (FetchToken(out token, out tokenType) == true)
                                                    {
                                                        if (tokenType == Tokeniser.TokenType.CloseBracket)
                                                        {
                                                            if (FetchToken(out token, out tokenType) == true)
                                                            {
                                                                if (tokenType == Tokeniser.TokenType.OpenCurly)
                                                                {
                                                                    //
                                                                    // Try parsing the body
                                                                    //
                                                                    FunctionBodyNode cmp = new FunctionBodyNode();
                                                                    if (ParseCompoundNode(cmp, FunctionName, false))
                                                                    {
                                                                        proc.Name = FunctionName;
                                                                        proc.Body = cmp;
                                                                        proc.ReturnType = FunctionType;
                                                                        proc.ReturnTypeName = returnTypeName;
                                                                        proc.IsInLibrary = bInLibrary;

                                                                        // It goes in a separate procedures list
                                                                        //
                                                                        FunctionCache.Instance().AddFunction(proc);

                                                                        parentNode.AddStatement(proc);
                                                                        //
                                                                        // add a reference to the symbol table
                                                                        //
                                                                        SymbolTable.Instance().AddSymbol(FunctionName, SymbolTable.SymbolType.functionname);

                                                                        result = true;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    ReportSyntaxError("Function declaration expected { but found " + token);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ReportSyntaxError("Function declaration expected ) but found " + token);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ReportSyntaxError("Function declaration expected ( but found " + token);
                                            }
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

        private ExpressionNode ParseGetSolidSizeFunction(string parentName, string prim)
        {
            ExpressionNode exp = null;
            ExpressionNode leftSolid = ParseExpressionNode(parentName);
            if (leftSolid != null)
            {
                GetSolidSizeNode mn = new GetSolidSizeNode(leftSolid, prim);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }
            return exp;
        }

        private bool ParseHandleStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;

            //
            // See if there is an identifier following the name
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "[")
                {
                    // array
                    ParseArrayDeclaration(parentNode, parentName, ref result, ref token, ref tokenType, SymbolTable.SymbolType.handlearrayvariable);
                }
                else
                {
                    HandleDeclarationNode node = new HandleDeclarationNode();
                    result = ParseSingleDeclaration(parentNode, parentName, ref token, tokenType, node, SymbolTable.SymbolType.handlevariable, "Handle");
                }
            }
            return result;
        }

        private ExpressionNode ParseHexNumericConstant(String token)
        {
            ExpressionNode exp = null;
            token = token.ToLower().Replace("0x", "");
            int Val;
            if (HexToInt(token, out Val))
            {
                IntConstantNode nd = new IntConstantNode();
                nd.Value = Val;
                exp = nd;
            }
            return exp;
        }

        private bool ParseIdentifiedStatement(string Identifier, CompoundNode parentNode, String parentName)
        {
            bool result = false;
            switch (Identifier)
            {
                case "alignleft":
                case "alignright":
                case "aligntop":
                case "alignbottom":
                case "alignfront":
                case "alignback":
                case "aligncentre":
                    {
                        result = ParseAlignStatement(parentNode, parentName, Identifier);
                    }
                    break;

                case "stackleft":
                case "stackright":
                case "stackabove":
                case "stackbelow":
                case "stackfront":
                case "stackbehind":

                    {
                        result = ParseStackStatement(parentNode, parentName, Identifier);
                    }
                    break;

                case "break":
                    {
                        result = ParseBreakStatement(parentNode);
                    }
                    break;

                case "bool":
                    {
                        result = ParseBoolStatement(parentNode, parentName);
                    }
                    break;

                case "delete":
                    {
                        result = ParseDeleteStatement(parentNode, parentName);
                    }
                    break;

                case "double":
                    {
                        result = ParseDoubleStatement(parentNode, parentName);
                    }
                    break;

                case "exit":
                    {
                        result = ParseExitStatement(parentNode);
                    }
                    break;

                case "floor":
                    {
                        result = ParseFloorStatement(parentNode, parentName);
                    }
                    break;

                case "for":
                    {
                        result = ParseForStatement(parentNode, parentName);
                    }
                    break;

                case "function":
                    {
                        result = ParseFunctionDeclarationStatement(parentNode, parentName);
                    }
                    break;

                case "handle":
                    {
                        result = ParseHandleStatement(parentNode, parentName);
                    }
                    break;

                case "move":
                    {
                        result = ParseMoveStatement(parentNode, parentName);
                    }
                    break;

                case "rmove":
                    {
                        result = ParseRMoveStatement(parentNode, parentName);
                    }
                    break;

                case "resize":
                    {
                        result = ParseResizeStatement(parentNode, parentName);
                    }
                    break;

                case "return":
                    {
                        result = ParseReturnStatement(parentNode, parentName);
                    }
                    break;

                case "rotate":
                    {
                        result = ParseRotateStatement(parentNode, parentName);
                    }
                    break;

                case "setcolour":
                    {
                        result = ParseSetColourStatement(parentNode, parentName);
                    }
                    break;

                case "setname":
                    {
                        result = ParseSetNameStatement(parentNode, parentName);
                    }
                    break;

                case "solid":
                    {
                        result = ParseSolidStatement(parentNode, parentName);
                    }
                    break;

                case "if":
                    {
                        result = ParseIfStatement(parentNode, parentName);
                    }
                    break;

                case "int":
                    {
                        result = ParseIntStatement(parentNode, parentName);
                    }
                    break;

                case "procedure":
                    {
                        result = ParseProcedureStatement(parentNode);
                    }
                    break;

                case "print":
                    {
                        result = ParsePrintStatement(parentNode, parentName);
                    }
                    break;

                case "string":
                    {
                        result = ParseStringStatement(parentNode, parentName);
                    }
                    break;

                case "struct":
                    {
                        result = ParseStructStatement(parentNode);
                    }
                    break;

                case "include":
                    {
                        result = ParseIncludeStatement(parentNode);
                    }
                    break;

                case "while":
                    {
                        result = ParseWhileStatement(parentNode, parentName);
                    }
                    break;

                default:
                    {
                        ReportSyntaxError("Unexpected keyword " + Identifier);
                    }
                    break;
            }

            return result;
        }

        private bool ParseIfStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.OpenBracket)
                {
                    ExpressionNode exp = ParseExpressionNode(parentName);
                    if (exp != null)
                    {
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            if (tokenType == Tokeniser.TokenType.CloseBracket)
                            {
                                if (FetchToken(out token, out tokenType) == true)
                                {
                                    if (tokenType == Tokeniser.TokenType.OpenCurly)
                                    {
                                        CompoundNode cmp = new CompoundNode();
                                        if (ParseCompoundNode(cmp, parentName, false))
                                        {
                                            CIfNode wn = new CIfNode();
                                            wn.Expression = exp;
                                            wn.TrueBody = cmp;
                                            wn.FalseBody = null;
                                            wn.IsInLibrary = tokeniser.InIncludeFile();
                                            parentNode.AddStatement(wn);
                                            result = true;

                                            //
                                            // Take a sneaky peek to see if the next token is an else
                                            //

                                            //   if (FetchToken(out token, out tokenType) == true)
                                            if (tokeniser.GetToken(out token, out tokenType) == true)
                                            {
                                                if (token == "else")
                                                {
                                                    if (FetchToken(out token, out tokenType) == true)
                                                    {
                                                        if (tokenType == Tokeniser.TokenType.OpenCurly)
                                                        {
                                                            CompoundNode elsecmp = new CompoundNode();
                                                            if (ParseCompoundNode(elsecmp, parentName, false))
                                                            {
                                                                wn.FalseBody = elsecmp;
                                                            }
                                                            else
                                                            {
                                                                result = false;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //
                                                    // Not an error, just no else present.
                                                    //
                                                    tokeniser.PutTokenBack();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ReportSyntaxError("if expected {, found " + token);
                                    }
                                }
                            }
                            else
                            {
                                ReportSyntaxError("if expected ), found " + token);
                            }
                        }
                        else
                        {
                            ReportSyntaxError("if expected )");
                        }
                    }
                }
            }
            return result;
        }

        private bool ParseIncludeStatement(CompoundNode parentNode)
        {
            bool result = false;
            //
            // This parse function is different from the rest in that "Use"
            // does not create a run time object. Its more of a interpreter directive
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            //
            // Fetch token which should be a name
            //
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.QuotedString)
                {
                    IncludeNode node = new IncludeNode();
                    node.Path = token;
                    node.IsInLibrary = tokeniser.InIncludeFile();
                    parentNode.AddUseStatement(node);

                    if (CheckForSemiColon())
                    {
                        token = token.Substring(1, token.Length - 2);

                        // Ask the tokeniser to start reading from the new path.
                        // Once its done it will revert back to the current one
                        // so in theory they can be be stacked quite deep
                        if (tokeniser.SetSource(token))
                        {
                            result = true;
                        }
                        else
                        {
                            ReportSyntaxError("Can't open Include file " + token);
                        }
                    }
                }
                else
                {
                    ReportSyntaxError("Include expected quoted string");
                }
            }

            return result;
        }

        private ExpressionNode ParseInputStringFunction(String parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode TextExpression = ParseExpressionNode(parentName);
            if (TextExpression != null)
            {
                CInputStringNode val = new CInputStringNode();
                val.Expression = TextExpression;
                exp = val;
            }

            return exp;
        }

        private ExpressionNode ParseInsertPartFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode param = ParseExpressionNode(parentName);
            if (param != null)
            {
                InsertPartNode mn = new InsertPartNode(param);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }
            return exp;
        }

        private ExpressionNode ParseIntersectionFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode leftSolid = ParseExpressionNode(parentName);
            if (leftSolid != null)
            {
                if (CheckForComma())
                {
                    ExpressionNode rightSolid = ParseExpressionNode(parentName);
                    if (rightSolid != null)
                    {
                        CSGNode mn = new CSGNode(leftSolid, rightSolid, "groupintersection");
                        mn.IsInLibrary = tokeniser.InIncludeFile();
                        exp = mn;
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseIntrinsicFunction(string functionName, String parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode resultNode = null;

            //
            // If this is a function call the next token should be bracket
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.OpenBracket)
                {
                    functionName = functionName.ToLower();
                    switch (functionName)
                    {
                        case "abs":
                            {
                                exp = GetFunctionNode<AbsNode>(parentName);
                            }
                            break;

                        case "atan":
                            {
                                exp = GetFunctionNode<AtanNode>(parentName);
                            }
                            break;

                        case "cos":
                            {
                                exp = GetFunctionNode<CosNode>(parentName);
                            }
                            break;

                        case "cutout":
                            {
                                exp = ParseCutoutFunction(parentName);
                            }
                            break;

                        case "copy":
                            {
                                exp = ParseCopyFunction(parentName);
                            }
                            break;

                        case "degrees":
                            {
                                exp = GetFunctionNode<DegreesNode>(parentName);
                            }
                            break;

                        case "difference":
                            {
                                exp = ParseDifferenceFunction(parentName);
                            }
                            break;

                        case "dim":
                            {
                                exp = ParseDimFunction(parentName);
                            }
                            break;

                        case "inputstring":
                            {
                                exp = ParseInputStringFunction(parentName);
                            }
                            break;

                        case "insertpart":
                            {
                                exp = ParseInsertPartFunction(parentName);
                            }
                            break;

                        case "intersection":
                            {
                                exp = ParseIntersectionFunction(parentName);
                            }
                            break;

                        case "len":
                            {
                                exp = GetFunctionNode<LenNode>(parentName);
                            }
                            break;

                        case "make":
                            {
                                exp = ParseMakeFunction(parentName);
                            }
                            break;

                        case "makebicorn":
                            {
                                exp = ParseMakeBicornFunction(parentName);
                            }
                            break;

                        case "makedualprofile":
                            {
                                exp = ParseMakeDualProfileFunction(parentName);
                            }
                            break;

                        case "makebricktower":
                            {
                                exp = ParseMakeBrickTowerFunction(parentName);
                            }
                            break;

                        case "makebrickwall":
                            {
                                exp = ParseMakeBrickWallFunction(parentName);
                            }
                            break;

                        case "makeshapedbrickwall":
                            {
                                exp = ParseMakeShapedBrickWallFunction(parentName);
                            }
                            break;

                        case "makereuleaux":
                            {
                                exp = ParseMakeReuleauxFunction(parentName);
                            }
                            break;

                        case "makehollow":
                            {
                                exp = ParseMakeHollowFunction(parentName);
                            }
                            break;

                        case "makerailwheel":
                            {
                                exp = ParseMakeRailWheelFunction(parentName);
                            }
                            break;

                        case "makewagonwheel":
                            {
                                exp = ParseMakeWagonWheelFunction(parentName);
                            }
                            break;

                        case "makepath":
                            {
                                exp = ParseMakePathFunction(parentName);
                            }
                            break;

                        case "makeparallelogram":
                            {
                                exp = ParseMakeParallelogramFunction(parentName);
                            }
                            break;

                        case "makeparabolicdish":
                            {
                                exp = ParseMakeParabolicDishFunction(parentName);
                            }
                            break;

                        case "makepathloft":
                            {
                                exp = ParseMakePathLoftFunction(parentName);
                            }
                            break;

                        case "makepie":
                            {
                                exp = ParseMakePieFunction(parentName);
                            }
                            break;

                        case "makepill":
                            {
                                exp = ParseMakePillFunction(parentName);
                            }
                            break;

                        case "makeplankwall":
                            {
                                exp = ParseMakePlankWallFunction(parentName);
                            }
                            break;

                        case "makeplatelet":
                            {
                                exp = ParseMakePlateletFunction(parentName);
                            }
                            break;

                        case "makepulley":
                            {
                                exp = ParseMakePulleyFunction(parentName);
                            }
                            break;

                        case "makeroofridge":
                            {
                                exp = ParseMakeRoofRidgeFunction(parentName);
                            }
                            break;

                        case "makespring":
                            {
                                exp = ParseMakeSpringFunction(parentName);
                            }
                            break;

                        case "makestadium":
                            {
                                exp = ParseMakeStadiumFunction(parentName);
                            }
                            break;

                        case "makesquaredstadium":
                            {
                                exp = ParseMakeSquaredStadiumFunction(parentName);
                            }
                            break;

                        case "makesquirkle":
                            {
                                exp = ParseMakeSquirkleFunction(parentName);
                            }
                            break;

                        case "makestonewall":
                            {
                                exp = ParseMakeStoneWallFunction(parentName);
                            }
                            break;

                        case "maketext":
                            {
                                exp = ParseMakeTextFunction(parentName);
                            }
                            break;

                        case "maketiledroof":
                            {
                                exp = ParseMakeTiledRoofFunction(parentName);
                            }
                            break;

                        case "maketorus":
                            {
                                exp = ParseMakeTorusFunction(parentName);
                            }
                            break;

                        case "maketrapezoid":
                            {
                                exp = ParseMakeTrapezoidFunction(parentName);
                            }
                            break;

                        case "maketrickle":
                            {
                                exp = ParseMakeTrickleFunction(parentName);
                            }
                            break;

                        case "makecurvedfunnel":
                            {
                                exp = ParseMakeCurvedFunnelFunction(parentName);
                            }
                            break;

                        case "maketube":
                            {
                                exp = ParseMakeTubeFunction(parentName);
                            }
                            break;

                        case "maketexturedtube":
                            {
                                exp = ParseMakeTexturedTubeFunction(parentName);
                            }
                            break;

                        case "now":
                            {
                                exp = ParseNowFunction(parentName);
                            }
                            break;

                        case "pcname":
                            {
                                exp = ParsePCNameFunction(parentName);
                            }
                            break;

                        case "pos":
                            {
                                exp = ParsePosFunction(parentName);
                            }
                            break;

                        case "rad":
                            {
                                exp = GetFunctionNode<RadNode>(parentName);
                            }
                            break;

                        case "rnd":
                            {
                                exp = GetFunctionNode<RndNode>(parentName);
                            }
                            break;

                        case "replaceall":
                            {
                                exp = ParseReplaceAllFunction(parentName);
                            }
                            break;

                        case "sin":
                            {
                                exp = GetFunctionNode<SinNode>(parentName);
                            }
                            break;

                        case "sqrt":
                            {
                                exp = GetFunctionNode<SqrtNode>(parentName);
                            }
                            break;

                        case "str":
                            {
                                exp = GetFunctionNode<StrNode>(parentName);
                            }
                            break;

                        case "substring":
                            {
                                exp = ParseSubStringFunction(parentName);
                            }
                            break;

                        case "tan":
                            {
                                exp = GetFunctionNode<TanNode>(parentName);
                            }
                            break;

                        case "trim":
                            {
                                exp = GetFunctionNode<TrimNode>(parentName);
                            }
                            break;

                        case "trimleft":
                            {
                                exp = GetFunctionNode<TrimLeftNode>(parentName);
                            }
                            break;

                        case "trimright":
                            {
                                exp = GetFunctionNode<TrimRightNode>(parentName);
                            }
                            break;

                        case "union":
                            {
                                exp = ParseUnionFunction(parentName);
                            }
                            break;

                        case "unionall":
                            {
                                exp = ParseUnionAllFunction(parentName);
                            }
                            break;

                        case "val":
                            {
                                exp = GetFunctionNode<ValNode>(parentName);
                            }
                            break;

                        case "validsolid":
                            {
                                exp = GetFunctionNode<ValidSolidNode>(parentName);
                            }
                            break;
                        case "length":
                        case "width":
                        case "height":
                            {
                                exp = ParseGetSolidSizeFunction(parentName, functionName);
                            }
                            break;
                    }

                    //
                    // The next token should be close bracket
                    //
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.CloseBracket)
                        {
                            if (exp != null)
                            {
                                resultNode = exp;
                            }
                        }
                        else
                        {
                            if (lastError == "")
                            {
                                ReportSyntaxError("Function " + functionName + " Expected )");
                            }
                        }
                    }
                }
                else
                {
                    ReportSyntaxError("Function " + functionName + " Expected (");
                }
            }
            else
            {
                ReportSyntaxError("Function " + functionName + " Expected (");
            }
            if (resultNode != null)
            {
                resultNode.IsInLibrary = tokeniser.InIncludeFile();
            }
            return resultNode;
        }

        private ExpressionNode ParseMakePathLoftFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakePathLoft";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 1;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakePathLoftNode mn = new MakePathLoftNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeTexturedTubeFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeTexturedTube";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 8;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeTexturedTubeNode mn = new MakeTexturedTubeNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeTexturedDiskFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeTexturedDisk";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 6;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeTexturedDiskNode mn = new MakeTexturedDiskNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeRoofRidgeFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeRoofRidge";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 7;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeRoofRidgeNode mn = new MakeRoofRidgeNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private bool ParseIntStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;

            //
            // See if there is an identifier following the int
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "[")
                {
                    // array
                    ParseArrayDeclaration(parentNode, parentName, ref result, ref token, ref tokenType, SymbolTable.SymbolType.intarrayvariable);
                }
                else
                {
                    IntDeclarationNode node = new IntDeclarationNode();
                    result = ParseSingleDeclaration(parentNode, parentName, ref token, tokenType, node, SymbolTable.SymbolType.intvariable, "Int");
                }
            }
            return result;
        }

        private ExpressionNode ParseLogicalExpression(String parentName)
        {
            ExpressionNode exp = null;
            exp = ParseComparativeExpression(parentName);
            if (exp != null)
            {
                bool bDone = false;
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                while (!bDone)
                {
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.LogicalAnd)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseComparativeExpression(parentName);
                            if (RightNode != null)
                            {
                                AndNode Andexp = new AndNode();
                                Andexp.LeftNode = LeftNode;
                                Andexp.RightNode = RightNode;
                                Andexp.IsInLibrary = tokeniser.InIncludeFile();
                                exp = Andexp;
                            }
                        }
                        else
                            if (tokenType == Tokeniser.TokenType.LogicalOr)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseComparativeExpression(parentName);
                            if (RightNode != null)
                            {
                                OrNode Orexp = new OrNode();
                                Orexp.LeftNode = LeftNode;
                                Orexp.RightNode = RightNode;
                                exp = Orexp;
                            }
                        }
                        else
                        {
                            tokeniser.PutTokenBack();
                            bDone = true;
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakeBicornFunction(string parentName)
        {
            ExpressionNode exp = null;

            ExpressionNode r1Exp = ParseExpressionNode(parentName);
            if (r1Exp != null)
            {
                if (CheckForComma())

                {
                    ExpressionNode r2Exp = ParseExpressionNode(parentName);
                    if (r2Exp != null)
                    {
                        if (CheckForComma())
                        {
                            ExpressionNode hexp = ParseExpressionNode(parentName);
                            if (hexp != null)
                            {
                                if (CheckForComma())
                                {
                                    ExpressionNode gExp = ParseExpressionNode(parentName);
                                    if (gExp != null)
                                    {
                                        MakeBicornNode mn = new MakeBicornNode(r1Exp, r2Exp, hexp, gExp);
                                        mn.IsInLibrary = tokeniser.InIncludeFile();
                                        exp = mn;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakeBrickTowerFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeBrickTower";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 7;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeBrickTowerNode mn = new MakeBrickTowerNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeBrickWallFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeBrickWall";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 7;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeBrickWallNode mn = new MakeBrickWallNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeCurvedFunnelFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeCurvedFunnel";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 4;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeCurvedFunnelNode mn = new MakeCurvedFunnelNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeDualProfileFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeDualProfile";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 2;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeDualProfileNode mn = new MakeDualProfileNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode solidexp1 = ParseExpressionNode(parentName);
            if (solidexp1 != null)
            {
                if (CheckForComma())
                {
                    ExpressionNode xExp = ParseExpressionNode(parentName);
                    if (xExp != null)
                    {
                        if (CheckForComma())
                        {
                            ExpressionNode yExp = ParseExpressionNode(parentName);
                            if (yExp != null)
                            {
                                if (CheckForComma())
                                {
                                    ExpressionNode zExp = ParseExpressionNode(parentName);
                                    if (zExp != null)
                                    {
                                        if (CheckForComma())
                                        {
                                            ExpressionNode xSize = ParseExpressionNode(parentName);
                                            if (xSize != null)
                                            {
                                                if (CheckForComma())
                                                {
                                                    ExpressionNode ySize = ParseExpressionNode(parentName);
                                                    if (ySize != null)
                                                    {
                                                        if (CheckForComma())
                                                        {
                                                            ExpressionNode zSize = ParseExpressionNode(parentName);
                                                            if (zSize != null)
                                                            {
                                                                exp = new MakeNode(solidexp1, xExp, yExp, zExp, xSize, ySize, zSize);
                                                                exp.IsInLibrary = tokeniser.InIncludeFile();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakeHollowFunction(string parentName)
        {
            string label = "MakeHollow";
            ExpressionNode exp = null;
            ExpressionNode pointArrayExp = ParseExpressionForCallNode(parentName);
            if (pointArrayExp != null)
            {
                if (CheckForComma($"{label}"))
                {
                    ExpressionNode heightExp = ParseExpressionNode(parentName);
                    if (heightExp != null)
                    {
                        if (CheckForComma($"{label}"))
                        {
                            ExpressionNode thickExp = ParseExpressionNode(parentName);
                            if (thickExp != null)
                            {
                                MakeHollowNode mn = new MakeHollowNode(pointArrayExp, heightExp, thickExp);
                                mn.IsInLibrary = tokeniser.InIncludeFile();
                                exp = mn;
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakeParabolicDishFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeParabolicDish";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 3;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (!CheckForComma(label))
                        {
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeParabolicDishNode mn = new MakeParabolicDishNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeParallelogramFunction(string parentName)
        {
            ExpressionNode exp = null;

            ExpressionNode lengthExp = ParseExpressionNode(parentName);
            if (lengthExp != null)
            {
                if (CheckForComma("MakeParallelogram"))
                {
                    ExpressionNode heightExp = ParseExpressionNode(parentName);
                    if (heightExp != null)
                    {
                        if (CheckForComma("MakeParallelogram"))
                        {
                            ExpressionNode widthExp = ParseExpressionNode(parentName);
                            if (widthExp != null)
                            {
                                if (CheckForComma("MakeParallelogram") == false)
                                {
                                    ExpressionNode aExp = ParseExpressionNode(parentName);
                                    if (aExp != null)
                                    {
                                        if (CheckForComma("MakeParallelogram") == false)
                                        {
                                            ExpressionNode bExp = ParseExpressionNode(parentName);
                                            if (bExp != null)
                                            {
                                                MakeParallelogramNode mn = new MakeParallelogramNode(lengthExp, heightExp, widthExp, aExp, bExp);
                                                mn.IsInLibrary = tokeniser.InIncludeFile();
                                                exp = mn;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakePathFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode pathTextExp = ParseExpressionForCallNode(parentName);
            if (pathTextExp != null)
            {
                if (CheckForComma("MakePath"))
                {
                    ExpressionNode heightExp = ParseExpressionNode(parentName);
                    if (heightExp != null)
                    {
                        MakePathNode mn = new MakePathNode(pathTextExp, heightExp);
                        mn.IsInLibrary = tokeniser.InIncludeFile();
                        exp = mn;
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakePlankWallFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakePlankWall";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 6;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakePlankWallNode mn = new MakePlankWallNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakePlateletFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode pointArrayExp = ParseExpressionForCallNode(parentName);
            if (pointArrayExp != null)
            {
                if (CheckForComma("MakePlatlet"))
                {
                    ExpressionNode heightExp = ParseExpressionNode(parentName);
                    if (heightExp != null)
                    {
                        MakePlateletNode mn = new MakePlateletNode(pointArrayExp, heightExp);
                        mn.IsInLibrary = tokeniser.InIncludeFile();
                        exp = mn;
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakePulleyFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakePulley";

            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 6;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (!CheckForComma(label))
                        {
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakePulleyNode mn = new MakePulleyNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeRailWheelFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeRailWheel";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 7;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (!CheckForComma(label))
                        {
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeRailWheelNode mn = new MakeRailWheelNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeReuleauxFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeReuleaux";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 3;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (!CheckForComma(label))
                        {
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeReuleauxNode mn = new MakeReuleauxNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeShapedBrickWallFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeShapedBrickWall";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 6;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeShapedBrickWallNode mn = new MakeShapedBrickWallNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeShapedTiledRoofFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeShapedTiledRoof";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 5;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeShapedTiledRoofNode mn = new MakeShapedTiledRoofNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeSpringFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeSpring";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 6;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeSpringNode mn = new MakeSpringNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeSquaredStadiumFunction(string parentName)
        {
            ExpressionNode exp = null;
            string label = "MakeSquaredStadium";
            ExpressionNode radiusExp = ParseExpressionNode(parentName);
            if (radiusExp != null)
            {
                if (CheckForComma(label))
                {
                    ExpressionNode gapExp = ParseExpressionNode(parentName);
                    if (gapExp != null)
                    {
                        if (CheckForComma(label))
                        {
                            ExpressionNode elExp = ParseExpressionNode(parentName);
                            if (elExp != null)
                            {
                                if (CheckForComma(label))
                                {
                                    ExpressionNode hExp = ParseExpressionNode(parentName);
                                    if (hExp != null)
                                    {
                                        if (CheckForComma(label))
                                        {
                                            ExpressionNode oExp = ParseExpressionNode(parentName);
                                            if (oExp != null)
                                            {
                                                MakeSquaredStadiumNode mn = new MakeSquaredStadiumNode(radiusExp, gapExp, elExp, hExp, oExp);
                                                mn.IsInLibrary = tokeniser.InIncludeFile();
                                                exp = mn;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakeSquirkleFunction(string parentName)
        {
            string label = "MakeSquirkle";
            string error = "";
            ExpressionNode exp = null;
            error = "Top Left expression";
            ExpressionNode tlExp = ParseExpressionNode(parentName);
            if (tlExp != null)
            {
                if (CheckForComma(label))
                {
                    error = "Top Right expression";
                    ExpressionNode trExp = ParseExpressionNode(parentName);
                    if (trExp != null)
                    {
                        if (CheckForComma(label))
                        {
                            error = "Bottom Left expression";
                            ExpressionNode blExp = ParseExpressionNode(parentName);
                            if (blExp != null)
                            {
                                if (CheckForComma(label))
                                {
                                    error = "Bottom Right expression";
                                    ExpressionNode brExp = ParseExpressionNode(parentName);
                                    if (brExp != null)
                                    {
                                        if (CheckForComma(label) == false)
                                        {
                                            error = "Length expression";
                                            ExpressionNode lExp = ParseExpressionNode(parentName);
                                            if (lExp != null)
                                            {
                                                if (CheckForComma(label) == false)
                                                {
                                                    error = "Height expression";
                                                    ExpressionNode hExp = ParseExpressionNode(parentName);
                                                    if (hExp != null)
                                                    {
                                                        if (CheckForComma(label))
                                                        {
                                                            error = "Width expression";
                                                            ExpressionNode dExp = ParseExpressionNode(parentName);
                                                            if (dExp != null)
                                                            {
                                                                MakeSquirkleNode mn = new MakeSquirkleNode(tlExp, trExp, blExp, brExp, lExp, hExp, dExp);
                                                                mn.IsInLibrary = tokeniser.InIncludeFile();
                                                                exp = mn;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (exp == null && error != "")
            {
                ReportSyntaxError($"{label} error {error}");
            }
            return exp;
        }

        private ExpressionNode ParseMakeStadiumFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode shapeExp = ParseExpressionNode(parentName);
            if (shapeExp != null)
            {
                if (CheckForComma() == false)
                {
                    ReportSyntaxError("MakeStadium expected ,");
                }
                else
                {
                    ExpressionNode r1Exp = ParseExpressionNode(parentName);
                    if (r1Exp != null)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError("MakeStadium expected ,");
                        }
                        else
                        {
                            ExpressionNode r2Exp = ParseExpressionNode(parentName);
                            if (r2Exp != null)
                            {
                                if (CheckForComma() == false)
                                {
                                    ReportSyntaxError("MakeStadium expected ,");
                                }
                                else
                                {
                                    ExpressionNode gexp = ParseExpressionNode(parentName);
                                    if (gexp != null)
                                    {
                                        if (CheckForComma() == false)
                                        {
                                            ReportSyntaxError("MakeStadium expected ,");
                                        }
                                        else
                                        {
                                            ExpressionNode hExp = ParseExpressionNode(parentName);
                                            if (hExp != null)
                                            {
                                                MakeStadiumNode mn = new MakeStadiumNode(shapeExp, r1Exp, r2Exp, gexp, hExp);
                                                mn.IsInLibrary = tokeniser.InIncludeFile();
                                                exp = mn;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakeStoneWallFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeStoneWall";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 4;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeStoneWallNode mn = new MakeStoneWallNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeTextFunction(string parentName)
        {
            string label = "MakeText";
            string error = "";
            ExpressionNode exp = null;
            ExpressionCollection ecol = new ExpressionCollection();
            error = "Text expression";

            ExpressionNode tmp = ParseExpressionNode(parentName);
            if (tmp != null)
            {
                ecol.Add(tmp);
                if (CheckForComma(label))
                {
                    error = "Font Name expression";
                    tmp = ParseExpressionNode(parentName);
                    if (tmp != null)
                    {
                        ecol.Add(tmp);
                        if (CheckForComma(label))
                        {
                            error = "Font Size expression";
                            tmp = ParseExpressionNode(parentName);
                            if (tmp != null)
                            {
                                ecol.Add(tmp);
                                if (CheckForComma(label))
                                {
                                    error = "Thickness expression";
                                    tmp = ParseExpressionNode(parentName);
                                    if (tmp != null)
                                    {
                                        ecol.Add(tmp);
                                        if (CheckForComma(label))
                                        {
                                            error = "Bold expression";
                                            tmp = ParseExpressionNode(parentName);
                                            if (tmp != null)
                                            {
                                                ecol.Add(tmp);
                                                if (CheckForComma(label))
                                                {
                                                    error = "Italic expression";
                                                    tmp = ParseExpressionNode(parentName);
                                                    if (tmp != null)
                                                    {
                                                        ecol.Add(tmp);

                                                        MakeTextNode mn = new MakeTextNode(ecol);
                                                        mn.IsInLibrary = tokeniser.InIncludeFile();
                                                        exp = mn;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (exp == null && error != "")
            {
                ReportSyntaxError($"{label} error {error}");
            }
            return exp;
        }

        private ExpressionNode ParseMakeTiledRoofFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeTiledRoof";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 8;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeTiledRoofNode mn = new MakeTiledRoofNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeTorusFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode mrExp = ParseExpressionNode(parentName);
            if (mrExp != null)
            {
                if (CheckForComma() == false)
                {
                    ReportSyntaxError("MakeTorus expected ,");
                }
                else
                {
                    ExpressionNode hr = ParseExpressionNode(parentName);
                    if (hr != null)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError("MakeTorus expected ,");
                        }
                        else
                        {
                            ExpressionNode vr = ParseExpressionNode(parentName);
                            if (vr != null)
                            {
                                if (CheckForComma() == false)
                                {
                                    ReportSyntaxError("MakeTorus expected ,");
                                }
                                else
                                {
                                    ExpressionNode curveExp = ParseExpressionNode(parentName);
                                    if (curveExp != null)
                                    {
                                        if (CheckForComma() == false)
                                        {
                                            ReportSyntaxError("MakeTorus expected ,");
                                        }
                                        else
                                        {
                                            ExpressionNode knobExp = ParseExpressionNode(parentName);
                                            if (knobExp != null)
                                            {
                                                if (CheckForComma() == false)
                                                {
                                                    ReportSyntaxError("MakeTorus expected ,");
                                                }
                                                else
                                                {
                                                    ExpressionNode hExp = ParseExpressionNode(parentName);
                                                    if (hExp != null)
                                                    {
                                                        MakeTorusNode mn = new MakeTorusNode(mrExp, hr, vr, curveExp, knobExp, hExp);
                                                        exp = mn;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakeTrapezoidFunction(string parentName)
        {
            ExpressionNode exp = null;
            string commaMess = "MakeTrapezoid expected ,";
            ExpressionNode topLengthExp = ParseExpressionNode(parentName);
            if (topLengthExp != null)
            {
                if (CheckForComma() == false)
                {
                    ReportSyntaxError(commaMess);
                }
                else
                {
                    ExpressionNode bottomLengthExp = ParseExpressionNode(parentName);
                    if (bottomLengthExp != null)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaMess);
                        }
                        else
                        {
                            ExpressionNode heightExp = ParseExpressionNode(parentName);
                            if (heightExp != null)
                            {
                                if (CheckForComma() == false)
                                {
                                    ReportSyntaxError(commaMess);
                                }
                                else
                                {
                                    ExpressionNode widthExp = ParseExpressionNode(parentName);
                                    if (widthExp != null)
                                    {
                                        if (CheckForComma() == false)
                                        {
                                            ReportSyntaxError(commaMess);
                                        }
                                        else
                                        {
                                            ExpressionNode bExp = ParseExpressionNode(parentName);
                                            if (bExp != null)
                                            {
                                                MakeTrapezoidNode mn = new MakeTrapezoidNode(topLengthExp, bottomLengthExp, heightExp, widthExp, bExp);
                                                mn.IsInLibrary = tokeniser.InIncludeFile();
                                                exp = mn;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakeTrickleFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeTrickle";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 3;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeTrickleNode mn = new MakeTrickleNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private ExpressionNode ParseMakeTubeFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode radiusExp = ParseExpressionNode(parentName);
            if (radiusExp != null)
            {
                if (CheckForComma() == false)
                {
                    ReportSyntaxError("MakeTube expected ,");
                }
                else
                {
                    ExpressionNode thExp = ParseExpressionNode(parentName);
                    if (thExp != null)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError("MakeTube expected ,");
                        }
                        else
                        {
                            ExpressionNode lowExp = ParseExpressionNode(parentName);
                            if (lowExp != null)
                            {
                                if (CheckForComma() == false)
                                {
                                    ReportSyntaxError("MakeTube expected ,");
                                }
                                else
                                {
                                    ExpressionNode upperExp = ParseExpressionNode(parentName);
                                    if (upperExp != null)
                                    {
                                        if (CheckForComma() == false)
                                        {
                                            ReportSyntaxError("MakeTube expected ,");
                                        }
                                        else
                                        {
                                            ExpressionNode hExp = ParseExpressionNode(parentName);
                                            if (hExp != null)
                                            {
                                                if (CheckForComma() == false)
                                                {
                                                    ReportSyntaxError("MakeTube expected ,");
                                                }
                                                else
                                                {
                                                    ExpressionNode sweepExp = ParseExpressionNode(parentName);
                                                    if (sweepExp != null)
                                                    {
                                                        MakeTubeNode mn = new MakeTubeNode(radiusExp, thExp, lowExp, upperExp, hExp, sweepExp);
                                                        exp = mn;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseMakeWagonWheelFunction(string parentName)
        {
            ExpressionNode exp = null;
            String label = "MakeWagonWheel";
            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();
            int exprCount = 8;

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                MakeWagonWheelNode mn = new MakeWagonWheelNode(coll);
                mn.IsInLibrary = tokeniser.InIncludeFile();
                exp = mn;
            }

            return exp;
        }

        private bool ParseMoveStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            string label = "Move";
            SolidMoveNode asn = new SolidMoveNode();
            result = ParseSolidStatement(parentNode, parentName, label, 4, asn);

            return result;
        }

        private ExpressionNode ParseMultiplicativeExpression(String parentName)
        {
            ExpressionNode exp = null;
            exp = ParseFactor(parentName);
            if (exp != null)
            {
                bool bDone = false;
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                while (!bDone)
                {
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.Multiplication)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseFactor(parentName);
                            if (RightNode != null)
                            {
                                MultiplicationNode Addexp = new MultiplicationNode();
                                Addexp.LeftNode = LeftNode;
                                Addexp.RightNode = RightNode;
                                Addexp.IsInLibrary = tokeniser.InIncludeFile();
                                exp = Addexp;
                            }
                        }
                        else
                            if (tokenType == Tokeniser.TokenType.Division)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseMultiplicativeExpression(parentName);
                            if (RightNode != null)
                            {
                                DivisionNode Subexp = new DivisionNode();
                                Subexp.LeftNode = LeftNode;
                                Subexp.RightNode = RightNode;
                                exp = Subexp;
                            }
                        }
                        else
                                if (tokenType == Tokeniser.TokenType.Mod)
                        {
                            ExpressionNode LeftNode = exp;
                            ExpressionNode RightNode = ParseMultiplicativeExpression(parentName);
                            if (RightNode != null)
                            {
                                ModNode Subexp = new ModNode();
                                Subexp.LeftNode = LeftNode;
                                Subexp.RightNode = RightNode;
                                exp = Subexp;
                            }
                        }
                        else
                        {
                            tokeniser.PutTokenBack();
                            bDone = true;
                        }
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseNowFunction(string parentName)
        {
            ExpressionNode exp = null;
            NowNode nd = new NowNode();
            nd.IsInLibrary = tokeniser.InIncludeFile();
            exp = nd;
            return exp;
        }

        private ExpressionNode ParseNumericConstant(String token)
        {
            ExpressionNode exp = null;
            //
            // Is it a double or an int
            //
            if (token.IndexOf(".") > -1)
            {
                try
                {
                    DoubleConstantNode nd = new DoubleConstantNode();
                    nd.Value = Convert.ToDouble(token);
                    nd.IsInLibrary = tokeniser.InIncludeFile();
                    exp = nd;
                }
                catch
                {
                    ReportSyntaxError("Invalid double constant");
                }
            }
            else
            {
                try
                {
                    IntConstantNode nd = new IntConstantNode();
                    nd.Value = Convert.ToInt32(token);
                    exp = nd;
                }
                catch
                {
                    ReportSyntaxError("Invalid int constant");
                }
            }
            return exp;
        }

        private bool ParseParam(SymbolTable.SymbolType symbolType, ProcedureNode proc, string strFunctionName)
        {
            bool result = false;
            String token = "";
            // we already know the type so look for a name
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "[")
                {
                    result = ParseArrayParam(symbolType, proc, strFunctionName);
                }
                else
                {
                    if (tokenType == Tokeniser.TokenType.Identifier)
                    {
                        String strParamName = strFunctionName + token;
                        String ExternalName = token;
                        // if its not a duplicate then add it to the symbol table
                        if (SymbolTable.Instance().FindSymbol(strParamName) == SymbolTable.SymbolType.unknown)
                        {
                            Symbol Symbol = SymbolTable.Instance().AddSymbol(strParamName, symbolType);
                            proc.AddParameter(strParamName, symbolType);

                            result = true;
                        }
                        else
                        {
                            ReportSyntaxError("Duplicate parameter name " + token);
                        }
                    }
                    else
                    {
                        ReportSyntaxError("Expected parameter name, found " + token);
                    }
                }
            }

            return result;
        }

        private ExpressionNode ParseParenthesis(String parentName)
        {
            ExpressionNode exp = null;

            ExpressionNode contents = ParseExpressionNode(parentName);
            if (contents != null)
            {
                String token = "";
                Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (tokenType == Tokeniser.TokenType.CloseBracket)
                    {
                        ParenthesisNode pn = new ParenthesisNode();
                        pn.Expression = contents;
                        pn.IsInLibrary = tokeniser.InIncludeFile();
                        exp = pn;
                    }
                    else
                    {
                        ReportSyntaxError("Closing bracket missing from expression");
                    }
                }
                else
                {
                    ReportSyntaxError("Unexpected eof in expression");
                }
            }
            return exp;
        }

        private ExpressionNode ParsePCNameFunction(string parentName)
        {
            ExpressionNode exp = null;
            GetPCNameNode nd = new GetPCNameNode();
            nd.IsInLibrary = tokeniser.InIncludeFile();
            exp = nd;
            return exp;
        }

        private ExpressionNode ParsePosFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode TextExpression = ParseExpressionNode(parentName);
            if (TextExpression != null)
            {
                if (CheckForComma() == false)
                {
                    ReportSyntaxError("Pos function expected ,");
                }
                else
                {
                    ExpressionNode SearchExpression = ParseExpressionNode(parentName);
                    if (SearchExpression != null)
                    {
                        IndexNode pos = new IndexNode();
                        pos.Expression = TextExpression;
                        pos.SearchExpression = SearchExpression;
                        pos.IsInLibrary = tokeniser.InIncludeFile();
                        exp = pos;
                    }
                }
            }

            return exp;
        }

        private bool ParsePrintStatement(CompoundNode parentNode, String parentName)
        {
            //
            // Syntax Report <Expression>[,Expression...];
            //
            bool result = false;
            PrintNode rep = new PrintNode();
            rep.IsInLibrary = tokeniser.InIncludeFile();
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;

            bool bDone = false;
            do
            {
                //
                // See if it there is a nice expression
                //
                ExpressionNode exp = ParseExpressionNode(parentName);
                if (exp != null)
                {
                    rep.AddExpression(exp);

                    //
                    // If there is a comma there should another expression
                    //
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.Comma)
                        {
                            bDone = false;
                        }
                        else if (tokenType == Tokeniser.TokenType.SemiColon)
                        {
                            parentNode.AddStatement(rep);
                            result = true;

                            bDone = true;
                        }
                        else
                        {
                            ReportSyntaxError("Unexpected token processing Print expect , or ; found " + token);
                            bDone = true;
                        }
                    }
                    else
                    {
                        ReportSyntaxError("Unexpected end of file processing Print");
                        bDone = true;
                    }
                }
                else
                {
                    bDone = true;
                }
            } while (bDone == false);

            return result;
        }

        private bool ParseProcedureCall(string Identifier, CompoundNode parentNode, string parentName)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (SymbolTable.Instance().FindSymbol(Identifier) == SymbolTable.SymbolType.procedurename)
            {
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (tokenType == Tokeniser.TokenType.OpenBracket)
                    {
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            CallProcedureNode call = new CallProcedureNode();
                            call.ProcedureName = TitleCase(Identifier);
                            call.IsInLibrary = tokeniser.InIncludeFile();
                            //
                            // special case, no parameters i.e. MyProc()
                            //
                            if (tokenType == Tokeniser.TokenType.CloseBracket)
                            {
                                if (CheckForSemiColon())
                                {
                                    parentNode.AddStatement(call);
                                    result = true;
                                }
                            }
                            else
                            {
                                tokeniser.PutTokenBack();

                                bool bDone = false;
                                do
                                {
                                    //
                                    // See if it there is a nice expression
                                    //
                                    ExpressionNode exp = ParseExpressionForCallNode(parentName);
                                    if (exp != null)
                                    {
                                        call.AddParameterExpression(exp);

                                        //
                                        // If there is a comma there should another expression
                                        //
                                        if (FetchToken(out token, out tokenType) == true)
                                        {
                                            if (tokenType == Tokeniser.TokenType.Comma)
                                            {
                                                bDone = false;
                                            }
                                            else if (tokenType == Tokeniser.TokenType.CloseBracket)
                                            {
                                                if (CheckForSemiColon())
                                                {
                                                    parentNode.AddStatement(call);
                                                    result = true;
                                                }

                                                bDone = true;
                                            }
                                            else
                                            {
                                                ReportSyntaxError("Unexpected token processing function call. Expected ) found " + token);
                                            }
                                        }
                                        else
                                        {
                                            ReportSyntaxError("Unexpected end of file ");
                                        }
                                    }
                                    else
                                    {
                                        bDone = true;
                                    }
                                } while (bDone == false);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private bool ParseProcedureParameters(ProcedureNode proc, string strFunctionName)
        {
            bool result = true;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            bool bDone = false;

            while (!bDone && result)
            {
                bDone = true;
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (tokenType == Tokeniser.TokenType.Identifier)
                    {
                        string tmp = TitleCase(token);
                        StructDefinition def = StructDefinitiontTable.Instance().FindStruct(tmp);
                        if (def != null)
                        {
                            result = ParseStructParam(proc, strFunctionName, def);
                            bDone = false;
                        }
                        else
                        {
                            switch (token)
                            {
                                case "int":
                                    {
                                        result = ParseParam(SymbolTable.SymbolType.intvariable, proc, strFunctionName);
                                        bDone = false;
                                    }
                                    break;

                                case "double":
                                    {
                                        result = ParseParam(SymbolTable.SymbolType.doublevariable, proc, strFunctionName);
                                        bDone = false;
                                    }
                                    break;

                                case "string":
                                    {
                                        result = ParseParam(SymbolTable.SymbolType.stringvariable, proc, strFunctionName);
                                        bDone = false;
                                    }
                                    break;

                                case "bool":
                                    {
                                        result = ParseParam(SymbolTable.SymbolType.boolvariable, proc, strFunctionName);
                                        bDone = false;
                                    }
                                    break;

                                case "solid":
                                    {
                                        result = ParseParam(SymbolTable.SymbolType.solidvariable, proc, strFunctionName);
                                        bDone = false;
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (tokenType == Tokeniser.TokenType.CloseBracket)
                        {
                            tokeniser.PutTokenBack();
                        }
                        else
                        {
                            if (tokenType == Tokeniser.TokenType.Comma)
                            {
                                // Just eat it and go round again
                                bDone = false;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private bool ParseProcedureStatement(CompoundNode parentNode)
        {
            //
            // Note this is handling a procedure declaration NOT a call
            //
            // only Basic syntax at the moment
            // procedure name()
            // {
            // }
            bool result = false;
            String token = "";
            bool bInLibrary = tokeniser.InIncludeFile();

            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            //
            // Fetch token which should be a name
            //
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    String strFunctionName = TitleCase(token);
                    //
                    // Make sure it hasn't aleady been declared
                    //
                    if (SymbolTable.Instance().FindSymbol(strFunctionName) == SymbolTable.SymbolType.unknown)
                    {
                        //
                        // Should be an open bracket
                        //
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            if (tokenType == Tokeniser.TokenType.OpenBracket)
                            {
                                ProcedureNode proc = new ProcedureNode();
                                if (ParseProcedureParameters(proc, strFunctionName))
                                {
                                    if (FetchToken(out token, out tokenType) == true)
                                    {
                                        if (tokenType == Tokeniser.TokenType.CloseBracket)
                                        {
                                            if (FetchToken(out token, out tokenType) == true)
                                            {
                                                if (tokenType == Tokeniser.TokenType.OpenCurly)
                                                {
                                                    //
                                                    // Try parsing the body
                                                    //
                                                    CompoundNode cmp = new CompoundNode();

                                                    if (ParseCompoundNode(cmp, strFunctionName, false))
                                                    {
                                                        proc.Name = strFunctionName;
                                                        proc.Body = cmp;
                                                        proc.ReturnType = SymbolTable.SymbolType.unknown;
                                                        proc.IsInLibrary = bInLibrary;

                                                        //
                                                        // Dont add this procedure to the parent body.
                                                        // It goes in a separate procedures list
                                                        //
                                                        CProcedureCache.Instance().AddProcedure(proc);

                                                        parentNode.AddStatement(proc);
                                                        //
                                                        // add a reference to the symbol table
                                                        //
                                                        SymbolTable.Instance().AddSymbol(strFunctionName, SymbolTable.SymbolType.procedurename);

                                                        result = true;
                                                    }
                                                }
                                                else
                                                {
                                                    ReportSyntaxError("Procedure declaration expected { but found " + token);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ReportSyntaxError("Procedure declaration expected ) but found " + token);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ReportSyntaxError("Procedure declaration expected ( but found " + token);
                            }
                        }
                    }
                    else
                    {
                        ReportSyntaxError("Multiple declaration of " + token);
                    }
                }
                else
                {
                    ReportSyntaxError("Procedure declaration expected name but found " + token);
                }
            }
            return result;
        }

        private bool ParseProgramBlock(Script script)
        {
            bool result = false;

            //
            // Fetch the first non comment token
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                String Identifier = token.ToLower();
                if (Identifier == "program")
                {
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.QuotedString)
                        {
                            CProgramNode tn = new CProgramNode();
                            tn.Name = token;
                            if (ParseBody(tn))
                            {
                                script.AddNode(tn);
                                result = true;
                            }
                        }
                        else
                        {
                            ReportSyntaxError("Expected Quoted String found " + token);
                        }
                    }
                }
                else
                {
                    ReportSyntaxError("Expected Program found" + token);
                }
            }
            return result;
        }

        private ExpressionNode ParseReplaceAllFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode TextExpression = ParseExpressionNode(parentName);
            if (TextExpression != null)
            {
                if (CheckForComma() == false)
                {
                    ReportSyntaxError("ReplaceAll expected ,");
                }
                else
                {
                    ExpressionNode OldCharsExpression = ParseExpressionNode(parentName);
                    if (OldCharsExpression != null)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError("ReplaceAll expected ,");
                        }
                        else
                        {
                            ExpressionNode NewCharsExpression = ParseExpressionNode(parentName);
                            if (NewCharsExpression != null)
                            {
                                ReplaceAllNode val = new ReplaceAllNode();
                                val.TextExpression = TextExpression;
                                val.OldExpression = OldCharsExpression;
                                val.NewExpression = NewCharsExpression;

                                val.IsInLibrary = tokeniser.InIncludeFile();
                                exp = val;
                            }
                        }
                    }
                }
            }

            return exp;
        }

        private bool ParseResizeStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            string label = "Resize";
            SolidResizeNode asn = new SolidResizeNode();
            result = ParseSolidStatement(parentNode, parentName, label, 4, asn);
            return result;
        }

        private bool ParseReturnStatement(CompoundNode parentNode, string parentName)
        {
            bool result = false;
            //
            // Format is
            // Return Expression;
            //
            ExpressionNode exp = ParseExpressionNode(parentName);
            if (exp != null)
            {
                result = CheckForSemiColon();
                if (!result)
                {
                    ReportSyntaxError("Return expected ;");
                }
                else
                {
                    ReturnNode ret = new ReturnNode();
                    ret.Expression = exp;
                    ret.IsInLibrary = tokeniser.InIncludeFile();
                    parentNode.AddStatement(ret);
                    ret.Parent = parentNode;
                }
            }

            return result;
        }

        private bool ParseRMoveStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            string label = "RMove";
            SolidRMoveNode asn = new SolidRMoveNode();
            result = ParseSolidStatement(parentNode, parentName, label, 4, asn);

            return result;
        }

        private bool ParseRotateStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            string label = "Rotate";
            SolidRotateNode asn = new SolidRotateNode();
            result = ParseSolidStatement(parentNode, parentName, label, 4, asn);
            return result;
        }

        private bool ParseSetColourStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            string label = "SetColour";
            SolidColourNode asn = new SolidColourNode();
            result = ParseSolidStatement(parentNode, parentName, label, 5, asn);
            return result;
        }

        private bool ParseSetNameStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            string label = "SetName";
            SolidNameNode asn = new SolidNameNode();
            result = ParseSolidStatement(parentNode, parentName, label, 2, asn);
            return result;
        }

        private void ParseSingleBoolDeclaration(CompoundNode parentNode, string parentName, ref bool result, ref string token, Tokeniser.TokenType tokenType)
        {
            CBoolDeclarationNode node = new CBoolDeclarationNode();
            result = ParseSingleDeclaration(parentNode, parentName, ref token, tokenType, node, SymbolTable.SymbolType.boolvariable, "Bool");
        }

        private bool ParseSingleDeclaration(CompoundNode parentNode, string parentName, ref string token, Tokeniser.TokenType tokenType, DeclarationNode node, SymbolTable.SymbolType st, string label)
        {
            bool result = false;
            if (tokenType == Tokeniser.TokenType.Identifier)
            {
                // cant use keywords or function names as variables
                string id = token.ToLower();
                if (IsIntrinsicFunction(id) || IsKeyword(id) || IsUserFunction(id))
                {
                    ReportSyntaxError($"{label} can't use keyword/function {token} in declaration");
                }
                else
                {
                    //
                    // To avoid local variable names clashing with other variables
                    // we prefix them with the parent [procedures name
                    //
                    String strVarName = token;
                    token = parentName + token;
                    if (parentNode.FindSymbol(token) != SymbolTable.SymbolType.unknown)
                    {
                        ReportSyntaxError($"{label} Duplicate variable name");
                    }
                    else
                    {
                        Symbol sym = parentNode.AddSymbol(token, st);

                        node.VarName = strVarName;
                        node.IsInLibrary = tokeniser.InIncludeFile();
                        node.Name = token;
                        node.Symbol = sym;
                        parentNode.AddStatement(node);

                        if (CheckForInitialiser(parentNode, parentName, token, strVarName, node))
                        {
                            result = CheckForSemiColon();
                            if (!result)
                            {
                                ReportSyntaxError($"{label} expected ;");
                            }
                        }
                    }
                }
            }
            else
            {
                ReportSyntaxError($"{label} Expected variable name");
            }
            return result;
        }

        private bool ParseSingleStructVarDeclaraion(StructDefinition def, CompoundNode parentNode, string parentName)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            tokeniser.GetToken(out token, out tokenType);
            if (tokenType == Tokeniser.TokenType.Identifier)
            {
                string id = token.ToLower();
                if (IsKeyword(id) || IsIntrinsicFunction(id) || IsUserFunction(id))
                {
                    ReportSyntaxError($" can't use keyword/function {token} in declaration");
                }
                else
                {
                    String strVarName = token;
                    token = parentName + token;

                    if (parentNode.FindSymbol(token) != SymbolTable.SymbolType.unknown)
                    {
                        ReportSyntaxError("Duplicate variable name");
                    }
                    else
                    {
                        StructSymbol sym = new StructSymbol();
                        sym.Name = token;
                        sym.SymbolType = SymbolTable.SymbolType.structname;
                        sym.Definition = def;
                        sym.SetFields();

                        SymbolTable.Instance().AddStructSymbol(sym);

                        StructVarDeclarationNode node = new StructVarDeclarationNode();
                        node.VarName = strVarName;
                        node.IsInLibrary = tokeniser.InIncludeFile();
                        node.DeclarationType = def.StructName;
                        parentNode.AddStatement(node);

                        result = CheckForSemiColon();
                        if (!result)
                        {
                            ReportSyntaxError(" expected ;");
                        }
                    }
                }
            }
            else
                ReportSyntaxError("Expected variable name");

            return result;
        }

        private bool ParseSolidStatement(CompoundNode parentNode, string parentName, string label, int expectedExpressions, SolidStatement asn)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;

            bool bDone = false;

            asn.IsInLibrary = tokeniser.InIncludeFile();
            do
            {
                //
                // See if it there some  nice expressions
                //
                ExpressionNode exp = ParseExpressionNode(parentName);
                if (exp != null)
                {
                    asn.AddExpression(exp);

                    //
                    // If there is a comma there should another expression
                    //
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.Comma)
                        {
                            bDone = false;
                        }
                        else if (tokenType == Tokeniser.TokenType.SemiColon)
                        {
                            bDone = true;
                        }
                        else
                        {
                            ReportSyntaxError($"Unexpected token processing {label} expect , or ; found " + token);
                            bDone = true;
                        }
                    }
                    else
                    {
                        ReportSyntaxError($"Unexpected end of file processing {label}");
                        bDone = true;
                    }
                }
                else
                {
                    bDone = true;
                }
            } while (bDone == false);

            if (tokenType != Tokeniser.TokenType.SemiColon)
            {
                ReportSyntaxError($"{label} Expected ;");
            }
            else
            {
                if (asn.ExpressionCount == expectedExpressions)
                {
                    parentNode.AddStatement(asn);
                    result = true;
                }
                else
                {
                    ReportSyntaxError($"{label} wrong number of expressions");
                }
            }

            return result;
        }

        private bool ParseSolidStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;

            //
            // See if there is an identifier following the name
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "[")
                {
                    // array
                    ParseArrayDeclaration(parentNode, parentName, ref result, ref token, ref tokenType, SymbolTable.SymbolType.solidarrayvariable);
                }
                else
                {
                    SolidDeclarationNode node = new SolidDeclarationNode();
                    result = ParseSingleDeclaration(parentNode, parentName, ref token, tokenType, node, SymbolTable.SymbolType.solidvariable, "Solid");
                }
            }
            return result;
        }

        private bool ParseSource(Script script)
        {
            bool result = false;
            result = ParseProgramBlock(script);
            return result;
        }

        private bool ParseStackStatement(CompoundNode parentNode, String parentName, string id)
        {
            bool result = false;
            SolidAlignmentNode sal = new SolidAlignmentNode();
            sal.OrientationMode = SolidAlignmentNode.Mode.Stack;
            switch (id)
            {
                case "stackleft":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Left;
                    }
                    break;

                case "stackright":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Right;
                    }
                    break;

                case "stackabove":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Top;
                    }
                    break;

                case "stackbelow":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Bottom;
                    }
                    break;

                case "stackfront":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Front;
                    }
                    break;

                case "stackbehind":
                    {
                        sal.Orientation = SolidAlignmentNode.Alignment.Back;
                    }
                    break;
            }

            result = ParseSolidStatement(parentNode, parentName, sal.label, 2, sal);
            return result;
        }

        private bool ParseStatement(CompoundNode parentNode, String parentName, bool IsBody)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (tokeniser.GetToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    String Identifier = token.ToLower();

                    if (IsKeyword(Identifier))
                    {
                        result = ParseIdentifiedStatement(Identifier, parentNode, parentName);
                    }
                    else
                    {
                        result = ParseUnidentifiedStatement(Identifier, parentNode, parentName);
                    }
                }
                else
                {
                    if (tokenType == Tokeniser.TokenType.Comment)
                    {
                        bool bProcessingUseFile = tokeniser.InIncludeFile();
                        String CommentLine = tokeniser.GetToEndOfLine();

                        if (IsBody)
                        {
                            if (bProcessingUseFile == false)
                            {
                                CommentNode node = new CommentNode();
                                node.Text = CommentLine.Trim();
                                parentNode.AddStatement(node);
                            }
                        }
                        else
                        {
                            CommentNode node = new CommentNode();
                            node.Text = CommentLine.Trim();
                            parentNode.AddStatement(node);
                        }
                        result = true;
                    }
                    else
                    {
                        ReportSyntaxError("Unknown Statement type " + token);
                    }
                }
            }
            return result;
        }

        private ExpressionNode ParseStrFunction(String parentName)
        {
            return GetFunctionNode<StrNode>(parentName);
        }

        private ExpressionNode ParseStringConstant(String strText)
        {
            ExpressionNode exp = null;
            //
            // remove quotes
            //
            strText = strText.Substring(1);
            strText = strText.Substring(0, strText.Length - 1);
            StringConstantNode nd = new StringConstantNode();
            nd.Value = strText;
            nd.IsInLibrary = tokeniser.InIncludeFile();
            exp = nd;
            return exp;
        }

        private bool ParseStringStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;

            //
            // See if there is an identifier following the int
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "[")
                {
                    // array
                    ParseArrayDeclaration(parentNode, parentName, ref result, ref token, ref tokenType, SymbolTable.SymbolType.stringarrayvariable);
                }
                else
                {
                    StringDeclarationNode node = new StringDeclarationNode();
                    result = ParseSingleDeclaration(parentNode, parentName, ref token, tokenType, node, SymbolTable.SymbolType.stringvariable, "String");
                }
            }

            return result;
        }

        private bool ParseStructArrayVarDeclaration(StructDefinition def, CompoundNode parentNode, string parentName)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            ExpressionNode exp = ParseExpressionNode(parentName);
            if (exp == null)
            {
                tokeniser.PutTokenBack();
            }
            else
            {
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (token == "]")
                    {
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            if (tokenType == Tokeniser.TokenType.Identifier)
                            {
                                //
                                // To avoid local variable names clashing with other variables
                                // we prefix them with the parent [procedures name
                                //
                                String strVarName = token;
                                token = parentName + token;
                                if (parentNode.FindSymbol(token) != SymbolTable.SymbolType.unknown)
                                {
                                    ReportSyntaxError("Duplicate variable name");
                                }
                                else
                                {
                                    Symbol sym = parentNode.AddStructArraySymbol(token, def);

                                    StructArrayDeclarationNode node = new StructArrayDeclarationNode();
                                    node.VarName = strVarName;
                                    node.Structure = def;
                                    node.Dimensions = exp;
                                    node.ActualSymbol = sym;
                                    node.IsInLibrary = tokeniser.InIncludeFile();
                                    parentNode.AddStatement(node);
                                    /* can't cope with initialisers for struct arrays yet
                                    if (FetchToken(out token, out tokenType) == true)
                                    {
                                        if (token == "=")
                                        {
                                            if (FetchToken(out token, out tokenType) == true)
                                            {
                                                if (token == "{")
                                                {
                                                    bool bDone = false;
                                                    do
                                                    {
                                                        //
                                                        // See if it there is a nice expression
                                                        //
                                                        ExpressionNode iexp = ParseExpressionNode(parentName);
                                                        if (iexp != null)
                                                        {
                                                            node.AddInitialiserExpression(iexp);

                                                            //
                                                            // If there is a comma there should another expression
                                                            //
                                                            if (FetchToken(out token, out tokenType) == true)
                                                            {
                                                                if (tokenType == Tokeniser.TokenType.Comma)
                                                                {
                                                                    bDone = false;
                                                                }
                                                                else if (tokenType == Tokeniser.TokenType.CloseCurly)
                                                                {
                                                                    bDone = true;
                                                                }
                                                                else
                                                                {
                                                                    ReportSyntaxError("Unexpected token processing array initialiser Expected ) found " + token);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                ReportSyntaxError("Unexpected end of file ");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            bDone = true;
                                                        }
                                                    } while (bDone == false);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            tokeniser.PutTokenBack();
                                        }
                                        */
                                    result = CheckForSemiColon();
                                    if (!result)
                                    {
                                        ReportSyntaxError("Expected ;");
                                    }
                                }
                            }
                        }
                        else
                            ReportSyntaxError("Expected variable name");
                    }
                }
                else
                {
                    ReportSyntaxError("Expected ]");
                }
            }
            return result;
        }

        private ExpressionNode ParseStructArrayVarNode(ExpressionNode indexExp, StructArraySymbol arsym, string externalName, string internalName)
        {
            String token = "";
            ExpressionNode exp = null;
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                // so we have a struct array factor e.g. = colours[0] but is it a
                // field of the struct e.g. colours[0].R
                if (token == ".")
                {
                    // yes its a field
                    // get the field name
                    if (FetchToken(out token, out tokenType) == true)
                    {
                        if (tokenType == Tokeniser.TokenType.Identifier)
                        {
                            // does it match a field in the  in the struct
                            int field = arsym.Structure.FieldIndex(token);
                            if (field == -1)
                            {
                                ReportSyntaxError($"Field ${token} is not part of structure.");
                            }
                            else
                            {
                                StructArrayFieldNode vn = new StructArrayFieldNode();
                                vn.Name = internalName;
                                vn.ExternalName = externalName;
                                vn.Symbol = arsym;
                                vn.FieldName = token;
                                vn.FieldNumber = field;
                                vn.IndexExpression = indexExp;
                                vn.IsInLibrary = tokeniser.InIncludeFile();
                                exp = vn;
                            }
                        }
                    }
                }
                else
                {
                    // no its the whole struct
                    tokeniser.PutTokenBack();

                    StructArrayVariableNode vn = new StructArrayVariableNode();
                    vn.Name = internalName;
                    vn.ExternalName = externalName;
                    vn.Symbol = arsym;
                    vn.IndexExpression = indexExp;
                    vn.IsInLibrary = tokeniser.InIncludeFile();
                    exp = vn;
                }
            }
            return exp;
        }

        private bool ParseStructFieldAssignment(String leftidentifier, CompoundNode parentNode, String parentName)
        {
            // we expect a fieldname then an assignment
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    string rightidentifier = token.ToLower();

                    // see if the field name matches a field in the struct definition
                    bool foundfield = false;
                    Symbol sym = SymbolTable.Instance().FindSymbol(parentName + leftidentifier, SymbolTable.SymbolType.structname);
                    StructSymbol strsym = sym as StructSymbol;
                    string fieldName = "";
                    if (strsym != null)
                    {
                        foreach (Symbol fs in strsym.FieldValues.Symbols)
                        {
                            if (fs.Name.ToLower() == rightidentifier)
                            {
                                foundfield = true;
                                fieldName = fs.Name;
                                break;
                            }
                        }
                    }
                    if (!foundfield)
                    {
                        ReportSyntaxError("Expected field name , found " + token);
                    }
                    else
                    {
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            if (tokenType != Tokeniser.TokenType.Assignment)
                            {
                                ReportSyntaxError("Expected =");
                            }
                            else
                            {
                                ExpressionNode exp = ParseExpressionNode(parentName);
                                if (exp != null)
                                {
                                    result = CheckForSemiColon();
                                    if (!result)
                                    {
                                        ReportSyntaxError("Expected ;");
                                    }
                                    else
                                    {
                                        AssignStructFieldNode asn = new AssignStructFieldNode();
                                        asn.VariableName = parentName + leftidentifier;
                                        asn.ExternalName = leftidentifier;
                                        asn.FieldName = fieldName;
                                        asn.ExpressionNode = exp;
                                        asn.IsInLibrary = tokeniser.InIncludeFile();
                                        parentNode.AddStatement(asn);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ReportSyntaxError("Expected structure name on right side of assignment");
                }
            }
            return result;
        }

        private bool ParseStructParam(ProcedureNode proc, string functionName, StructDefinition def)
        {
            bool result = false;
            String token = "";
            // we already know the type so look for a name
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    String paramName = functionName + token;
                    String externalName = token;
                    // if its not a duplicate then add it to the symbol table
                    if (SymbolTable.Instance().FindSymbol(paramName) == SymbolTable.SymbolType.unknown)
                    {
                        Symbol symbol = SymbolTable.Instance().AddStructSymbol(paramName, def);
                        proc.AddStructParameter(paramName, def);

                        result = true;
                    }
                    else
                    {
                        ReportSyntaxError("Duplicate parameter name");
                    }
                }
                else
                {
                    ReportSyntaxError("Expected parameter name");
                }
            }

            return result;
        }

        private bool ParseStructStatement(CompoundNode parentNode)
        {
            //
            // Note this is handling a struct declaration NOT a reference
            //
            // only Basic syntax at the moment
            // struct name
            // {
            //   int x;
            //   double y;
            // }
            bool result = false;
            String token = "";
            bool bInLibrary = tokeniser.InIncludeFile();

            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            //
            // Fetch token which should be a name
            //
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    token = TitleCase(token);
                    //
                    // Make sure it hasn't aleady been declared
                    //
                    if (StructDefinitiontTable.Instance().FindStruct(token) == null)
                    {
                        String strStructName = token;

                        //
                        // Should be an open bracket
                        //
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            if (tokenType == Tokeniser.TokenType.OpenCurly)
                            {
                                //
                                // Try parsing the body
                                //
                                CompoundNode cmp = new CompoundNode();

                                if (ParseCompoundNode(cmp, strStructName, false))
                                {
                                    StructNode stNode = new StructNode();
                                    stNode.Name = strStructName;
                                    stNode.Body = cmp;

                                    //
                                    // Dont add this procedure to the parent body.
                                    // It goes in a separate structures list
                                    //
                                    StructDefinitiontTable.Instance().AddStruct(stNode);
                                    stNode.IsInLibrary = bInLibrary;
                                    parentNode.AddStatement(stNode);
                                    result = true;
                                }
                            }
                            else
                            {
                                ReportSyntaxError("Structure declaration expected { but found " + token);
                            }
                        }
                    }
                    else
                    {
                        ReportSyntaxError("Structure declaration expected name but found " + token);
                    }
                }
            }
            return result;
        }

        private ExpressionNode ParseStructSymbolForCall(string externalName, string parentName, StructSymbol structSymbol)
        {
            StructSymbolForCallNode exp = new StructSymbolForCallNode();
            exp.ExternalName = externalName;
            exp.Symbol = structSymbol;
            exp.Name = parentName + externalName;
            return exp;
        }

        private bool ParseStructVarDeclaration(StructDefinition def, CompoundNode parentNode, string parentName)
        {
            bool result = false;

            // See if there is an identifier
            //
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (token == "[")
                {
                    result = ParseStructArrayVarDeclaration(def, parentNode, parentName);
                }
                else
                {
                    tokeniser.PutTokenBack();
                    result = ParseSingleStructVarDeclaraion(def, parentNode, parentName);
                }
            }
            return result;
        }

        private ExpressionNode ParseSubStringFunction(string parentName)
        {
            //
            // Syntax is either
            // SubString( Source, Index )
            // SubString( Source, Index, Length )
            //
            ExpressionNode exp = null;
            ExpressionNode TextExpression = ParseExpressionNode(parentName);
            if (TextExpression != null)
            {
                if (CheckForComma() == false)
                {
                    ReportSyntaxError("Substring function expected ,");
                }
                else
                {
                    ExpressionNode IndexExpression = ParseExpressionNode(parentName);
                    if (IndexExpression != null)
                    {
                        if (CheckForComma() == false)
                        {
                            tokeniser.PutTokenBack();

                            SubStringNode pos = new SubStringNode();
                            pos.SourceExpression = TextExpression;
                            pos.IndexExpression = IndexExpression;
                            pos.IsInLibrary = tokeniser.InIncludeFile();
                            exp = pos;
                        }
                        else
                        {
                            ExpressionNode LengthExpression = ParseExpressionNode(parentName);
                            if (LengthExpression != null)
                            {
                                SubStringNode pos = new SubStringNode();
                                pos.SourceExpression = TextExpression;
                                pos.IndexExpression = IndexExpression;
                                pos.LengthExpression = LengthExpression;
                                exp = pos;
                            }
                        }
                    }
                }
            }

            return exp;
        }

        private ExpressionNode ParseTrue()
        {
            ExpressionNode exp = new TrueConstantNode();
            exp.IsInLibrary = tokeniser.InIncludeFile();
            return exp;
        }

        private ExpressionNode ParseUnaryMinus(String parentName)
        {
            //
            // A unary miinus as oppsed to a subtraction i.e.
            // x = -1;
            //
            ExpressionNode exp = null;
            ExpressionNode factor = ParseFactor(parentName);
            if (factor != null)
            {
                CUnaryMinusNode minus = new CUnaryMinusNode();
                minus.LeftNode = factor;
                minus.IsInLibrary = tokeniser.InIncludeFile();
                exp = minus;
            }
            return exp;
        }

        private bool ParseUnidentifiedStatement(string Identifier, CompoundNode parentNode, string parentName)
        {
            bool result = false;

            if (SymbolTable.Instance().FindSymbol(Identifier) == SymbolTable.SymbolType.procedurename)
            {
                result = ParseProcedureCall(Identifier, parentNode, parentName);
            }
            else
            {
                string tmp = TitleCase(Identifier);
                StructDefinition def = StructDefinitiontTable.Instance().FindStruct(tmp);
                if (def != null)
                {
                    result = ParseStructVarDeclaration(def, parentNode, parentName);
                }
                else
                {
                    if (SymbolTable.Instance().FindSymbol(Identifier) == SymbolTable.SymbolType.structname)
                    {
                        result = ParseAssignmentToStruct(Identifier, parentNode, "");
                    }
                    else
                    if (SymbolTable.Instance().FindSymbol(parentName + Identifier) == SymbolTable.SymbolType.structname)
                    {
                        result = ParseAssignmentToStruct(Identifier, parentNode, parentName);
                    }
                    else
                    if (SymbolTable.Instance().FindStructArraySymbol(parentName, Identifier) != null)
                    {
                        result = ParseAssignmentToStructArrayElement(Identifier, parentNode, parentName);
                    }
                    else
                    if (SymbolTable.Instance().FindArraySymbol(parentName, Identifier) != null)
                    {
                        result = ParseAssignmentToArrayElement(Identifier, parentNode, parentName);
                    }
                    else
                    {
                        result = ParseAssignment(Identifier, parentNode, parentName);
                    }
                }
            }
            return result;
        }

        private ExpressionNode ParseUnionAllFunction(string parentName)
        {
            ExpressionNode exp = null;
            UnionAllNode mn = new UnionAllNode();
            mn.IsInLibrary = tokeniser.InIncludeFile();
            exp = mn;
            return exp;
        }

        private ExpressionNode ParseUnionFunction(string parentName)
        {
            ExpressionNode exp = null;
            ExpressionNode leftSolid = ParseExpressionNode(parentName);
            if (leftSolid != null)
            {
                if (CheckForComma() == false)
                {
                    ReportSyntaxError("Union expected ,");
                }
                else
                {
                    ExpressionNode rightSolid = ParseExpressionNode(parentName);
                    if (rightSolid != null)
                    {
                        CSGNode mn = new CSGNode(leftSolid, rightSolid, "groupunion");
                        mn.IsInLibrary = tokeniser.InIncludeFile();
                        exp = mn;
                    }
                }
            }
            return exp;
        }

        private ExpressionNode ParseUserFunctionCall(string Identifier, string parentName)
        {
            ExpressionNode callexp = null;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (SymbolTable.Instance().FindSymbol(Identifier) == SymbolTable.SymbolType.functionname)
            {
                if (FetchToken(out token, out tokenType) == true)
                {
                    if (tokenType == Tokeniser.TokenType.OpenBracket)
                    {
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            CCallFunctionNode call = new CCallFunctionNode();
                            call.FunctionName = Identifier;
                            call.IsInLibrary = tokeniser.InIncludeFile();
                            //
                            // special case, no parameters i.e. MyProc()
                            //
                            if (tokenType == Tokeniser.TokenType.CloseBracket)
                            {
                                callexp = call;
                            }
                            else
                            {
                                tokeniser.PutTokenBack();

                                bool bDone = false;
                                do
                                {
                                    //
                                    // See if it there is a nice expression
                                    //
                                    ExpressionNode exp = ParseExpressionForCallNode(parentName);
                                    if (exp != null)
                                    {
                                        call.AddParameterExpression(exp);

                                        //
                                        // If there is a comma there should another expression
                                        //
                                        if (FetchToken(out token, out tokenType) == true)
                                        {
                                            if (tokenType == Tokeniser.TokenType.Comma)
                                            {
                                                bDone = false;
                                            }
                                            else if (tokenType == Tokeniser.TokenType.CloseBracket)
                                            {
                                                callexp = call;
                                                bDone = true;
                                            }
                                            else
                                            {
                                                ReportSyntaxError("Unexpected token processing function call. Expected ) found " + token);
                                            }
                                        }
                                        else
                                        {
                                            ReportSyntaxError("Unexpected end of file ");
                                        }
                                    }
                                    else
                                    {
                                        bDone = true;
                                    }
                                } while (bDone == false);
                            }
                        }
                    }
                }
            }
            return callexp;
        }

        private ExpressionNode ParseValFunction(String parentName)
        {
            return GetFunctionNode<ValNode>(parentName);
        }

        private ExpressionNode ParseVariableNode(String name, String parentName)
        {
            ExpressionNode exp = null;

            //
            // Check for local variable
            //
            String externalName = name;
            String internalName = name;
            bool fd = false;

            SymbolTable.SymbolType typ = SymbolTable.Instance().FindSymbol(parentName + name);
            if (typ != SymbolTable.SymbolType.unknown)
            {
                internalName = parentName + name;
                fd = true;
            }
            else
            {
                typ = SymbolTable.Instance().FindSymbol(externalName);
                if (typ != SymbolTable.SymbolType.unknown)
                {
                    internalName = externalName;
                    fd = true;
                }
            }
            if (fd)
            {
                VariableNode vn = new VariableNode();
                vn.Name = internalName;
                vn.ExternalName = externalName;
                vn.Symbol = SymbolTable.Instance().FindSymbol(internalName, typ);
                vn.IsInLibrary = tokeniser.InIncludeFile();
                exp = vn;
            }
            else
            {
                ReportSyntaxError("Unidentified variable " + name);
            }

            return exp;
        }

        private bool ParseWhileStatement(CompoundNode parentNode, String parentName)
        {
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.OpenBracket)
                {
                    ExpressionNode exp = ParseExpressionNode(parentName);
                    if (exp != null)
                    {
                        if (FetchToken(out token, out tokenType) == true)
                        {
                            if (tokenType == Tokeniser.TokenType.CloseBracket)
                            {
                                if (FetchToken(out token, out tokenType) == true)
                                {
                                    if (tokenType == Tokeniser.TokenType.OpenCurly)
                                    {
                                        CompoundNode cmp = new CompoundNode();
                                        if (ParseCompoundNode(cmp, parentName, false))
                                        {
                                            WhileNode wn = new WhileNode();
                                            wn.Expression = exp;
                                            wn.Body = cmp;
                                            wn.IsInLibrary = tokeniser.InIncludeFile();
                                            parentNode.AddStatement(wn);
                                            result = true;
                                        }
                                    }
                                    else
                                    {
                                        ReportSyntaxError("While expected {");
                                    }
                                }
                            }
                            else
                            {
                                ReportSyntaxError("While expected )");
                            }
                        }
                    }
                }
                else
                {
                    ReportSyntaxError("While expected (");
                }
            }
            return result;
        }

        private bool ParseWholeStructAssignment(String leftidentifier, CompoundNode parentNode, String parentName)
        {
            // we should either see  a struct variable name followed by a semi colon;
            // or an expression;
            bool result = false;
            String token = "";
            Tokeniser.TokenType tokenType = Tokeniser.TokenType.None;
            if (FetchToken(out token, out tokenType) == true)
            {
                if (tokenType == Tokeniser.TokenType.Identifier)
                {
                    string rightidentifier = "";
                    string rightlocal = token;

                    // look for the symbol as a local
                    bool foundSym = false;
                    if (parentNode.FindSymbol(parentName + token) == SymbolTable.SymbolType.structname)
                    {
                        foundSym = true;

                        rightidentifier = parentName + token;
                    }
                    else
                    if (parentNode.FindSymbol(token) == SymbolTable.SymbolType.structname)
                    {
                        // its a global
                        foundSym = true;
                        rightidentifier = token;
                    }

                    if (foundSym)
                    {
                        result = CheckForSemiColon();
                        if (!result)
                        {
                            ReportSyntaxError("Expected ;");
                        }
                        else
                        {
                            AssignWholeStructNode asn = new AssignWholeStructNode();
                            asn.LeftExternalName = leftidentifier;
                            asn.RightExternalName = rightlocal;
                            asn.LeftVariableName = parentName + leftidentifier;
                            asn.RightVariableName = rightidentifier;
                            asn.IsInLibrary = tokeniser.InIncludeFile();
                            parentNode.AddStatement(asn);
                        }
                    }
                    else
                    {
                        tokeniser.PutTokenBack();

                        // treat as struct = expression;
                        ExpressionNode exp = ParseExpressionNode(parentName);
                        if (exp != null)
                        {
                            result = CheckForSemiColon();
                            if (!result)
                            {
                                ReportSyntaxError("Expected ;");
                            }
                            else
                            {
                                AssignExpressionToStructNode asn = new AssignExpressionToStructNode();
                                asn.ExternalName = leftidentifier;

                                asn.VariableName = parentName + leftidentifier;

                                asn.ExpressionNode = exp;
                                parentNode.AddStatement(asn);
                            }
                        }
                    }
                }
                else
                {
                    ReportSyntaxError("Expected structure name on right side of assignment");
                }
            }
            return result;
        }

        private void ReportSyntaxError(string p)
        {
            if (lastError == "")
            {
                Log.Instance().AddEntry(tokeniser.GetSourceUpToIndex());
            }
            Log.Instance().AddEntry(p);
            lastError = p;
        }

        private string TitleCase(string str)
        {
            string res = "";
            if (str.Length > 0)
            {
                if (str.Length > 1)
                {
                    res = str.Substring(0, 1).ToUpper() + str.Substring(1);
                }
                else
                {
                    res = str.ToUpper();
                }
            }
            return res;
        }
    }
}