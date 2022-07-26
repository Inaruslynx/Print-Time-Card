﻿using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Print_Time_Card
{
    public partial class frmPrintTimeCard : Form
    {
        DateTime[] inTime = new DateTime[7];
        DateTime[] outTime = new DateTime[7];
        DateTime currentDay = DateTime.Today;
        DateTime Sunday;
        int howManyDaysSinceSunday = 0;
        int daysUntilSaturday = 0;

        public frmPrintTimeCard()
        {
            InitializeComponent();
        }

        public int adjustDay(DayOfWeek day, DateTime time, bool past)
        {
            int numberOfDays = 0;
            if (past)
            {
                while (day != time.AddDays(-numberOfDays).DayOfWeek)
                {
                    numberOfDays++;
                }
            }
            else
            {
                while (day != time.AddDays(numberOfDays).DayOfWeek)
                {
                    numberOfDays++;
                }
            }
            return numberOfDays;
        }

        public DateTime adjustTime(DateTime time, int hour, int minute)
        {
            while (time.Hour != hour)
            {
                if (time.Hour > hour)
                {
                    time = time.AddHours(-1);
                }

                if (time.Hour < hour)
                {
                    time = time.AddHours(1);
                }
            }
            while (time.Minute != minute)
            {
                if (time.Minute > minute)
                {
                    time = time.AddMinutes(-1);
                }

                if (time.Minute < minute)
                {
                    time = time.AddMinutes(1);
                }
            }
            return time;
        }

        public void ProcessTextboxChange(TextBox obj)
        {
            // Get a Timespan then if a day simply adjust inTime and outTime
            // if night then make sure outTime is the next day

            // first we need to find which day it is
            int number;
            bool In;
            if (obj.Name.ToUpper().Contains("IN"))
            {
                number = int.Parse(obj.Name.Remove(0, 5));
                In = true;
            }
            else
            {
                number = int.Parse(obj.Name.Remove(0, 6));
                In = false;
            }
            // now we get a TimeSpan and change in and out time
            if (cb24hr.Checked == false)
            {
                // expected text for time: hh:mm pm/am but it could be h:mm pm/am
                if (obj.Text == "SDO")
                {
                    return;
                }
                TimeSpan convertedTime = TimeSpan.Parse(obj.Text.Remove(5).Trim());
                if (obj.Text.Remove(0, 5).Trim().ToUpper() == "PM")
                {
                    convertedTime += new TimeSpan(12, 0, 0);
                }
                // I need to first determine if difference will be earlier or later than previous time
                // then I need to add or substract the difference in time to inTime or outTime
                // to determine if earlier or later I need to compare convertedTime to inTime/outTime (depending if obj)
                if (In)
                {
                    adjInTime(number, convertedTime);
                }
                else
                {
                    adjOutTime(number, convertedTime);
                }
            }
            else
            {
                if (obj.Text == "SDO")
                {
                    return;
                }
                TimeSpan convertedTime = TimeSpan.Parse(obj.Text);
                if (In)
                {
                    adjInTime(number, convertedTime);
                }
                else
                {
                    adjOutTime(number, convertedTime);
                }
            }
        }

        private void adjInTime(int number, TimeSpan convertedTime)
        {
            // Get number for checkbox passed in and time changed
            TimeSpan timeInDay = inTime[number - 1] - Sunday.AddDays(number - 1);
            if (convertedTime > timeInDay)
            {
                inTime[number - 1] += convertedTime - timeInDay;
            }
            else if (convertedTime < timeInDay)
            {
                inTime[number - 1] -= convertedTime - timeInDay;
            }
        }

        private void adjOutTime(int number, TimeSpan convertedTime)
        {
            // Get number for checkbox passed in and time changed
            CheckBox night = (CheckBox)Controls["cbNight" + number];
            TimeSpan timeOutDay;
            if (night.Checked)
            {
                timeOutDay = outTime[number - 1] - Sunday.AddDays(number);
            }
            else
            {
                timeOutDay = outTime[number - 1] - Sunday.AddDays(number - 1);
            }
            if (convertedTime > timeOutDay)
            {
                outTime[number - 1] += convertedTime - timeOutDay;
            }
            else if (convertedTime < timeOutDay)
            {
                outTime[number - 1] -= convertedTime - timeOutDay;
            }
        }

        public void textBox_WorkHours(object sender, EventArgs args)
        {
            // sender is a TextBox
            TextBox ctl = sender as TextBox;
            int number = 0;
            //TimeSpan convertedTimeIn, convertedTimeOut, originalTimeIn, originalTimeOut;
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
                ProcessTextboxChange((TextBox)ctlIn);
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
                ProcessTextboxChange((TextBox)ctlOut);
            }
            // Need logic here to take into account for Night (19:30 - 8:00 is 11.5 not 12.5)
            TimeSpan difference = outTime[number - 1] - inTime[number - 1]; ;
            //CheckBox cbNight = (CheckBox)Controls["cbNight" + number];
            //if (cbNight.Checked)
            //{
            //    // Already changed inTime and outTime
            //    DateTime midnight = adjustTime(inTime[number - 1], 0, 0);
            //    TimeSpan firstDifference = midnight - outTime[number - 1];
            //    TimeSpan secondDifference = inTime[number - 1] - midnight;
            //    difference.Add(firstDifference);
            //    difference.Add(secondDifference);
            //}
            //else
            //{
            //    difference = outTime[number - 1] - inTime[number - 1];
            //}
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
            if (txtTotalW.Text == "" || Convert.ToDouble(txtTotalW.Text) < 40)
            {
                return;
            }
            else
            {
                double totalOvertime = 0;
                double totalWorkedHours = Convert.ToDouble(txtTotalW.Text);
                if (totalWorkedHours > 40)
                {
                    totalOvertime = totalWorkedHours - 40;
                }
                txtTotalO.Text = totalOvertime.ToString();
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

        private void printMenu_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.Document = this.printDocument1;
            printPreviewDialog1.ShowDialog();
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            DrawAll(e.Graphics);
        }

        private void DrawAll(Graphics graphics)
        {
            // I need to print on a width of 6.25" and height of 4"
            float Hard_Margin_X = printDocument1.DefaultPageSettings.HardMarginX;
            float Hard_Margin_Y = printDocument1.DefaultPageSettings.HardMarginY;
            float Page_Height = printDocument1.DefaultPageSettings.Bounds.Height;
            float Page_Width = printDocument1.DefaultPageSettings.Bounds.Width;
            float Top_Margin = printDocument1.DefaultPageSettings.Margins.Top;
            float Left_Margin = printDocument1.DefaultPageSettings.Margins.Left;
            float Right_Margin = printDocument1.DefaultPageSettings.Margins.Right;
            float Bottom_Margin = printDocument1.DefaultPageSettings.Margins.Bottom;

            if (Top_Margin < Hard_Margin_Y)
            {
                Top_Margin = Hard_Margin_Y;
            }
            if (Left_Margin < Hard_Margin_X)
            {
                Left_Margin = Hard_Margin_X;
            }
            if (Right_Margin < Hard_Margin_X)
            {
                Right_Margin = Hard_Margin_X;
            }
            if (Bottom_Margin < Hard_Margin_Y)
            {
                Bottom_Margin = Hard_Margin_Y;
            }

            Page_Height -= (Top_Margin + Bottom_Margin);
            Page_Width -= (Right_Margin + Left_Margin);
            Page_Height *= (DeviceDpi / 100f);
            Page_Width *= (DeviceDpi / 100f);

            RectangleF srcRect = new Rectangle(0, 0, this.BackgroundImage.Width, this.BackgroundImage.Height);
            int nWidth = 600;
            int nHeight = 425;
            Font printFont = new Font(FontFamily.GenericSansSerif, 10);
            RectangleF destRect = new Rectangle(0, 0, nWidth, nHeight);
            //graphics.DrawImage(this.BackgroundImage, destRect, srcRect, GraphicsUnit.Pixel);
            float scalex = destRect.Width / srcRect.Width;
            float scaley = destRect.Height / (srcRect.Height - 24);
            // Pen aPen = new Pen(Brushes.Black, 1);
            for (int i = 0; i < this.Controls.Count; i++)
            {
                if (Controls[i].GetType() == this.Controls["txtIn1"].GetType() && !Controls[i].Name.ToUpper().Contains("WORKDAYS"))
                {
                    TextBox theText = (TextBox)Controls[i];
                    graphics.DrawString(theText.Text, printFont, Brushes.Black, theText.Bounds.Left * scalex, theText.Bounds.Top * scaley, new StringFormat());
                }
            }
        }

        private void aboutMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Instructions:\n1. Fill out name and employee number.\n2. Check day or night on days worked.\n3. If you came in early every day for 30 minute meeting select 30 minute Meetings\n\nThis app was created by Joshua Edwards.");
        }

        private void frmPrintTimeCard_Load(object sender, EventArgs e)
        {
            howManyDaysSinceSunday = adjustDay(DayOfWeek.Sunday, currentDay, true);
            daysUntilSaturday = adjustDay(DayOfWeek.Saturday, currentDay, false);
            Sunday = currentDay.AddDays(-howManyDaysSinceSunday);
            Sunday = adjustTime(Sunday, 0, 0);
            txtFrom.Text = currentDay.AddDays(-howManyDaysSinceSunday).ToShortDateString();
            txtTo.Text = currentDay.AddDays(daysUntilSaturday).ToShortDateString();
            var textBoxes = new System.Collections.Generic.List<Control>();
            var checkBoxes = new System.Collections.Generic.List<Control>();
            foreach (Control control in Controls)
            {
                if (control != null && control is TextBox)
                {
                    textBoxes.Add(control as TextBox);
                }
                else if (control != null && control is CheckBox)
                {
                    checkBoxes.Add(control as CheckBox);
                }
            }
            foreach (TextBox ctl in textBoxes)
            {

                if ((ctl).Name.ToUpper().Contains("TXTIN") || (ctl).Name.ToUpper().Contains("TXTOUT"))
                {
                    EventHandler eventHandler = new EventHandler(textBox_WorkHours);
                    ctl.TextChanged += eventHandler;
                }

                if ((ctl).Name.ToUpper().Contains("TXTWORKED"))
                {
                    EventHandler workedEventHandler = new EventHandler(textBox_TotalWorkedHours);
                    ctl.TextChanged += workedEventHandler;
                    EventHandler bonusEventHandler = new EventHandler(textBox_BonusHours);
                    ctl.TextChanged += bonusEventHandler;
                }

                if ((ctl).Name.ToUpper().Contains("TXTBONUS"))
                {
                    EventHandler eventHandler = new EventHandler(textBox_TotalBonusHours);
                    ctl.TextChanged += eventHandler;
                }

                if ((ctl).Name.ToUpper().Contains("TXTOTHER"))
                {
                    EventHandler eventHandler = new EventHandler(textBox_TotalOtherHours);
                    ctl.TextChanged += eventHandler;
                }

            }
            foreach (CheckBox ctl in checkBoxes)
            {
                if ((ctl).Name.ToUpper().Contains("CBDAY") || (ctl).Name.ToUpper().Contains("CBNIGHT"))
                {
                    EventHandler eventHandler = new EventHandler(checkBox_ChangeCheck);
                    ctl.Click += eventHandler;
                }
            }
        }

        private void checkBox_ChangeCheck(object sender, EventArgs e)
        {
            Control ctl = (Control)sender;
            int ctlNumber;
            CheckBox day, night;
            TextBox In, Out;
            if (ctl.Name.ToUpper().Contains("CBDAY"))
            {
                ctlNumber = int.Parse(ctl.Name.Remove(0, 5));
                day = (CheckBox)Controls["cbDay" + ctlNumber];
                night = (CheckBox)Controls["cbNight" + ctlNumber];
                In = (TextBox)Controls["txtIn" + ctlNumber];
                Out = (TextBox)Controls["txtOut" + ctlNumber];
                night.Checked = false;
            }
            else
            {
                ctlNumber = int.Parse(ctl.Name.Remove(0, 7));
                day = (CheckBox)Controls["cbDay" + ctlNumber];
                night = (CheckBox)Controls["cbNight" + ctlNumber];
                In = (TextBox)Controls["txtIn" + ctlNumber];
                Out = (TextBox)Controls["txtOut" + ctlNumber];
                day.Checked = false;
            }
            if (!day.Checked && !night.Checked)
            {
                In.Text = "SDO";
                Out.Text = "";

            }
            else if (!day.Checked && night.Checked)
            {
                if (cbEarly.Checked)
                {
                    if (cb24hr.Checked)
                    {
                        enterTime(ctlNumber, "H:mm", 19, 30, 8, 0, night);
                    }
                    else
                    {
                        enterTime(ctlNumber, "h:mm tt", 19, 30, 8, 0, night);
                    }
                }
                else
                {
                    if (cb24hr.Checked)
                    {
                        enterTime(ctlNumber, "H:mm", 20, 0, 8, 0, night);
                    }
                    else
                    {
                        enterTime(ctlNumber, "h:mm tt", 20, 0, 8, 0, night);
                    }
                }
            }
            else if (day.Checked && !night.Checked)
            {
                if (cbEarly.Checked)
                {
                    if (cb24hr.Checked)
                    {
                        enterTime(ctlNumber, "H:mm", 7, 30, 20, 0, night);
                    }
                    else
                    {
                        enterTime(ctlNumber, "h:mm tt", 7, 30, 20, 0, night);
                    }
                }
                else
                {
                    if (cb24hr.Checked)
                    {
                        enterTime(ctlNumber, "H:mm", 8, 0, 20, 0, night);
                    }
                    else
                    {
                        enterTime(ctlNumber, "h:mm tt", 8, 0, 20, 0, night);
                    }
                }
            }
        }
        public void enterTime(int indexChecked, string format, int hr1, int min1, int hr2, int min2, CheckBox cbNight)
        {
            Control ctnIn = Controls["txtIn" + (indexChecked)];
            Control ctnOut = Controls["txtOut" + (indexChecked)];
            if (cbNight.Checked)
            {
                inTime[indexChecked - 1] = adjustTime(Sunday.AddDays(indexChecked - 1), hr1, min1);
                outTime[indexChecked - 1] = adjustTime(Sunday.AddDays(indexChecked), hr2, min2);
            }
            else
            {
                inTime[indexChecked - 1] = adjustTime(Sunday.AddDays(indexChecked - 1), hr1, min1);
                outTime[indexChecked - 1] = adjustTime(Sunday.AddDays(indexChecked - 1), hr2, min2);
            }
            ctnIn.Text = inTime[indexChecked - 1].ToString(format);
            ctnOut.Text = outTime[indexChecked - 1].ToString(format);
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 7; i++)
            {
                inTime[i] = Sunday;
                outTime[i] = Sunday;
            }
            for (int i = 1; i < 8; i++)
            {
                CheckBox day = (CheckBox)Controls["cbDay" + i];
                CheckBox night = (CheckBox)Controls["cbNight" + i];
                TextBox In = (TextBox)Controls["txtIn" + i];
                TextBox Out = (TextBox)Controls["txtOut" + i];
                TextBox worked = (TextBox)Controls["txtWorked" + i];
                TextBox bonus = (TextBox)Controls["txtBonus" + i];
                TextBox overtime = (TextBox)Controls["txtOvertime" + i];
                TextBox other = (TextBox)Controls["txtOther" + i];
                TextBox last = (TextBox)Controls["textBox" + i];
                day.Checked = false;
                night.Checked = false;
                In.Text = "";
                Out.Text = "";
                worked.Text = "";
                bonus.Text = "";
                overtime.Text = "";
                other.Text = "";
                last.Text = "";
            }
            txtName.Text = "";
            txtNumber.Text = "220";
            txtCrew.Text = "";
            txtTotalO.Text = "";
            txtTotalW.Text = "";
            txtTotalB.Text = "";
            txtTotalOth.Text = "";
            cb24hr.Checked = false;
            cbEarly.Checked = false;
        }
    }
}

