using System;
using Mozart.SeePlan;
using System.Globalization;

namespace CSOT.UserInterface.Utils
{
    public class DateHelper
    {
        /// <summary>
        /// yyyyMMddHHmmss
        /// </summary>
        public static readonly string dbDateTimeFormatOld = "yyyyMMddHHmmss";
        /// <summary>
        /// yyyyMMdd HHmmss
        /// </summary>
        private static readonly string dbDateTimeFormatNew = "yyyyMMdd HHmmss";

        public static bool UseSpacedDbDateTimeFormat = true;

        /// <summary>
        /// yyyyMMdd
        /// </summary>
        public static string DbDateFormat = "yyyyMMdd";
        /// <summary>
        /// HHmmss
        /// </summary>
        public static string DbTimeFormat = "HHmmss";

        public static string DbDateTimeFormat
        {
            get { return UseSpacedDbDateTimeFormat ? dbDateTimeFormatNew : dbDateTimeFormatOld; }
        }

        public static String GetNowString()
        {
            return DateTime.Now.ToString(DbDateTimeFormat);
        }

        public static DateTime StringToDate(string value)
        {
            return StringToDateTime(value, false);
        }

        public static DateTime StringToDateTime(string value)
        {
            return StringToDateTime(value, true);
        }
        static public DateTime StringToDateTime(string value, bool withTime)
        {
            if (value == null)
                return DateTime.MinValue;

            value = StringHelper.Trim(value);
            int length = value.Length;

            if (length < 8)
                return DateTime.MinValue;

            int year = 0;
            int month = 0;
            int day = 0;
            int hour = 0;
            int minute = 0;
            int second = 0;
            try
            {
                year = int.Parse(value.Substring(0, 4));
                month = int.Parse(value.Substring(4, 2));
                day = int.Parse(value.Substring(6, 2));
                if (withTime)
                {
                    int t = 8;

                    if (length >= 10)
                    {
                        if (value[8] == ' ')
                            t++;

                        hour = int.Parse(value.Substring(t + 0, 2));
                    }

                    if (length >= 12)
                    {
                        if (value[8] == ' ')
                            t++;

                        minute = int.Parse(value.Substring(t + 2, 2));
                    }

                    if (length >= 14)
                    {
                        second = int.Parse(value.Substring(t + 4, 2));
                    }
                }
            }
            catch
            {

            }
            return new DateTime(year, month, day, hour, minute, second);
        }
        static public TimeSpan StringToTime(string value)
        {
            if (value == null)
                return TimeSpan.Zero;

            value = value.Trim();
            int length = value.Length;

            if (length < 4)
                return TimeSpan.Zero;


            int hour = 0;
            int minute = 0;
            int second = 0;
            try
            {
                hour = int.Parse(value.Substring(0, 2));
                minute = int.Parse(value.Substring(2, 2));

                if (length >= 6)
                {
                    second = int.Parse(value.Substring(4, 2));
                }
            }
            catch
            {
            }
            return new TimeSpan(hour, minute, second);
        }

        public static string DateToString(DateTime dateTime)
        {
            return DateTimeToString(dateTime, false);
        }

        public static string DateTimeToString(DateTime dateTime)
        {
            return DateTimeToString(dateTime, true);
        }
        public static string DateTimeToString(DateTime dateTime, bool withTime)
        {
            if (dateTime == DateTime.MinValue)
                return "0";
            if (withTime)
                return dateTime.ToString(DbDateTimeFormat);
            else return dateTime.ToString(DbDateFormat);
        }
        public static string DateTimeToStringTrimSec(DateTime dateTime)
        {
            int sec = dateTime.Second;
            if (sec > 0) dateTime = dateTime.AddSeconds(-sec);
            return DateTimeToString(dateTime, true);
        }
        public static DateTime DateTimeToStringTrimMilliSec(DateTime dateTime)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
        }

        public static string TimeToStringTrimSec(TimeSpan time)
        {
            int sec = time.Seconds;
            if (sec > 0) time = time.Add(TimeSpan.FromSeconds(-sec));
            return TimeToString(time);
        }
        public static string TimeToString(TimeSpan time)
        {
            if (time == TimeSpan.Zero)
                return "0";
            return String.Format("{0:00}{1:00}{2:00}", time.Hours, time.Minutes, time.Seconds);
        }

