using Mozart.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace CSOT.UserInterface.Utils
{
    public class CommonHelper
    {
        public static bool IsDigit( object oValue )
        {
            if ( IsNullOrDBNullValue( oValue ) ) return false;

            string sValue = oValue.ToString().Trim();
            Regex r = new Regex( @"^\d+$" );
            Match m = r.Match( sValue );
            return m.Success;
        }

        public static bool IsNumber( object oValue )
        {
            if ( IsNullOrDBNullValue( oValue ) ) return false;

            string sValue = oValue.ToString().Trim();
            Regex r = new Regex( @"^[+-]?\d+(\.\d+)?$" );
            Match m = r.Match( sValue );
            return m.Success;
        }

        public static bool IsNullOrDBNullValue( object oValue )
        {
            return ( oValue == null || oValue == DBNull.Value );
        }

        public static string CreateKey(params string[] strArr)
        {
            string sValue = null;
            foreach (string str in strArr)
            {
                if (sValue == null) sValue = str;
                else sValue += StringHelper.KeySeparator + str;
            }
            return sValue;
        }

        public static string CreateKey(params object[] args)
        {
            return StringHelper.ConcatKey(args);
        }

        #region Date/Time
        internal static string GetYearMon(DateTime dt)
        {
            return WeekHelper.GetYearMon(dt);
            //DateTimeFormatInfo en_US = new CultureInfo("en-US", false).DateTimeFormat;
            //string year = dt.Year.ToString();
            //return "" + year[year.Length - 1] + dt.ToString("MMM", en_US)[0];
        }

        public static DateTime StartDayOfWeek(string week)
        {
            return WeekHelper.StartDayOfWeek(week);
            //int year = Convert.ToInt32(week.Substring(0, 4));
            //int w = Convert.ToInt32(week.Substring(4, 2)) - 1;

            //DateTime sdt = new DateTime(year, 1, 1);
            //DateTime sow = StartDayOfWeek(sdt);

            //int rw = WeekNo(sdt);
            //if (rw / 100 < year)
            //    w++;

            //return sow.AddDays(w * 7);
        }

        public static int WeekOfYear(DateTime date)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

#if true
            return WeekHelper.WeekOfYear(date); // GetIso8601WeekOfYear(ci.Calendar, date);
#else 

            return ci.Calendar.GetWeekOfYear(date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
#endif
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

        public static int WeekNo(DateTime date)
        {
            return WeekHelper.WeekNo(date);
        }

        public static DateTime BaseDayOfWeek(DateTime date)
        {
            return WeekHelper.BaseDayOfWeek(date);            
        }

        public static DayOfWeek DiffWeek(DayOfWeek dow, int d)
        {

            return WeekHelper.DiffWeek(dow, d);
            //int i = (int)(dow - d);

            //return i < 0 ? (DayOfWeek)(7 + i) : (DayOfWeek)i;
        }

        public static DateTime StartDayOfWeek(DateTime date)
        {
            return WeekHelper.StartDayOfWeek(date);
        }

        public static DateTime StartDayOfWeek(DateTime date, int days, bool isConsiderMonth)
        {
            return WeekHelper.StartDayOfWeek(date, days, isConsiderMonth);
        }

        public static DateTime EndDayOfWeek(DateTime date)
        {
            return WeekHelper.EndDayOfWeek(date);
        }

        public static DateTime EndDayOfWeek(DateTime date, int days, bool isConsiderMonth)
        {
            return WeekHelper.EndDayOfWeek(date, days, isConsiderMonth);
        }
                        
        public static string GetWeekPlanNo(DateTime planDate)
        {
            return WeekHelper.GetWeekPlanNo(planDate);
        }

        // 확인요
        public static void GetWeekRange(string weekNo, out DateTime start, out DateTime end)
        {
            int year = int.Parse(weekNo.Substring(0, 4));
            int week = int.Parse(weekNo.Substring(4, 2));

            CultureInfo ci = CultureInfo.InvariantCulture;
            DateTime yearStart = new DateTime(year, 1, 1);

            start = StartDayOfWeek(yearStart);
            start = ci.Calendar.AddWeeks(start, week - 1);
            end = EndDayOfWeek(start);

            if (weekNo.Length > 6)
            {
                if (weekNo[6] == 'a')
                {
                    end = new DateTime(start.Year, start.Month,
                        ci.Calendar.GetDaysInMonth(start.Year, start.Month));
                }
                else if (weekNo[6] == 'b')
                {
                    start = new DateTime(end.Year, end.Month, 1);
                }
            }
        }

        public static string[] ParseModItemRoute(string routeID)
        {
            string[] routes = new string[4] { string.Empty, string.Empty, string.Empty, string.Empty };

            if (string.IsNullOrEmpty(routeID))
                return routes;
                            
            string[] items = routeID.Split('_');

            if (items.Length >= 1)
                routes[0] = items[0].Trim();    //FA

            if (items.Length >= 2)
                routes[1] = items[1].Trim();    //LAMI

            if (items.Length >= 3)
                routes[2] = items[2].Trim();    //OLB

            if (items.Length >= 4)
                routes[3] = items[3].Trim();    //CP

            return routes;
        }

        #endregion Date/Time

        #region NumberParse

        public static double ToDouble(string s)
        {
            return ToDouble(s, 0);
        }

        public static double ToDouble(string s, double defaultValue)
        {
            try
            {
                double vaule;
                if (double.TryParse(s, out vaule))
                    return vaule;

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static float ToFloat(string s)
        {
            return ToFloat(s, 0);
        }

        public static float ToFloat(string s, float defaultValue)
        {
            try
            {
                float vaule;
                if (float.TryParse(s, out vaule))
                    return vaule;

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int ToInt32(string s)
        {
            return ToInt32(s, 0);
        }

        public static int ToInt32(string s, int defaultValue)
        {
            try
            {
                int vaule;
                if (int.TryParse(s, out vaule))
                    return vaule;

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion NumberParse

        #region EnumHelper

        public static string ToEnumName(object name)
        {
            return Enum.GetName(name.GetType(), name);
        }

        public static List<string> ToEnumNames<T>()
        {
            return Enum.GetNames(typeof(T)).ToList();
        }

        public static T ToEnum<T>(string name)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), name);
            }
            catch
            {
                return default(T);
            }
        }

        public static T ToEnum<T>(string name, T defaultValue) where T : struct, IConvertible
        {
            try
            {
                T result;

                if (Enum.TryParse<T>(name, true, out result) == false)
                    return defaultValue;

                return result;
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region Bool Y/N

        public static string ToStringYN(bool yn)
        {
            return yn ? "Y" : "N";
        }

        public static bool ToBoolYN(string yn)
        {
            return ToBoolYN(yn, false);
        }

        public static bool ToBoolYN(string yn, bool defaultValue)
        {
            if (CommonHelper.IsEmptyID(yn))
                return defaultValue;

            if (string.Equals(CommonHelper.Trim(yn), "Y", StringComparison.CurrentCultureIgnoreCase))
                return true;
            else
                return false;
        }

        public static string IdentityNull()
        {
            return Mozart.SeePlan.StringUtility.IdentityNull;
        }

        public static bool IsEmptyID(string text)
        {
            return Mozart.SeePlan.StringUtility.IsEmptyID(text);
        }

        public static string Trim(string text)
        {
            if (text == null)
                return null;

            return text.Trim();
        }

        public static string ToString(string[] keys, string separator = ",")
        {
            if (keys == null)
                return null;

            StringBuilder s = new StringBuilder();

            int count = keys.Length;
            for (int i = 0; i < count; i++)
            {
                string key = keys[i];
                if (i > 0)
                    s.Append(separator);

                s.Append(key);
            }

            return s.ToString();
        }

        public static string ToString(List<string> keys, string separator = ",")
        {
            if (keys == null)
                return null;

            return ToString(keys.ToArray(), separator);
        }

        public static void AddSort<T>(List<T> list, T item, Comparison<T> compare)
        {
            var index = list.BinarySearch(item, compare);
            if (index < 0)
                index = ~index;

            list.Insert(index, item);
        }

        public static void AddSort<T>(List<T> list, T item, IComparer<T> compare)
        {
            var index = list.BinarySearch(item, compare);
            if (index < 0)
                index = ~index;

            list.Insert(index, item);
        }

        public static bool Equals(string a, string b, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)
        {
            return string.Equals(a, b, comparisonType);
        }

        #endregion
    }    
}
