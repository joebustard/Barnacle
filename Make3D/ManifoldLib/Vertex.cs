using System.Windows.Media.Media3D;

namespace ManifoldLib
{
    public class Vertex
    {
        public Point3D Pos { get; set; }
        public int DuplicateOf { get; set; }
        public int OriginalNumber { get; set; }
        public int NewNumber { get; set; }

        public Vertex()
        {
            DuplicateOf = -1;
        }
    }
}