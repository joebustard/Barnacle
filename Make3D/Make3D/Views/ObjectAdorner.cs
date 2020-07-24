using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Make3D.Models;

namespace Make3D.Adorners
{
    public class ObjectAdorner
    {
        Model3DCollection adornments;
        public Model3DCollection Adornments { get { return adornments;} }

        private List<Object3D> taggedObjects;

        public ObjectAdorner()
        {
            adornments = new Model3DCollection();
            taggedObjects = new List<Object3D>();
        }

        public void Clear()
        {
            adornments.Clear();
            taggedObjects.Clear();
        }
        public void AdornObject(Object3D obj)
        {
            taggedObjects.Add(obj);
            GenerateAdornments();
        }

        private void GenerateAdornments()
        {
            adornments.Clear();
            Bounds3D bnds = new Bounds3D();
            foreach( Object3D obj in taggedObjects)
            {
                bnds += obj.AbsoluteBounds;
            }
            Point3D midp = bnds.MidPoint();
            Point3D size = bnds.Size();
            CreateAdornments(midp, size);
        }

   

        private void CreateAdornments(Point3D position, Point3D size)
        {
            Object3D tmp = new Object3D();

            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCube(ref pnts, ref indices, ref normals);
            tmp.RelativeObjectVertices = pnts;
            tmp.TriangleIndices = indices;
            tmp.Normals = normals;
            tmp.Position = position;
            tmp.Scale = new Scale3D(size.X, size.Y, size.Z);
            tmp.Scale.Adjust(1.1, 1.1, 1.1);
            tmp.Color = Color.FromArgb(200, 64, 64, 64);
            tmp.RelativeToAbsolute();
            tmp.SetMesh();
            adornments.Add(GetMesh(tmp));
        }

        private static GeometryModel3D GetMesh(Object3D obj)
        {
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = obj.Mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = obj.Color;
            mt.Brush = new SolidColorBrush(obj.Color);
            gm.Material = mt;
            return gm;
        }

        internal bool Select(GeometryModel3D geo)
        {
            bool handled = false;
            foreach(GeometryModel3D g in adornments)
            {
                if (g.Geometry == geo.Geometry )
                {
                    handled = true;
                }
            }
            return handled;
        }
    }
}
