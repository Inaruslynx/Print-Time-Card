using Print_Time_Card.Tools;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;

namespace Print_Time_Card
{
    public partial class frmPrintTimeCard : Form
    {
        Time TimeTools = new Time();
        UI UITools = new UI();

        public frmPrintTimeCard()
        {
            InitializeComponent();
        }

        public void textBox_WorkHours(object sender, EventArgs args)
        {
            // sender is a TextBox
            TextBox ctl = sender as TextBox;
            int number = 0;
            Control ctlIn, ctlOut;
            // Need the day
            if (ctl.Name.ToUpper().Contains("IN"))
            {
                number = int.Parse(ctl.Name.Remove(0, 5));
                ctlIn = Controls["txtIn" + number];
                if (ctlIn.Text == "")
                {
                    Control c = Controls["txtWorked" + number];
                    c.Text = "";
                    Control b = Controls["txtBonus" + number];
                    b.Text = "";
                    return;
                }
                UITools.ProcessTextboxChange((TextBox)ctlIn, TimeTools, cb24hr, grpBox);
            }
            else if (ctl.Name.ToUpper().Contains("OUT"))
            {
                number = int.Parse(ctl.Name.Remove(0, 6));
                ctlOut = Controls["txtOut" + number];
                if (ctlOut.Text == "")
                {
                    Control c = Controls["txtWorked" + number];
                    c.Text = "";
                    Control b = Controls["txtBonus" + number];
                    b.Text = "";
                    return;
                }
                UITools.ProcessTextboxChange((TextBox)ctlOut, TimeTools, cb24hr, grpBox);
            }
            // Need logic here to take into account for Night (19:30 - 8:00 is 11.5 not 12.5)
            TimeSpan difference = TimeTools.outTime[number - 1] - TimeTools.inTime[number - 1];
            Control ctlWork = Controls["txtWorked" + number];
            ctlWork.Text = difference.TotalHours.ToString();
        }

        private void textBox_TotalWorkedHours(object sender, EventArgs e)
        {
            // get textboxes and find txtWorked
            // get values in textboxes and add together
            // Put total in txtTotalW
            var txtWorked = new System.Collections.Generic.List<Control>();
            float totalHoursWorked = 0;
            foreach (Control control in Controls)
            {
                if (control != null && control is TextBox && control.Name.ToUpper().Contains("WORKED"))
                {
                    txtWorked.Add(control as TextBox);
                }
            }
            foreach (Control hour in txtWorked)
            {
                if (hour.Text != "")
                {
                    totalHoursWorked += float.Parse(hour.Text, CultureInfo.InvariantCulture.NumberFormat);
                }
            }
            txtTotalW.Text = totalHoursWorked.ToString();
        }

        private void textBox_TotalOtherHours(object sender, EventArgs e)
        {
            // Similar to TotalWorkedHours
            var txtOther = new System.Collections.Generic.List<Control>();
            double totalOtherHours = 0;
            foreach (Control control in Controls)
            {
                if (control != null && control is TextBox && control.Name.ToUpper().Contains("OTHER"))
                {
                    txtOther.Add(control as TextBox);
                }
            }
            foreach (Control other in txtOther)
            {
                if (other.Text != "")
                {
                    totalOtherHours += Convert.ToDouble(other.Text);
                }
            }
            txtTotalOth.Text = totalOtherHours.ToString();
        }

        private void textBox_BonusHours(object sender, EventArgs e)
        {
            // The sender is now the hours worked in a day. Not bonus hours
            // Change the below for hours worked first then bonus hours
            Control hoursWorked = (Control)sender;
            if (hoursWorked.Text == "")
            {
                return;
            }
            int number = int.Parse(hoursWorked.Name.Remove(0, 9));
            Control bonusHours = Controls["txtBonus" + number];
            if (hoursWorked.Text != "")
            {
                double valueOfHoursWorked = Convert.ToDouble(hoursWorked.Text);
                if (valueOfHoursWorked < 12.0)
                {
                    bonusHours.Text = valueOfHoursWorked.ToString();
                }
                else
                {
                    bonusHours.Text = "12.0";
                }
            }
        }

        private void textBox_TotalBonusHours(object sender, EventArgs e)
        {
            double totalBonusHours = 0;
            for (int i = 1; i < 8; i++)
            {
                Control ctl = Controls["txtBonus" + i];
                if (ctl.Text != "")
                {
                    totalBonusHours += Convert.ToDouble(ctl.Text);
                }
            }
            txtTotalB.Text = totalBonusHours.ToString();

        }

