using asdflibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lerp.LerpLib
{
    public class LerpBox
    {
        public Voxel[,,] Voxels;
        public byte CornerMask;

        public LerpBox()
        {
            Voxels = new Voxel[2, 2, 2];
            CornerMask = 0;
        }

        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine("********");
            Voxels[0, 0, 0].Dump();
            Voxels[1, 0, 0].Dump();
            Voxels[0, 1, 0].Dump();
            Voxels[1, 1, 0].Dump();
            Voxels[0, 0, 1].Dump();
            Voxels[1, 0, 1].Dump();
            Voxels[0, 1, 1].Dump();
            Voxels[1, 1, 1].Dump();
        }

        public LerpBox(Voxel v1, Voxel v2, Voxel v3, Voxel v4, Voxel v5, Voxel v6, Voxel v7, Voxel v8)
        {
            Voxels = new Voxel[2, 2, 2];
            Voxels[0, 0, 0] = v1;
            Voxels[1, 0, 0] = v2;
            Voxels[0, 1, 0] = v3;
            Voxels[1, 1, 0] = v4;
            Voxels[0, 0, 1] = v5;
            Voxels[1, 0, 1] = v6;
            Voxels[0, 1, 1] = v7;
            Voxels[1, 1, 1] = v8;
        }

        public void CheckMask(int x, int y, int z)
        {
            if (Voxels[x, y, z].V < 0)
            {
                int index = x << 2 + y << 2 + z;
                CornerMask = (byte)(CornerMask | (1 << index));
            }
        }

        public void SetMask()
        {
            CornerMask = 0;
            CheckMask(0, 0, 0);
            CheckMask(0, 0, 1);
            CheckMask(0, 1, 0);
            CheckMask(0, 1, 1);
            CheckMask(1, 0, 0);
            CheckMask(1, 0, 1);
            CheckMask(1, 1, 0);
            CheckMask(1, 1, 1);
        }

        public void SetVoxel(int x, int y, int z, float px, float py, float pz, float v)
        {
            Voxels[x, y, z] = new Voxel(px, py, pz, v);
            if (v < 0)
            {
                int index = x << 2 + y << 2 + z;
                CornerMask = (byte)(CornerMask | (1 << index));
            }
        }

        public void SetVoxel(int x, int y, int z, Voxel v)
        {
            Voxels[x, y, z] = v;
            if (v.V < 0)
            {
                int index = x << 2 + y << 2 + z;
                CornerMask = (byte)(CornerMask | (1 << index));
            }
        }

        public List<LerpBox> Subdivide()
        {
            List<LerpBox> result = new List<LerpBox>();

            Voxel fr0 = LerpEdge(0, 0, 0, 1, 0, 0, 0.5F);
            Voxel fr1 = LerpEdge(0, 1, 0, 1, 1, 0, 0.5F);
            Voxel frm = LerpVoxel(fr0, fr1, 0.5F);

            Voxel back0 = LerpEdge(0, 0, 1, 1, 0, 1, 0.5F);
            Voxel back1 = LerpEdge(0, 1, 1, 1, 1, 1, 0.5F);
            Voxel backm = LerpVoxel(back0, back1, 0.5F);

            Voxel left0 = LerpEdge(0, 0, 0, 0, 0, 1, 0.5F);
            Voxel left1 = LerpEdge(0, 1, 0, 0, 1, 1, 0.5F);
            Voxel leftm = LerpVoxel(left0, left1, 0.5F);

            Voxel right0 = LerpEdge(1, 0, 0, 1, 0, 1, 0.5F);
            Voxel right1 = LerpEdge(1, 1, 0, 1, 1, 1, 0.5F);
            Voxel rightm = LerpVoxel(right0, right1, 0.5F);

            Voxel centre0 = LerpEdge(0, 0, 0, 1, 0, 1, 0.5F);
            Voxel centrem = LerpEdge(0, 0, 0, 1, 1, 1, 0.5F);
            Voxel centre1 = LerpEdge(0, 1, 0, 1, 1, 1, 0.5F);

            Voxel lfm = LerpEdge(0, 0, 0, 0, 1, 0, 0.5F);
            Voxel rfm = LerpEdge(1, 0, 0, 1, 1, 0, 0.5F);

            Voxel lbm = LerpEdge(0, 0, 1, 0, 1, 1, 0.5F);
            Voxel rbm = LerpEdge(1, 0, 1, 1, 1, 1, 0.5F);

            LerpBox lb = new LerpBox(Voxels[0, 0, 0], fr0, lfm, frm, left0, centre0, leftm, centrem);
            result.Add(lb);
            //  lb.Dump();
            lb = new LerpBox(fr0, Voxels[1, 0, 0], frm, rfm, centre0, right0, centrem, rightm);
            result.Add(lb);
            //  lb.Dump();
            lb = new LerpBox(lfm, frm, Voxels[0, 1, 0], fr1, leftm, centrem, left1, centre1);
            result.Add(lb);
            // lb.Dump();
            lb = new LerpBox(frm, rfm, fr1, Voxels[1, 1, 0], centrem, rightm, centre1, right1);
            result.Add(lb);
            //          lb.Dump();
            lb = new LerpBox(left0, centre0, leftm, centrem, Voxels[0, 0, 1], back0, lbm, backm);
            result.Add(lb);
            //      lb.Dump();
            lb = new LerpBox(centre0, right0, centrem, rightm, back0, Voxels[1, 0, 1], backm, rbm);
            result.Add(lb);
            //     lb.Dump();
            lb = new LerpBox(leftm, centrem, left1, centre1, lbm, backm, Voxels[0, 1, 1], back1);
            result.Add(lb);
            //    lb.Dump();
            lb = new LerpBox(centrem, rightm, centre1, right1, backm, rbm, back1, Voxels[1, 1, 1]);
            result.Add(lb);
            //     lb.Dump();
            return result;
        }

        public Voxel LerpEdge(int x1, int y1, int z1, int x2, int y2, int z2, float t)
        {
            Voxel res = new Voxel();
            res.X = Lerp(Voxels[x1, y1, z1].X, Voxels[x2, y2, z2].X, t);
            res.Y = Lerp(Voxels[x1, y1, z1].Y, Voxels[x2, y2, z2].Y, t);
            res.Z = Lerp(Voxels[x1, y1, z1].Z, Voxels[x2, y2, z2].Z, t);
            res.V = Lerp(Voxels[x1, y1, z1].V, Voxels[x2, y2, z2].V, t);
            return res;
        }

        public Voxel LerpVoxel(Voxel v0, Voxel v1, float t)
        {
            Voxel res = new Voxel();
            res.X = Lerp(v0.X, v1.X, t);
            res.Y = Lerp(v0.Y, v1.Y, t);
            res.Z = Lerp(v0.Z, v1.Z, t);
            res.V = Lerp(v0.V, v1.V, t);
            return res;
        }

        private float Lerp(float x0, float x1, float t)
        {
            return (x0 + (x1 - x0) * t);
        }

        public GridCell ToGridCell()
        {
            GridCell gc = new GridCell();
            gc.p[0] = Voxels[0, 0, 0].ToXZY();
            gc.p[1] = Voxels[1, 0, 0].ToXZY();
            gc.p[2] = Voxels[1, 0, 1].ToXZY();
            gc.p[3] = Voxels[0, 0, 1].ToXZY();
            gc.p[4] = Voxels[0, 1, 0].ToXZY();
            gc.p[5] = Voxels[1, 1, 0].ToXZY();
            gc.p[6] = Voxels[1, 1, 1].ToXZY();
            gc.p[7] = Voxels[0, 1, 1].ToXZY();

            gc.val[0] = Voxels[0, 0, 0].V;
            gc.val[1] = Voxels[1, 0, 0].V;
            gc.val[2] = Voxels[1, 0, 1].V;
            gc.val[3] = Voxels[0, 0, 1].V;
            gc.val[4] = Voxels[0, 1, 0].V;
            gc.val[5] = Voxels[1, 1, 0].V;
            gc.val[6] = Voxels[1, 1, 1].V;
            gc.val[7] = Voxels[0, 1, 1].V;
            return gc;
        }
    }
}