using Barnacle.Views;
using System.Windows.Controls;

namespace Barnacle.ViewModels
{
    public class SubViewManager
    {
        private string currentViewName = "";
        private string previousViewName = "";
        public string CurrentViewName
        { get { return currentViewName; } }

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
            if (name != currentViewName)
            {
                previousViewName = currentViewName;
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

        internal UserControl ReturnToPrevious()
        {
            if (previousViewName != null && previousViewName != "")
            {
                return GetView(previousViewName);
            }
            else
            {
                return GetView("editor");
            }
        }
    }
}