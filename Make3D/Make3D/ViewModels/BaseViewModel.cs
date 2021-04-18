using Make3D.Models;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using VisualSolutionExplorer;

namespace Make3D.ViewModels
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        // only one document shared between all the views
        protected static Document document;

        protected static Project project;

        protected bool lastChangeWasNudge;

        private static SolidColorBrush _fillBrushColor = Brushes.Black;

        private static SolidColorBrush _strokeBrushColor = Brushes.Black;

        private static string selectedFont;

        public BaseViewModel()
        {
            if (document == null)
            {
                document = new Document();
            }
            if (project == null)
            {
                project = new Project();
                project.CreateDefault();
            }
            document.PropertyChanged += Document_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static Document Document
        {
            get { return document; }
        }

        public static Project Project
        {
            get { return project; }
        }

        public SolidColorBrush FillColor
        {
            get
            {
                return _fillBrushColor;
            }
            set
            {
                if (value != _fillBrushColor)
                {
                    _fillBrushColor = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("FillColour", _fillBrushColor);
                }
            }
        }

        public String SelectedFont
        {
            get
            {
                return selectedFont;
            }

            set
            {
                if (selectedFont != value)
                {
                    selectedFont = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("FontName", selectedFont);
                }
            }
        }

        public SolidColorBrush StrokeColor
        {
            get
            {
                return _strokeBrushColor;
            }
            set
            {
                if (value != _strokeBrushColor)
                {
                    _strokeBrushColor = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("StrokeColour", _strokeBrushColor);
                }
            }
        }

        public Viewport3D ViewPort { get; set; }

        public void CheckPoint()
        {
            lastChangeWasNudge = false;
            if (Document != null)
            {
                string s = undoer.GetNextCheckPointName();
                Document.Write(s);
            }
        }

        public void CheckPointForNudge()
        {
            if (!lastChangeWasNudge)
            {
                CheckPoint();
            }
            lastChangeWasNudge = true;
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Undo()
        {
            if (undoer.CanUndo())
            {
                string s = undoer.GetLastCheckPointName();
                Document.Read(s, true);
                NotificationManager.Notify("Refresh", null);
            }
        }

        private void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Caption")
            {
                NotifyPropertyChanged("Caption");
            }
        }
    }
}