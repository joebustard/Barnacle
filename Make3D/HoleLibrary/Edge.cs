namespace HoleLibrary
{
    public class Edge
    {
        public Edge()
        {
            Start = -1;
            End = -1;
            Face1 = null;
            Face2 = null;
        }

        public Edge(int s, int e, Face f)
        {
            Start = s;
            End = e;
            Face1 = f;
            Face2 = null;
        }

        public int End { get; set; }
        public Face Face1 { get; set; }
        public Face Face2 { get; set; }
        public int Start { get; set; }

        internal bool EdgeMatch(int v0, int v1)
        {
            if (v0 == Start && v1 == End) return true;
            if (v1 == Start && v0 == End) return true;
            return false;
        }
    }
}