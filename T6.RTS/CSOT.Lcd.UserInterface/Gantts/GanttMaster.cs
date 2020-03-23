using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

using DevExpress.Utils;
using DevExpress.XtraGrid;
using DevExpress.XtraEditors;
using DevExpress.XtraSpreadsheet;

using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.Analysis;
using CSOT.Lcd.UserInterface.DataMappings;

using Mozart.Collections;
using CSOT.UserInterface.Utils;
using DevExpress.XtraEditors.Controls;
using CSOT.Lcd.Scheduling.Outputs;

namespace CSOT.Lcd.UserInterface.Gantts
{
    public enum GanttType
    {
        Default,
        SimGantt        
    }

    public enum MouseSelectType
    {
        Product,
        PB,
        PO,
        POB,
        Pattern
    }

    public enum MesEqpStatus
    {
        PM,
        DOWN,
        OFF,
        RUN,
        E_RUN,
        IDLE,
        Set_Up,
        W_CST,
        NONE,
    }

    public class GanttMaster : GanttView
    {   
        private IExperimentResultItem _result;
        private ResultDataContext _resultCtx;
             
        private bool SelectMode { get; set; }
        //private ColorGenerator ColorGen { get; set; }
        private BrushInfo BrushEmpth = new BrushInfo(Color.Transparent);
        private Dictionary<string, Color> ProdColors { get; set; }

        public MouseSelectType MouseSelType { get; set; }
                
        protected DateTime _planStartTime;

        private Dictionary<string, int> _jobChgCntByHour;
        private Dictionary<string, int> _jobChgCntByShift;
        private HashSet<string> _visibleItems;

        protected EqpMaster EqpMgr { get; set; }
        protected DoubleDictionary<string, DateTime, DataRow> DispInfos { get; set; }
        protected Dictionary<string, EqpGanttChartData.StdStep> StdSteps { get; set; }
        protected Dictionary<string, EqpMaster.Eqp> ValidEqps { get; set; }

        //protected Dictionary<string, List<Tuple<string, string>>> EqpStatusDic { get; set; }       

        public bool IsProductInBarTitle { get; private set; }

        public string TargetShopID { get; set; }
        public IList<string> EqpGroups { get; set; }
                
        public GanttType GanttType { get; set; }

        new public bool EnableSelect
        {
            get { return this.SelectMode; }
        }       

        private bool IsUsedProdColor { get; set; }

        public GanttMaster(
            SpreadsheetControl grid,
            IExperimentResultItem result,
            ResultDataContext resultCtx,                  
            DateTime planStartTime,
            GanttType type,
            EqpMaster eqpMgr
        )
            : base (grid)
        {
            _result = result;
            _resultCtx = resultCtx;

            _planStartTime = planStartTime;

            this.GanttType = type;
            this.EqpMgr = eqpMgr;

            this.SelectMode = false;
            //this.ColorGen = new ColorGenerator();

            _visibleItems = new HashSet<string>();

            _jobChgCntByHour = new Dictionary<string, int>();
            _jobChgCntByShift = new Dictionary<string, int>();
            
            this.ValidEqps = new Dictionary<string, EqpMaster.Eqp>();
            this.StdSteps = new Dictionary<string,EqpGanttChartData.StdStep>();

            this.DispInfos = new DoubleDictionary<string, DateTime, DataRow>();                        
        }

        #region GanttOption

        public void TurnOnSelectMode() { this.SelectMode = true; }

        public void TurnOffSelectMode() { this.SelectMode = false; }