        public static DateTime StringToDTime(string value)
        {
            return (new DateTime(1900, 1, 1)) + StringToTime(value);
        }
        public static string DTimeToStringTrimSec(DateTime time)
        {
            int sec = time.Second;
            if (sec > 0) time = time.AddSeconds(-sec);
            return DTimeToString(time);
        }
        public static string DTimeToString(DateTime time)
        {
            return time.ToString(DbTimeFormat);
        }

        public static object DBNullable(DateTime dt)
        {
            return dt == DateTime.MinValue ? DBNull.Value : (object)dt;
        }

        public static string Format(string dbdt)
        {
            return Format(StringToDateTime(dbdt));
        }

        public static string Format(string dbdt, bool noTime)
        {
            return Format(StringToDate(dbdt), noTime);
        }

        public static string Format(DateTime dt)
        {
            return Format(dt, false);
        }

        public static string Format(DateTime dt, bool noTime)
        {
            if (noTime)
                return dt.ToString("yyyy-MM-dd");
            else
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string SplitDateToString(DateTime dt)
        {
            //TODO : 확인 필요
            return DateToString(ShopCalendar.SplitDate(dt));
        }
        public static DateTime StartDayOfWeek(DateTime date)
        {
            int dayOfWeek = (int)date.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7;

            return date.AddDays((int)DayOfWeek.Monday - dayOfWeek);
        }

        public static DateTime BaseDayOfWeek(DateTime date)
        {
            //목요일 기준으로 주차 결정
            return StartDayOfWeek(date).AddDays(3);
        }
        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(Calendar cal, DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = cal.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return cal.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        public static int WeekOfYear(DateTime date)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            return GetIso8601WeekOfYear(ci.Calendar, date);
        }
        public static int WeekNo(DateTime date)
        {
            DateTime baseDay = BaseDayOfWeek(ShopCalendar.SplitDate(date));

            return baseDay.Year * 100 + WeekOfYear(baseDay);
        }
        public static string GetWeekNo(DateTime date)
        {
            return WeekNo(date).ToString();
        }

        public static DateTime GetStartDateWithMonthNo(string monthNo)
        {
            if (monthNo.Length != 6)
                return DateTime.MaxValue;

            int year = Convert.ToInt32(monthNo.Substring(0, 4));
            int month = Convert.ToInt32(monthNo.Substring(4, 2));

            DateTime dt = new DateTime(year, month, 01);
            return DateHelper.GetStartDayOfMonth(dt);
        }

        public static DateTime GetStartDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime GetEndDateWithMonthNo(string monthNo)
        {
            DateTime dt = GetStartDateWithMonthNo(monthNo);
            if (dt == DateTime.MaxValue)
                return dt;

            return dt.AddDays(GetDaysInMonth(monthNo) - 1);
        }

        public static int GetDaysInMonth(string monthNo)
        {
            if (monthNo.Length > 6)
                return 30;

            int year = Convert.ToInt32(monthNo.Substring(0, 4));
            int month = Convert.ToInt32(monthNo.Substring(4, 2));

            CultureInfo ci = CultureInfo.InvariantCulture;

            return ci.Calendar.GetDaysInMonth(year, month);
        }

        public static DateTime Trim(DateTime date, string format)
        {
            if (string.IsNullOrEmpty(format))
                return date;

            switch (format)
            {
                case "yyyy":
                    return new DateTime(date.Year, 0, 0);
                case "MM":
                    return new DateTime(date.Year, date.Month, 0);
                case "dd":
                    return new DateTime(date.Year, date.Month, date.Day);
                case "HH":
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                case "mm":
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                case "m0":
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, ((date.Minute / 10) * 10), 0);
                case "ss":
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                case "s0":
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, ((date.Second / 10) * 10));
            }
            return date;
        }

        public static DateTime GetRptDate_1Hour(DateTime t, int baseMinute)
        {
            //1시간 단위
            int baseHours = 1;
            
            //ex) HH:30:00
            DateTime rptDate = DateHelper.Trim(t, "HH").AddMinutes(baseMinute);

            //baseMinute(ex.30분) 이상인 경우 이후 시간대 baseMinute의 실적
            //07:30 = 06:30(초과) ~ 07:30(이하)인경우, 06:40 --> 07:30, 07:30 --> 07:30, 07:40 --> 08:30
            if (t.Minute > baseMinute)
            {
                rptDate = rptDate.AddHours(baseHours);
            }

            return rptDate;
        }
    }
}
