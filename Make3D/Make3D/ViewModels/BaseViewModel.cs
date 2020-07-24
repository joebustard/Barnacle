using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Make3D.Models;

namespace Make3D.ViewModels
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // only one document shared between all the views
        protected static Document document = new Document();

        public static Document Document
        {
            get { return document; }
        }

        private static SolidColorBrush _fillBrushColor = Brushes.Black;

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

        private static SolidColorBrush _strokeBrushColor = Brushes.Black;

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

        private static string selectedFont;

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
     

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
