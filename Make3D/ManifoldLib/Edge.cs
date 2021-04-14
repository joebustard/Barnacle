namespace ManifoldLib
{
    public class Edge
    {
        public Edge(int p0, int p1)
        {
            P0 = p0;
            P1 = p1;
            NP0 = 0;
            NP1 = 0;
        }

        public int P0 { get; set; }
        public int P1 { get; set; }
        public int NP0 { get; set; }
        public int NP1 { get; set; }

        internal bool EqualTo(Edge ft, bool reverse)
        {
            bool res = false;
            if (reverse)
            {
                if ((P0 == ft.P1) && (P1 == ft.P0))
                {
                    res = true;
                }
            }
            else
            {
                if ((P0 == ft.P0) && (P1 == ft.P1))
                {
                    res = true;
                }
            }
            return res;
        }
    }
}