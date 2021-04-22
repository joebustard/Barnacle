using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Make3D.ViewModels
{
    internal class CameraViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private int buttonAreaHeight;

        private Visibility cameraPaletteVisible;

        public CameraViewModel()
        {
            CameraCommand = new RelayCommand(OnCameraCommand);
            NotificationManager.Subscribe("CameraPaletteVisible", OnCameraPaletteVisibleChanged);
            ButtonAreaHeight = 100;
        }

        public int ButtonAreaHeight
        {
            get
            {
                return buttonAreaHeight;
            }
            set
            {
                if (buttonAreaHeight != value)
                {
                    buttonAreaHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand CameraCommand { get; set; }

        public Visibility CameraPaletteVisible
        {
            get
            {
                return cameraPaletteVisible;
            }

            set
            {
                if (cameraPaletteVisible != value)
                {
                    cameraPaletteVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void OnCameraCommand(object obj)
        {
            // Don't try to handle it here
            // just post on
            NotificationManager.Notify("CameraCommand", obj);
        }

        private void OnCameraPaletteVisibleChanged(object param)
        {
            Visibility v = (Visibility)param;
            CameraPaletteVisible = v;
        }
    }
}