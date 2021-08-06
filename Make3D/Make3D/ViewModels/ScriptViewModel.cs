using ScriptLanguage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Make3D.ViewModels
{
    internal class ScriptViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private const String defaultSource = @"
program ""Name""
{
 Include ""libs\limplib.txt"" ;
}
";

        private string filePath;
        private FlowDocument flowDocument;
        private Interpreter interpreter;
        private string rtf;
        private Script script;
        private bool updatedInternally;

        public ScriptViewModel()
        {
            filePath = "";
            Dirty = false;
            Source = defaultSource;
            Rtf = Source;

            interpreter = new Interpreter();
            script = new Script();
            interpreter.LoadFromText(script, defaultSource);
            Rtf = script.ToRichText();
            ScriptCommand = new RelayCommand(OnScriptCommand);

            NotificationManager.Subscribe("LimpetLoaded", OnLimpetLoaded);
            NotificationManager.Subscribe("LimpetClosing", OnLimpetClosing);
        }

        public bool Dirty { get; set; }

        public FlowDocument FlowDoc
        {
            get
            {
                return flowDocument;
            }

            set
            {
                if (flowDocument != value)
                {
                    flowDocument = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String Rtf
        {
            get
            {
                return rtf;
            }
            set
            {
                if (rtf != value)
                {
                    rtf = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand ScriptCommand
        {
            get; set;
        }

        public String Source
        {
            get;
            set;
        }

        public bool UpdatedInternally
        {
            get
            {
                return updatedInternally;
            }
            set
            {
                if (updatedInternally != value)
                {
                    updatedInternally = value;
                    NotifyPropertyChanged();
                }
            }
        }

        internal String RawText { get; set; }

        internal void RunScript()
        {
            script.Execute();
        }

        internal bool ScriptText(string scriptText)
        {
            Source = scriptText;
            bool result = interpreter.LoadFromText(script, Source);
            if (result)
            {
                Rtf = script.ToRichText();
                RawText = scriptText;
            }
            UpdatedInternally = false;
            return result;
        }

        internal void SetResultsBox(System.Windows.Controls.TextBox resultsBox)
        {
            Log.Instance().SetTextBox(resultsBox);
        }

        private void OnLimpetClosing(object param)
        {
            if (Dirty == true && filePath != "")
            {
                MessageBoxResult res = MessageBox.Show("Script changed.|Save changes before closing?", "Warning", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.WriteAllText(filePath, RawText);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void OnLimpetLoaded(object param)
        {
            string fileName = param.ToString();
            if (File.Exists(fileName))
            {
                Source = File.ReadAllText(fileName);
                interpreter = new Interpreter();
                script = new Script();
                if (interpreter.LoadFromText(script, Source))
                {
                    Rtf = script.ToRichText();
                }
                else
                {
                    Rtf = script.ToErrorRichText(Source);
                }
                Dirty = false;
                filePath = fileName;
                NotificationManager.Notify("UpdateScript", null);
            }
        }

        private void OnScriptCommand(object obj)
        {
            String com = obj.ToString();
            com = com.ToLower();
            switch (com)
            {
                case "new":
                    {
                        Source = defaultSource;
                        interpreter.LoadFromText(script, Source);
                        Rtf = script.ToRichText();
                        NotificationManager.Notify("UpdateScript", null);
                        Dirty = true;
                    }
                    break;

                case "clear":
                    {
                        Log.Instance().Clear();
                    }
                    break;

                case "save":
                    {
                        if (filePath != "")
                        {
                            try
                            {
                                File.WriteAllText(filePath, RawText);
                                Dirty = false;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                    break;

                case "load":
                    {
                        /*
                        OpenFileDialog dlg = new OpenFileDialog();
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                Source = File.ReadAllText(dlg.FileName);
                                interpreter = new Interpreter();
                                script = new Script();
                                if (interpreter.LoadFromText(script, Source))
                                {
                                    Rtf = script.ToRichText();
                                }
                                else
                                {
                                    Rtf = Source;
                                }
                                NotificationManager.Notify("Update", null);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                        */
                    }
                    break;
            }
        }
    }
}