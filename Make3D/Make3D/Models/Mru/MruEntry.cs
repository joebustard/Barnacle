using System;

namespace Barnacle.Models.Mru
{
    public class MruEntry
    {
        public MruEntry(string n, string p)
        {
            Name = n;
            Path = p;
        }

        public String Name { get; set; }
        public String Path { get; set; }
    }
}