        public BrushInfo GetBrushInfo(GanttBar bar, string patternOfProdID)
        {
            BrushInfo brushinfo = null;

            if (bar.State == EqpState.SETUP)
            {
                brushinfo = new BrushInfo(Color.Red);
            }
            else if (bar.State == EqpState.IDLE || bar.State == EqpState.IDLERUN)
            {
                brushinfo = new BrushInfo(Color.White);
            }
            else if (bar.State == EqpState.PM)
            {
                brushinfo = new BrushInfo(Color.Black);
            }
            else if (bar.State == EqpState.DOWN)
            {
                brushinfo = new BrushInfo(HatchStyle.Percent30, Color.Gray, Color.Black);
            }
            else if (bar.IsGhostBar)
            {
                    brushinfo = new BrushInfo(HatchStyle.Percent30, Color.LightGray, Color.White);
            }
            else
            {
                var color = GetBarColorByProductID(bar.ProductID);

                if (bar.WipInitRun == "Y")
                    brushinfo = new BrushInfo(HatchStyle.Percent30, Color.Black, color);
                else
                    brushinfo = new BrushInfo(color);
            }
            
            var selBar = this.SelectedBar;

            if (!this.EnableSelect || selBar == null)
            {
                bar.BackColor = brushinfo.BackColor;
                return brushinfo;
            }

            if (!CompareToSelectedBar(bar, patternOfProdID))
            {
                bar.BackColor = this.BrushEmpth.BackColor;

                return this.BrushEmpth;
            }

            bar.BackColor = brushinfo.BackColor;
            return brushinfo;
        }

        private Color GetBarColorByProductID(string productID)
        {
            Color color = this.BrushEmpth.BackColor;
            if (string.IsNullOrEmpty(productID))
                return color;

            if (this.IsUsedProdColor)
            {
                Color prodColor;
                if (this.ProdColors.TryGetValue(productID, out prodColor))
                    color = prodColor;
            }
            else
            {
                return ColorGenHelper.GetColorByKey(productID, ColorGenHelper.CATETORY_PRODUCT);
            }

            return color;
        }

        public bool CompareToSelectedBar(GanttBar bar, string patternOfProdID)
        {
            if (bar.IsGhostBar)
                return true;

            if (bar.State == EqpState.PM || bar.State == EqpState.DOWN)
                return true;

            if (this.MouseSelType == MouseSelectType.Pattern)
            {
                if (string.IsNullOrEmpty(patternOfProdID))
                    return true;

                bool isLike = StringHelper.Like(bar.ProductID, patternOfProdID);

                return isLike;
            }

            var selBar = this.SelectedBar as GanttBar;

            if (selBar == null)
                return true;

            if (this.MouseSelType == MouseSelectType.Product)
            {
                return selBar.ProductID == bar.ProductID;
            }
            else if (this.MouseSelType == MouseSelectType.PB)
            {
                return selBar.ProductID + selBar.ProductVersion == bar.ProductID + bar.ProductVersion;
            }
            else if (this.MouseSelType == MouseSelectType.PO)
            {
                return selBar.ProductID + selBar.StepID == bar.ProductID + bar.StepID;
            }
            else if (this.MouseSelType == MouseSelectType.POB)
            {
                return selBar.ProductID + selBar.StepID + selBar.ProductVersion
                    == bar.ProductID + bar.StepID + bar.ProductVersion;
            }

            return false;
        }

        #endregion

        #region Initialize

        protected virtual void ClearData()
        {
            this.Clear();
           
            _visibleItems.Clear();
            _jobChgCntByHour.Clear();
            _jobChgCntByShift.Clear();            
        }

        public void PrepareData(bool isProductInBarTitle, bool isUsedProdColor)
        {
            this.IsProductInBarTitle = isProductInBarTitle;
            this.IsUsedProdColor = isUsedProdColor;

            PrepareData_StdStep();
            PrepareDispatchingData();
        }

        private void PrepareData_StdStep()
        {
            if (this.StdSteps == null)
                this.StdSteps = new Dictionary<string, EqpGanttChartData.StdStep>();

            var modelContext = _result.GetCtx<ModelDataContext>();

            foreach (var item in modelContext.StdStep)
            {
                EqpGanttChartData.StdStep info = new EqpGanttChartData.StdStep(item.SHOP_ID,
                                                                               item.STEP_ID,
                                                                               item.LAYER_ID,
                                                                               item.STEP_SEQ);
                string key = info.GetKey();

                if (this.StdSteps.ContainsKey(key) == false)
                    this.StdSteps[key] = info;
            }
        }

