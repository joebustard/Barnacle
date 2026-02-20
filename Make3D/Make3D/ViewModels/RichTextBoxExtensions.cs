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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using static System.Net.Mime.MediaTypeNames;

namespace Barnacle.ViewModels
{
    public static class RichTextBoxExtensions
    {
        private static TextPointer oldposstart = null;

        public static int Find(this RichTextBox richTextBox, string text, out TextRange foundTextRange, int startIndex = 0)
        {
            foundTextRange = null;
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            richTextBox.Selection.Select(textRange.Start, textRange.Start);     // clear previous select if there was one

            textRange.ClearAllProperties();
            var index = textRange.Text.IndexOf(text, startIndex, StringComparison.Ordinal);
            if (index > -1)
            {
                var textPointerStart = GetPoint(textRange.Start, index);
                // var textPointerEnd = textRange.Start.GetPositionAtOffset(index + text.Length, LogicalDirection.Backward);
                var textPointerEnd = GetPoint(textRange.Start, index + text.Length);

                var textRangeSelection = new TextRange(textPointerStart, textPointerEnd);
                textRangeSelection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                richTextBox.Selection.Select(textRangeSelection.Start, textRangeSelection.End);
                richTextBox.Focus();
                foundTextRange = textRangeSelection;
            }

            return index;
        }

        public static int FindEx(this RichTextBox richTextBox, string text, int startIndex = 0)
        {
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            richTextBox.Selection.Select(textRange.Start, textRange.Start);     // clear previous select if there was one

            textRange.ClearAllProperties();
            var index = textRange.Text.IndexOf(text, startIndex, StringComparison.Ordinal);
            if (index > -1)
            {
                var textPointerStart = textRange.Start.GetPositionAtOffset(index);
                var textPointerEnd = textRange.Start.GetPositionAtOffset(index + text.Length);

                var textRangeSelection = new TextRange(textPointerStart, textPointerEnd);
                textRangeSelection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                richTextBox.Selection.Select(textRangeSelection.Start, textRangeSelection.End);
                richTextBox.Focus();
            }

            return index;
        }

        public static void RecordCurrentPosition(this RichTextBox richTextBox)
        {
            oldposstart = richTextBox.CaretPosition;
            //  oldposend = richTextBox.Selection.End;
        }

        public static bool Replace(this RichTextBox richTextBox, int index, string src, string trg, TextRange foundTextRange)
        {
            bool res = false;
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            richTextBox.Selection.Select(textRange.Start, textRange.Start);     // clear previous select if there was one

            textRange.ClearAllProperties();
            var index2 = textRange.Text.IndexOf(src, index, StringComparison.Ordinal);
            if (index == index2 && foundTextRange != null)
            {
                foundTextRange.Text = trg;
                res = true;
            }
            return res;
        }

        public static void RestoreCurrentPosition(this RichTextBox richTextBox)
        {
            if (oldposstart != null)
            {
                richTextBox.CaretPosition = oldposstart;
                richTextBox.Focus();
            }
        }

        private static TextPointer GetPoint(TextPointer start, int x)
        {
            var ret = start;
            var i = 0;
            while (ret != null)
            {
                string stringSoFar = new TextRange(ret, ret.GetPositionAtOffset(i, LogicalDirection.Forward)).Text;
                if (stringSoFar.Length == x)
                    break;
                i++;
                if (ret.GetPositionAtOffset(i, LogicalDirection.Forward) == null)
                    return ret.GetPositionAtOffset(i - 1, LogicalDirection.Forward);
            }
            ret = ret.GetPositionAtOffset(i, LogicalDirection.Forward);
            return ret;
        }
    }
}