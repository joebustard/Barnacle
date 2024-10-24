﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Barnacle.ViewModels
{
    public static class RichTextBoxExtensions
    {
        private static TextPointer oldposend = null;
        private static TextPointer oldposstart = null;

        public static int Find(this RichTextBox richTextBox, string text, int startIndex = 0)
        {
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            richTextBox.Selection.Select(textRange.Start, textRange.Start);     // clear previous select if there was one

            textRange.ClearAllProperties();
            var index = textRange.Text.IndexOf(text, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index > -1)
            {
                var textPointerStart = GetPoint(textRange.Start, index);
                // var textPointerEnd = textRange.Start.GetPositionAtOffset(index + text.Length, LogicalDirection.Backward);
                var textPointerEnd = GetPoint(textRange.Start, index + text.Length);

                var textRangeSelection = new TextRange(textPointerStart, textPointerEnd);
                textRangeSelection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                richTextBox.Selection.Select(textRangeSelection.Start, textRangeSelection.End);
                richTextBox.Focus();
            }

            return index;
        }

        public static int FindEx(this RichTextBox richTextBox, string text, int startIndex = 0)
        {
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            richTextBox.Selection.Select(textRange.Start, textRange.Start);     // clear previous select if there was one

            textRange.ClearAllProperties();
            var index = textRange.Text.IndexOf(text, startIndex, StringComparison.OrdinalIgnoreCase);
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