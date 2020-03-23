using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;

using DevExpress.XtraEditors;
using DevExpress.XtraSpreadsheet;       // 추가

using Mozart.Collections;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Analysis;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.LotGantts
{
    public enum GanttType
    {
        Lot
    }

    public enum MouseSelectType
    {
        StepSeq,
        LotId,
        PartNo,
        PkgCode,
        Pattern
    }

    public class GanttMaster : GanttView
    {
        public bool BarTitleGroup { get; set; }

        public MouseSelectType MouseSelType { get; set; }

        public IExperimentResultItem _result;

        protected DateTime _planStartTime;

        HashSet<string> _visibleItems;

        DataTable _dtEqpPlan;

        GanttType _ganttType;

        Dictionary<string, string> _productKindDict;
        ColorGenerator colorGen;
        DoubleDictionary<string, DateTime, DataRow> _dispatchingInfo;
        
        public bool InTimeFlag { get; private set; }


        public GanttMaster(
            SpreadsheetControl grid,
            IExperimentResultItem result,
            DateTime planStartTime,
            GanttType type
        )
            : base(grid)
        {
            _result = result;
            _planStartTime = planStartTime;
            _ganttType = type;
            colorGen = new ColorGenerator();

            _visibleItems = new HashSet<string>();

            _jobChgCntByHour = new Dictionary<string, int>();
            _jobChgCntByShift = new Dictionary<string, int>();

            _dispatchingInfo = new DoubleDictionary<string, DateTime, DataRow>();

            LoadBaseData();
        }

        #region from GanttOption

        BrushInfo brushEmpth = new BrushInfo(Color.Transparent);
        public BrushInfo GetBrushInfo(GanttBar bar)
        {
            BrushInfo brushinfo = new BrushInfo(ColorGenHelper.GetColorByKey(bar.BarKey, ColorGenHelper.CATETORY_PRODUCT));
            var selBar = this.SelectedBar;

            if (!this.EnableSelect || selBar == null)
            {
                bar.BackColor = brushinfo.BackColor;
                return brushinfo;
            }

            if (!CompareToSelectedBar(bar))
            {
                bar.BackColor = brushEmpth.BackColor;
                return brushEmpth;
            }

            bar.BackColor = brushinfo.BackColor;
            return brushinfo;
        }

        public bool CompareToSelectedBar(GanttBar bar)
        {
            var selBar = this.SelectedBar as GanttBar;

            if (this.MouseSelType == MouseSelectType.StepSeq)
                return selBar.BarKey == bar.BarKey;
            else if (this.MouseSelType == MouseSelectType.LotId)
            {
                string selLotid = GetLotId(selBar.LotID);
                string curLotid = GetLotId(bar.LotID);

                return selLotid == curLotid;
            }

            return false;
        }

        private string GetLotId(string lotId)
        {
            int idx = lotId.IndexOf('_');
            if (idx > 0)
                return lotId.Substring(0, idx);

            return lotId;
        }
        #endregion

        #region Initialize

        private void LoadBaseData()
        {
            _dtEqpPlan = _result.LoadOutput(LotGanttChartData.EQP_PLAN_TABLE_NAME);

            _productKindDict = new Dictionary<string, string>();

            DataTable dt = _result.LoadInput(EqpGanttChartData.PRODUCT_TABLE_NAME);

            foreach (DataRow row in dt.Rows)
            {
                LotGanttChartData.Product prod = new LotGanttChartData.Product(row);
                
                string key = prod.LineID + prod.ProductID;
                if (!_productKindDict.Keys.Contains(key))
                    _productKindDict.Add(key, prod.ProductKind);
            }
        }

        public virtual void ClearData()
        {
            this.Clear();

            _visibleItems.Clear();
            _jobChgCntByHour.Clear();
            _jobChgCntByShift.Clear();
        }
        #endregion

        #region GetData

        private DataView GetEqpPlanToDataView(string lineId, DateTime fromTime, DateTime toTime)
        {
            string strFromTime = DateUtility.ToSortableString(fromTime);
            string strToTime = DateUtility.ToSortableString(toTime);

            string filter = string.Empty;
            if (lineId.Equals(Consts.ALL) == false)
                filter = string.Format("{0} = '{1}'", LotGanttChartData.Lot.Schema.SHOP_ID, lineId);

            string sorter = string.Format("{0},{1}", ColName.LotID, ColName.TargetDay);

            DataView dv = new DataView(_dtEqpPlan, filter, sorter, DataViewRowState.CurrentRows);

            return dv;
        }
        #endregion

        #region BindData

        public virtual object TryGetItem(string lotID)
        {
            return null;
        }

        DataView _dvEqpPlan;
        //private DataView GetEqpPlanToDataView()
        //{
        //    if (this._dvEqpPlan == null)
        //    {
        //        string fromDate = DateUtility.ToSortableString(FromTime);
        //        string toDate = DateUtility.ToSortableString(ToTime);

        //        string filter = string.Format("TARGET_DAY >= '{0}' And TARGET_DAY < '{1}'", fromDate, toDate);
                
        //        if (_dtEqpPlan.Rows.Count > 0)
        //            this._dvEqpPlan = new DataView(_dtEqpPlan, filter, "LOT_ID", DataViewRowState.CurrentRows);
        //    }

        //    return this._dvEqpPlan;
        //}

        public void Build(
            string selectedShopId,
            DateTime fromTime,
            DateTime toTime,
            DateTime planStartTime,
            string lotPattern,
            int fromShift,
            bool InTime
        )
        {
            ClearData();

            this.FromTime = fromTime;
            this.ToTime = toTime;

            this.InTimeFlag = InTime;

            //DateTime selectedShiftStartTime = fromTime > planStartTime ? fromTime : planStartTime;

            FillLotMovePlan(selectedShopId, lotPattern, fromTime, toTime);
        }

        public virtual void AddItem
            (
                string lotID,
                string processID,
                string stepSeq,
                string productID,
                DateTime tkin, DateTime tkout,
                int qty,
                EqpState state,
                DateTime toTime
            )
        {
        }

        //private DateTime MergeStateTime(DateTime dt, string time)
        //{
        //    dt = dt.Date + DateUtility.DbToTimeSpan(time);
        //    return ShopCalendar.AdjustSectionDateTime(dt);
        //}

        protected void FillLotMovePlan(string selectedShopId, string lotPattern, DateTime fromTime, DateTime toTime)
        {
            var resultContext = _result.GetCtx<ResultDataContext>();

            IEnumerable<CSOT.Lcd.Scheduling.Outputs.EqpPlan> resultList;
            if (selectedShopId == "ALL")
                resultList = resultContext.EqpPlan;
            else
                resultList = resultContext.EqpPlan.Where(x => x.SHOP_ID == selectedShopId);

            if (string.IsNullOrEmpty(lotPattern) == false)
                resultList = resultList.Where(x => StringHelper.Like(x.LOT_ID, lotPattern) == true);

            resultList = resultList.Where(x => fromTime <= x.START_TIME & x.START_TIME < toTime);

            foreach (var it in resultList)
            {
                //var endTime = it.END_TIME;
                //if (endTime == default(DateTime))
                //    endTime = toTime;

                DateTime startTime = it.START_TIME != null ? (DateTime)it.START_TIME : DateTime.MinValue;
                DateTime endTime = it.END_TIME != null ? (DateTime)it.END_TIME : DateTime.MinValue;

                this.AddItem(it.LOT_ID, it.PROCESS_ID, it.STEP_ID, it.PRODUCT_ID, startTime, endTime, (int)it.UNIT_QTY, EqpState.NONE, toTime);
            }
        }
        #endregion

        #region Job Change

        Dictionary<string, int> _jobChgCntByHour;
        Dictionary<string, int> _jobChgCntByShift;
        public void AddJobChange(DateTime tkinTime)
        {
            string chgTime = tkinTime.ToString(this.DateKeyPattern);
            string shiftTime = ShopCalendar.ShiftStartTimeOfDayT(tkinTime).ToString(this.DateGroupPattern);

            if (_jobChgCntByHour.ContainsKey(chgTime) == false)
                _jobChgCntByHour.Add(chgTime, 0);

            if (_jobChgCntByShift.ContainsKey(shiftTime) == false)
                _jobChgCntByShift.Add(shiftTime, 0);

            _jobChgCntByHour[chgTime]++;
            _jobChgCntByShift[shiftTime]++;
        }

        public string GetJobChgHourCntFormat(DateTime targetTime)
        {
            string hourString = targetTime.Hour.ToString();
            string chgTime = targetTime.ToString(this.DateKeyPattern);

            return string.Format("{0}", hourString);
        }

        public string GetJobChgShiftCntFormat(DateTime shiftTime)
        {
            string shift = shiftTime.ToString(this.DateGroupPattern);
            return string.Format("{0}", shift);
        }

        #endregion
    }


    public class CompareMBarList : IComparer<LinkedBarNode>
    {
        #region IComparer<List<LinkedBar>> 멤버

        public int Compare(LinkedBarNode x, LinkedBarNode y)
        {
            Bar a = x.LinkedBarList[0].BarList[0];
            Bar b = y.LinkedBarList[0].BarList[0];

            int cmp = a.TkinTime.CompareTo(b.TkinTime);

            if (cmp == 0)
                cmp = y.LinkedBarList.Count.CompareTo(x.LinkedBarList.Count);

            return cmp;
        }
        #endregion
    }
}
