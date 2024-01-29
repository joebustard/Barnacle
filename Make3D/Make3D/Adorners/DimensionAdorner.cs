using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models.Adorners
{
    internal class DimensionAdorner : Adorner
    {
        internal PolarCamera Camera { get; set; }

        private enum DimState
        {
            WaitingForStart,
            WaitingForEnd,
            ShowingDimension
        }

        private DimState currentState;
        private List<Object3D> content;
        private Object3D startObject;
        private Object3D endObject;
        private Point3D startPoint;
        private Point3D endPoint;

        public DimensionAdorner(PolarCamera camera, List<Object3D> objects, Point3D hitPos)
        {
            NotificationManager.ViewUnsubscribe("DimensionAdorner");

            Camera = camera;
            Adornments = new Model3DCollection();
            SelectedObjects = new List<Object3D>();
            currentState = DimState.WaitingForStart;
            content = objects;
            startPoint = hitPos;
        }

        public override void AdornObject(Object3D obj)
        {
            GenerateAdornments();
        }

        public override void Clear()
        {
            Adornments.Clear();
            SelectedObjects.Clear();
            currentState = DimState.WaitingForStart;
            startObject = null;
            endObject = null;
        }

        internal override void GenerateAdornments()
        {
            CreateMarker(startPoint, 3, Colors.Crimson);
        }

        private void CreateMarker(Point3D position, double size, Color col)
        {
            Object3D marker = new Object3D();

            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCube(ref pnts, ref indices, ref normals);
            marker.Name = "Marker";
            List<P3D> tmp = new List<P3D>();
            PointUtils.PointCollectionToP3D(pnts, tmp);

            marker.RelativeObjectVertices = tmp;
            marker.TriangleIndices = indices;
            marker.Normals = normals;

            marker.Position = position;
            marker.Scale = new Scale3D(size, size, size);
            marker.ScaleMesh(size, size, size);
            marker.Color = col;
            marker.Remesh();

            Adornments.Add(GetMesh(marker));
        }
    }
}