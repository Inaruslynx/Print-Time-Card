using System;
using System.Windows.Forms;

namespace Print_Time_Card.Tools
{
    internal class UI
    {
        public void ProcessTextboxChange(TextBox obj, Time TimeTools, CheckBox cb24hr, GroupBox grpBox)
        {
            // Get a Timespan then if a day simply adjust inTime and outTime
            // if night then make sure outTime is the next day

            // first we need to find which day it is
            int number;
            bool In, night = false;
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
            ComboBox Box = (ComboBox)grpBox.Controls["cbBox" + number];
            if (Box.SelectedIndex == 2) night = true;
            // now we get a TimeSpan and change in and out time
            if (cb24hr.Checked == false)
            {
                // expected text for time: hh:mm pm/am but it could be h:mm pm/am
                // also the user could be in the middle of changing the time so
                // it could be h:m pm/am so need to return if not valid
                string entry = obj.Text.ToUpper().Trim();
                bool morning = entry.Contains("AM");
                bool afternoon = entry.Contains("PM");
                TimeSpan convertedTime;
                if (morning)
                {
                    if (!TimeSpan.TryParse(entry.Remove(entry.Length - 2), out convertedTime)) return;
                }
                else if (afternoon)
                {
                    if (!TimeSpan.TryParse(entry.Remove(entry.Length - 2), out convertedTime)) return;
                    convertedTime += new TimeSpan(12, 0, 0);
                }
                else
                {
                    return;
                }
                // I need to first determine if difference will be earlier or later than previous time
                // then I need to add or substract the difference in time to inTime or outTime
                // to determine if earlier or later I need to compare convertedTime to inTime/outTime (depending if obj)
                if (In)
                {
                    TimeTools.adjInTime(number, convertedTime);
                }
                else
                {
                    TimeTools.adjOutTime(number, convertedTime, night);
                }
            }
            else
            {
                TimeSpan convertedTime;
                if (obj.Text == "SDO" || !TimeSpan.TryParse(obj.Text, out convertedTime))
                {
                    return;
                }
                if (In)
                {
                    TimeTools.adjInTime(number, convertedTime);
                }
                else
                {
                    TimeTools.adjOutTime(number, convertedTime, night);
                }
            }
        }
    }
}