        private void PrepareDispatchingData()
        {
            string filter = GetFilterString_EqpGroup();
            var dispatchDt = _result.LoadOutput(EqpGanttChartData.EQP_DISPATCH_LOG_TABLE_NAME, filter);
            if (dispatchDt == null)
                return;

            var infos = this.DispInfos;

            foreach (DataRow row in dispatchDt.Rows)
            {
                string eqpID = row.GetString(EqpGanttChartData.EqpDispatchLog.Schema.EQP_ID);
                string subEqpID = row.GetString(EqpGanttChartData.EqpDispatchLog.Schema.SUB_EQP_ID);

                string eqpKey = CommonHelper.CreateKey(eqpID, subEqpID);
                
                string timeTemp = row.GetString(EqpGanttChartData.EqpDispatchLog.Schema.DISPATCH_TIME);                

                if (string.IsNullOrEmpty(timeTemp))
                    continue;

                DateTime dispatchingTime = timeTemp.DbToDateTime();

                Dictionary<DateTime, DataRow> dic;
                if (infos.TryGetValue(eqpKey, out dic) == false)
                    infos[eqpKey] = dic = new Dictionary<DateTime, DataRow>();

                DataRow info;
                if (dic.TryGetValue(dispatchingTime, out info) == false)
                    dic[dispatchingTime] = info = row;
            }
        }

        private string GetFilterString_EqpGroup()
        {
            string filter = string.Empty;

            //if (this.EqpGroups.Count == 0)
            //    filter = "1 = 1";
            //else
            //{
            //    filter += "EQP_GROUP" + " IN (";

            //    for (int i = 0; i < this.EqpGroups.Count - 1; i++)
            //    {
            //        filter += "'" + this.EqpGroups[i] + "', ";
            //    }
            //    filter += "'" + this.EqpGroups[this.EqpGroups.Count - 1] + "')";

            //}

            return filter;
        }

        #endregion

        #region Bind Controls

        public void BindChkListEqpGroup(CheckedListBoxItemCollection control, string targetShopID)
        {
            control.Clear();

            SortedSet<string> list = new SortedSet<string>();

            var eqpDt = this.EqpMgr.Table;

            string eqpGrpColName = EqpGanttChartData.Eqp.Schema.EQP_GROUP_ID;
            DataRow[] filteredRows = eqpDt.Select("", eqpGrpColName);

            foreach (DataRow srow in eqpDt.Rows)
            {
                string eqpGroup = srow.GetString(eqpGrpColName);                                
                if (string.IsNullOrEmpty(eqpGroup)) 
                    continue;

                string shopID = srow.GetString(EqpGanttChartData.Eqp.Schema.SHOP_ID);
                if (targetShopID != Consts.ALL)
                {                    
                    bool isAdd = false;

                    //ARRRAY의 경우 CF PHOTO 설비는 포함 처리.                    
                    if(StringHelper.Equals(targetShopID, "ARRAY"))
                    {
                        string eqpGroupUpper = StringHelper.ToSafeUpper(eqpGroup);
                        if (eqpGroupUpper.Contains("PHL") || eqpGroupUpper.Contains("PHOTO"))
                            isAdd = true;
                    }

                    if (isAdd == false)
                        isAdd = shopID == targetShopID;

                    if (isAdd == false)
                        continue;                        
                }

                if (list.Contains(eqpGroup) == false)
                    list.Add(eqpGroup);
            }

            foreach (var eqpGroup in list)
                control.Add(eqpGroup);
        }

        #endregion
        
        #region Job Change

