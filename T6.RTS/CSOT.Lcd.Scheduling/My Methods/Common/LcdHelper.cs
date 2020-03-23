using Mozart.Extensions;
using Mozart.SeePlan;
using Mozart.SeePlan.DataModel;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class LcdHelper
    {
        public static T GetArguments<T>(IDictionary<string, object> args, string key, T defaultValue)
        {
            if (args == null || key == null)
                return defaultValue;

            object oval;
            if (args.TryGetValue(key, out oval) && oval is T)
                return (T)oval;

            return defaultValue;
        }

        public static T GetParameter<T>(IDictionary<string, object> dic, string key, T defaultValue)
        {
            object result;
            if (dic.TryGetValue(key, out result) == false || result == null)
                return defaultValue;

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public static bool IsAllID(this string p)
        {
            if (LcdHelper.Equals(p, Constants.ALL))
                return true;

            return false;
        }

        public static bool IsSameID(this string p)
        {
            if (LcdHelper.Equals(p, Constants.SAME))
                return true;

            return false;
        }

        public static bool IsOtherID(this string p)
        {
            if (LcdHelper.Equals(p, Constants.OTHER))
                return true;

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

        internal static string ToSafeString(string p)
        {
            if (string.IsNullOrEmpty(p))
                return string.Empty; //Constants.NULL_ID;

            return p;
        }

        public static string MergeString(string org, string dst)
        {
            string result = LcdHelper.IdentityNull();
            if (LcdHelper.IsEmptyID(org) == true)
                result = dst;
            else
                result = string.Format("{0},{1}", org, dst);

            return result;
        }

        public static string DbToString(DateTime t, bool withTime = true)
        {
            return Mozart.SeePlan.DateUtility.DbToString(t, withTime);
        }

        public static DateTime DbToDateTime(string value)
        {
            return DbToDateTime(value, DateTime.MinValue);
        }

        public static DateTime DbToDateTime(string value, DateTime defaultValue)
        {
            try
            {
                return StringToDateTime(value);
            }
            catch { }

            return defaultValue;
        }

        public static string WeekNoOfYear(DateTime date)
        {
            DateTime splitDate = ShopCalendar.SplitDate(date);
            return Mozart.SeePlan.DateUtility.WeekNoOfYear(splitDate);
        }

        public static string RemoveSpace(string text)
        {
            if (text == null)
                return null;

            return text.Replace(" ", "");
        }

        public static string Trim(string text)
        {
            if (text == null)
                return null;

            return text.Trim();
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

        public static string Concat(params string[] arr)
        {
            string str = "";
            foreach (var it in arr)
                str = string.Concat(str, it);

            return str;
        }

        public static bool Like(string text, string pattern)
        {
            if (object.Equals(text, pattern))
                return true;

            if (text == null)
                return false;

            return Mozart.Text.LikeUtility.Like(text, pattern);
        }

        public static string ToUpper(string s)
        {
            if (s == null)
                return s;

            return s.ToUpper();
        }

        #region Bool Y/N

        public static string ToStringYN(this bool yn)
        {
            return yn ? "Y" : "N";
        }

        public static bool ToBoolYN(this string yn)
        {
            return ToBoolYN(yn, false);
        }

        public static bool ToBoolYN(this string yn, bool defaultValue)
        {
            if (LcdHelper.IsEmptyID(yn))
                return defaultValue;

            if (string.Equals(LcdHelper.Trim(yn), "Y", StringComparison.CurrentCultureIgnoreCase))
                return true;
            else
                return false;
        }

        #endregion


        //public static int ExecutionModuleComparer(ExecutionModule x, ExecutionModule y)
        //{
        //    if (object.ReferenceEquals(x, y))
        //        return 0;

        //    ExecModule x_ExecModule;
        //    if (Enum.TryParse(x.Name, out x_ExecModule) == false)
        //        x_ExecModule = ExecModule.NONE;

        //    ExecModule y_ExecModule;
        //    if (Enum.TryParse(y.Name, out y_ExecModule) == false)
        //        y_ExecModule = ExecModule.NONE;

        //    int cmp = x_ExecModule.CompareTo(y_ExecModule);

        //    if (cmp == 0)
        //        cmp = x.Name.CompareTo(y.Name);

        //    return cmp;
        //}

        public static TimeSpan ParseTimeSpan(double t, string timeUnit)
        {
            if (Equals(timeUnit, "seconds") || Equals(timeUnit, "seconds"))
            {
                return TimeSpan.FromSeconds(t);
            }

            if (Equals(timeUnit, "minute") || Equals(timeUnit, "minutes") || Equals(timeUnit, "min"))
            {
                return TimeSpan.FromMinutes(t);
            }

            if (Equals(timeUnit, "hour") || Equals(timeUnit, "hours"))
            {
                return TimeSpan.FromHours(t);
            }

            if (Equals(timeUnit, "day") || Equals(timeUnit, "days"))
            {
                return TimeSpan.FromDays(t);
            }

            return TimeSpan.Zero;
        }

        public static bool Equals(string a, string b, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)
        {
            return string.Equals(a, b, comparisonType);
        }

        public static int CompareTo(string x, string y, OrderType orderBy = OrderType.ASC)
        {
            bool isEmpty_x = IsEmptyID(x);
            bool isEmpty_y = IsEmptyID(y);

            int cmp = isEmpty_x.CompareTo(isEmpty_y);
            if (cmp == 0)
                cmp = x.CompareTo(y);

            if (orderBy == OrderType.DESC)
                cmp = cmp * -1;

            return cmp;
        }

        public static string SubString(string text, int startIndex, int length)
        {
            if (text == null)
                return null;

            return text.Substring(startIndex, length);
        }

        public static DateTime Min(DateTime x, DateTime y)
        {
            return (x < y) ? x : y;
        }

        public static DateTime Max(DateTime x, DateTime y)
        {
            return (x > y) ? x : y;
        }

        public static int GetNearIndex(int index, int count)
        {
            if (index < 0)
            {
                index = ~index;
                index--;
            }

            if (index < 0)
                index = 0;
            if (index >= count)
                index = count - 1;

            return index;
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

        public static void SaveLocal<T>(string name, Mozart.Data.Entity.EntityTable<T> table)
        {
            if (table == null)
                return;

            ModelContext.Current.Inputs.SaveLocal(name, table.Rows);
        }

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

        /// <summary>
        /// 반올림(Default : 소수점 둘째자리)
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double ToRound(this double num)
        {
            return num.ToRound(2);
        }

        public static double ToRound(this double num, int round)
        {
            if (double.IsInfinity(num) || double.IsNaN(num))
                return 0d;

            return Math.Round(num, round, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 반올림(Default : 소수점 둘째자리)
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static float ToRound(this float num)
        {
            return num.ToRound(2);
        }

        public static float ToRound(this float num, int round)
        {
            if(float.IsInfinity(num) || float.IsNaN(num))
                return 0f;

            return (float)Math.Round(num, round, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 반올림(Default : 소수점 둘째자리)
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static decimal ToRound(this decimal num)
        {
            return num.ToRound(2);
        }

        public static decimal ToRound(this decimal num, int round)
        {
            return Math.Round(num, round, MidpointRounding.AwayFromZero);     
        }



        /// <summary>
        /// 문자열이 숫자로만 구성되어 있는지?
        /// </summary>
        public static bool IsNumber(string strValue)
        {
            if (strValue == null || strValue.Length < 1)
                return false;

            Regex reg = new Regex(@"^(\d)+$");

            return reg.IsMatch(strValue);
        }


    
   
        #endregion NumberParse

        #region HashSet

        public static HashSet<string> CreateSet(string data)
        {
            HashSet<string> set = new HashSet<string>();
            string[] items = data.Split(',');
            foreach (string s in items)
                set.Add(s.Trim());

            return set;
        }

        #endregion

        #region Common

        public static string CreateKey(params string[] strArr)
        {
            string sValue = null;
            foreach (string str in strArr)
            {
                if (sValue == null) 
                    sValue = str;
                else 
                    sValue += '@' + str;
            }

            return sValue ?? string.Empty;
        }

        #endregion

        #region DateExtension

        public static DateTime StringToDate(string value)
        {
            return StringToDateTime(value, false);
        }

        static public DateTime StringToDateTime(string value, bool withTime = true)
        {
            if (value == null)
                return DateTime.MinValue;

            value = RemoveSpace(value);  //value.Trim();
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

        public static DateTime StartDayOfWeek(string weekNo)
        {
            int year = Convert.ToInt32(weekNo.Substring(0, 4));
            int w = Convert.ToInt32(weekNo.Substring(4, 2)) - 1;

            DateTime sdt = new DateTime(year, 1, 1);
            DateTime sow = Mozart.SeePlan.DateUtility.StartDayOfWeek(sdt);

            int rw = sdt.WeekNo();
            if (rw / 100 < year)
                w++;

            return sow.AddDays(w * 7);
        }

        public static int WeekNo(this DateTime date)
        {
            DateTime baseDay = BaseDayOfWeek(Mozart.SeePlan.ShopCalendar.SplitDate(date));

            return baseDay.Year * 100 + Mozart.SeePlan.DateUtility.WeekOfYear(baseDay);
        }

        public static string ToWeekString(this DateTime date)
        {
            int weekNo = WeekNo(date);

            return weekNo.ToString();
        }

        public static string ToMonthString(this DateTime date)
        {
            int monthNo = date.ToYearMonth();

            return monthNo.ToString();
        }

        public static DateTime BaseDayOfWeek(this DateTime date)
        {
            //목요일 기준으로 주차 결정
            return Mozart.SeePlan.DateUtility.StartDayOfWeek(date).AddDays(3);

        }

        public static DateTime? DbNullDateTime(this DateTime date)
        {
            if (date == DateTime.MinValue || date == DateTime.MaxValue)
                return null;

            return date;
        }

        public static bool IsMinValue(this DateTime date)
        {
            if (date == DateTime.MinValue)
                return true;
            return false;
        }

        public static bool IsMaxValue(this DateTime date)
        {
            if (date == DateTime.MaxValue)
                return true;
            return false;
        }

        #endregion

        public static List<string> ToListString(string s, string separator = ",")
        {
            if (string.IsNullOrEmpty(s))
                return new List<string>();

            char[] sep = separator.ToCharArray();
            var result = Mozart.Text.StringUtility.SplitToList(s, sep);

            return new List<string>(result);
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
                if(i > 0) 
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

        public static string GetTargetFactoryID()
        {
            return InputMart.Instance.GlobalParameters.TargetFactoryID;
        }

        public static string GetTargetShopList()
        {
            var targetShopList = InputMart.Instance.GlobalParameters.TargetShopList;

            StringBuilder sb = new StringBuilder();
            foreach (var shopID in targetShopList)
            {
                sb.AppendFormat("'{0}'", shopID);

                if (shopID != targetShopList.Last())
                    sb.Append(",");
            }

            return sb.ToString();
        }

        public static DateTime GetVersionDate()
        {
            var context = ModelContext.Current;

            DateTime versionDate = context.StartTime.SplitDate();

            string versionDay;
            if (context.QueryArgs.TryGetValue("VERSION_DATE", out versionDay))
            {
                if (string.IsNullOrEmpty(versionDay) == false)
                    versionDate = LcdHelper.DbToDateTime(versionDay);
            }

            return versionDate;
        }

        #region LIST helpers

        public static string ListToSring<T>(IList<T> list)
        {
            return ListToSring(list, ",");
        }

        public static string ListToSring<T>(IList<T> list, string delimiter)
        {
            List<string> temp = new List<string>();
            foreach (var item in list)
            {
                temp.Add(item.ToString());
            }

            return string.Join(delimiter, temp.ToArray());
        }

        public static void AddToList<T>(List<T> list, T item)
        {
            GrowList<T>(list, 1, 0);
            list.Add(item);
        }

        public static void AddToList<T>(List<T> list, T item, int growBy)
        {
            GrowList<T>(list, 1, growBy);
            list.Add(item);
        }

        public static void GrowList<T>(List<T> list)
        {
            GrowList<T>(list, 1, 0);
        }

        public static void GrowList<T>(List<T> list, int addBy, int growBy)
        {
            int count = list.Count;
            int capacity = list.Capacity;
            if (capacity >= count + addBy)
                return;

            if (growBy == 0)
            {
                growBy = count / 8;
                growBy = (growBy < 4) ? 4 : (growBy > 1024) ? 1024 : growBy;
            }

            if (count + addBy < capacity + growBy)
                list.Capacity = capacity + growBy;
            else
                list.Capacity = count + addBy;
        }

        internal static void ArrayAdd<T>(ref T[] array, T item)
        {
            if (array == null || array.Length == 0)
                array = new T[] { item };
            else
            {
                Array.Resize<T>(ref array, array.Length + 1);
                array[array.Length - 1] = item;
            }
        }

        internal static bool ArrayContains<T>(T[] array, T item)
        {
            if (array == null || array.Length == 0)
                return false;

            for (int i = 0; i < array.Length; i++)
                if (object.Equals(array[i], item))
                    return true;
            return false;
        }

        internal static void ArrayRemoveAt<T>(ref T[] array, int index, T[] empty)
        {
            if (array == null || array.Length == 0)
                return;
            if (index < 0 || array.Length <= index)
                return;

            if (array.Length == 1)
                array = empty;
            else
            {
                T[] newArray = new T[array.Length - 1];
                if (index > 0)
                    Array.Copy(array, 0, newArray, 0, index);

                if (index < array.Length - 1)
                    Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);

                array = newArray;
            }
        }

        internal static void ArrayRemove<T>(ref T[] array, T item, T[] empty)
        {
            if (array == null || array.Length == 0)
                return;

            for (int i = 0; i < array.Length; i++)
            {
                if (object.Equals(array[i], item))
                {
                    ArrayRemoveAt(ref array, i, empty);
                    break;
                }
            }
        }

        internal static void ArrayAddRange<T>(ref T[] array, ICollection<T> items)
        {
            if (items == null)
                return;

            int cnt = array == null ? 0 : array.Length;
            if (cnt == 0)
                array = new T[items.Count];
            else
                Array.Resize<T>(ref array, cnt + items.Count);

            int i = cnt;
            foreach (T item in items)
                array[i++] = item;
        }

        internal static void AddSorted<T>(List<T> list, IComparer<T> comparer, T item)
        {
            int idx = list.BinarySearch(item, comparer);
            if (idx < 0)
                idx = ~idx;
            list.Insert(idx, item);
        }
        #endregion        
    
        internal static bool IsIncludeInRange(double qty, double min, double max)
        {
            if (qty > min && qty < max)
                return true;

            return false;
        }

        internal static List<LimitType> ParseLimitType(string limitTypeStr)
        {
            List<LimitType> list = new List<LimitType>();

            if (string.IsNullOrEmpty(limitTypeStr))
                return list;

            var arr = limitTypeStr.ToCharArray();
            foreach (var c in arr)
            {
                var limitType = LcdHelper.ToEnum(c.ToString(), LimitType.NONE);
                if (limitType == LimitType.NONE)
                    continue;

                list.Add(limitType);
            }

            return list;
        }

        internal static bool IsUnpackWipStep(string shopID, string stepID)
        {
            if (LcdHelper.Equals(shopID, "ARRAY") && stepID == "1100")
                return true;

            if (LcdHelper.Equals(shopID, "CF") && stepID == "0100")
                return true;

            return false;
        }

        internal static bool IsUnpackTargetStep(string shopID, string stepID)
        {
            if (LcdHelper.Equals(shopID, "ARRAY") && stepID == "1200")
                return true;

            if (LcdHelper.Equals(shopID, "CF") && stepID == "1300")
                return true;

            return false;
        }
    }
}