        private void textBox_totalOvertime(object sender, EventArgs e)
        {
            for (int i = 1; i < 8; i++)
            {
                Control ctl = Controls["txtOvertime" + i];
                ctl.Text = "";
            }
            if (!(txtTotalW.Text == ""))
            {
                double totalWorkedHours = Convert.ToDouble(txtTotalW.Text);
                if (totalWorkedHours > 40)
                {
                    double totalOvertime = totalWorkedHours - 40;
                    txtTotalO.Text = totalOvertime.ToString();
                }
                else
                {
                    txtTotalO.Text = "";
                }
            }
        }

        private void textBox_Overtime(object sender, EventArgs e)
        {
            if (txtTotalO.Text == "") return;
            double totalOvertime = Convert.ToDouble(txtTotalO.Text);
            double remainingOvertime = totalOvertime;
            for (int i = 7; i > 1; i--)
            {
                Control txtHoursWorked = Controls["txtWorked" + i];
                if (txtHoursWorked.Text == "") continue;
                double hoursWorked = Convert.ToDouble(txtHoursWorked.Text);
                if (hoursWorked < remainingOvertime)
                {
                    Controls["txtOvertime" + i].Text = hoursWorked.ToString();
                    remainingOvertime -= hoursWorked;
                }
                else
                {
                    Controls["txtOvertime" + i].Text = remainingOvertime.ToString();
                    return;
                }
            }
        }

        private void DrawAll(Graphics graphics, int copies = 1)
        {
            // I need to print on a width of 6" and height of 4.25"

            RectangleF srcRect = new Rectangle(0, 0, this.BackgroundImage.Width, this.BackgroundImage.Height);
            int nWidth = 600;
            int nHeight = 425;
            Font printFont = new Font(FontFamily.GenericSansSerif, 9);
            RectangleF destRect = new Rectangle(0, 0, nWidth, nHeight);
            //graphics.DrawImage(this.BackgroundImage, destRect, srcRect, GraphicsUnit.Pixel);
            float scalex = destRect.Width / srcRect.Width;
            float scaley = destRect.Height / (srcRect.Height - 24);
            // Pen aPen = new Pen(Brushes.Black, 1);
            for (int i = 0; i < this.Controls.Count; i++)
            {
                if (Controls[i].GetType() == this.Controls["txtIn1"].GetType() && !Controls[i].Name.ToUpper().Contains("CLOCK"))
                {
                    TextBox theText = (TextBox)Controls[i];
                    graphics.DrawString(theText.Text, printFont, Brushes.Black, (theText.Bounds.Left * scalex) - 15 + (int)numHor.Value, (theText.Bounds.Top * scaley) - 15 + (int)numVert.Value, new StringFormat());
                }
            }
        }

        // This checks what slection the user makes when they change a combobox
        // 0 - SDO
        // 1 - Days
        // 2 - Nights
        // 3 - Holiday
        // 4 - Vacation
        // 5 - Bereavement
        // 6 - Jury
        private void cmbBox_Change(object sender, EventArgs e)
        {
            ComboBox ctl = (ComboBox)sender;
            int ctlNumber = int.Parse(ctl.Name.Remove(0, 5));
            int userChoice = ctl.SelectedIndex;
            int[] clockTimes = ProcessClockTimes();
            TextBox In = (TextBox)Controls["txtIn" + ctlNumber];
            TextBox Out = (TextBox)Controls["txtOut" + ctlNumber];
            TextBox Worked = (TextBox)Controls["txtWorked" + ctlNumber];
            TextBox Bonus = (TextBox)Controls["txtBonus" + ctlNumber];
            TextBox Other = (TextBox)Controls["txtOther" + ctlNumber];
            TextBox Code = (TextBox)Controls["txtCode" + ctlNumber];
            switch (userChoice)
            {
                case 0:
                    // SDO
                    In.Text = "SDO";
                    Out.Text = "";
                    Worked.Text = "";
                    Bonus.Text = "";
                    Other.Text = "";
                    Code.Text = "";
                    break;
                case 1:
                    // Day
                    if (clockTimes[0] == 0 && clockTimes[1] == 0 && clockTimes[2] == 0 && clockTimes[3] == 0)
                    {
                        MessageBox.Show("Please enter valid clock in and out times. If there is an error notify Joshua Edwards.");
                    }
                    else
                    {
                        if (cb24hr.Checked)
                        {
                            TimeTools.enterTime(Controls, ctlNumber, "H:mm", clockTimes[0], clockTimes[1], clockTimes[2], clockTimes[3], false);
                        }
                        else
                        {
                            TimeTools.enterTime(Controls, ctlNumber, "h:mm tt", clockTimes[0], clockTimes[1], clockTimes[2], clockTimes[3], false);
                        }
                    }
                    break;
                case 2:
                    // Night
                    if (clockTimes[0] == 0 && clockTimes[1] == 0 && clockTimes[2] == 0 && clockTimes[3] == 0)
                    {
                        MessageBox.Show("Please enter valid clock in and out times. If there is an error notify Joshua Edwards.");
                    }
                    else
                    {
                        if (cb24hr.Checked)
                        {
                            TimeTools.enterTime(Controls, ctlNumber, "H:mm", clockTimes[0] + 12, clockTimes[1], clockTimes[2] - 12, clockTimes[3], true);
                        }
                        else
                        {
                            TimeTools.enterTime(Controls, ctlNumber, "h:mm tt", clockTimes[0] + 12, clockTimes[1], clockTimes[2] - 12, clockTimes[3], true);
                        }
                    }
                    break;
                case 3:
                    // Holiday
                    In.Text = "Holiday";
                    Out.Text = "";
                    Worked.Text = "";
                    Bonus.Text = "";
                    Other.Text = "8";
                    Code.Text = "H";
                    break;
                case 4:
                    // Vacation
                    In.Text = "Vacation";
                    Out.Text = "";
                    Worked.Text = "";
                    Bonus.Text = "";
                    Other.Text = "12";
                    Code.Text = "V";
                    break;
                case 5:
                    // Bereavement
                    In.Text = "BRVMNT";
                    Out.Text = "";
                    Worked.Text = "";
                    Bonus.Text = "";
                    Other.Text = "8";
                    Code.Text = "B";
                    break;
                case 6:
                    // Jury
                    In.Text = "Jury";
                    Out.Text = "";
                    Worked.Text = "";
                    Bonus.Text = "";
                    Other.Text = "8";
                    Code.Text = "J";
                    break;
                default:
                    MessageBox.Show("Error. Let Joshua Edwards know what happened.");
                    break;
            }
        }

