using System;
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

        // This is where I left off at.
        // I need to automatically assign an event to all of the text boxes with input time
        // I'm too tired to understand where I'm suppose to define the Event and then assign it.
        // I'd guess define the event here and assign it at form load (which is what I've tried to do)
        // But it isn't happy about a return, which I didn't think events needed?
        public Action<object> HoursWorked(TextBox obj)
        {
            int number = obj.Name[-1];
            Control ctlIn = this.Controls["txtIn" + number];
            Control ctlOut = this.Controls["txtOut" + number];
            if (DateTime.Parse(ctlIn.Text.ToString()).CompareTo(inTime[number - 1]) != 0 || ctlOut.Text.ToString().CompareTo(outTime[number - 1]) != 0)
            {
                try
                {
                    TimeSpan timeIn = TimeSpan.Parse(ctlIn.Text);
                    TimeSpan timeOut = TimeSpan.Parse(ctlOut.Text);
                    inTime[number - 1] = Sunday.AddDays(number - 1);
                    inTime[number - 1].Add(timeIn);
                    outTime[number - 1] = Sunday.AddDays(number - 1);
                    outTime[number - 1].Add(timeOut);
                }
                catch (FormatException e)
                {
                    MessageBox.Show("There was a problem with the format of the entered time" + Environment.NewLine + e.Message + Environment.NewLine + e.HelpLink);
                    throw;
                }
            }
            TimeSpan difference = outTime[number - 1] - inTime[number - 1];
            Control ctlWork = this.Controls["txtWorked" + number];
            ctlWork.Text = difference.ToString();
        }

        public void enterTime(int indexChecked, string format, int hr1, int min1, int hr2, int min2)
        {
            Control ctnIn = this.Controls["txtIn" + (indexChecked + 1)] as Control;
            Control ctnOut = this.Controls["txtOut" + (indexChecked + 1)] as Control;
            inTime[indexChecked] = adjustTime(Sunday.AddDays(indexChecked), hr1, min1);
            outTime[indexChecked] = adjustTime(Sunday.AddDays(indexChecked), hr2, min2);
            ctnIn.Text = inTime[indexChecked].ToString(inTime[indexChecked].ToString(format));
            ctnOut.Text = outTime[indexChecked].ToString(outTime[indexChecked].ToString(format));
        }

        private void frmPrintTimeCard_Load(object sender, System.EventArgs e)
        {
            howManyDaysSinceSunday = adjustDay(DayOfWeek.Sunday, currentDay, true);
            daysUntilSaturday = adjustDay(DayOfWeek.Saturday, currentDay, false);
            Sunday = currentDay.AddDays(-howManyDaysSinceSunday);
            Sunday = adjustTime(Sunday, 0, 0);
            txtFrom.Text = currentDay.AddDays(-howManyDaysSinceSunday).ToShortDateString();
            txtTo.Text = currentDay.AddDays(daysUntilSaturday).ToShortDateString();
            foreach (TextBox ctl in this.Controls)
            {
                if ((ctl as TextBox).Name.ToUpper().Contains("IN") || (ctl as TextBox).Name.ToUpper().Contains("OUT"))
                {
                    ctl.TextChanged += HoursWorked(ctl);
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
                Control cntIn = this.Controls["txtIn" + i] as Control;
                if (cntIn.Text == "")
                {
                    cntIn.Text = "SDO";
                }
            }
        }
    }
}

