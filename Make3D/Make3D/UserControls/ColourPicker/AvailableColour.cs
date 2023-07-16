using System;
using System.Windows.Media;

namespace Barnacle.UserControls
{
    public class AvailableColour
    {
        public String Name { get; set; }
        public string Title { get; set; }
        public Color Colour { get; set; }

        public AvailableColour(string n, string t, Color c)
        {
            Name = n;
            Title = t;
            Colour = c;
        }

        public AvailableColour(string name)
        {
            Name = name;
            Title = name.Substring(0, 1);
            for (int i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]))
                {
                    Title += " ";
                }
                Title += name[i];
            }

            Colour = (Color)ColorConverter.ConvertFromString(Name);
        }
    }
}