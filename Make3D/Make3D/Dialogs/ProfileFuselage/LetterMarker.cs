using System.Drawing;

namespace Barnacle.Dialogs
{
    public class LetterMarker
    {
        public LetterMarker(string l, Point pos)
        {
            Letter = l;
            Position = pos;
        }

        public string Letter { get; set; }
        public Point Position { get; set; }
        public RibPathControl Rib { get; set; }
    }
}