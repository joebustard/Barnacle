using System.Drawing;

namespace Make3D.Dialogs
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
        public RibControl Rib { get; set; }
    }
}