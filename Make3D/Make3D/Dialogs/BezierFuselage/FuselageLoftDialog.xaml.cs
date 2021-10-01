using Barnacle.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for FuselageLoftDialog.xaml
    /// </summary>
    public partial class FuselageLoftDialog : BaseModellerDialog, INotifyPropertyChanged
    {
        public double bezierStep = 0.05;

        private ObservableCollection<BulkheadControl> bulkHeads;

        private string filter = "Fuselage Files (*.fsl)|*.fsl";

        private int[,] indexFrame;

        private int selectedBulkhead;

        public FuselageLoftDialog()
        {
            InitializeComponent();
            DataContext = this;
            BulkHeads = new ObservableCollection<BulkheadControl>();
            DataContext = this;
            selectedBulkhead = -1;
            EditorParameters.ToolName = "BezierFuselage";
            ModelGroup = MyModelGroup;
        }

        public ObservableCollection<BulkheadControl> BulkHeads
        {
            get
            {
                return bulkHeads;
            }
            set
            {
                if (bulkHeads != value)
                {
                    bulkHeads = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public override bool ShowAxies
        {
            get
            {
                return showAxies;
            }
            set
            {
                if (showAxies != value)
                {
                    showAxies = value;
                    NotifyPropertyChanged();
                    Redraw();
                }
            }
        }

        public override bool ShowFloor
        {
            get
            {
                return showFloor;
            }
            set
            {
                if (showFloor != value)
                {
                    showFloor = value;
                    NotifyPropertyChanged();
                    Redraw();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            RecordEditorParameters();
            DialogResult = true;
            Close();
        }

        private void AddBulkhead_Click(object sender, RoutedEventArgs e)
        {
            double distance = bulkHeads[bulkHeads.Count - 1].GetDistance() + 5;
            FuselageBulkhead fb = new FuselageBulkhead();
            fb.Depth = 40;
            fb.Height = 40;
            fb.OffsetX = distance;
            bulkHeads[bulkHeads.Count - 1].CopyToNextButton.Visibility = Visibility.Visible;
            BulkheadControl bhc = new BulkheadControl();
            bhc.FuselageBulkHead = fb;
            bhc.OnPerformAction += PerformAction;
            bhc.IdNumber = bulkHeads.Count + 1;
            bulkHeads.Add(bhc);
            bhc.CopyToNextButton.Visibility = Visibility.Hidden;
            NotifyPropertyChanged("BulkHeads");
        }

        private void CopyOn(int i)
        {
            i = i - 1;
            if (i >= 0 && i < bulkHeads.Count - 1)
            {
                List<Point> pnts = bulkHeads[i].Points;
                List<Point> ctrls = bulkHeads[i].ControlPoints;
                List<double> dists = bulkHeads[i].PointDistance;

                bulkHeads[i + 1].Set(pnts, ctrls, dists);
            }
        }

        private void GetXY(string v, ref double x, ref double y)
        {
            string[] words = v.Split('!');
            x = Convert.ToDouble(words[0]);
            y = Convert.ToDouble(words[1]);
        }

        private void InsertBulkhead_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBulkhead != -1)
            {
                // if they click insert while on the last one
                if (selectedBulkhead == bulkHeads.Count)
                {
                    AddBulkhead_Click(sender, e);
                }
                else
                {
                    double distance = bulkHeads[selectedBulkhead].GetDistance() - bulkHeads[selectedBulkhead - 1].GetDistance();
                    FuselageBulkhead fb = new FuselageBulkhead();
                    fb.Depth = 40;
                    fb.Height = 40;
                    fb.OffsetX = bulkHeads[selectedBulkhead - 1].GetDistance() + distance / 2;

                    BulkheadControl bhc = new BulkheadControl();
                    bhc.FuselageBulkHead = fb;
                    bhc.OnPerformAction += PerformAction;
                    bhc.IdNumber = bulkHeads.Count + 1;
                    bulkHeads.Insert(selectedBulkhead, bhc);
                    bhc.CopyToNextButton.Visibility = Visibility.Visible;

                    for (int i = 0; i < bulkHeads.Count; i++)
                    {
                        bulkHeads[i].IdNumber = i + 1;
                        bulkHeads[i].Redraw();
                    }
                    NotifyPropertyChanged("BulkHeads");
                }
            }
        }

        private void LoadBulkhead_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = filter;
            if (dlg.ShowDialog() == true)
            {
                string f = dlg.FileName;
                Read(f);
            }
            this.InvalidateVisual();
            foreach (BulkheadControl bk in bulkHeads)
            {
                bk.InvalidateVisual();
            }
        }

        private void PerformAction(int id, string cmd)
        {
            if (cmd == "CopyToNext")
            {
                CopyOn(id);
            }
            else if (cmd == "Refresh")
            {
                if (bulkHeads.Count > 1)
                {
                    Redraw();
                }
            }
            else if (cmd == "BulkheadFocused")
            {
                selectedBulkhead = id;
            }
        }

        private int PointsPerBulkHead()
        {
            double res = bulkHeads[0].BzLines.GetLength(0);
            res = res * (1.0 / bezierStep) + 1;

            return (int)res;
        }

        private void PrependBulkhead_Click(object sender, RoutedEventArgs e)
        {
            foreach (BulkheadControl c in bulkHeads)
            {
                c.IdNumber++;
                c.FuselageBulkHead.OffsetX += 5;
            }

            FuselageBulkhead fb = new FuselageBulkhead();
            fb.Depth = 40;
            fb.Height = 40;
            fb.OffsetX = 0;

            BulkheadControl bhc = new BulkheadControl();
            bhc.FuselageBulkHead = fb;
            bhc.OnPerformAction += PerformAction;
            bhc.IdNumber = 1;
            bulkHeads.Insert(0, bhc);
            bhc.CopyToNextButton.Visibility = Visibility.Visible;

            foreach (BulkheadControl c in bulkHeads)
            {
                c.Redraw();
            }
            NotifyPropertyChanged("BulkHeads");
        }

        private void Read(string f)
        {
            try
            {
                ObservableCollection<BulkheadControl> tmp = new ObservableCollection<BulkheadControl>();
                XmlDocument doc = new XmlDocument();
                doc.Load(f);

                XmlNode docNode = doc.SelectSingleNode("Fuselage");
                XmlNodeList nodeList = docNode.SelectNodes("Bulkhead");
                foreach (XmlNode nd in nodeList)
                {
                    BulkheadControl bhc = new BulkheadControl();
                    bhc.Read(doc, nd);
                    bhc.OnPerformAction += PerformAction;
                    tmp.Add(bhc);
                }
                BulkHeads = tmp;
                NotifyPropertyChanged("BulkHeads");
                Redraw();
            }
            catch
            {
            }
        }

        private void RecordEditorParameters()
        {
            EditorParameters.Set("Bulkheads", bulkHeads.Count.ToString());
            for (int i = 0; i < bulkHeads.Count; i++)
            {
                string name = "Bulkhead" + i;
                string text = bulkHeads[i].ToEditorParameters();
                EditorParameters.Set(name, text);
            }
        }

        private void Redraw()
        {
            double radius = 5;
            if (bulkHeads != null && bulkHeads.Count > 0 && bulkHeads[bulkHeads.Count - 1].BzLines[0] != null)
            {
                int pointsPerBulkHead = PointsPerBulkHead();
                // create a place to store the indices of the vertices for each bulkhead.
                // Used to make the triangles
                indexFrame = new int[bulkHeads.Count, pointsPerBulkHead];
                Vertices.Clear();

                // create a place to store all the points from the bulkheads
                List<Point>[] blkPoints = new List<Point>[bulkHeads.Count];
                for (int i = 0; i < bulkHeads.Count; i++)
                {
                    blkPoints[i] = new List<Point>();
                }

                double x = 0;

                int pntIndex;

                // calculate the points for each bulk head
                for (int blk = 0; blk < bulkHeads.Count; blk++)
                {
                    pntIndex = 0;

                    x = bulkHeads[blk].FuselageBulkHead.OffsetX;

                    for (int line = 0; line < bulkHeads[blk].BzLines.GetLength(0); line++)
                    {
                        double lim = 1;
                        if (line == 3)
                        {
                            lim += bezierStep;
                        }
                        for (double t = 0; t < lim; t += bezierStep)
                        {
                            Point p = bulkHeads[blk].BzLines[line].GetCoord(t);
                            blkPoints[blk].Add(p);
                            int v = AddVertice(new Point3D(x, -p.Y * radius, p.X * radius));
                            indexFrame[blk, pntIndex++] = v;
                        }
                    }
                }

                Faces.Clear();

                int g;
                for (int blk = 0; blk < bulkHeads.Count - 1; blk++)
                {
                    for (int f = 0; f < pointsPerBulkHead; f++)
                    {
                        g = f + 1;
                        if (g >= pointsPerBulkHead)
                        {
                            g = 0;
                        }
                        Faces.Add(indexFrame[blk, f]);
                        Faces.Add(indexFrame[blk + 1, g]);
                        Faces.Add(indexFrame[blk + 1, f]);

                        Faces.Add(indexFrame[blk, f]);
                        Faces.Add(indexFrame[blk, g]);
                        Faces.Add(indexFrame[blk + 1, g]);
                    }
                }
                // close left side
                int leftCentre = AddVertice(new Point3D(0, 0, 0));
                for (int i = 0; i < pointsPerBulkHead - 1; i++)
                {
                    Faces.Add(leftCentre);
                    Faces.Add(indexFrame[0, i + 1]);
                    Faces.Add(indexFrame[0, i]);
                }

                // close right side
                int rightCentre = AddVertice(new Point3D(x, 0, 0));
                for (int i = 0; i < pointsPerBulkHead - 1; i++)
                {
                    Faces.Add(rightCentre);
                    Faces.Add(indexFrame[bulkHeads.Count - 1, i]);
                    Faces.Add(indexFrame[bulkHeads.Count - 1, i + 1]);
                }

                CentreVertices();
                Redisplay();
            }
        }

        private void SaveBulkhead_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = filter;
            if (dlg.ShowDialog() == true)
            {
                string f = dlg.FileName;
                Write(f);
            }
        }

        private void TopBulkhead_Click(object sender, RoutedEventArgs e)
        {
            foreach (BulkheadControl ob in bulkHeads)
            {
                ob.MoveToTop();
            }
            NotifyPropertyChanged("BulkHeads");
            Redraw();
        }

        private bool UnpackEditorParameters()
        {
            bool found = false;
            string s = EditorParameters.Get("Bulkheads");
            if (s != "")
            {
                found = true;
                int c = Convert.ToInt16(s);
                if (c > 0)
                {
                    BulkHeads.Clear();
                    for (int i = 0; i < c; i++)
                    {
                        string name = "Bulkhead" + i.ToString();
                        s = EditorParameters.Get(name);
                        if (s != "")
                        {
                            BulkheadControl bc = new BulkheadControl();
                            bc.FuselageBulkHead = new FuselageBulkhead();
                            List<Point> pnts = new List<Point>();

                            List<Point> ctrls = new List<Point>();

                            List<double> dists = new List<double>();

                            String[] lines = s.Split(',');
                            double x = 0;
                            double y = 0;

                            foreach (string t in lines)
                            {
                                string[] words = t.Split('=');
                                switch (words[0])
                                {
                                    case "Id":
                                        {
                                            bc.IdNumber = Convert.ToInt32(words[1]);
                                        }
                                        break;

                                    case "D":
                                        {
                                            bc.FuselageBulkHead.Depth = Convert.ToDouble(words[1]);
                                        }
                                        break;

                                    case "H":
                                        {
                                            bc.FuselageBulkHead.Height = Convert.ToDouble(words[1]);
                                        }
                                        break;

                                    case "X":
                                        {
                                            bc.FuselageBulkHead.OffsetX = Convert.ToDouble(words[1]);
                                        }
                                        break;

                                    case "Y":
                                        {
                                            bc.FuselageBulkHead.OffsetY = Convert.ToDouble(words[1]);
                                        }
                                        break;

                                    case "Z":
                                        {
                                            bc.FuselageBulkHead.OffsetZ = Convert.ToDouble(words[1]);
                                        }
                                        break;

                                    case "P":
                                        {
                                            GetXY(words[1], ref x, ref y);
                                            Point p = new Point(x, y);
                                            pnts.Add(p);
                                        }
                                        break;

                                    case "C":
                                        {
                                            GetXY(words[1], ref x, ref y);
                                            Point p = new Point(x, y);
                                            ctrls.Add(p);
                                        }
                                        break;

                                    case "V":
                                        {
                                            double d = Convert.ToDouble(words[1]);
                                            dists.Add(d);
                                        }
                                        break;
                                }
                            }
                            bc.OnPerformAction += PerformAction;
                            bc.Set(pnts, ctrls, dists);
                            bc.CopyToNextButton.Visibility = Visibility.Visible;
                            BulkHeads.Add(bc);
                        }
                    }
                    foreach (BulkheadControl co in bulkHeads)
                    {
                        co.Redraw();
                    }

                    bulkHeads[bulkHeads.Count - 1].CopyToNextButton.Visibility = Visibility.Hidden;
                    NotifyPropertyChanged("BulkHeads");
                }
            }
            return found;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyModelGroup.Children.Clear();
            DataContext = this;
            if (!UnpackEditorParameters())
            {
                FuselageBulkhead fb = new FuselageBulkhead();
                fb.Depth = 20;
                fb.Height = 20;
                fb.OffsetX = 0;
                BulkheadControl bhc = new BulkheadControl();
                bhc.FuselageBulkHead = fb;
                bhc.IdNumber = 1;
                bhc.CopyToNextButton.Visibility = Visibility.Visible;
                bhc.OnPerformAction += PerformAction;
                bulkHeads.Add(bhc);
                bhc.Redraw();
                fb = new FuselageBulkhead();
                fb.Depth = 20;
                fb.Height = 20;
                fb.OffsetX = 5;
                bhc = new BulkheadControl();
                bhc.FuselageBulkHead = fb;
                bhc.CopyToNextButton.Visibility = Visibility.Hidden;
                bhc.OnPerformAction += PerformAction;
                bulkHeads.Add(bhc);
                bhc.IdNumber = 2;
                bhc.Redraw();
                NotifyPropertyChanged("BulkHeads");
            }
            //Camera.Distance = 300;
            UpdateCameraPos();
            Redraw();
        }

        private void Write(string f)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement docNode = doc.CreateElement("Fuselage");
            foreach (BulkheadControl ob in bulkHeads)
            {
                ob.Write(doc, docNode);
            }
            doc.AppendChild(docNode);
            doc.Save(f);
        }
    }
}