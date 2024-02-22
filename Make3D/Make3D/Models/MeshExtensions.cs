using System.Linq;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public static class MeshExtensions
    {
        // Apply a transformation Matrix3D or transformation class.
        public static void ApplyTransformation(this MeshGeometry3D mesh, Matrix3D transformation)
        {
            Point3D[] points = mesh.Positions.ToArray();
            transformation.Transform(points);
            mesh.Positions = new Point3DCollection(points);
        }

        public static void ApplyTransformation(this MeshGeometry3D mesh, Transform3D transformation)
        {
            Point3D[] points = mesh.Positions.ToArray();
            transformation.Transform(points);
            mesh.Positions = new Point3DCollection(points);
        }
    }
}