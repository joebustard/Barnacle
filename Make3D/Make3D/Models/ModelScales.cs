using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.Models
{
    public class ModelScales
    {
        
        public static Dictionary<string, double> Standard = new Dictionary<string, double>();
        public static void Initialise()
        {
            Standard["1"] = 1.0;
            Standard["OO"] = 1.0/ 76.2;
            Standard["HO"] = 1.0 / 87;
            Standard["N"] = 1.0 / 148;
        }

        internal static List<string> ScaleNames()
        {
            List<string> names = new List<string>();
            foreach( string a in Standard.Keys)
            {
                names.Add(a);
            }
            return names;

        }
    }
}
