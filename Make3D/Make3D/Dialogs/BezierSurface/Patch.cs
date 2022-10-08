namespace Barnacle.Dialogs.BezierSurface
{
    internal class Patch
    {
        public int[,] Points;

        public Patch()
        {
            Points = new int[4, 4];
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    Points[r, c] = -1;
                }
            }
        }
    }
}