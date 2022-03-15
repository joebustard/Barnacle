using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Barnacle.ViewModels
{
    public class AvailableColour
    {
        public String Name { get; set; }
        public string Title { get; set; }
        public System.Drawing.Color Colour { get; set; }
        public AvailableColour(string n, string t, System.Drawing.Color c)
        {
            Name = n;
            Title = t;
            Colour = c;
        }

        public AvailableColour(string name)
        {
            Name = name;
            Title = name.Substring(0,1);
            for (int i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]))
                {
                    Title += " ";
                }
                Title += name[i];
            }
            
            Colour = System.Drawing.Color.FromName(name);
            
        }
    }
}
