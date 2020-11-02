using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for NUDBox.xaml
    /// </summary>
    public partial class NUDBox : UserControl
    {
        public delegate void ValueChanged(double i);

        public ValueChanged OnValueChanged;

        private double minvalue = 0,
    maxvalue = 100,
    startvalue = 10;

        public double Value
        {
            get; set;
        }

        public double MinimumValue
        {
            get { return minvalue; }
            set { minvalue = value; }
        }

        public double MaximumValue
        {
            get { return maxvalue; }
            set { maxvalue = value; }
        }

        public double StartValue
        {
            get { return startvalue; }
            set { startvalue = value; Value = value; NUDTextBox.Text = startvalue.ToString(); }
        }

        public NUDBox()
        {
            InitializeComponent();
            NUDTextBox.Text = startvalue.ToString();
            OnValueChanged = null;
        }

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
            else number = 0;
            if (number < maxvalue)
                NUDTextBox.Text = Convert.ToString(number + 1);
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
            else number = 0;
            if (number > minvalue)
                NUDTextBox.Text = Convert.ToString(number - 1);
        }

        private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                NUDButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { true });
            }

            if (e.Key == Key.Down)
            {
                NUDButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { true });
            }
        }

        private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { false });

            if (e.Key == Key.Down)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { false });
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox.Text != "")
                if (!int.TryParse(NUDTextBox.Text, out number)) NUDTextBox.Text = startvalue.ToString();
            if (number > maxvalue) NUDTextBox.Text = maxvalue.ToString();
            if (number < minvalue) NUDTextBox.Text = minvalue.ToString();
            NUDTextBox.SelectionStart = NUDTextBox.Text.Length;
            int.TryParse(NUDTextBox.Text, out number);
            Value = number;
            if (OnValueChanged != null)
            {
                OnValueChanged(Value);
            }
        }
    }
}