using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace AdvancedCalculatorGUI
{
    // Dodanie wpisywania niewiadomej X, tworzenia wykresu na określonym przedziale
    // Całkowanie narysowanego przedziału

    public partial class CalculatorGUI : Form
    {
        int clock_width = 100, clock_height = 100, center_x, center_y;

        public CalculatorGUI()
        {
            InitializeComponent();
            styleComboBoxMenuStrip.SelectedItem = "Default Style";
            clockComboBoxMenuStrip.SelectedItem = "Digital Clock";
            center_x = clock_width / 2;
            center_y = clock_height / 2;
        }

        private void ProcessActionButton(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string text_update = button.Text;

            if (int.TryParse(text_update, out _) || text_update == ".")
                UpdateDisplay(text_update);
            else if (mainTextBox.Text.Length > 0 && mainTextBox.Text.Substring(mainTextBox.Text.Length - 1, 1) == " ")
                UpdateDisplay(text_update + " ");
            else
                UpdateDisplay(" " + text_update + " ");
        }

        private void UpdateDisplay(string update, Boolean replace = false)
        {
            if (mainTextBox.Text == "0" || replace)        
                mainTextBox.Text = update;
            
            else
                mainTextBox.Text += update;
        }

        private void ProcessBracketButton(object sender, EventArgs e)
        {
            string bracket = "(";
            if (mainTextBox.Text.Length > 1)
            {
                int left_brackets = mainTextBox.Text.Count(x => x == '(');
                int right_brackets = mainTextBox.Text.Count(x => x == ')');
                string previous_char = mainTextBox.Text.Substring(mainTextBox.Text.Length - 1, 1);
                string previous_char2 = mainTextBox.Text.Substring(mainTextBox.Text.Length - 2, 1);
                if (left_brackets > right_brackets && (int.TryParse(previous_char, out _) || previous_char2.Equals(")")))
                    bracket = ")";
            }

            if (mainTextBox.Text.Length == 0 || mainTextBox.Text.Length > 0 && mainTextBox.Text.Substring(mainTextBox.Text.Length - 1, 1) == " ")
                UpdateDisplay(bracket + " ");
            else
                UpdateDisplay(" " + bracket + " ");
        }

        private void ProcessBackButton(object sender, EventArgs e)
        {
            if (mainTextBox.Text.Length > 1)
            {
                string char_to_delete = mainTextBox.Text.Substring(mainTextBox.Text.Length - 1, 1);
                string prev_char = mainTextBox.Text.Substring(mainTextBox.Text.Length - 2, 1);
                if (Char.IsLetter(prev_char, 0))
                    mainTextBox.Text = mainTextBox.Text.Substring(0, mainTextBox.Text.Length - 4);
                else if (char_to_delete == " ")
                    mainTextBox.Text = mainTextBox.Text.Substring(0, mainTextBox.Text.Length - 3);
                else
                    mainTextBox.Text = mainTextBox.Text.Substring(0, mainTextBox.Text.Length - 1);

            }
            else
                mainTextBox.Text = ""; 
        }

        private void ProcessClearButton(object sender, EventArgs e)
        {
            mainTextBox.Text = "";
        }

        private void ProcessCalcButton(object sender, EventArgs e)
        {
            String input = mainTextBox.Text;
            
            try
            {
                input = this.ProcessSpecialSigns(input);
                var operation = new DataTable().Compute(input, null);
                this.UpdateDisplay(Convert.ToString(operation), true);
            }
            catch (Exception error)
            {
                MessageBox.Show("Syntax Error !");
            }
        }

        private string ProcessSpecialSigns(string equation)
        {
            if (equation.Contains("(") && equation.Contains(")"))
                equation = this.CalculateBrackets(equation);
            if (equation.Contains("^"))
                equation = this.CalculateAdvancedMath(equation, "^");
            if (equation.Contains("√"))
                equation = this.CalculateAdvancedMath(equation, "√");
            if (equation.Contains("log"))
                equation = this.CalculateAdvancedMath(equation, "log");
            if (equation.Contains("sin"))
                equation = this.CalculateAdvancedMath(equation, "sin");
            return equation;
        }

        private string CalculateBrackets(string equation)
        {
            int bracket_left_index = equation.IndexOf("("), bracket_right_index = equation.IndexOf(")");
            int left_brackets_counter, right_brackets_counter;
            string bracketed_equation;

            while (bracket_left_index != -1)
            {
                left_brackets_counter = 0;
                right_brackets_counter = 0;
                for (int i = bracket_left_index; i < equation.Length; i++)
                {
                    if (mainTextBox.Text.Substring(i, 1) == "(")
                        left_brackets_counter++;
                    if (mainTextBox.Text.Substring(i, 1) == ")")
                        right_brackets_counter++;
                    if (left_brackets_counter == right_brackets_counter && mainTextBox.Text.Substring(i, 1) == ")")
                        bracket_right_index = i;
                }

                bracketed_equation = equation.Substring(bracket_left_index + 2, bracket_right_index - bracket_left_index - 2);
                bracketed_equation = this.ProcessSpecialSigns(bracketed_equation);
                var value = new DataTable().Compute(bracketed_equation, null);
                equation = equation.Remove(bracket_left_index, bracket_right_index - bracket_left_index + 1);
                equation = equation.Insert(bracket_left_index, Convert.ToString(value));
                bracket_left_index = equation.IndexOf("(");
            }
                return equation;
        }

        private string CalculateAdvancedMath(string equation, string operation)
        {
            int operation_index = equation.IndexOf(operation);
            double basis, operand, value = 0;
         
            while (operation_index != -1)
            {
                basis = this.GetBasisValue(equation, operation_index - 2);
                operand = this.GetOperandValue(equation, operation_index + operation.Length + 1);
                
                if (operation == "^")
                    value = Math.Round(Math.Pow(basis, operand), 5);
                else if (operation == "√")
                    value = Math.Round(Math.Sqrt(operand), 5);
                else if (operation == "log" && basis == 0)
                    value = Math.Round(Math.Log(operand), 5);
                else if (operation == "log" && basis != 0)
                    value = Math.Round(Math.Log(operand, basis), 5);
                else if (operation == "sin")
                    value = Math.Round(Math.Sin(operand), 5);

                if (basis == 0)
                {
                    equation = equation.Remove(operation_index, operation.Length + operand.ToString().Length + 1);
                    equation = equation.Insert(operation_index, value.ToString() + " ");
                }
                else
                {
                    equation = equation.Remove(operation_index - 1 - basis.ToString().Length,
                                               basis.ToString().Length + operation.Length + operand.ToString().Length + 2);
                    equation = equation.Insert(operation_index - 1 - basis.ToString().Length,
                                               value.ToString() + " ");
                }
                operation_index = equation.IndexOf(operation);
            }
            return equation.Replace(@",", ".");
        }

        private double GetBasisValue(string operation, int string_index)
        {
            int i = 0;
            string basis;
            if (string_index < 0)
                return 0;
            else if (!double.TryParse(operation.Substring(string_index - i, 1), out _))
                return 0;
            while (true)
            {
                if (string_index - i == 0 || operation.Substring(string_index - i, 1) == " ")
                {
                    basis = operation.Substring(string_index - i, i + 1);
                    return Double.Parse(basis.Replace(@".", ","));
                }
                else
                    i++;
            }
        }

        private double GetOperandValue(string operation, int string_index)
        {
            int i = 0;
            string operand;
            while (true)
            {
                if (string_index + i == operation.Length - 1 || operation.Substring(string_index + i, 1) == " ")
                {
                    operand = operation.Substring(string_index, i + 1);
                    return Double.Parse(operand.Replace(@".", ","));
                }
                else
                    i++;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Function which detects clicked button and process its function
            switch(keyData)
            {
                case Keys.NumPad0:
                    this.button0.PerformClick();
                    break;
                case Keys.NumPad1:
                    this.button1.PerformClick();
                    break;
                case Keys.NumPad2:
                    this.button2.PerformClick();
                    break;
                case Keys.NumPad3:
                    this.button3.PerformClick();
                    break;
                case Keys.NumPad4:
                    this.button4.PerformClick();
                    break;
                case Keys.NumPad5:
                    this.button5.PerformClick();
                    break;
                case Keys.NumPad6:
                    this.button6.PerformClick();
                    break;
                case Keys.NumPad7:
                    this.button7.PerformClick();
                    break;
                case Keys.NumPad8:
                    this.button8.PerformClick();
                    break;
                case Keys.NumPad9:
                    this.button9.PerformClick();
                    break;
                case Keys.Decimal:
                    this.buttonDot.PerformClick();
                    break;
                case Keys.Multiply:
                    this.buttonMult.PerformClick();
                    break;
                case Keys.Divide:
                    this.buttonDiv.PerformClick();
                    break;
                case Keys.Add:
                    this.buttonAdd.PerformClick();
                    break;
                case Keys.Subtract:
                    this.buttonSub.PerformClick();
                    break;
                case Keys.D5 | Keys.Shift:
                    this.buttonMod.PerformClick();
                    break;
                case Keys.Back:
                    this.buttonBack.PerformClick();
                    break;
                case Keys.Delete:
                    this.buttonClear.PerformClick();
                    break;
                case Keys.Enter:
                    this.buttonCalc.PerformClick();
                    break;
                default:
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ClockStyleUpdate(object sender, EventArgs e)
        {
            if ((string)styleComboBoxMenuStrip.SelectedItem == "Default Style")
            {
                AdvancedCalculatorGUI.Properties.Settings.Default.Style = "Default";
                this.SetStyle(SystemColors.Info, SystemColors.WindowText);
            }
            if ((string)styleComboBoxMenuStrip.SelectedItem == "Dark Style")
            {
                AdvancedCalculatorGUI.Properties.Settings.Default.Style = "Dark";
                this.SetStyle(System.Drawing.Color.FromArgb((int)((byte)(64)), (int)((byte)(64)), (int)(byte)(64)), SystemColors.MenuBar);
            }
            if ((string)clockComboBoxMenuStrip.SelectedItem == "Digital Clock")
                this.SetDigitalClock();
            if ((string)clockComboBoxMenuStrip.SelectedItem == "Analog Clock")
                this.SetAnalogClock();
        }

        private void SetStyle(Color back_color, Color fore_color)
        {
            this.BackColor = back_color;
            this.pictureBoxClock.BackColor = back_color;
            this.menuStrip.BackColor = back_color;
            this.styleComboBoxMenuStrip.BackColor = back_color;
            this.clockComboBoxMenuStrip.BackColor = back_color;
            this.styleComboBoxMenuStrip.ForeColor = fore_color;
            this.clockComboBoxMenuStrip.ForeColor = fore_color;
            this.labelTimer.ForeColor = fore_color;
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                item.BackColor = back_color;
                item.ForeColor = fore_color;
                foreach (ToolStripMenuItem children in item.DropDownItems)
                {
                    children.BackColor = back_color;
                    children.ForeColor = fore_color;
                }
            }
            foreach (Control c in this.Controls)
            {
                UpdateColorControls(c);
            }
        }

        private void UpdateColorControls(Control ui_element)
        {
            if (ui_element is Button || ui_element is TextBox)
            {
                if (AdvancedCalculatorGUI.Properties.Settings.Default.Style == "Default")
                {
                    ui_element.BackColor = System.Drawing.Color.Wheat;
                    ui_element.ForeColor = SystemColors.ControlText;
                }
                else if (AdvancedCalculatorGUI.Properties.Settings.Default.Style == "Dark")
                {
                    ui_element.BackColor = SystemColors.GrayText;
                    ui_element.ForeColor = SystemColors.MenuBar;
                }
            }

            foreach (Control subC in ui_element.Controls)
            {
                UpdateColorControls(subC);
            }
        }

        private void SetDigitalClock()
        {
            int hour = DateTime.Now.Hour, minute = DateTime.Now.Minute, second = DateTime.Now.Second;
            string hour_string = "", minute_string = "", second_string = "";
            labelTimer.Visible = true;
            pictureBoxClock.Visible = false;
            if (hour < 10)
                hour_string += "0" + hour;
            else
                hour_string += hour;
            if (minute < 10)
                minute_string += "0" + minute;
            else
                minute_string += minute;
            if (second < 10)
                second_string += "0" + second;
            else
                second_string += second;
            labelTimer.Text = hour_string + ":" + minute_string + ":" + second_string;
        }

        private void SetAnalogClock()
        {
            int hour = DateTime.Now.Hour, minute = DateTime.Now.Minute, second = DateTime.Now.Second, second_hand_len = 45, minute_hand_len = 35, hour_hand_len = 25;
            int[] secCoord = new int[2], minCoord = new int[2], hourCoord = new int[2];
            Bitmap clock_bitmap = new Bitmap(clock_width + 1, clock_height + 1);
            Graphics clock_graphics = Graphics.FromImage(clock_bitmap);
            Brush clock_brush = Brushes.Black;
            Color clock_color = Color.Black;
            if (AdvancedCalculatorGUI.Properties.Settings.Default.Style == "Dark")
            {
                clock_brush = Brushes.White;
                clock_color = Color.White;
            }
            labelTimer.Visible = false;
            pictureBoxClock.Visible = true;

            secCoord = GetHandOfClockCoord(GetMinuteSecondRadianTimeValue(second), second_hand_len);
            minCoord = GetHandOfClockCoord(GetMinuteSecondRadianTimeValue(minute), minute_hand_len);
            hourCoord = GetHandOfClockCoord(GetHourRadianTimeValue(hour, minute), hour_hand_len);


            clock_graphics.Clear(this.BackColor);
            clock_graphics.DrawEllipse(new Pen(clock_color, 2f), 0, 0, clock_width, clock_height);
            clock_graphics.DrawString("12", new Font("Arial", 12, FontStyle.Bold), clock_brush, new PointF(39, 2));
            clock_graphics.DrawString("3", new Font("Arial", 12, FontStyle.Bold), clock_brush, new PointF(86, 39));
            clock_graphics.DrawString("6", new Font("Arial", 12, FontStyle.Bold), clock_brush, new PointF(43, 80));
            clock_graphics.DrawString("9", new Font("Arial", 12, FontStyle.Bold), clock_brush, new PointF(0, 39));
            clock_graphics.DrawLine(new Pen(Color.Red, 2f), new Point(center_x, center_y), new Point(secCoord[0], secCoord[1]));
            clock_graphics.DrawLine(new Pen(Color.Gray, 3f), new Point(center_x, center_y), new Point(minCoord[0], minCoord[1]));
            clock_graphics.DrawLine(new Pen(Color.Black, 4f), new Point(center_x, center_y), new Point(hourCoord[0], hourCoord[1]));
            pictureBoxClock.Image = clock_bitmap;
            clock_graphics.Dispose();
        }

        private int GetMinuteSecondRadianTimeValue (int time_value)
        {
            return time_value * 6;
        }

        private int GetHourRadianTimeValue(int hour, int minute)
        {
            return Convert.ToInt32((hour * 30 + minute / 2));
        }

        private int[] GetHandOfClockCoord(int radian_time_value, int time_hand_len)
        {
            int[] coord = new int[2];
            if (radian_time_value >= 0 && radian_time_value <= 180)
            {
                coord[0] = center_x + Convert.ToInt32((time_hand_len * Math.Sin(Math.PI * radian_time_value / 180)));
                coord[1] = center_y - Convert.ToInt32((time_hand_len * Math.Cos(Math.PI * radian_time_value / 180)));
            }
            else
            {
                coord[0] = center_x - Convert.ToInt32((time_hand_len * -Math.Sin(Math.PI * radian_time_value / 180)));
                coord[1] = center_y - Convert.ToInt32((time_hand_len * Math.Cos(Math.PI * radian_time_value / 180)));
            }
            return coord;
        }
    }
}