        private int[] ProcessClockTimes()
        {
            string clockIn = txtClockIn.Text.ToLower();
            string clockOut = txtClockOut.Text.ToLower();
            TimeSpan timeIn, timeOut;
            int[] returnZeros = new int[] { 0, 0, 0, 0 };
            bool milTime = cb24hr.Checked;
            if (milTime)
            {
                if (!TimeSpan.TryParse(clockIn, out timeIn)) return returnZeros;
                if (!TimeSpan.TryParse(clockOut, out timeOut)) return returnZeros;
            }
            else
            {
                if (!TimeSpan.TryParse(clockIn.Remove(clockIn.Length - 2), out timeIn)) return returnZeros;
                if (!TimeSpan.TryParse(clockOut.Remove(clockOut.Length - 2), out timeOut)) return returnZeros;
                timeOut += new TimeSpan(12, 0, 0);
            }
            int[] returnValues = new int[] { timeIn.Hours, timeIn.Minutes, timeOut.Hours, timeOut.Minutes };
            return returnValues;
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 7; i++)
            {
                TimeTools.inTime[i] = TimeTools.Sunday;
                TimeTools.outTime[i] = TimeTools.Sunday;
            }
            for (int i = 1; i < 8; i++)
            {
                //CheckBox day = (CheckBox)Controls["cbDay" + i];
                //CheckBox night = (CheckBox)Controls["cbNight" + i];
                ComboBox Box = (ComboBox)grpBox.Controls["cbBox" + i];
                TextBox In = (TextBox)Controls["txtIn" + i];
                TextBox Out = (TextBox)Controls["txtOut" + i];
                TextBox worked = (TextBox)Controls["txtWorked" + i];
                TextBox bonus = (TextBox)Controls["txtBonus" + i];
                TextBox overtime = (TextBox)Controls["txtOvertime" + i];
                TextBox other = (TextBox)Controls["txtOther" + i];
                TextBox last = (TextBox)Controls["txtCode" + i];
                //day.Checked = false;
                //night.Checked = false;
                Box.SelectedIndex = 0;
                In.Text = "SDO";
                Out.Text = "";
                worked.Text = "";
                bonus.Text = "";
                overtime.Text = "";
                other.Text = "";
                last.Text = "";
            }
            txtName.Text = Properties.Settings.Default.empName;
            txtNumber.Text = Properties.Settings.Default.empNum;
            txtCrew.Text = Properties.Settings.Default.empCrew;
            txtDept.Text = Properties.Settings.Default.empDept;
            numHor.Value = Properties.Settings.Default.horOffset;
            numVert.Value = Properties.Settings.Default.verOffset;
            cb24hr.Checked = Properties.Settings.Default.milTime;
            txtClockIn.Text = Properties.Settings.Default.clockIn;
            txtClockOut.Text = Properties.Settings.Default.clockOut;
            txtTotalO.Text = "";
            txtTotalW.Text = "";
            txtTotalB.Text = "";
            txtTotalOth.Text = "";
            cb24hr.Checked = false;
        }

