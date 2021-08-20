using System;
using System.Collections.Generic;

namespace ScriptLanguage
{
    internal class Tokeniser
    {
        private bool bHasPutBackToken;

        //private char[] m_Buffer;
        private char by;

        private SourceFileStackEntry CurrentSourceFile;

        private int CurrentSourceStackIndex;

        private List<SourceFileStackEntry> SourceFileStack;

        //private int m_BufferIndex;
        //private int m_PutBackPos;
        private String strLastTokenPutBack;

        private TokenType TypeOfLastTokenPutBack;

        // Instance constructor
        public Tokeniser()
        {
            SourceFileStack = new List<SourceFileStackEntry>();
            CurrentSourceFile = null;
            CurrentSourceStackIndex = -1;
            by = ' ';
            strLastTokenPutBack = "";
            TypeOfLastTokenPutBack = TokenType.None;
            bHasPutBackToken = false;
        }

        public enum TokenType
        {
            None,
            Comment,
            Number,
            HexNumber,
            Identifier,
            Divide,
            QuotedString,
            OpenCurly,
            CloseCurly,
            SemiColon,
            Comma,
            Assignment,
            Equality,
            InEquality,
            LessThan,
            GreaterThan,
            LessThanOrEqual,
            GreaterThanOrEqual,
            And,
            LogicalAnd,
            Pipe,
            LogicalOr,
            Addition,
            Subtraction,
            Multiplication,
            Division,
            OpenBracket,
            CloseBracket,
            Mod,
            True,
            False,
            OpenBlockComment,
            CloseBlockComment,
            Hash,
            Dot,
            Not,
            UnknownChar,
            OpenSquare,
            CloseSquare
        };

        public String GetSourceUpToIndex()
        {
            String result = "";
            if (CurrentSourceFile != null)
            {
                for (int i = 0; i < CurrentSourceFile.m_BufferIndex; i++)
                {
                    result += CurrentSourceFile.m_Buffer[i];
                }
            }
            return result;
        }

        public void PutTokenBack()
        {
            bHasPutBackToken = true;
        }

        internal string GetToEndOfLine()
        {
            String Result = CurrentSourceFile.GetToEndOfLine();

            return Result;
        }

