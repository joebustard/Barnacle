// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class SolidFunctionNode : ExpressionNode
    {
        public ExpressionCollection expressions;
        public string Label;

        public int ExpressionCount
        {
            get
            {
                return expressions.Count();
            }
        }

        public void AddExpression(ExpressionNode exp)
        {
            expressions.InsertAtStart(exp);
        }

        public override String ToRichText()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = RichTextFormatter.KeyWord($"{Label}( ");
                result += expressions.ToRichText();
                result += " )";
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
                result = $"{Label} (" + expressions.ToString();
                result += " )";
            }
            return result;
        }
    }
}