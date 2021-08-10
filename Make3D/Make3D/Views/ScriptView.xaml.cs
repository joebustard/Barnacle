using Make3D.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Make3D.Views
{
    /// <summary>
    /// Interaction logic for ScriptView.xaml
    /// </summary>
    public partial class ScriptView : UserControl
    {
        private DispatcherTimer dispatcherTimer;

        private String previousText;

        private ScriptViewModel vm;

        public ScriptView()
        {
            InitializeComponent();
        }

        private void DeferSyntaxCheck()
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();

                dispatcherTimer.Start();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();

            TextRange tr = new TextRange(ScriptBox.Document.ContentStart, ScriptBox.Document.ContentEnd);
            string scriptText = tr.Text;
            if (previousText != scriptText)
            {
                if (vm.ScriptText(scriptText))
                {
                    SetDisplayRtf();
                }
                previousText = scriptText;
            }
        }

        private void OnUpdate(object param)
        {
            SetDisplayRtf();
        }

        private void RunClicked(object sender, RoutedEventArgs e)
        {
            ResultsBox.Text = "";
            vm.SetResultsBox(ResultsBox);
            vm.RunScript();
        }

        private void ScriptBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            DeferSyntaxCheck();
        }

        private void ScriptBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange tr = new TextRange(ScriptBox.Document.ContentStart, ScriptBox.Document.ContentEnd);
            vm.RawText = tr.Text;
            System.Diagnostics.Debug.WriteLine("******");
            System.Diagnostics.Debug.WriteLine(tr.Text);

            DeferSyntaxCheck();
            vm.Dirty = true;
        }

        private void ScriptView_Loaded(object sender, RoutedEventArgs e)
        {
            vm = DataContext as ScriptViewModel;
            ScriptBox.Width = ScriptGrid.ActualWidth;
            vm.SetResultsBox(ResultsBox);
            SetDisplayRtf();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();
            NotificationManager.Subscribe("UpdateScript", OnUpdate);
        }

        private void SetDisplayRtf()
        {
            TextRange tr = new TextRange(ScriptBox.Document.ContentStart, ScriptBox.Document.ContentEnd);
            MemoryStream ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(vm.Rtf));
            tr.Load(ms, DataFormats.Rtf);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            vm.KeyDown(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                                 Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl));
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            vm.KeyUp(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                             Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl));
        }
    }
}