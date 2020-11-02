using System;
using System.Windows;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for DecimatorSettings.xaml
    /// </summary>
    public partial class DecimatorSettings : Window
    {
        public int OriginalFaceCount { get; set; }
        public int TargetFaceCount { get; set; }

        public DecimatorSettings()
        {
            InitializeComponent();
            OriginalFaceCount = 0;
            TargetFaceCount = 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            TargetFaceCount = Convert.ToInt32(TargetBox.Text);
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OriginalFacesLabel.Content = $"Object has {OriginalFaceCount} faces.";
            TargetBox.Text = (OriginalFaceCount * 90 / 100).ToString();
        }
    }
}