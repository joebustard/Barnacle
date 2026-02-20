// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using Barnacle.ViewModels.Logging;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using static CSGLib.BooleanModeller;

namespace Barnacle.Views
{
    /// <summary>
    /// Interaction logic for ScriptView.xaml
    /// </summary>
    public partial class ScriptView : UserControl
    {
        private DispatcherTimer dispatcherTimer;
        private GeometryModel3D lastHitModel;
        private Point3D lastHitPoint;
        private Point lastMousePos;
        private bool loaded;
        private DispatcherTimer refocusTimer;
        private ScriptViewModel vm;

        public ScriptView()
        {
            InitializeComponent();
            loaded = false;
            NotificationManager.Subscribe("InsertIntoScript", InsertIntoScript);
        }

        private void CheckClicked(object sender, RoutedEventArgs e)
        {
            vm.ClearResults();
            if (RefreshInterpreterSource())
            {
                Log.Instance().AddEntry("OK");
            }
            else
            {
                CopyLanguageLogs();
            }
        }

        private void CopyLanguageLogs()
        {
            Log.Instance().Clear();
            foreach (ScriptLanguage.LogEntry le in ScriptLanguage.Log.Instance().LogEntrys)
            {
                Log.Instance().AddEntry(le.DateStamp, le.Text);
            }
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
            if (vm.EnableRun)
            {
                //  RefreshInterpreterSource();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            lastMousePos = e.GetPosition(this);
            vm.MouseMove(lastMousePos, e);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lastMousePos = e.GetPosition(this);
            vm.MouseUp(lastMousePos, e);
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            vm.MouseWheel(e);
        }

        private void InsertIntoScript(object param)
        {
            string what = param.ToString();
            if (what != null)
            {
                String ins = "";
                switch (what)
                {
                    case "SolidFunction":
                        {
                            ins =
 @"
// Name       :
// Does       :
// Parameters :
// Returns    :
Function Solid MyFunc( string Name, double px, double py, double pz, double l, double h, double w )
{
Solid result;
return result;
}";
                        }
                        break;

                    case "Procedure":
                        {
                            ins =
 @"
// Name       :
// Does       :
// Parameters :
Procedure MyProc( double px, double py, double pz, double l, double h, double w )
{
}";
                        }
                        break;

                    case "BlankProcedure":
                        {
                            ins =
 @"
// Name       :
// Does       :
// Parameters :
Procedure MyProc(  )
{
}";
                        }
                        break;

                    case "FuncHead":
                        {
                            ins =
 @"
// Name       :
// Does       :
// Parameters :
// Returns    :
";
                        }
                        break;
                }
                if (ins != "")
                {
                    ScriptBox.Selection.Text = ins;
                }
            }
        }

        private void OnEditTabSelected(object sender, RoutedEventArgs e)
        {
            refocusTimer = new DispatcherTimer();
            refocusTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            refocusTimer.Tick += RefocusTimer_Tick;
            refocusTimer.Start();
        }

        private void OnUpdate(object param)
        {
            SetDisplayRtf();
        }

        private void RefocusTimer_Tick(object sender, EventArgs e)
        {
            refocusTimer.Stop();
            ScriptBox.Focus();
            ScriptBox.RestoreCurrentPosition();
        }

        private bool RefreshInterpreterSource()
        {
            bool ok = false;
            TextRange tr = new TextRange(ScriptBox.Document.ContentStart, ScriptBox.Document.ContentEnd);
            string scriptText = tr.Text;
            if (vm.EnableRun)
            {
                if (vm.ScriptText(scriptText))
                {
                    SetDisplayRtf();
                    ok = true;
                }
            }
            return ok;
        }

        private void Run()
        {
            dispatcherTimer.Stop();
            ResultsBox.Text = "";

            vm.ClearResults();

            if (RefreshInterpreterSource())
            {
                vm.RunScript();
            }
            else
            {
                foreach (ScriptLanguage.LogEntry le in ScriptLanguage.Log.Instance().LogEntrys)
                {
                    ResultsBox.Text += $"{le.DateStamp}, {le.Text}\r\n";
                }
            }
        }

        private void RunClicked(object sender, RoutedEventArgs e)
        {
            Run();
        }

        private void ScriptBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            DeferSyntaxCheck();

            if (e.Key == Key.F3)
            {
                vm.FindCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.F4)
            {
                vm.SwitchTabs();
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                e.Handled = true;
                Run();
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                vm.Escape();
            }
        }

        private void ScriptBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange tr = new TextRange(ScriptBox.Document.ContentStart, ScriptBox.Document.ContentEnd);
            vm.Source = tr.Text;
            DeferSyntaxCheck();
            if (loaded)
            {
                vm.Dirty = true;
            }
        }

        private void ScriptView_Loaded(object sender, RoutedEventArgs e)
        {
            vm = DataContext as ScriptViewModel;
            ScriptBox.Width = ScriptGrid.ActualWidth;
            vm.Viewer = Viewer;
            SetDisplayRtf();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 1, 0);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();
            NotificationManager.Subscribe("Script", "UpdateScript", OnUpdate);
            vm.ScriptBox = ScriptBox;
            vm.SetResultsBox(ResultsBox);
            ScriptBox.Focusable = true;
            loaded = true;
            vm.EnableRun = true;
            vm.Dirty = false;
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

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScriptBox.InvalidateArrange();
            ScriptBox.InvalidateMeasure();
            ScriptBox.UpdateLayout();
        }
    }
}