        public void AddJobChange(DateTime startTime)
        {
            string chgTime = startTime.ToString(this.DateKeyPattern);
            string shiftTime = ShopCalendar.ShiftStartTimeOfDayT(startTime).ToString(this.DateGroupPattern);

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
            
            return string.Format("{0}", hourString);
        }

        public string GetJobChgShiftCntFormat(DateTime shiftTime)
        {
            string shift = shiftTime.ToString(this.DateGroupPattern);

            return string.Format("{0}", shift);
        }

        #endregion
                
        #region BindData
        
#if false //LoadHistory
        protected DataView GetPlanData()        
        {
            var loadHistDt = _result.LoadOutput(EqpGanttChartData.LOAD_HIST_TABLE_NAME);  

            string targetShopID = this.TargetShopID;

            string filter = string.Empty;
            if (targetShopID.Equals(Consts.ALL) == false)
            {
                string str = StringHelper.Equals(targetShopID, "ARRAY") ? "({0} = '{1}' OR {0} = 'CF')" : "({0} = '{1}')";
                filter = string.Format(str, EqpGanttChartData.LoadingHistory.Schema.SHOP_ID, targetShopID);
            }

            string eqpGrpFilter = string.Empty;
            if (this.EqpGroups != null)
            {
                foreach (string eqpGrpID in this.EqpGroups)
                {
                    eqpGrpFilter = eqpGrpFilter == string.Empty ?
                        string.Format("{0} = '{1}'", EqpGanttChartData.LoadingHistory.Schema.EQP_GROUP_ID, eqpGrpID)
                        : eqpGrpFilter = eqpGrpFilter + string.Format(" OR {0} = '{1}'"
                            , EqpGanttChartData.LoadingHistory.Schema.EQP_GROUP_ID, eqpGrpID);
                }
            }

            eqpGrpFilter = eqpGrpFilter == string.Empty ? eqpGrpFilter : filter + " AND (" + eqpGrpFilter + ")";
            DataTable dtEqp = _result.LoadInput(EqpGanttChartData.EQP_TABLE_NAME, eqpGrpFilter);

            string eqpFilter = string.Empty;
            if (dtEqp != null)
            {
                foreach (DataRow drow in dtEqp.Rows)
                {
                    string eqpID = drow.GetString("EQP_ID");
                    eqpFilter = eqpFilter == string.Empty ?
                        string.Format("{0} = '{1}'", EqpGanttChartData.LoadingHistory.Schema.EQP_ID, eqpID)
                        : eqpFilter = eqpFilter + string.Format(" OR {0} = '{1}'", EqpGanttChartData.LoadingHistory.Schema.EQP_ID, eqpID);
                }
            }

            if (string.IsNullOrEmpty(eqpFilter))
                eqpFilter = string.Format("{0} = 'NULL'", EqpGanttChartData.LoadingHistory.Schema.EQP_ID);

            filter = filter + " AND (" + eqpFilter + ")";

            string sorter = string.Format("{0},{1}", ColName.EqpID, ColName.TargetDate);                    

            DataView dv = new DataView(loadHistDt, filter, sorter, DataViewRowState.CurrentRows);

            return dv;
        }

        protected void TryGetEndTime(
            string[] data,
            DateTime targetDate,
            DateTime startTime,
            string currentStep,
            string currentTime,
            int currentIndex,
            ref DateTime endTime)
        {
            for (int k = currentIndex; k < data.Length - 1; k++)
            {
                var loadInfo = PackedTable.Split<SchedOut.LoadInfo>(data[k + 1]);

                if (loadInfo.StartTime == currentTime)
                {
                    continue;
                }

                DateTime dt = SchedOut.LHStateTime(targetDate, loadInfo.StartTime);
                if (startTime < dt)
                {
                    endTime = dt;
                    break;
                }
            }
        }

#else
        protected List<EqpPlan> GetPlanData(string targetEqpPattern)
        {
            var eqpPlan = _resultCtx.EqpPlan;
                        
            var finds = eqpPlan.Where(t => IsMatchedEqpPlan(t, this.FromTime, this.ToTime, this.EqpGroups, targetEqpPattern));
            
            var list = finds.ToList();
            list.Sort(ComparerEqpPlan);

            return list;
        }

