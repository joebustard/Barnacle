using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Forms;
using System.IO;

namespace DlgMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static string ctrString = @"                <Label VerticalAlignment=""Center"" Width=""100""><pName></Label>
                 <TextBox Margin=""8,0,0,0"" Text=""{Binding <pName>}"" Width=""150""/>
";

        private static string ldrb = @"
          if ( EditorParameters.Get(""<pName>"") !="""")
          {
              <pName>= EditorParameters.GetBool(""<pName>"");
          }
";

        private static string ldrd = @"
          if ( EditorParameters.Get(""<pName>"") !="""")
          {
              <pName>= EditorParameters.GetDouble(""<pName>"");
          }
";

        private static string ldri = @"
          if ( EditorParameters.Get(""<pName>"") !="""")
          {
              <pName>= EditorParameters.GetInt(""<pName>"");
          }
";

        private static string propTemplate = @"

private <pType> <pName>;
public <pType> <PName>
{
    get
    {
      return <pName>;
    }
    set
    {
        if ( <pName> != value )
        {
           <pName> = value;
           NotifyPropertyChanged();
              UpdateDisplay();
        }
    }
}

";

        private static string propTemplateh = @"

private <pType> <pName>;
public <pType> <PName>
{
    get
    {
      return <pName>;
    }
    set
    {
        if ( <pName> != value )
        {
            if ( value <= <pMax>)
            {
              <pName> = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}

";

        private static string propTemplatel = @"

private <pType> <pName>;
public <pType> <PName>
{
    get
    {
      return <pName>;
    }
    set
    {
        if ( <pName> != value )
        {
            if ( value >= <pMin>)
            {
              <pName> = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}

";

        private static string propTemplatelh = @"

private <pType> <pName>;
public <pType> <PName>
{
    get
    {
      return <pName>;
    }
    set
    {
        if ( <pName> != value )
        {
            if (value >= <pMin> && value <= <pMax>)
            {
              <pName> = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}

";

        private static string rchkh = @"
            if (val<pName> > <pMax> )
            {
                Log.Instance().AddEntry(""Make<ToolName> : <pName> value > max (<pMax>)"");
                inRange= false;
            }
";

        private static string rchkl = @"
            if (val<pName> < <pMin> )
            {
                Log.Instance().AddEntry(""Make<ToolName> : <pName> value < min (<pMin>)"");
                inRange= false;
            }
";

        private static string rchklh = @"
            if (val<pName> < <pMin> || val<pName> > <pMax> )
            {
                Log.Instance().AddEntry(""Make<ToolName> : <pName> value out of range (<pMin>..<pMax>)"");
                inRange= false;
            }
";

        private string dialogName;
        private string exportPath;
        private String makerName;
        private String p1Max;
        private String p1Min;
        private String p1Name;
        private String p1Type;

        //
        private String p2Max;

        private String p2Min;
        private String p2Name;
        private String p2Type;

        //
        private String p3Max;

        private String p3Min;
        private String p3Name;
        private String p3Type;

        //
        private String p4Max;

        private String p4Min;
        private String p4Name;
        private String p4Type;

        //
        private String p5Max;

        private String p5Min;
        private String p5Name;
        private String p5Type;

        //
        private String p6Max;

        private String p6Min;
        private String p6Name;
        private String p6Type;

        //
        private List<String> parameterTypes;

        private String toolName;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string DialogName
        {
            get
            {
                return dialogName;
            }
            set
            {
                if (dialogName != value)
                {
                    dialogName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ExportPath
        {
            get
            {
                return exportPath;
            }
            set
            {
                if (exportPath != value)
                {
                    exportPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String MakerName
        {
            get
            {
                return makerName;
            }
            set
            {
                if (makerName != value)
                {
                    makerName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P1Max
        {
            get
            {
                return p1Max;
            }
            set
            {
                if (p1Max != value)
                {
                    p1Max = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P1Min
        {
            get
            {
                return p1Min;
            }
            set
            {
                if (p1Min != value)
                {
                    p1Min = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P1Name
        {
            get
            {
                return p1Name;
            }
            set
            {
                if (p1Name != value)
                {
                    p1Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P1Type
        {
            get
            {
                return p1Type;
            }
            set
            {
                if (p1Type != value)
                {
                    p1Type = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //
        public String P2Max
        {
            get
            {
                return p2Max;
            }
            set
            {
                if (p2Max != value)
                {
                    p2Max = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P2Min
        {
            get
            {
                return p2Min;
            }
            set
            {
                if (p2Min != value)
                {
                    p2Min = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P2Name
        {
            get
            {
                return p2Name;
            }
            set
            {
                if (p2Name != value)
                {
                    p2Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P2Type
        {
            get
            {
                return p2Type;
            }
            set
            {
                if (p2Type != value)
                {
                    p2Type = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P3Max
        {
            get
            {
                return p3Max;
            }
            set
            {
                if (p3Max != value)
                {
                    p3Max = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P3Min
        {
            get
            {
                return p3Min;
            }
            set
            {
                if (p3Min != value)
                {
                    p3Min = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P3Name
        {
            get
            {
                return p3Name;
            }
            set
            {
                if (p3Name != value)
                {
                    p3Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P3Type
        {
            get
            {
                return p3Type;
            }
            set
            {
                if (p3Type != value)
                {
                    p3Type = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //
        public String P4Max
        {
            get
            {
                return p4Max;
            }
            set
            {
                if (p4Max != value)
                {
                    p4Max = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P4Min
        {
            get
            {
                return p4Min;
            }
            set
            {
                if (p4Min != value)
                {
                    p4Min = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P4Name
        {
            get
            {
                return p4Name;
            }
            set
            {
                if (p4Name != value)
                {
                    p4Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P4Type
        {
            get
            {
                return p4Type;
            }
            set
            {
                if (p4Type != value)
                {
                    p4Type = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //
        public String P5Max
        {
            get
            {
                return p5Max;
            }
            set
            {
                if (p5Max != value)
                {
                    p5Max = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P5Min
        {
            get
            {
                return p5Min;
            }
            set
            {
                if (p5Min != value)
                {
                    p5Min = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P5Name
        {
            get
            {
                return p5Name;
            }
            set
            {
                if (p5Name != value)
                {
                    p5Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P5Type
        {
            get
            {
                return p5Type;
            }
            set
            {
                if (p5Type != value)
                {
                    p5Type = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //
        public String P6Max
        {
            get
            {
                return p6Max;
            }
            set
            {
                if (p6Max != value)
                {
                    p6Max = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P6Min
        {
            get
            {
                return p6Min;
            }
            set
            {
                if (p6Min != value)
                {
                    p6Min = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P6Name
        {
            get
            {
                return p6Name;
            }
            set
            {
                if (p6Name != value)
                {
                    p6Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String P6Type
        {
            get
            {
                return p6Type;
            }
            set
            {
                if (p6Type != value)
                {
                    p6Type = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //
        public List<String> ParameterTypes
        {
            get
            {
                return parameterTypes;
            }
            set
            {
                if (parameterTypes != value)
                {
                    parameterTypes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String ToolName
        {
            get
            {
                return toolName;
            }
            set
            {
                if (toolName != value)
                {
                    toolName = value;
                    NotifyPropertyChanged();
                    DialogName = toolName.Trim() + "Dlg";
                    MakerName = "Make" + toolName.Trim();
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static string GenProp(string s, string t, string l, string h)
        {
            string res1 = "";
            if (s != null && s != "")
            {
                string pName = s.ToLower().Substring(0, 1) + s.Substring(1);
                string PName = s.ToUpper().Substring(0, 1) + s.Substring(1);

                if (l == "" && h == "")
                {
                    res1 = propTemplate;
                }
                if (l != "" && h == "")
                {
                    res1 = propTemplatel;
                }
                if (l == "" && h != "")
                {
                    res1 = propTemplateh;
                }
                if (l != "" && h != "")
                {
                    res1 = propTemplatelh;
                }
                res1 = res1.Replace("<pName>", pName);
                res1 = res1.Replace("<PName>", PName);
                res1 = res1.Replace("<pType>", t);
                res1 = res1.Replace("<pMin>", l);
                res1 = res1.Replace("<pMax>", h);
            }
            return res1;
        }

        private string AddParam(string res, string name)
        {
            if (name != null && name != "")
            {
                if (res != "")
                {
                    res += ", ";
                }
                res += name.ToLower().Substring(0, 1) + name.Substring(1);
            }
            return res;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (exportPath != null && exportPath.Trim() != "")
                {
                    dialog.SelectedPath = exportPath;
                }
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ExportPath = dialog.SelectedPath;
                    Properties.Settings.Default.lastExportPath = ExportPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void CreateDialog(string templateRoot, string targetFolder)
        {
            string toolProps = GetToolProperties();
            string makerParams = GetMakerParams();
            string saveParams = "";
            saveParams += GetSaveParams(p1Name);
            saveParams += GetSaveParams(p2Name);
            saveParams += GetSaveParams(p3Name);
            saveParams += GetSaveParams(p4Name);
            saveParams += GetSaveParams(p5Name);
            saveParams += GetSaveParams(p6Name);

            string loadParams = "";
            loadParams += GetLoadParams(p1Name, p1Type);
            loadParams += GetLoadParams(p2Name, p2Type);
            loadParams += GetLoadParams(p2Name, p3Type);
            loadParams += GetLoadParams(p2Name, p4Type);
            loadParams += GetLoadParams(p2Name, p5Type);
            loadParams += GetLoadParams(p2Name, p6Type);
            string p1Controls = GetControls(P1Name);
            string p2Controls = GetControls(P2Name);
            string p3Controls = GetControls(P3Name);
            string p4Controls = GetControls(P4Name);
            string p5Controls = GetControls(P5Name);
            string p6Controls = GetControls(P6Name);

            System.IO.Directory.CreateDirectory(targetFolder);
            string[] files = System.IO.Directory.GetFiles(templateRoot, "Blank*.*");
            foreach (string fn in files)
            {
                string targetName = fn.Replace(templateRoot, targetFolder);
                targetName = targetName.Replace("Blank", DialogName);
                StreamReader fin = new StreamReader(fn);
                if (fin != null)
                {
                    StreamWriter fout = new StreamWriter(targetName);
                    while (!fin.EndOfStream)
                    {
                        String l = fin.ReadLine();
                        l = l.Replace("Blank", ToolName);
                        l = l.Replace("<TOOLPROPS>", toolProps);
                        l = l.Replace("<MAKEPARAMETERS>", makerParams);
                        l = l.Replace("<P1CONTROLS>", p1Controls);
                        l = l.Replace("<P2CONTROLS>", p2Controls);
                        l = l.Replace("<P3CONTROLS>", p3Controls);
                        l = l.Replace("<P4CONTROLS>", p4Controls);
                        l = l.Replace("<P5CONTROLS>", p5Controls);
                        l = l.Replace("<P6CONTROLS>", p6Controls);
                        l = l.Replace("<LOADPARMETERS>", loadParams);
                        l = l.Replace("<SAVEPARMETERS>", saveParams);
                        fout.WriteLine(l);
                    }
                    fin.Close();
                    fout.Close();
                }
            }
        }

        private void CreateInterpreterNode(string templateRoot, string targetFolder)
        {
            string[] files = System.IO.Directory.GetFiles(templateRoot, "Node*.*");
            string constructorParams = GetAllNodeConstructorParams();
            string nodeFields = GetAllNodeFields();
            string copyFields = GetAllNodeCopyFields();
            string exeValueFields = GetExeValueFields();
            string evalExpressions = GetEvalExpressions();
            string makerParams = GetMakerNodeParams();
            string richTextParams = GetRichTextParams();
            string plainTextParams = GetPlainTextParams();
            string rangeChecks = GetRangeChecks();
            foreach (string fn in files)
            {
                string targetName = fn.Replace(templateRoot, targetFolder);
                targetName = targetName.Replace("Node", "Make" + ToolName + "Node");
                StreamReader fin = new StreamReader(fn);
                if (fin != null)
                {
                    StreamWriter fout = new StreamWriter(targetName);
                    while (!fin.EndOfStream)
                    {
                        String l = fin.ReadLine();
                        l = l.Replace("<TOOLNAME>", ToolName);
                        l = l.Replace(@"//CONSTRUCTORPARAMETERS", constructorParams);
                        l = l.Replace(@"//NODEFIELDS", nodeFields);
                        l = l.Replace(@"//COPYFIELDS", copyFields);
                        l = l.Replace(@"//EXECUTIONVALUEDECLARATIONS", exeValueFields);
                        l = l.Replace(@"//EVALEXPRESSIONS", evalExpressions);
                        l = l.Replace(@"//MAKERPARAMS", makerParams);
                        l = l.Replace(@"//EXPRESSIONTORICHTEXT", richTextParams);
                        l = l.Replace(@"//EXPRESSIONTOSTRING", plainTextParams);
                        l = l.Replace(@"//RANGECHECKS", rangeChecks);
                        fout.WriteLine(l);
                    }
                    fin.Close();
                    fout.Close();
                }
            }
        }

        private void CreateMaker(string templateRoot, string targetFolder)
        {
            System.IO.Directory.CreateDirectory(targetFolder);

            string[] files = System.IO.Directory.GetFiles(templateRoot, "Maker.*");
            foreach (string fn in files)
            {
                string fields = GetAllFields();
                string constructorParams = GetAllConstructorParams();
                string fieldCopy = GetAllFieldCopies();
                string targetName = fn.Replace(templateRoot, targetFolder);
                targetName = targetName.Replace("Maker", "Make" + ToolName);
                StreamReader fin = new StreamReader(fn);
                if (fin != null)
                {
                    StreamWriter fout = new StreamWriter(targetName);
                    while (!fin.EndOfStream)
                    {
                        String l = fin.ReadLine();
                        l = l.Replace("<TOOL>", ToolName);
                        l = l.Replace("<FIELDS>", fields);
                        l = l.Replace("<CONSTRUCTORPARAMS>", constructorParams);
                        l = l.Replace("<FIELDCOPY>", fieldCopy);

                        fout.WriteLine(l);
                    }
                    fin.Close();
                    fout.Close();
                }
            }
        }

        private void GenerateClicked(object sender, RoutedEventArgs e)
        {
            if (exportPath != null && exportPath.Trim() != "")
            {
                try
                {
                    string templateRoot = AppDomain.CurrentDomain.BaseDirectory + "templates";
                    string targetFolder = exportPath + "\\" + DialogName;

                    CreateDialog(templateRoot, targetFolder);
                    CreateMaker(templateRoot, targetFolder);
                    CreateInterpreterNode(templateRoot, targetFolder);
                    System.Windows.MessageBox.Show("Done");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }

        private string GetAllConstructorParams()
        {
            string res = "";
            res = GetContructorParam(res, p1Name, p1Type);
            res = GetContructorParam(res, p2Name, p2Type);
            res = GetContructorParam(res, p3Name, p3Type);
            res = GetContructorParam(res, p4Name, p4Type);
            res = GetContructorParam(res, p5Name, p5Type);
            res = GetContructorParam(res, p6Name, p6Type);
            return res;
        }

        private string GetAllFieldCopies()
        {
            string res = "";
            res += GetFieldCopy(p1Name);
            res += GetFieldCopy(p2Name);
            res += GetFieldCopy(p3Name);
            res += GetFieldCopy(p4Name);
            res += GetFieldCopy(p5Name);
            res += GetFieldCopy(p6Name);
            return res;
        }

        private string GetAllFields()
        {
            string res = "";
            res += GetField(p1Name, p1Type);
            res += GetField(p2Name, p2Type);
            res += GetField(p3Name, p3Type);
            res += GetField(p4Name, p4Type);
            res += GetField(p5Name, p5Type);
            res += GetField(p6Name, p6Type);

            return res;
        }

        private string GetAllNodeConstructorParams()
        {
            string res = "";
            res = GetNodeContructorParam(res, p1Name, p1Type);
            res = GetNodeContructorParam(res, p2Name, p2Type);
            res = GetNodeContructorParam(res, p3Name, p3Type);
            res = GetNodeContructorParam(res, p4Name, p4Type);
            res = GetNodeContructorParam(res, p5Name, p5Type);
            res = GetNodeContructorParam(res, p6Name, p6Type);
            return res;
        }

        private string GetAllNodeCopyFields()
        {
            string res = "";
            res += GetNodeCopyField(p1Name);
            res += GetNodeCopyField(p2Name);
            res += GetNodeCopyField(p3Name);
            res += GetNodeCopyField(p4Name);
            res += GetNodeCopyField(p5Name);
            res += GetNodeCopyField(p6Name);
            return res;
        }

        private string GetAllNodeFields()
        {
            string res = "";
            res += GetNodeField(p1Name);
            res += GetNodeField(p2Name);
            res += GetNodeField(p3Name);
            res += GetNodeField(p4Name);
            res += GetNodeField(p5Name);
            res += GetNodeField(p6Name);
            return res;
        }

        private string GetControls(string name)
        {
            string res = "";
            if (name != null && name != "")
            {
                name = name.ToUpper().Substring(0, 1) + name.Substring(1);
                res = ctrString.Replace("<pName>", name);
            }
            return res;
        }

        private string GetContructorParam(string res, string name, string t)
        {
            if (name != "")
            {
                if (res != "")
                {
                    res += ", ";
                }
                res += t + " " + LowName(name);
            }
            return res;
        }

        private string GetEvalExpression(string res, string name)
        {
            if (name != null && name != "")
            {
                if (res != "")
                {
                    res += " &&";
                }
                res += "\n               EvalExpression(" + LowName(name) + "Exp, ref val" + name + @", """ + name + @""", ""Make" + ToolName + @""") ";
            }
            return res;
        }

        private string GetEvalExpressions()
        {
            string res = "";
            res = GetEvalExpression(res, P1Name);
            res = GetEvalExpression(res, P2Name);
            res = GetEvalExpression(res, P3Name);
            res = GetEvalExpression(res, P4Name);
            res = GetEvalExpression(res, P5Name);
            res = GetEvalExpression(res, P6Name);
            return res;
        }

        private string GetExeValueField(string n, string t)
        {
            string res = "";
            if (n != null && n != "")
            {
                res = "                " + t + " val" + n;
                if (t == "double" || t == "int")
                {
                    res += "= 0;";
                }
                if (t == "bool")
                {
                    res += "= false;";
                }
            }
            return res;
        }

        private string GetExeValueFields()
        {
            string res = "";
            res += GetExeValueField(p1Name, p1Type);
            res += GetExeValueField(p2Name, p2Type);
            res += GetExeValueField(p3Name, p3Type);
            res += GetExeValueField(p4Name, p4Type);
            res += GetExeValueField(p5Name, p5Type);
            res += GetExeValueField(p6Name, p6Type);
            return res;
        }

        private string GetField(string name, string t)
        {
            string res = "";
            if (name != "")
            {
                res += @"
    private " + t + " " + LowName(name) + " ;";
            }
            return res;
        }

        private string GetFieldCopy(string name)
        {
            string res = "";
            if (name != "")
            {
                res += "        this." + LowName(name) + " = " + LowName(name) + " ;\n";
            }
            return res;
        }

        private string GetLoadParams(String n, string t)
        {
            string res = "";
            if (n != "")
            {
                string r = "";
                if (t == "double")
                {
                    r = ldrd;
                }
                if (t == "int")
                {
                    r = ldri;
                }
                if (t == "bool")
                {
                    r = ldrb;
                }
                res += r.Replace("<pName>", n);
            }
            return res;
        }

        private string GetMakerNodeParam(string res, string name)
        {
            if (name != null && name != "")
            {
                if (res != "")
                {
                    res += ", ";
                }
                res += "val" + name;
            }
            return res;
        }

        private string GetMakerNodeParams()
        {
            string res = "";
            res = GetMakerNodeParam(res, P1Name);
            res = GetMakerNodeParam(res, P2Name);
            res = GetMakerNodeParam(res, P3Name);
            res = GetMakerNodeParam(res, P4Name);
            res = GetMakerNodeParam(res, P5Name);
            res = GetMakerNodeParam(res, P6Name);
            return res;
        }

        private string GetMakerParams()
        {
            String res = "";
            res = AddParam(res, P1Name);
            res = AddParam(res, P2Name);
            res = AddParam(res, P3Name);
            res = AddParam(res, P4Name);
            res = AddParam(res, P5Name);
            res = AddParam(res, P6Name);
            return res;
        }

        private string GetNodeContructorParam(string res, string name, string t)
        {
            if (name != null && name != "")
            {
                if (res != "")
                {
                    res += ", ";
                }
                res += "ExpressionNode " + LowName(name);
            }
            return res;
        }

        private string GetNodeCopyField(string p1Name)
        {
            string res = "";
            if (p1Name != null && p1Name != "")
            {
                res = "          this." + LowName(p1Name) + "Exp = " + LowName(p1Name) + " ;\n";
            }
            return res;
        }

        private string GetNodeField(string p1Name)
        {
            string res = "";
            if (p1Name != null && p1Name != "")
            {
                res = "        private ExpressionNode " + LowName(p1Name) + "Exp ;\n";
            }
            return res;
        }

        private string GetPlainTextParam(string res, string name)
        {
            if (name != null && name != "")
            {
                res += "\n        result += " + LowName(name) + @"Exp.ToString()+"", "";";
            }
            return res;
        }

        private string GetPlainTextParams()
        {
            string res = "";
            res = GetPlainTextParam(res, P1Name);
            res = GetPlainTextParam(res, P2Name);
            res = GetPlainTextParam(res, P3Name);
            res = GetPlainTextParam(res, P4Name);
            res = GetPlainTextParam(res, P5Name);
            res = GetPlainTextParam(res, P6Name);
            int index = res.LastIndexOf(@"+"", """);
            if (index > -1)
            {
                res = res.Substring(0, index);
                res += ";";
            }
            return res;
        }

        private string GetRangeCheck(string n, string l, string m)
        {
            string res = "";
            if (n != null && n != "")
            {
                if (l != null && l != "")
                {
                    if (m != null && m != "")
                    {
                        res = rchklh;
                        res = res.Replace("<pMin>", l);
                        res = res.Replace("<pMax>", m);
                    }
                    else
                    {
                        res = rchkl;
                        res = res.Replace("<pMin>", l);
                    }
                }
                else
                {
                    if (m != null && m != "")
                    {
                        res = rchkh;
                        res = res.Replace("<pMax>", m);
                    }
                }
            }
            res = res.Replace("<pName>", n);
            return res;
        }

        private string GetRangeChecks()
        {
            string res = "";
            res += GetRangeCheck(P1Name, p1Min, p1Max);
            res += GetRangeCheck(P2Name, p2Min, p2Max);
            res += GetRangeCheck(P3Name, p3Min, p3Max);
            res += GetRangeCheck(P4Name, p4Min, p4Max);
            res += GetRangeCheck(P5Name, p5Min, p5Max);
            res += GetRangeCheck(P6Name, p6Min, p6Max);
            return res;
        }

        private string GetRichTextParam(string res, string name)
        {
            if (name != null && name != "")
            {
                res += "\n        result += " + LowName(name) + @"Exp.ToRichText()+"", "";";
            }
            return res;
        }

        private string GetRichTextParams()
        {
            string res = "";
            res = GetRichTextParam(res, P1Name);
            res = GetRichTextParam(res, P2Name);
            res = GetRichTextParam(res, P3Name);
            res = GetRichTextParam(res, P4Name);
            res = GetRichTextParam(res, P5Name);
            res = GetRichTextParam(res, P6Name);
            int index = res.LastIndexOf(@"+"", """);
            if (index > -1)
            {
                res = res.Substring(0, index);
                res += ";";
            }
            return res;
        }

        private string GetSaveParams(String s)
        {
            string res;
            res = @"
EditorParameters.Set(""" + s + @"""," + s + ".ToString());";
            return res;
        }

        private string GetToolProperties()
        {
            string result = "";

            result += GenProp(p1Name, p1Type, p1Min, p1Max);

            result += GenProp(p2Name, p2Type, p2Min, p2Max);
            result += GenProp(p3Name, p3Type, p3Min, p3Max);
            result += GenProp(p4Name, p4Type, p4Min, p4Max);
            result += GenProp(p5Name, p5Type, p5Min, p5Max);
            result += GenProp(p6Name, p6Type, p6Min, p6Max);

            return result;
        }

        private string LowName(string name)
        {
            string res = "";
            if (name != null && name != "")
            {
                res = name.ToLower().Substring(0, 1) + name.Substring(1);
            }
            return res;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ToolName = "NewTool";
            parameterTypes = new List<string>();
            parameterTypes.Add("double");
            parameterTypes.Add("int");
            parameterTypes.Add("bool");
            NotifyPropertyChanged("ParameterTypes");
            P1Name = "";
            P1Max = "10";
            P1Min = "0";
            P1Type = "double";

            P2Name = "";
            P2Max = "10";
            P2Min = "0";
            P2Type = "double";

            P3Name = "";
            P3Max = "10";
            P3Min = "0";
            P3Type = "double";

            P4Name = "";
            P4Max = "10";
            P4Min = "0";
            P4Type = "double";

            P5Name = "";
            P5Max = "10";
            P5Min = "0";
            P5Type = "double";

            P6Name = "";
            P6Max = "10";
            P6Min = "0";
            P6Type = "double";
            ExportPath = Properties.Settings.Default.lastExportPath;
            if (exportPath.Trim() == "")
            {
                ExportPath = "C:\\tmp";
            }
        }
    }
}