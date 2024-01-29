using Barnacle.Object3DLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Barnacle.Models.Adorners
{
    internal class DimensionAdorner : Adorner
    {
        private List<Object3D> content;
        private Color endColour;
        private Point3D endPoint;
        private Point3D labelPos;
        private Color startColour;
        private Point3D startPoint;

        public DimensionAdorner(PolarCamera camera, List<Object3D> objects, Point3D hitPos)
        {
            NotificationManager.ViewUnsubscribe("DimensionAdorner");

            Camera = camera;
            Adornments = new Model3DCollection();
            SelectedObjects = new List<Object3D>();

            content = objects;
            startPoint = hitPos;
            startColour = Colors.CornflowerBlue;
            endColour = Colors.GreenYellow;
            CreateMarker(startPoint, 2, startColour);
        }

        internal PolarCamera Camera { get; set; }

        public override void AdornObject(Object3D obj)
        {
            GenerateAdornments();
        }

        public override void Clear()
        {
            Adornments.Clear();
            SelectedObjects.Clear();
        }

        internal void SecondPoint(Point3D hitPos)
        {
            Adornments.Clear();
            Overlay?.Children.Clear();
            endPoint = hitPos;
            CreateMarker(startPoint, 2, startColour);
            CreateMarker(endPoint, 2, endColour);
            double dist = startPoint.Distance(endPoint);
            AddLine(startPoint, endPoint);
            AddLabel(startPoint.MidPoint(endPoint), dist.ToString("F3"));
        }

        private void AddLabel(Point3D lp, string v2)
        {
            labelPos = lp;
            TextBox l = new TextBox();
            l.Text = v2;
            l.Background = new SolidColorBrush(Color.FromArgb(64, 255, 255, 255));
            l.FontWeight = FontWeights.Bold;
            l.FontSize = 18;
            l.MinWidth = 200;
            l.Height = 60;
            l.HorizontalAlignment = HorizontalAlignment.Center;

            Point point = CameraUtils.Convert3DPoint(labelPos, ViewPort);

            Canvas.SetLeft(l, point.X);
            Canvas.SetTop(l, point.Y);
            if (!Overlay.Children.Contains(l))
            {
                Overlay.Children.Add(l);
            }
        }

        private void AddLine(Point3D startPoint, Point3D endPoint)
        {
            Line l = new Line();
            Point point1 = CameraUtils.Convert3DPoint(startPoint, ViewPort);
            Point point2 = CameraUtils.Convert3DPoint(endPoint, ViewPort);
            l.X1 = point1.X;
            l.Y1 = point1.Y;
            l.X2 = point2.X;
            l.Y2 = point2.Y;
            l.StrokeThickness = 4;
            l.Stroke = Brushes.Black;
            l.Fill = Brushes.Black;
            if (!Overlay.Children.Contains(l))
            {
                Overlay.Children.Add(l);
            }
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