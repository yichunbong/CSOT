using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using Mozart.SeePlan;
using Mozart.SeePlan.DataModel;
using System.Text;
using System.Data;
using System.Threading;
using System.Diagnostics;

namespace CSOT.RTS.Inbound
{
    [FeatureBind()]
    public static partial class LcdHelper
    {
        public const string DEFAULT_VERSION_TYPE = "RTS-t6";        
        private const string ARRAY = "ARRAY";
        private const string CF = "CF";
        private const string CELL = "CELL";

        private const int BASE_MINUTE = 30;
        private const string STEP_STB1 = "0000";

        private const string DEFAULT_PRODUCT_VERSION_CF = "00001";
        private const string DEFAULT_PRODUCT_VERSION_CELL = "00001";

        private const string PHOTO_EQP_GROUP_PATTERN = "PHL";

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

        public static string IdentityNull()
        {
            return Mozart.SeePlan.StringUtility.IdentityNull;
        }

        public static bool IsEmptyID(string text)
        {
            return Mozart.SeePlan.StringUtility.IsEmptyID(text);
        }

        public static string ToSafeString(string text, string defaultValue = null)
        {
            if (IsEmptyID(text))
            {
                if (defaultValue == null)
                    defaultValue = string.Empty;
                return defaultValue;
            }

            return text;
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

        public static DateTime? DbDateTime(DateTime date)
        {
            if (date == DateTime.MinValue || date == DateTime.MaxValue)
                return null;

            return date;
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

        public static string ToUpper(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text.ToUpper();
        }
  
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

        public static string SafeSubstring(string text, int startIndex, int length)
        {
            if (text == null)
                return null;
                        
            return text.SafeSubstring(startIndex, length);
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

        public static bool ToBool(string s)
        {
            return ToBool(s, false);
        }

        public static bool ToBool(string s, bool defaultValue)
        {
            try
            {
                bool value;
                if (bool.TryParse(s, out value))
                    return value;

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
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
                if (sValue == null) sValue = str;
                else sValue += '@' + str;
            }
            return sValue;
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

            value = value.Trim();
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

        #endregion

        public static List<string> ToListString(string s)
        {
            char[] sep = ",".ToCharArray();
            var result = Mozart.Text.StringUtility.SplitToList(s, sep);
            return new List<string>(result);
        }

        public static string ToString(string[] keys)
        {
            StringBuilder s = new StringBuilder();
            foreach (var key in keys)
                s.Append(key);

            return s.ToString();
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
            if(context.QueryArgs.TryGetValue("VERSION_DATE", out versionDay))
            {
                if(string.IsNullOrEmpty(versionDay) == false)
                    versionDate = LcdHelper.DbToDateTime(versionDay);
            }

            return versionDate;
        }

        public static DateTime GetInterfaceTime_Default()
        {
            var context = ModelContext.Current;
            DateTime now = context.StartTime;

            return now;
        }

        public static DateTime GetActFixedDate_Default()
        {
            var context = ModelContext.Current;
            DateTime now = context.StartTime;

            int baseMinute = LcdHelper.BASE_MINUTE;
            DateTime nowRptDate = GetRptDate_1Hour(now, baseMinute);

            //到2小时前
            //2시간 전까지 확정
            DateTime actFixedDate = nowRptDate.AddHours(-2);
            return actFixedDate;
        }

        public static DateTime GetRptDate_1Hour(DateTime t, int baseMinute)
        {
            // 1H为单位
            //1시간 단위
            int baseHours = 1;

            //ex) HH:30:00
            DateTime rptDate = LcdHelper.Trim(t, "HH").AddMinutes(baseMinute);

            //超过baseMinute(ex.30min)时，后续时间段的baseMinute的Actual
            //baseMinute(ex.30분) 이상인 경우 이후 시간대 baseMinute의 실적
            //07:30 = 06:30(超过) ~ 07:30(以下)时， 06:40 --> 07:30, 07:30 --> 07:30, 07:40 --> 08:30
            //07:30 = 06:30(초과) ~ 07:30(이하)인경우, 06:40 --> 07:30, 07:30 --> 07:30, 07:40 --> 08:30
            if (t.Minute > baseMinute)
            {
                rptDate = rptDate.AddHours(baseHours);
            }

            return rptDate;
        }

        public static InboudRunType GetRunType()
        {
            string runTypeStr = GlobalParameters.Instance.RunType;
            var runType = LcdHelper.ToEnum(runTypeStr, InboudRunType.NONE);

            return runType;
        }

        public static bool IsNullOrEmpty(DateTime t)
        {
            if (t == DateTime.MinValue || t == DateTime.MaxValue)
                return true;

            return false;
        }

        public static bool IsNullOrEmpty_AnyOne(params string[] arr)
        {
            if (arr == null || arr.Count() == 0)
                return true;

            foreach (var text in arr)
            {
                if (string.IsNullOrEmpty(text))
                    return true;
            }

            return false;
        }

        public static bool IsNullOrEmpty_All(params string[] arr)
        {
            if (arr == null || arr.Count() == 0)
                return true;

            foreach (var text in arr)
            {
                if (string.IsNullOrEmpty(text) == false)
                    return false;
            }

            return true;
        }
                
        public static InOutType GetInOutType(string shopID, string stepID)
        {
            if(LcdHelper.Equals(shopID, LcdHelper.ARRAY))
            {
                if (LcdHelper.Equals(stepID, "1100"))
                    return InOutType.IN;
                else if (LcdHelper.Equals(stepID, "9900"))
                    return InOutType.OUT;
            }
            else if (LcdHelper.Equals(shopID, LcdHelper.CF))
            {
                if (LcdHelper.Equals(stepID, "0100"))
                    return InOutType.IN;
                else if (LcdHelper.Equals(stepID, "9990"))
                    return InOutType.OUT;
            }
            else if (LcdHelper.Equals(shopID, LcdHelper.CELL))
            {
                if (LcdHelper.Equals(stepID, "2100"))
                    return InOutType.IN;
                else if (LcdHelper.Equals(stepID, "2300"))
                    return InOutType.OUT;
            }

            return InOutType.NONE;
        }

        public static bool IsChamberType(string simType)
        {
            var chamberType = GetChamberType(simType);

            return IsChamberType(chamberType);
        }

        public static bool IsChamberType(ChamberType chamberType)
        {
            if (chamberType == ChamberType.Chamber || chamberType == ChamberType.ParallelChamber)
                return true;

            return false;
        }

        public static ChamberType GetChamberType(string simType)
        {
            return LcdHelper.ToEnum(simType, ChamberType.NONE);
        }

        public static string GetShopIDByProductID(string productID, string defaultShopID)
        {
            if (string.IsNullOrEmpty(productID))
                return defaultShopID;

            if (productID.StartsWith("F", StringComparison.InvariantCultureIgnoreCase))
                return LcdHelper.CF;
            else
                return LcdHelper.ARRAY;
        }

        public static bool IsArrayShop(string shopID)
        {
            return LcdHelper.Equals(shopID, LcdHelper.ARRAY);
        }

        public static bool IsCfShop(string shopID)
        {
            return LcdHelper.Equals(shopID, LcdHelper.CF);
        }

        public static bool IsCellShop(string shopID)
        {
            return LcdHelper.Equals(shopID, LcdHelper.CELL);
        }

        public static bool IsSTB1(string stepID)
        {
            return LcdHelper.Equals(stepID, LcdHelper.STEP_STB1);
        }

        public static bool IsPhotoEqpGroup(string eqpGroup)
        {
            if (string.IsNullOrEmpty(eqpGroup))
                return false;

            string key = ConfigHelper.PHOTO_EQP_GROUP_PATTERN;
            string defaultValue = PHOTO_EQP_GROUP_PATTERN;

            string pattern = ConfigHelper.GetCodeMap_InboundCodeMap(key, defaultValue);

            return LcdHelper.ToUpper(eqpGroup).Contains(pattern);
        }
        
        public static string GetDefaultProductVersion(string shopID)
        {
            string key = ConfigHelper.DEFAULT_PRODUCT_VERSION;

            string defaultValue = string.Empty;

            if (shopID == CF)
                defaultValue = DEFAULT_PRODUCT_VERSION_CF;
            else if (shopID == CELL)
                defaultValue = DEFAULT_PRODUCT_VERSION_CELL;

            return ConfigHelper.GetCodeMap_InboundCodeMap(key, defaultValue);
        }
    }
}
