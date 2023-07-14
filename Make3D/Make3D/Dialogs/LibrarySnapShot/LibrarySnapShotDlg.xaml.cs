using Barnacle.Models;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using ImageUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for LibrarySnapShotDlg.xaml
    /// </summary>
    public partial class LibrarySnapShotDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        public LibrarySnapShotDlg()
        {
            InitializeComponent();
            DataContext = this;
            ModelGroup = MyModelGroup;
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
                    Redisplay();
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
                    Redisplay();
                }
            }
        }

        private string partName;

        public String PartName
        {
            get
            {
                return partName;
            }

            set
            {
                if (value != partName)
                {
                    partName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string partDescription;

        public String PartDescription
        {
            get
            {
                return partDescription;
            }

            set
            {
                if (value != partDescription)
                {
                    partDescription = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Object3D part;

        public Object3D Part
        {
            get { return part; }
            set
            {
                if (value != part)
                {
                    part = value;
                    if (part != null)
                    {
                        MeshColour = part.Color;
                        Vertices.Clear();
                        Faces.Clear();
                        foreach (Point3D p in part.AbsoluteObjectVertices)
                        {
                            Vertices.Add(p);
                        }
                        foreach (int f in part.TriangleIndices)
                        {
                            Faces.Add(f);
                        }
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public string PartPath { get; internal set; }
        public string PartProjectSection { get; internal set; }

        private DispatcherTimer snapShotTimer;

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (NameIsValid())
            {
                Part.Name = PartName;
            }
            if (DescriptionIsValid())
            {
                PartDescription = Part.Description;
            }

            Document localDoc = new Document();
            localDoc.ParentProject = BaseViewModel.PartLibraryProject;
            localDoc.Content.Add(Part);

            localDoc.Save(PartPath + System.IO.Path.DirectorySeparatorChar + PartName + ".txt");
            BaseViewModel.PartLibraryProject.AddFileToFolder(PartProjectSection, PartName + ".txt", false);
            BaseViewModel.PartLibraryProject.Save();
            showFloor = false;
            showAxies = false;
            Redisplay();
            // need to give GUI a chance to repaint.
            TimeSpan interval = new TimeSpan(0, 0, 0, 0, 500);
            snapShotTimer = new DispatcherTimer();
            snapShotTimer.Interval = interval;
            snapShotTimer.Tick += SnapShotTImer_Tick;
            snapShotTimer.Start();
        }

        private void SnapShotTImer_Tick(object sender, EventArgs e)
        {
            snapShotTimer.Stop();
            ImageCapture.ScreenCaptureElement(viewport3D1, PartPath + System.IO.Path.DirectorySeparatorChar + PartName + ".png", true);
            DialogResult = true;
            Close();
        }

        private bool DescriptionIsValid()
        {
            bool result = true;
            if (PartDescription.Length > 128)
            {
                result = false;
            }
            String low = PartDescription.ToLower();
            foreach (string s in notAllowed)
            {
                if (low.IndexOf(s) > -1)
                {
                    result = false;
                }
            }
            return result;
        }

        private string[] notAllowed =
        {
            "http",
            "www",
            ".com",
            "mail"
        };

        private bool NameIsValid()
        {
            bool result = true;
            if (PartName == "" || PartName.Length > 32)
            {
                result = false;
            }
            String low = PartName.ToLower();
            foreach (string s in notAllowed)
            {
                if (low.IndexOf(s) > -1)
                {
                    result = false;
                }
            }
            return result;
        }

        private void BaseModellerDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Redisplay();
            Camera.HomeFront();
            SetCameraDistance();
            UpdateCameraPos();
            if (Part != null)
            {
                PartName = Part.Name;
                PartDescription = Part.Description;
            }
        }
    }
}