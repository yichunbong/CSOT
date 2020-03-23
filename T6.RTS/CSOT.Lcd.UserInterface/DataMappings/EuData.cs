using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mozart.Studio.TaskModel.UserLibrary;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    public class EuData
    {
        #region Input Table Name

        public const string DATA_TABLE_1 = "Eqp";

        public const string DATA_TABLE_2 = "LoadStat";

        #endregion



        #region Input Data Transform


        public class Eqp
        {
            public string ShopID;
            public string EqpID;
            public string EqpGroup;
            public string DspEqpGroup;

            public Eqp(DataRow row)
            {
                ShopID = string.Empty;
                EqpID = string.Empty;
                EqpGroup = string.Empty;
                DspEqpGroup = string.Empty;

                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */
            private void ParseRow(DataRow row)
            {
                // EqpID
                ShopID = row.GetString("SHOP_ID");

                // EqpID
                EqpID = row.GetString("EQP_ID");

                // StepGroup
                EqpGroup = row.GetString("EQP_GROUP_ID");

                DspEqpGroup = row.GetString("DSP_EQP_GROUP_ID");
            }
        }


        public class LoadStat
        {
            public string EqpID;
            public DateTime TargetDate;
            public float Setup;
            public float Busy;
            public float Idle;
            public float IdleRun;
            public float PM;
            public float Down;

            public LoadStat(DataRow row)
            {
                EqpID = string.Empty;
                TargetDate = DateTime.MinValue;
                Setup = 0.0f;
                Busy = 0.0f;
                Idle = 0.0f;
                IdleRun = 0.0f;
                PM = 0.0f;
                Down = 0.0f;

                ParseRow(row);
            }

            /***
             * 개발 포인트 
             */
            private void ParseRow(DataRow row)
            {
                // EqpID
                EqpID = row.GetString("EQP_ID");

                // TargetDate - Default :: yyyyMMdd 형태의 string 
                TargetDate = row.GetString("TARGET_DATE").DbToDateTime();

                // Setup
                Setup = row.GetFloat("SETUP");

                // Busy
                Busy = row.GetFloat("BUSY");

                // Idle
                Idle = row.GetFloat("IDLE");

                // IdleRun
                IdleRun = row.GetFloat("IDLERUN");

                // PM
                PM = row.GetFloat("PM");

                // Down
                Down = row.GetFloat("DOWN");
            }
        }


        #endregion



        #region UI 화면 Caption

        public const string FOR_SHOP = "FOR_SHOP";
        public const string FOR_AREA = "FOR_AREA";

        // 메인 UI

        public const string TITLE = "EQP Utilization View";
        public const string QUERY_RANGE = "Date Range";

        public const string CHART_TITLE = "Util. Rate Chart";

        public const string SHOP_ID = "SHOP_ID";
        public const string AREA = "AREA";
        public const string EQP_GROUP_ID = "EQP_GROUP_ID";
        public const string DATE = "DATE";
        public const string UTILIZATION = "UTILIZATION";
        public const string AVG = "AVG";

        // 팝업 UI
        public const string POPUP_EQP_ID = "EQP_ID";
        public const string POPUP_AVG = "AVG";
        public const string POPUP_CHART_TITLE = "Util. Rate Chart : Detail";

        #endregion

        #region Classes
        public class GroupUtilInfo
        {
            private string _shopId;
            private string _area;
            private string _eqpGroup;
            private Dictionary<string, UtilInfo> _infos;
            private List<DateTime> _dates;

            public string ShopId
            {
                get { return _shopId; }
            }

            public string Area
            {
                get { return _area; }
            }

            public string EqpGroup
            {
                get { return _eqpGroup; }
            }

            public Dictionary<string, UtilInfo> Infos
            {
                get { return _infos; }
            }

            public GroupUtilInfo(string shopId, string area, string eqpGroup)
            {
                _shopId = shopId;
                _area = area;
                _eqpGroup = eqpGroup;
                _infos = new Dictionary<string, UtilInfo>();
                _dates = new List<DateTime>();
            }

            public void AddData(string eqpID, DateTime date, float setup, float busy, float idleRun, float idle, float pm, float down, bool isShift)
            {
                var withoutTime = date.Date;

                UtilInfo info;
                if (_infos.TryGetValue(eqpID, out info) == false)
                    _infos[eqpID] = info = new UtilInfo(this, eqpID);

                info.AddData(withoutTime, setup, busy, idleRun, idle, pm, down, isShift);

                if (_dates.Contains(withoutTime) == false)
                    _dates.Add(withoutTime);
            }

            public UtilInfo GetData(string eqpID)
            {
                UtilInfo info;
                if (_infos.TryGetValue(eqpID, out info) == false)
                    return null;

                return info;
            }

            public float GetTotalBusy(DateTime fromDate, DateTime toDate)
            {
                if (_infos.Count == 0)
                    return 0;

                float sum = 0;

                for (DateTime date = fromDate; date < toDate; date = date.AddDays(1))
                {
                    foreach (UtilInfo info in _infos.Values)
                    {
                        UtilData data = info.GetData(date);

                        if (data != null)
                            sum += data.Busy;
                    }
                }

                return sum;
            }

            public float GetAvgBusy(DateTime date)
            {
                if (_infos.Count == 0)
                    return 0;

                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    UtilData data = info.GetData(date);

                    if (data != null)
                        sum += (data.Busy / data.Count);
                }

                float avg = sum / _infos.Count;
                return avg;
            }

            public float GetAvgSetup(DateTime date)
            {
                if (_infos.Count == 0)
                    return 0;

                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    UtilData data = info.GetData(date);

                    if (data != null)
                        sum += (data.Setup / data.Count);
                }

                float avg = sum / _infos.Count;
                return avg;
            }


            public Tuple<float, DateTime> GetMaxBusy(DateTime fromDate, DateTime toDate)
            {
                DateTime maxDate = DateTime.MinValue;
                float max = 0;

                for (DateTime date = fromDate; date < toDate; date = date.AddDays(1))
                {
                    float busy = GetAvgBusy(date);

                    if (busy > max)
                    {
                        maxDate = date;
                        max = busy;
                    }
                }

                return new Tuple<float, DateTime>(max, maxDate);
            }

            public Tuple<float, DateTime> GetMinBusy(DateTime fromDate, DateTime toDate)
            {
                DateTime minDate = DateTime.MinValue;
                float min = float.MaxValue;

                for (DateTime date = fromDate; date < toDate; date = date.AddDays(1))
                {
                    float busy = GetAvgBusy(date);

                    if (busy < min)
                    {
                        minDate = date;
                        min = busy;
                    }
                }

                return new Tuple<float, DateTime>(min, minDate);
            }

            public int GetWorkedDays()
            {
                return _dates.Count;
            }

            public int GetEqpCount()
            {
                return _infos.Count;
            }

            public float GetAvgRun(double days)
            {
                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    float avg = info.GetAvgRun(days);
                    sum += avg;
                }

                return sum / _infos.Count;
            }

            public float GetAvgBusy(double days)
            {
                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    float avg = info.GetAvgBusy(days);
                    sum += avg;
                }

                return sum / _infos.Count;
            }

            public float GetAvgIdleRun(double days)
            {
                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    float avg = info.GetAvgIdleRun(days);
                    sum += avg;
                }

                return sum / _infos.Count;
            }

            public float GetAvgIdleSum(double days)
            {
                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    float avg = info.GetAvgIdleSum(days);
                    sum += avg;
                }

                return sum / _infos.Count;
            }

            public float GetAvgSetup(double days)
            {
                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    float avg = info.GetAvgSetup(days);
                    sum += avg;
                }

                return sum / _infos.Count;
            }

            public float GetAvgPM(double days)
            {
                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    float avg = info.GetAvgPM(days);
                    sum += avg;
                }

                return sum / _infos.Count;
            }

            public float GetAvgDown(double days)
            {
                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    float avg = info.GetAvgDown(days);
                    sum += avg;
                }

                return sum / _infos.Count;
            }

            public float GetAvgIdle(double days)
            {
                float sum = 0;

                foreach (UtilInfo info in _infos.Values)
                {
                    float avg = info.GetAvgIdle(days);
                    sum += avg;
                }

                return sum / _infos.Count;
            }
        }

        public class UtilInfo
        {
            private GroupUtilInfo _group;
            private string _eqpID;
            private Dictionary<DateTime, UtilData> _info;

            public Dictionary<DateTime, UtilData> Info
            {
                get { return _info; }
            }

            public UtilInfo(GroupUtilInfo group, string eqpID)
            {
                _group = group;
                _eqpID = eqpID;
                _info = new Dictionary<DateTime, UtilData>();
            }

            public void AddData(DateTime date, float setup, float busy, float idleRun, float idle, float pm, float down, bool isShift)
            {
                if (idleRun > 0)
                    Console.WriteLine();

                if (isShift)
                {
                    setup /= ShopCalendar.ShiftCount;
                    busy /= ShopCalendar.ShiftCount;
                    idleRun /= ShopCalendar.ShiftCount;
                    idle /= ShopCalendar.ShiftCount;
                }

                UtilData data;
                if (_info.TryGetValue(date, out data) == false)
                    _info[date] = data = new UtilData(setup, busy, idleRun, idle, pm, down);
                else
                    data.Add(setup, busy, idleRun, idle, pm, down);
            }

            public UtilData GetData(DateTime date)
            {
                UtilData data;
                if (_info.TryGetValue(date, out data) == false)
                    return null;

                return data;
            }

            public float GetAvgRun(double days)
            {
                float sum = 0;

                foreach (UtilData data in _info.Values)
                {
                    sum += (data.Busy / data.Count);
                    sum += (data.IdleRun / data.Count);
                }

                return (float)(sum / days);
            }

            public float GetAvgBusy(double days)
            {
                float sum = 0;

                foreach (UtilData data in _info.Values)
                {
                    sum += (data.Busy / data.Count);
                }

                return (float)(sum / days);
            }

            public float GetAvgIdleRun(double days)
            {
                float sum = 0;

                foreach (UtilData data in _info.Values)
                {
                    sum += (data.IdleRun / data.Count);
                }

                return (float)(sum / days);
            }

            public float GetAvgIdleSum(double days)
            {
                float sum = 0;

                foreach (UtilData data in _info.Values)
                {
                    sum += (data.Setup / data.Count);
                    sum += (data.Idle / data.Count);
                    sum += (data.PM / data.Count);
                    sum += (data.Down / data.Count);
                }

                return (float)(sum / days);
            }

            public float GetAvgSetup(double days)
            {
                float sum = 0;

                foreach (UtilData data in _info.Values)
                {
                    sum += (data.Setup / data.Count);
                }

                return (float)(sum / days);
            }

            public float GetAvgPM(double days)
            {
                float sum = 0;

                foreach (UtilData data in _info.Values)
                {
                    sum += (data.PM / data.Count);
                }

                return (float)(sum / days);
            }

            public float GetAvgDown(double days)
            {
                float sum = 0;

                foreach (UtilData data in _info.Values)
                {
                    sum += (data.Down / data.Count);
                }

                return (float)(sum / days);
            }

            public float GetAvgIdle(double days)
            {
                float sum = 0;

                foreach (UtilData data in _info.Values)
                {
                    sum += (data.Idle / data.Count);
                }

                return (float)(sum / days);
            }
        }

        public class UtilData
        {
            private float _setup;
            private float _busy;
            private float _idleRun;
            private float _idle;
            private float _pm;
            private float _down;
            private int _count;

            public float AvgBusy
            {
                get { return _busy / _count; }
            }

            public float AvgSetup
            {
                get { return _setup / _count; }
            }

            public float Setup
            {
                get { return _setup; }
            }

            public float Busy
            {
                get { return _busy; }
            }

            public float IdleRun
            {
                get { return _idleRun; }
            }

            public float Idle
            {
                get { return _idle; }
            }

            public float PM
            {
                get { return _pm; }
            }

            public float Down
            {
                get { return _down; }
            }

            public float Count
            {
                get { return _count; }
            }

            public UtilData(float setup, float busy, float idleRun, float idle, float pm, float down)
            {
                _setup = setup;
                _busy = busy;
                _idleRun = idleRun;
                _idle = idle;
                _pm = pm;
                _down = down;
                _count = 1;
            }

            public void Add(float setup, float busy, float idleRun, float idle, float pm, float down)
            {
                _setup += setup;
                _busy += busy;
                _idleRun += idleRun;
                _idle += idle;
                _pm += pm;
                _down += down;
                _count += 1;
            }
        }
        #endregion Classes
    }
}
