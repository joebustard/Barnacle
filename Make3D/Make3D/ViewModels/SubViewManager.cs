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
        private string currentViewName = "";
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
            if (currentViewName != null && currentViewName != "")
            {
                NotificationManager.ViewUnsubscribe(currentViewName);
            }
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
            if (name == "script")
            {
                result = new ScriptView();
            }
            currentViewName = name;
            return result;
        }
    }
}