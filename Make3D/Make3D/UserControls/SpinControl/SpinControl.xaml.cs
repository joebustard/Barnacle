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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    /// <summary>
    /// Interaction logic for Twirlywoo.xaml
    /// </summary>
    public partial class SpinControl : UserControl
    {
        private Storyboard myStoryboard;

        public SpinControl()
        {
            InitializeComponent();
            myStoryboard = new Storyboard();
            var myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 0.0;
            myDoubleAnimation.To = 360;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1.5));
            myDoubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            myStoryboard.Children.Add(myDoubleAnimation);

            Storyboard.SetTargetName(myDoubleAnimation, loadingImage.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            loadingImage.Loaded += new RoutedEventHandler(myRectangleLoaded);
        }

        private void myRectangleLoaded(object sender, RoutedEventArgs e)
        {
            myStoryboard.Begin(this);
        }
    }
}