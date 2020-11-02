using System;

namespace Make3D.Models
{
    public class MultiPasteConfig
    {
        public String Direction { get; set; }
        public int Repeats { get; set; }
        public double Spacing { get; set; }
        public double AlternatingOffset { get; internal set; }

        public MultiPasteConfig()
        {
            Repeats = 2;
            Spacing = 1.0;
            Direction = "X";
        }
    }
}