        private void printMenu_Click(object sender, EventArgs e)
        {
            short numberOfCopies = (short)numCopies.Value;
            printPreviewDialog1.Document = this.printDocument1;
            printDocument1.PrinterSettings.Copies = numberOfCopies;
            printDocument1.DefaultPageSettings.PaperSize = new PaperSize("Time Card", 400, 600);
            printDocument1.DefaultPageSettings.Landscape = true;
            printPreviewDialog1.ShowDialog();
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            DrawAll(e.Graphics);
        }

        private void aboutMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This app was created by Joshua Edwards.");
        }

        private void printSettingsMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please verify the following settings for printer:\nPaper Size: 4.25 x 6.0 in\nOrientation: Landscape\n\nPlease make sure time cards are loaded appropriately");
        }

        private void exitMenu_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void frmPrintTimeCard_Load(object sender, EventArgs e)
        {
            TimeTools.Initialize();
            txtName.Text = Properties.Settings.Default.empName;
            txtCrew.Text = Properties.Settings.Default.empCrew;
            txtNumber.Text = Properties.Settings.Default.empNum;
            txtDept.Text = Properties.Settings.Default.empDept;
            numHor.Value = Properties.Settings.Default.horOffset;
            numVert.Value = Properties.Settings.Default.verOffset;
            cb24hr.Checked = Properties.Settings.Default.milTime;
            txtClockIn.Text = Properties.Settings.Default.clockIn;
            txtClockOut.Text = Properties.Settings.Default.clockOut;
            txtFrom.Text = TimeTools.Sunday.ToShortDateString();
            txtTo.Text = TimeTools.Saturday.ToShortDateString();
            var textBoxes = new System.Collections.Generic.List<Control>();
            //var checkBoxes = new System.Collections.Generic.List<Control>();
            var comboBoxes = new System.Collections.Generic.List<Control>();
            foreach (Control control in Controls)
            {
                if (control != null && control is TextBox)
                {
                    textBoxes.Add(control as TextBox);
                } // Old code
                /*else if (control != null && control is CheckBox)
                {
                    checkBoxes.Add(control as CheckBox);
                }*/
            }
            foreach (Control control in grpBox.Controls)
            {
                comboBoxes.Add(control as ComboBox);
            }
            foreach (TextBox ctl in textBoxes)
            {

                if ((ctl).Name.ToUpper().Contains("TXTIN") || (ctl).Name.ToUpper().Contains("TXTOUT"))
                {
                    EventHandler eventHandler = new EventHandler(textBox_WorkHours);
                    ctl.TextChanged += eventHandler;
                    ctl.TabStop = false;
                }

                if ((ctl).Name.ToUpper().Contains("TXTWORKED"))
                {
                    EventHandler workedEventHandler = new EventHandler(textBox_TotalWorkedHours);
                    ctl.TextChanged += workedEventHandler;
                    EventHandler bonusEventHandler = new EventHandler(textBox_BonusHours);
                    ctl.TextChanged += bonusEventHandler;
                    ctl.TabStop = false;
                }

                if ((ctl).Name.ToUpper().Contains("TXTBONUS"))
                {
                    EventHandler eventHandler = new EventHandler(textBox_TotalBonusHours);
                    ctl.TextChanged += eventHandler;
                    ctl.TabStop = false;
                }

                if ((ctl).Name.ToUpper().Contains("TXTOTHER"))
                {
                    EventHandler eventHandler = new EventHandler(textBox_TotalOtherHours);
                    ctl.TextChanged += eventHandler;
                }

            }
            foreach (ComboBox ctl in comboBoxes)
            {
                if (ctl != null)
                {
                    EventHandler eventHandler = new EventHandler(cmbBox_Change);
                    ctl.SelectedIndexChanged += eventHandler;
                    ctl.SelectedIndex = 0;
                }
            }
        }

        private void btnPrevWeek_Click(object sender, EventArgs e)
        {
            TimeTools.changeWeek(-7);
            txtFrom.Text = TimeTools.Sunday.ToShortDateString();
            txtTo.Text = TimeTools.Saturday.ToShortDateString();
        }

        private void btnNextWeek_Click(object sender, EventArgs e)
        {
            TimeTools.changeWeek(7);
            txtFrom.Text = TimeTools.Sunday.ToShortDateString();
            txtTo.Text = TimeTools.Saturday.ToShortDateString();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.empName = txtName.Text;
            Properties.Settings.Default.empCrew = txtCrew.Text;
            Properties.Settings.Default.empNum = txtNumber.Text;
            Properties.Settings.Default.empDept = txtDept.Text;
            Properties.Settings.Default.horOffset = (int)numHor.Value;
            Properties.Settings.Default.verOffset = (int)numVert.Value;
            Properties.Settings.Default.milTime = cb24hr.Checked;
            Properties.Settings.Default.clockIn = txtClockIn.Text;
            Properties.Settings.Default.clockOut = txtClockOut.Text;
            Properties.Settings.Default.Save();
        }
    }
}