        internal bool GetToken(out String kToken, out TokenType kType)
        {
            bool result = false;
            kType = TokenType.None;
            kToken = "";
            if (bHasPutBackToken == true)
            {
                kToken = strLastTokenPutBack;
                kType = TypeOfLastTokenPutBack;
                bHasPutBackToken = false;
                result = true;
            }
            else
            {
                if (CurrentSourceFile != null)
                {
                    //CurrentSourceFile.m_PutBackPos = m_BufferIndex;
                    CurrentSourceFile.RecordPutBackPos();
                    //
                    // Skip noise in the input
                    //
                    SkipNoise();
                    while ((by == (char)0) && (CurrentSourceStackIndex >= 0))
                    {
                        GetBy();
                        SkipNoise();
                    }
                    if (by != (char)0)
                    {
                        //
                        // If it starts with a letter assume its an identifier
                        //
                        if (IsLetter(by))
                        {
                            kType = TokenType.Identifier;
                            do
                            {
                                kToken += by;
                                GetBy();
                            } while (IsDigit(by) || IsLetter(by));
                            kToken = kToken.ToLower();
                            result = true;

                            //
                            // Is it a special word
                            //
                            if (kToken == "false")
                            {
                                kType = TokenType.False;
                            }
                            else
                                if (kToken == "true")
                            {
                                kType = TokenType.True;
                            }
                        }
                        else
                            if (IsDigit(by))
                        {
                            //
                            // is it 0x
                            //
                            if (by == '0')
                            {
                                kType = TokenType.Number;
                                kToken += by;
                                GetBy();
                                if ((by == 'x') ||
                                     (by == 'X'))
                                {
                                    kToken += by;
                                    GetBy();
                                    kType = TokenType.HexNumber;
                                    while (IsHexDigit(by))
                                    {
                                        kToken += by;
                                        GetBy();
                                    }
                                    result = true;
                                }
                                else
                                {
                                    //
                                    // Just a number that starts with 0
                                    //
                                    while (IsDigit(by) || by == '.')
                                    {
                                        kToken += by;
                                        GetBy();
                                    }
                                    result = true;
                                }
                            }
                            else
                            {
                                //
                                // starts with a digit so assume its a number
                                //
                                kType = TokenType.Number;
                                do
                                {
                                    kToken += by;
                                    GetBy();
                                } while (IsDigit(by) || by == '.');
                                result = true;
                            }
                        }
                        else
                        {
                            switch (by)
                            {
                                //
                                // Is it divide or comment
                                //
                                case '/':
                                    {
                                        kToken = "/";
                                        kType = TokenType.Division;
                                        GetBy();
                                        if (by == '/')
                                        {
                                            kToken = "//";
                                            kType = TokenType.Comment;
                                            GetBy();
                                        }
                                        else
                                            if (by == '*')
                                        {
                                            kToken = "/*";
                                            kType = TokenType.OpenBlockComment;
                                            GetBy();
                                        }
                                        result = true;
                                    }
                                    break;

                                case '"':
                                    {
                                        kType = TokenType.QuotedString;
                                        do
                                        {
                                            kToken += by;
                                            GetBy();
                                        } while ((by != (char)0) &&
                                                  (by != '"') &&
                                                  (by != '\r') &&
                                                  (by != '\n'));
                                        //
                                        // Make sure we haven't seen a new line in the middle of a quoted string
                                        //
                                        if ((by != '\r') &&
                                             (by != '\n'))
                                        {
                                            kToken += by;
                                            result = true;
                                            GetBy();
                                        }
                                        else
                                        {
                                            Log.Instance().AddEntry("Syntax Error new line encountered in quoted string");

                                            result = false;
                                        }
                                    }
                                    break;

                                case '{':
                                    {
                                        kType = TokenType.OpenCurly;
                                        kToken = "{";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case '#':
                                    {
                                        kType = TokenType.Hash;
                                        kToken = "#";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case '.':
                                    {
                                        kType = TokenType.Dot;
                                        kToken = ".";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case '}':
                                    {
                                        kType = TokenType.CloseCurly;
                                        kToken = "}";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case '[':
                                    {
                                        kType = TokenType.OpenSquare;
                                        kToken = "[";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case ']':
                                    {
                                        kType = TokenType.CloseSquare;
                                        kToken = "]";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case '(':
                                    {
                                        kType = TokenType.OpenBracket;
                                        kToken = "(";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case ')':
                                    {
                                        kType = TokenType.CloseBracket;
                                        kToken = ")";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case ';':
                                    {
                                        kType = TokenType.SemiColon;
                                        kToken = ";";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case ',':
                                    {
                                        kType = TokenType.Comma;
                                        kToken = ",";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case '+':
                                    {
                                        kType = TokenType.Addition;
                                        kToken = "+";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case '%':
                                    {
                                        kType = TokenType.Mod;
                                        kToken = "%";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case '-':
                                    {
                                        kType = TokenType.Subtraction;
                                        kToken = "-";
                                        GetBy();
                                        result = true;
                                    }
                                    break;

                                case '*':
                                    {
                                        kType = TokenType.Multiplication;
                                        kToken = "*";
                                        GetBy();
                                        if (by == '/')
                                        {
                                            kToken = "*/";
                                            kType = TokenType.CloseBlockComment;
                                            GetBy();
                                        }
                                        result = true;
                                    }
                                    break;

                                case '=':
                                    {
                                        //
                                        // Assume its a single = meaning assignment
                                        //
                                        kType = TokenType.Assignment;
                                        kToken = "=";
                                        GetBy();
                                        //
                                        // CHeck if its actually == meaning equality
                                        //
                                        if (by == '=')
                                        {
                                            kType = TokenType.Equality;
                                            kToken = "==";
                                            GetBy();
                                        }

                                        result = true;
                                    }
                                    break;

                                case '<':
                                    {
                                        //
                                        // Assume its lessthan
                                        //
                                        kType = TokenType.LessThan;
                                        kToken = "<";
                                        GetBy();
                                        //
                                        // CHeck if its actually >=
                                        //
                                        if (by == '=')
                                        {
                                            kType = TokenType.LessThanOrEqual;
                                            kToken = "<=";
                                            GetBy();
                                        }
                                        else
                                        {
                                            if (by == '>')
                                            {
                                                kType = TokenType.InEquality;
                                                kToken = "<>";
                                                GetBy();
                                            }
                                        }

                                        result = true;
                                    }
                                    break;

                                case '>':
                                    {
                                        //
                                        // Assume its greatethan
                                        //
                                        kType = TokenType.GreaterThan;
                                        kToken = ">";
                                        GetBy();
                                        //
                                        // CHeck if its actually >=
                                        //
                                        if (by == '=')
                                        {
                                            kType = TokenType.GreaterThanOrEqual;
                                            kToken = ">=";
                                            GetBy();
                                        }

                                        result = true;
                                    }
                                    break;

                                case '&':
                                    {
                                        //
                                        // Assume its a single &
                                        //
                                        kType = TokenType.And;
                                        kToken = "&";
                                        GetBy();
                                        //
                                        // CHeck if its actually &&
                                        //
                                        if (by == '&')
                                        {
                                            kType = TokenType.LogicalAnd;
                                            kToken = "&&";
                                            GetBy();
                                        }

                                        result = true;
                                    }
                                    break;

                                case '|':
                                    {
                                        //
                                        // Assume its a single |
                                        //
                                        kType = TokenType.Pipe;
                                        kToken = "|";
                                        GetBy();
                                        //
                                        // CHeck if its actually ||
                                        //
                                        if (by == '|')
                                        {
                                            kType = TokenType.LogicalOr;
                                            kToken = "||";
                                            GetBy();
                                        }

                                        result = true;
                                    }
                                    break;

                                case '\n':
                                    {
                                    }
                                    break;

                                case '!':
                                    {
                                        //
                                        // Assume its a single !
                                        //
                                        kType = TokenType.Not;
                                        kToken = "!";
                                        GetBy();
                                        //
                                        // CHeck if its actually &&
                                        //
                                        if (by == '=')
                                        {
                                            kType = TokenType.InEquality;
                                            kToken = "!=";
                                            GetBy();
                                        }

                                        result = true;
                                    }
                                    break;

                                case (char)65533:
                                    {
                                    }
                                    break;

                                default:
                                    {
                                        kType = TokenType.UnknownChar;
                                        kToken = by.ToString();
                                        GetBy();
                                        result = false;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            strLastTokenPutBack = kToken;
            TypeOfLastTokenPutBack = kType;

            return result;
        }

        internal bool InIncludeFile()
        {
            bool Result = false;
            if (CurrentSourceStackIndex > 0)
            {
                Result = true;
            }
            return Result;
        }

        internal void Initialise()
        {
            SourceFileStack = new List<SourceFileStackEntry>();
            CurrentSourceFile = null;
            bHasPutBackToken = false;
            CurrentSourceStackIndex = -1;
        }

        internal bool SetSource(string FilePath)
        {
            bool result;
            SourceFileStackEntry sourcefile = new SourceFileStackEntry();
            result = sourcefile.SetSource(FilePath);
            SourceFileStack.Add(sourcefile);
            CurrentSourceFile = sourcefile;
            CurrentSourceStackIndex++;

            return result;
        }

        internal void SetText(string text)
        {
            SourceFileStackEntry sourcefile = new SourceFileStackEntry();
            sourcefile.SetContent(text);
            SourceFileStack.Add(sourcefile);
            CurrentSourceFile = sourcefile;
            CurrentSourceStackIndex++;
        }

        internal void SkipToEndOfLine()
        {
            CurrentSourceFile.SkipToEndOfLine();
        }

        private void GetBy()
        {
            //
            // Remember we may be processing a stack of nested source files
            // When we have read all the chars from the file at the top of the stack we
            // carry on with the next file etc
            //
            do
            {
                if (CurrentSourceFile != null)
                {
                    by = CurrentSourceFile.GetBy();
                }
                //
                // Have we reached end of current file?
                //
                if (by == 0)
                {
                    if (CurrentSourceStackIndex > -1)
                    {
                        if (CurrentSourceStackIndex <= SourceFileStack.Count)
                        {
                            SourceFileStack[CurrentSourceStackIndex] = null;
                        }
                        CurrentSourceStackIndex--;
                    }
                    CurrentSourceFile = null;

                    //
                    // Is there another one
                    //
                    if (CurrentSourceStackIndex >= 0)
                    {
                        // Try to get the next char from it
                        CurrentSourceFile = SourceFileStack[CurrentSourceStackIndex];
                    }
                }
            } while ((by == (char)0) &&
                      (CurrentSourceStackIndex >= 0));
        }

        private bool IsDigit(char by)
        {
            bool result = false;
            if (by >= '0' && by <= '9')
            {
                result = true;
            }
            return result;
        }

        private bool IsHexDigit(char by)
        {
            bool result = false;
            if (by >= '0' && by <= '9')
            {
                result = true;
            }
            else
                if (by >= 'a' && by <= 'f')
            {
                result = true;
            }
            else
                    if (by >= 'A' && by <= 'F')
            {
                result = true;
            }
            return result;
        }

        private bool IsLetter(char by)
        {
            bool result = false;
            if (by >= 'a' && by <= 'z')
            {
                result = true;
            }
            if (by >= 'A' && by <= 'Z')
            {
                result = true;
            }
            else
                if (by == '_')
            {
                result = true;
            }

            return result;
        }

        private bool IsNoise(char by)
        {
            bool result = false;
            if ((by == ' ') ||
                 (by == '\r') ||
                 (by == '\n') ||
                 (by == '\t'))
            {
                result = true;
            }
            return result;
        }

        private void SkipNoise()
        {
            if (CurrentSourceFile != null)
            {
                by = CurrentSourceFile.SkipNoise();
            }
            else
            {
                by = (char)0;
            }
        }
    }
}