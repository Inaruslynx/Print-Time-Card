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

        public static int adjustDay(DayOfWeek day, DateTime time, bool past)
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

        public static DateTime adjustTime(DateTime time, int hour, int minute)
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

        private void frmPrintTimeCard_Load(object sender, System.EventArgs e)
        {
            howManyDaysSinceSunday = adjustDay(DayOfWeek.Sunday, currentDay, true);
            daysUntilSaturday = adjustDay(DayOfWeek.Saturday, currentDay, false);
            Sunday = currentDay.AddDays(-howManyDaysSinceSunday);
            txtFrom.Text = currentDay.AddDays(-howManyDaysSinceSunday).ToShortDateString();
            txtTo.Text = currentDay.AddDays(daysUntilSaturday).ToShortDateString();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            var dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
            if (!cbDays.Checked && !cbNights.Checked)
            {
                MessageBox.Show("Please choose nights or days");
                return;
            }
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
                            Control ctnIn = this.Controls["txtIn" + (indexChecked + 1)] as Control;
                            Control ctnOut = this.Controls["txtOut" + (indexChecked + 1)] as Control;
                            inTime[indexChecked] = adjustTime(Sunday.AddDays(indexChecked), 7, 30);
                            outTime[indexChecked] = adjustTime(Sunday.AddDays(indexChecked), 20, 0);
                            ctnIn.Text = inTime[indexChecked].ToString(inTime[indexChecked].ToString("H:mm"));
                            ctnOut.Text = outTime[indexChecked].ToString(outTime[indexChecked].ToString("H:mm"));
                            ctnIn.Visible = true;
                            ctnOut.Visible = true;
                        }
                        else
                        {

                        }
                    }
                }
            }
        }
    }
}

