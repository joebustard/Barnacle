using Barnacle.Object3DLib;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    public class PlateletSurface
    {
        public List<Edge> Edges;
        public List<Face> Faces;
        private Point3DCollection Vertices;

        public PlateletSurface()
        {
            Edges = new List<Edge>();
            Faces = new List<Face>();
            Vertices = new Point3DCollection();
        }

        public int AddVertice(double x, double y, double z)
        {
            int res = -1;
            for (int i = 0; i < Vertices.Count; i++)
            {
                if (PointUtils.equals(Vertices[i], x, y, z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                Vertices.Add(new Point3D(x, y, z));
                res = Vertices.Count - 1;
            }
            return res;
        }

        internal int AddEdge(int s, int e, int f)
        {
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].Matches(s, e))
                {
                    if (Edges[i].FaceA == -1)
                    {
                        Edges[i].FaceA = f;
                        return i;
                    }
                    if (Edges[i].FaceB == -1)
                    {
                        Edges[i].FaceB = f;
                        return i;
                    }
                }
            }
            Edge en = new Edge(s, e);
            en.FaceA = f;
            Edges.Add(en);
            return Edges.Count - 1;
        }

        internal void AddFace(System.Drawing.PointF[] points)
        {
            int v0 = AddVertice((double)points[0].X, (double)points[0].Y, 0.0);
            int v1 = AddVertice((double)points[1].X, (double)points[1].Y, 0.0);
            int v2 = AddVertice((double)points[2].X, (double)points[2].Y, 0.0);
            Face fc = new Face();
            Faces.Add(fc);
            fc.Edges[0] = AddEdge(v0, v1, Faces.Count - 1);
            fc.Edges[1] = AddEdge(v1, v2, Faces.Count - 1);
            fc.Edges[2] = AddEdge(v2, v0, Faces.Count - 1);
        }

        internal void AddFaces(double px, double py, double sx, double sy)
        {
            int v0 = AddVertice(px, py, 0.0);
            int v1 = AddVertice(px + sx, py, 0.0);
            int v2 = AddVertice(px + sx, py + sy, 0.0);
            int v3 = AddVertice(px, py + sy, 0.0);

            Face fc = new Face();
            Faces.Add(fc);
            fc.Edges[0] = AddEdge(v0, v1, Faces.Count - 1);
            fc.Edges[1] = AddEdge(v1, v2, Faces.Count - 1);
            fc.Edges[2] = AddEdge(v2, v0, Faces.Count - 1);

            fc = new Face();
            Faces.Add(fc);
            fc.Edges[0] = AddEdge(v0, v2, Faces.Count - 1);
            fc.Edges[1] = AddEdge(v2, v3, Faces.Count - 1);
            fc.Edges[2] = AddEdge(v3, v0, Faces.Count - 1);
        }
        double imageWidth=1;
        double imageHeight = 1;
        private enum PixelColour
        {
            Unknown,
            Back,
            Front,
            Mixed
        }
        internal void ClassifyFaces(System.Drawing.Bitmap workingImage)
        {
            imageWidth = workingImage.Width;
            imageHeight = workingImage.Height;
            foreach( Face f in Faces)
            {
                Point mid = f.GetFlatCentroid(Vertices, Edges);
                PixelColour pc = GetTexturePixel(workingImage, mid.X, mid.Y);
                f.Mode = (int) pc;
                if ( pc == PixelColour.Front)
                {
                    f.MoveForward(Vertices,Edges);
                }

            }
        }
        private PixelColour GetTexturePixel(System.Drawing.Bitmap workingImage, double cx, double cy)
        {
           
            double px = cx % imageWidth;
            double py = cy % imageHeight;
            if ((px >= 0 && px < imageWidth) && (py >= 0 && py < imageHeight))
            {
                System.Drawing.Color col = workingImage.GetPixel((int)px, (int)py);

                if (col.R < 128)
                {
                    return PixelColour.Front;
                }
            }
            return PixelColour.Back;
        }
    }
}