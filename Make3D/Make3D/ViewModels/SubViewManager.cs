using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Make3D.Views;

namespace Make3D.ViewModels
{
    public class SubViewManager
    {
        public void CloseCurrent()
        {
            NotificationManager.Unsubscribe("Copy");
            NotificationManager.Unsubscribe("Cut");
            NotificationManager.Unsubscribe("Paste");
            NotificationManager.Unsubscribe("AddPage");
            NotificationManager.Unsubscribe("AddNewPage");
            NotificationManager.Unsubscribe("AddObject");
            NotificationManager.Unsubscribe("ShowGrid");
            NotificationManager.Unsubscribe("ShowMargins");
            NotificationManager.Unsubscribe("ZoomIn");
            NotificationManager.Unsubscribe("ZoomOut");
            NotificationManager.Unsubscribe("ZoomReset");
            NotificationManager.Unsubscribe("FillColour");
            NotificationManager.Unsubscribe("StrokeColour");
            NotificationManager.Unsubscribe("Alignment");
            NotificationManager.Unsubscribe("Size");
        }

        public UserControl GetView(string name)
        {
            name = name.ToLower().Trim();
            UserControl result = null;
            if (name == "default")
            {
                result = new DefaultView();
            }
            if (name == "editor")
            {
                result = new EditorView();
            }
            if (name == "startup")
            {
                result = new StartupView();
            }
            /*
            if (name == "about")
            {
                result = new AboutBoxView();
            }
            if (name == "printpreview")
            {
                result = new PrintPreviewView();
            }
            if (name == "pageedit")
            {
                result = new PageEditView();
            }
            */
            return result;
        }
    }
}