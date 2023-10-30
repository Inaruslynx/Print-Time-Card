using System;
using System.Windows.Forms;

namespace Print_Time_Card.Tools
{
    internal class Time
    {
        public DateTime[] inTime = new DateTime[7];
        public DateTime[] outTime = new DateTime[7];
        public DateTime currentDay = DateTime.Today;
        public DateTime Sunday;
        public DateTime Saturday;
        int howManyDaysSinceSunday = 0;
        int daysUntilSaturday = 0;

        public void Initialize()
        {
            howManyDaysSinceSunday = adjustDay(DayOfWeek.Sunday, currentDay, true);
            daysUntilSaturday = adjustDay(DayOfWeek.Saturday, currentDay, false);
            Sunday = currentDay.AddDays(-howManyDaysSinceSunday);
            Sunday = adjustTime(Sunday, 0, 0);
            Saturday = currentDay.AddDays(daysUntilSaturday);
            Saturday = adjustTime(Saturday, 0, 0);
        }

        public void enterTime(Control.ControlCollection Controls, int indexChecked, string format, int hr1, int min1, int hr2, int min2, bool night)
        {
            Control ctnIn = Controls["txtIn" + indexChecked];
            Control ctnOut = Controls["txtOut" + indexChecked];
            if (night)
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

        public void adjInTime(int number, TimeSpan convertedTime)
        {
            // Get number for element passed in and time changed
            TimeSpan timeInDay = inTime[number - 1] - Sunday.AddDays(number - 1);
            if (convertedTime > timeInDay)
            {
                inTime[number - 1] += (convertedTime - timeInDay).Duration(); //need Duration for absolute time instead of relative time (+/-)
            }
            else if (convertedTime < timeInDay)
            {
                inTime[number - 1] -= (timeInDay - convertedTime).Duration(); //need Duration for absolute time instead of relative time (+/-)
            }
        }

        public void adjOutTime(int number, TimeSpan convertedTime, bool night)
        {
            // Pass in number of element, time changed, and if it's night shift
            //CheckBox night = (CheckBox)Controls["cbNight" + number]; // old code
            TimeSpan timeOutDay;
            if (night)
            {
                timeOutDay = outTime[number - 1] - Sunday.AddDays(number);
            }
            else
            {
                timeOutDay = outTime[number - 1] - Sunday.AddDays(number - 1);
            }
            if (convertedTime > timeOutDay)
            {
                outTime[number - 1] += (convertedTime - timeOutDay).Duration(); //need Duration for absolute time instead of relative time (+/-)
            }
            else if (convertedTime < timeOutDay)
            {
                outTime[number - 1] -= (convertedTime - timeOutDay).Duration(); //need Duration for absolute time instead of relative time (+/-)
            }
        }

        public void changeWeek(int days)
        {
            Sunday = Sunday.AddDays(days);
            Saturday = Sunday.AddDays(6);
            for (int i = 0; i < 7; i++)
            {
                if (inTime[i] != new DateTime())
                {
                    inTime[i] = inTime[i].AddDays(days);
                }
                if (outTime[i] != new DateTime())
                {
                    outTime[i] = outTime[i].AddDays(days);
                }
            }
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
    }
}
