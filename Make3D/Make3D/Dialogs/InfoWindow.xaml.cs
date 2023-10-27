using System;
using System.Windows;

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
            IsClosed = false;
            Closed += InfoWindow_Closed;
        }

        private void InfoWindow_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        public static InfoWindow Instance()
        {
            if (instance == null)
            {
                instance = new InfoWindow();
                instance.Owner = Application.Current.MainWindow;
            }
            else
            {
                if (instance.IsClosed)
                {
                    instance = new InfoWindow();
                }
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
            Instance().Show();
        }

        public bool IsClosed { get; set; } = false;

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

        public void UpdateText(String s)
        {
            instance.InfoLabel.Content = s;
        }
    }
}