        private bool IsMatchedEqpPlan(EqpPlan info, DateTime fromTime, DateTime toTime, IList<string> eqpGroupList, string targetEqpPattern)
        {
            if (info == null)
                return false;

            if (info.START_TIME >= toTime)
                return false;

            if (info.END_TIME <= fromTime)
                return false;

            if(eqpGroupList != null)
            {
                if (eqpGroupList.Contains(info.EQP_GROUP_ID) == false)
                    return false;
            }

            if (string.IsNullOrEmpty(targetEqpPattern) == false)
            {
                if (StringHelper.Like(info.EQP_ID, targetEqpPattern) == false)
                    return false;
            }

            return true;
        }

        public static int ComparerEqpPlan(EqpPlan x, EqpPlan y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = string.Compare(x.EQP_ID, y.EQP_ID);

            if (cmp == 0)
            {
                DateTime x_START_TIME = x.START_TIME.GetValueOrDefault();
                DateTime y_START_TIME = y.START_TIME.GetValueOrDefault();

                cmp = DateTime.Compare(x_START_TIME, y_START_TIME);
            }

            return cmp;
        }

#endif

        protected List<string> GetRunWipList(string targetShopID)
        {
            List<string> runWipList = new List<string>();

            string filter = string.Empty;
            if (targetShopID.Equals(Consts.ALL) == false)
            {
                string str = StringHelper.Equals(targetShopID, "ARRAY") ? "({0} = '{1}' OR {0} = 'CF')" : "({0} = '{1}')";
                filter = string.Format(str, EqpGanttChartData.LoadingHistory.Schema.SHOP_ID, targetShopID);
            }

            filter = filter == string.Empty ? string.Format("({0} = '{1}')", "LOT_STATUS", "RUN")
                : filter + string.Format(" AND ({0} = '{1}')", "LOT_STATUS", "RUN");

            DataTable dtWipRun = _result.LoadInput(EqpGanttChartData.WIP_TABLE_NAME, filter);
            if (dtWipRun != null)
            {
                foreach (DataRow drow in dtWipRun.Rows)
                {
                    string lotID = drow.GetString("LOT_ID");
                    runWipList.Add(lotID);
                }
            }

            return runWipList;
        }      

        protected string GetLayer(string shopID, string stepID)
        {
            string key = EqpGanttChartData.StdStep.CreateKey(shopID, stepID);

            EqpGanttChartData.StdStep info;
            if (this.StdSteps.TryGetValue(key, out info))
            {                
                return string.Format("{0} ({1})", info.StdStepID, info.Layer ?? string.Empty);
            }

            return string.Empty;
        }

        //protected void SetEqpStatuDic()
        //{
        //    var dic = this.EqpStatusDic = new Dictionary<string, List<Tuple<string, string>>>();
        //    var eqpStatus = _result.GetCtx<ModelDataContext>().EqpStatus;

        //    foreach (var item in eqpStatus)
        //    {
        //        string key = item.EQP_ID;
        //        Tuple<string, string> value = new Tuple<string, string>(item.SHOP_ID, item.LAST_STEP_ID);

        //        List<Tuple<string, string>> list = new List<Tuple<string, string>>();
        //        if (dic.ContainsKey(key) == false)
        //            dic.Add(key, list);

        //        list.Add(value);
        //    }
        //}

        //protected List<Tuple<string, string>> GetEqpStatusList(string eqpID)
        //{
        //    var dic = this.EqpStatusDic;

        //    List<Tuple<string, string>> list;
        //    if (dic.TryGetValue(eqpID, out list))
        //        return list;

        //    return null;
        //}

