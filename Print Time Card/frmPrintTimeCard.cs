using System;
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

        public TimeSpan ProcessTextboxToTime(TextBox obj)
        {
            TimeSpan convertedTime = TimeSpan.Parse(obj.Text.Remove(5).Trim());
            if (obj.Text.Remove(0, 5).Trim().ToUpper() == "PM")
            {
                convertedTime += new TimeSpan(12, 0, 0);
            }
            return convertedTime;
        }

        public void textBox_WorkHours(object sender, EventArgs args)
        {
            TextBox ctl = sender as TextBox;
            int number = 0;
            TimeSpan convertedTimeIn, convertedTimeOut, originalTimeIn, originalTimeOut;
            if (ctl.Name.ToUpper().Contains("IN"))
            {
                number = int.Parse(ctl.Name.Remove(0, 5));
            }
            else if (ctl.Name.ToUpper().Contains("OUT"))
            {
                number = int.Parse(ctl.Name.Remove(0, 6));
            }
            Control ctlIn = Controls["txtIn" + number];
            Control ctlOut = Controls["txtOut" + number];
            if (ctlIn.Text == "" || ctlOut.Text == "")
            {
                return;
            }
            try
            {
                convertedTimeIn = ProcessTextboxToTime((TextBox)ctlIn);
                convertedTimeOut = ProcessTextboxToTime((TextBox)ctlOut);
                originalTimeIn = inTime[(number - 1)] - Sunday.AddDays(number - 1);
                originalTimeOut = outTime[(number - 1)] - Sunday.AddDays(number - 1);
            }
            catch
            {
                return;
            }
            if (convertedTimeIn.CompareTo(originalTimeIn) != 0 || convertedTimeOut.CompareTo(originalTimeOut) != 0)
            {
                try
                {
                    // Not Working
                    TimeSpan timeIn = ProcessTextboxToTime((TextBox)ctlIn);
                    TimeSpan timeOut = ProcessTextboxToTime((TextBox)ctlOut);
                    inTime[number - 1] = Sunday.AddDays(number - 1);
                    inTime[number - 1] = inTime[number - 1].Add(timeIn);
                    outTime[number - 1] = Sunday.AddDays(number - 1);
                    outTime[number - 1] = outTime[number - 1].Add(timeOut);
                }
                catch (FormatException e)
                {
                    MessageBox.Show("There was a problem with the format of the entered time" + Environment.NewLine + e.Message + Environment.NewLine + e.HelpLink);
                    throw;
                }
            }
            TimeSpan difference = outTime[number - 1] - inTime[number - 1];
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

        public void enterTime(int indexChecked, string format, int hr1, int min1, int hr2, int min2)
        {
            Control ctnIn = Controls["txtIn" + (indexChecked + 1)];
            Control ctnOut = Controls["txtOut" + (indexChecked + 1)];
            inTime[indexChecked] = adjustTime(Sunday.AddDays(indexChecked), hr1, min1);
            outTime[indexChecked] = adjustTime(Sunday.AddDays(indexChecked), hr2, min2);
            ctnIn.Text = inTime[indexChecked].ToString(format);
            ctnOut.Text = outTime[indexChecked].ToString(format);
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
            foreach (Control control in Controls)
            {
                if (control != null && control is TextBox)
                {
                    textBoxes.Add(control as TextBox);
                }
            }
            foreach (TextBox ctl in textBoxes)
            {

                if ((ctl).Name.ToUpper().Contains("IN") || (ctl).Name.ToUpper().Contains("OUT"))
                {
                    EventHandler eventHandler = new EventHandler(textBox_WorkHours);
                    ctl.TextChanged += eventHandler;
                }

                if ((ctl).Name.ToUpper().Contains("WORKED"))
                {
                    EventHandler workedEventHandler = new EventHandler(textBox_TotalWorkedHours);
                    ctl.TextChanged += workedEventHandler;
                    EventHandler bonusEventHandler = new EventHandler(textBox_BonusHours);
                    ctl.TextChanged += bonusEventHandler;
                }

                if ((ctl).Name.ToUpper().Contains("BONUS"))
                {
                    EventHandler eventHandler = new EventHandler(textBox_TotalBonusHours);
                    ctl.TextChanged += eventHandler;
                }

            }
        }


        private void btnApply_Click(object sender, EventArgs e)
        {
            if (clbDays.CheckedItems.Count == 0)
            {
                MessageBox.Show("Select days worked");
                return;
            }
            foreach (int indexChecked in clbDays.CheckedIndices)
            {
                if (cbDays.Checked)
                {
                    if (cbEarly.Checked)
                    {
                        if (cb24hr.Checked)
                        {
                            enterTime(indexChecked, "H:mm", 7, 30, 20, 0);
                        }
                        else
                        {
                            enterTime(indexChecked, "h:mm tt", 7, 30, 20, 0);
                        }
                    }
                    else
                    {
                        if (cb24hr.Checked)
                        {
                            enterTime(indexChecked, "H:mm", 8, 0, 20, 0);
                        }
                        else
                        {
                            enterTime(indexChecked, "h:mm tt", 8, 0, 20, 0);
                        }
                    }
                }
                else if (cbNights.Checked)
                {
                    if (cbEarly.Checked)
                    {
                        if (cb24hr.Checked)
                        {
                            enterTime(indexChecked, "H:mm", 19, 30, 8, 0);
                        }
                        else
                        {
                            enterTime(indexChecked, "h:mm tt", 19, 30, 8, 0);
                        }
                    }
                    else
                    {
                        if (cb24hr.Checked)
                        {
                            enterTime(indexChecked, "H:mm", 20, 0, 8, 0);
                        }
                        else
                        {
                            enterTime(indexChecked, "h:mm tt", 20, 0, 8, 0);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please choose nights or days");
                    return;
                }
            }
            for (int i = 1; i < 8; i++)
            {
                Control cntIn = Controls["txtIn" + i];
                if (cntIn.Text == "")
                {
                    cntIn.Text = "SDO";
                }
            }
        }
    }
}

