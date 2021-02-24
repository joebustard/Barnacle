namespace Make3D.Dialogs
{
    public class LetterMarker
    {
        public string Letter { get; set; }
        public int Position { get; set; }
        public RibControl Rib { get; set; }

        public LetterMarker(string l, int pos)
        {
            Letter = l;
            Position = pos;
        }
    }
}