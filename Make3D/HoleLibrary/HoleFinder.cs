using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HoleLibrary
{
    public class HoleFinder
    {
        private List<P3D> points;
        private List<Face> faces;
        private List<Edge> edges;

        public HoleFinder(List<P3D> meshPoints, Int32Collection meshFaces)
        {
            points = meshPoints;
            edges = new List<Edge>();
            faces = new List<Face>();
            for (int i = 0; i < meshFaces.Count; i += 3)
            {
                Face nf = new Face(meshFaces[i],
                    meshFaces[i + 1],
                    meshFaces[i + 2],
                    edges);
                faces.Add(nf);
            }

        }
        public void FindHoles()
        {
            List<Edge> duffEdges = new List<Edge>();
            foreach (Edge e in edges)
            {
                if (e.Face2 == null)
                {
                    duffEdges.Add(e);
                }
            }


        }
    }
}