        protected int GetSortSeq(string shopID, string stepID)
        {
            string key = EqpGanttChartData.StdStep.CreateKey(shopID, stepID);

            EqpGanttChartData.StdStep info;
            if (this.StdSteps.TryGetValue(key, out info))
                return info.StepSeq;

            return int.MaxValue;
        }
        
        protected void SetValidEqpIDList(string selectedShopId = null, string eqpPattern = null)
        {
            if (this.ValidEqps.Count > 0)
                this.ValidEqps.Clear();

            Dictionary<string, EqpMaster.Eqp> eqps = EqpMgr.GetEqpsByLine(selectedShopId);
            if (eqps == null)
                return;

            foreach (var eqp in eqps.Values)
            {
                if (string.IsNullOrEmpty(eqpPattern) == false && eqp.EqpID.Contains(eqpPattern) == false)
                    continue;

                if (this.ValidEqps.ContainsKey(eqp.EqpID) == false)
                    this.ValidEqps.Add(eqp.EqpID, eqp);
            }
        }

        public bool TryGetValidEqp(string eqpId, out EqpMaster.Eqp eqp)
        {
            if (this.ValidEqps.TryGetValue(eqpId, out eqp))
                return true;

            return false;
        }

        protected void SetProdColors()
        {
            var dic = this.ProdColors = new Dictionary<string, Color>();

            var modelContext = _result.GetCtx<ModelDataContext>();
            var product = modelContext.Product;
            if (product == null)
                return;           

            foreach (var it in product)
            {
                string key = it.PRODUCT_ID;
                if (string.IsNullOrEmpty(key) || dic.ContainsKey(key))
                    continue;

                int a, r, g, b;
                if (TryGetArgb(it.VIEW_COLOR, out a, out r, out g, out b) == false)
                    continue;

                Color color = Color.FromArgb(a, r, g, b);
                dic.Add(key, color);
            }
        }

        private bool TryGetArgb(string viewColor, out int a, out int r, out int g, out int b)
        {
            a = -1;
            r = -1;
            g = -1;
            b = -1;

            if (string.IsNullOrEmpty(viewColor))
                return false;

            var arr = viewColor.Split(',');

            int count = arr.Length;
            if (count < 4)
                return false;

            if (int.TryParse(arr[0], out a) == false)
                return false;

            if (int.TryParse(arr[1], out r) == false)
                return false;

            if (int.TryParse(arr[2], out g) == false)
                return false;

            if (int.TryParse(arr[3], out b) == false)
                return false;


            return true;
        }

        #endregion

        #region Inner Class

        private class DownItemInfo
        {
            public string EqpID;
            public string EqpGroup;
            public DateTime LoadTime;
            public DateTime EndTime;
            public DateTime StateEndTime;
            public EqpState State;
            public DateTime ToTime;
            public EqpMaster.Eqp Eqp;

            public DownItemInfo(string eqpID, string eqpGroup, DateTime loadTime, DateTime endTime, DateTime stateEndTime, EqpState state, DateTime toTime, EqpMaster.Eqp eqp)
            {
                this.EqpID = eqpID;
                this.EqpGroup = eqpGroup;
                this.LoadTime = loadTime;
                this.EndTime = endTime;
                this.StateEndTime = stateEndTime;
                this.State = state;
                this.ToTime = toTime;
                this.Eqp = eqp;
            }
        }

        public class CompareMBarList : IComparer<LinkedBarNode>
        {
            public int Compare(LinkedBarNode x, LinkedBarNode y)
            {
                Bar a = x.LinkedBarList[0].BarList[0];
                Bar b = y.LinkedBarList[0].BarList[0];

                int cmp = a.TkinTime.CompareTo(b.TkinTime);

                if (cmp == 0)
                    cmp = y.LinkedBarList.Count.CompareTo(x.LinkedBarList.Count);

                return cmp;
            }
        }

        #endregion
    }   
}
