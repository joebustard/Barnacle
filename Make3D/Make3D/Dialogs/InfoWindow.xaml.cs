using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        private static Action EmptyDelegate = delegate () { };
        private static InfoWindow instance;

        private InfoWindow()
        {
            InitializeComponent();
        }

        public static InfoWindow Instance()
        {
            if (instance == null)
            {
                instance = new InfoWindow();
            }
            return instance;
        }

        public static void Refresh(System.Windows.UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }

        public void CloseInfo()
        {
            instance.Close();
            instance = null;
        }

        public void ShowInfo()
        {
            instance.Show();
        }

        public void ShowInfo(String title)
        {
            Instance().Title = title;
            try
            {
                instance.Show();
            }
            catch
            {

            }
        }
        public void ShowText(String s)
        {
            instance.InfoLabel.Content = s;
            Refresh(instance.InfoLabel);
        }
    }
}