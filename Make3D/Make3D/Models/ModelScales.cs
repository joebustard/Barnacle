using System.Collections.Generic;

namespace Make3D.Models
{
    public class ModelScales
    {
        public static Dictionary<string, double> Standard = new Dictionary<string, double>();

        public static void Initialise()
        {
            Standard["1"] = 1.0;
            Standard["1/2"] = 1.0 / 2.0;
            Standard["1/3"] = 1.0 / 3.0;
            Standard["1/4"] = 1.0 / 4.0;
            Standard["1/5"] = 1.0 / 5.0;
            Standard["1/6"] = 1.0 / 6.0;
            Standard["1/10"] = 1.0 / 10.0;
            Standard["1/12"] = 1.0 / 12.0;
            Standard["1/16"] = 1.0 / 16.0;
            Standard["1/24"] = 1.0 / 24;
            Standard["1/32"] = 1.0 / 32.0;
            Standard["1/35"] = 1.0 / 35.0;
            Standard["1/48"] = 1.0 / 48.0;
            Standard["1/50"] = 1.0 / 50.0;
            Standard["1/72"] = 1.0 / 72.0;
            Standard["1/100"] = 1.0 / 100.0;
            Standard["1/144"] = 1.0 / 144.0;
            Standard["1/200"] = 1.0 / 200.0;
            Standard["1/300"] = 1.0 / 300.0;
            Standard["1/400"] = 1.0 / 400.0;
            Standard["OO"] = 1.0 / 76.2;
            Standard["HO"] = 1.0 / 87;
            Standard["N"] = 1.0 / 148;
        }

        internal static double ConversionFactor(string baseScale, string exportScale)
        {
            double res = 1.0;
            if (Standard.ContainsKey(baseScale) && Standard.ContainsKey(exportScale))
            {
                double startScale = Standard[baseScale];
                double endScale = Standard[exportScale];
                res = 1.0 / startScale;
                res = res * endScale;
            }
            return res;
        }

        internal static List<string> ScaleNames()
        {
            List<string> names = new List<string>();
            foreach (string a in Standard.Keys)
            {
                names.Add(a);
            }
            return names;
        }
    }
}