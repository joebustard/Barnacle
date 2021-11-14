using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace MakerLib
{
   public class TextHelper
    {
            public static PathGeometry PathFrom(String text, string culture, bool leftToRight, string font, int fontSize)
            {
                if (culture == "") culture = "en-gb";

                var ci = new CultureInfo(culture);
                var fd = leftToRight ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
                var ff = new System.Windows.Media.FontFamily(font);
                var tf = new Typeface(ff, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                var t = new FormattedText(text, ci, fd, tf, fontSize, System.Windows.Media.Brushes.Black);
                var g = t.BuildGeometry(new Point(0, 0));
            var f = g.GetOutlinedPathGeometry();
                var p = g.GetFlattenedPathGeometry();

                return p;
            }

            public static IEnumerable GetAvailableCultures()
            {
                return CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures)
                                  .Select(x => x.Name)
                                  .ToList();
            }

            public static IEnumerable GetDirections()
            {
                return new[] { "Left to right", "Right to left" };
            }

            public static IEnumerable GetFonts()
            {
                return new InstalledFontCollection().Families.Select(font => font.Name).ToList();
            }

            public static IEnumerable GetFontSizes()
            {
                return new[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 26, 28, 36, 48, 72 }.Select(x => x.ToString());
            }
        }
    
}
