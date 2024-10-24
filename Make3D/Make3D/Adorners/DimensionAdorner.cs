/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

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
        private TextBox label;
        private Point3D labelPos;
        private Line line;
        private Color startColour;
        private Point3D startPoint;

        public DimensionAdorner(PolarCamera camera, List<Object3D> objects, Point3D hitPos, Object3D object3D)
        {
            NotificationManager.ViewUnsubscribe("DimensionAdorner");

            Camera = camera;
            Adornments = new Model3DCollection();
            SelectedObjects = new List<Object3D>();

            content = objects;
            startPoint = hitPos;
            StartObject = object3D;
            EndObject = null;
            startColour = Colors.CornflowerBlue;
            endColour = Colors.GreenYellow;
            CreateMarker(startPoint, 2, startColour);
            label = null;
            line = null;
            TwoPoints = false;
            NotificationManager.Subscribe("DimensionAdorner", "CameraMoved", OnCameraMoved);
        }

        public Object3D EndObject
        {
            get; set;
        }

        public Point3D EndPoint
        {
            get { return endPoint; }
        }

        public Object3D StartObject
        {
            get; set;
        }

        public Point3D StartPoint
        {
            get { return startPoint; }
        }

        public bool TwoPoints { get; set; }
        internal PolarCamera Camera { get; set; }

        public override void AdornObject(Object3D obj)
        {
            GenerateAdornments();
        }

        public override void Clear()
        {
            Adornments.Clear();
            SelectedObjects.Clear();
            Overlay.Children.Clear();
            TwoPoints = false;
        }

        internal void SecondPoint(Point3D hitPos, Object3D object3D)
        {
            Adornments.Clear();
            Overlay?.Children.Clear();
            endPoint = hitPos;
            CreateMarker(startPoint, 2, startColour);
            CreateMarker(endPoint, 2, endColour);
            double dist = startPoint.Distance(endPoint);
            AddLine(startPoint, endPoint);
            AddLabel(startPoint.MidPoint(endPoint), dist.ToString("F3"));
            TwoPoints = true;
            EndObject = object3D;
        }

        private void AddLabel(Point3D lp, string v2)
        {
            labelPos = lp;
            label = new TextBox();
            label.Text = v2;
            label.Background = new SolidColorBrush(Color.FromArgb(64, 255, 255, 255));
            label.FontWeight = FontWeights.Bold;
            label.FontSize = 18;
            label.MinWidth = 200;
            label.Height = 60;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.IsReadOnly = true;
            Point point = CameraUtils.Convert3DPoint(labelPos, ViewPort);

            Canvas.SetLeft(label, point.X);
            Canvas.SetTop(label, point.Y);
            if (!Overlay.Children.Contains(label))
            {
                Overlay.Children.Add(label);
            }
        }

        private void AddLine(Point3D startPoint, Point3D endPoint)
        {
            line = new Line();
            Point point1 = CameraUtils.Convert3DPoint(startPoint, ViewPort);
            Point point2 = CameraUtils.Convert3DPoint(endPoint, ViewPort);
            line.X1 = point1.X;
            line.Y1 = point1.Y;
            line.X2 = point2.X;
            line.Y2 = point2.Y;
            line.StrokeThickness = 4;
            line.Stroke = Brushes.Black;
            line.Fill = Brushes.Black;
            if (!Overlay.Children.Contains(line))
            {
                Overlay.Children.Add(line);
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

        private void OnCameraMoved(object param)
        {
            PositionLabel();
            if (line != null)
            {
                Point point1 = CameraUtils.Convert3DPoint(startPoint, ViewPort);
                Point point2 = CameraUtils.Convert3DPoint(endPoint, ViewPort);
                line.X1 = point1.X;
                line.Y1 = point1.Y;
                line.X2 = point2.X;
                line.Y2 = point2.Y;
            }
        }

        private void PositionLabel()
        {
            Point point = CameraUtils.Convert3DPoint(labelPos, ViewPort);
            if (label != null)
            {
                Canvas.SetLeft(label, point.X);
                Canvas.SetTop(label, point.Y);
                if (!Overlay.Children.Contains(label))
                {
                    Overlay.Children.Add(label);
                }
            }
        }
    }
}