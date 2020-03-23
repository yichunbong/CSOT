using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraSpreadsheet;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling;
using Mozart.Text;

namespace CSOT.Lcd.UserInterface.Gantts
{
    class EqpGantt : GanttMaster
    {
        public enum ViewMode
        {
            EQPGROUP,
            EQP,
            LAYER
        }

        public Dictionary<string, GanttInfo> GanttInfos { get; set; }

        public EqpGantt(
            SpreadsheetControl grid,
            IExperimentResultItem result,
            ResultDataContext resultCtx,
            DateTime planStartTime,
            EqpMaster mart
            )
            : base(grid, result, resultCtx, planStartTime, GanttType.Default, mart)
        {
            this.GanttInfos = new Dictionary<string, GanttInfo>();
        }

        public void BuildData_Sim(
            string targetShopID,
            IList<string> eqpGroups,
            string targetEqpPattern,
            DateTime fromTime,
            DateTime toTime,
            ViewMode viewMode,
            bool isFilterDownEqp)
        {
            ClearData();

            this.TargetShopID = targetShopID;
            this.FromTime = fromTime;
            this.ToTime = toTime;

            this.EqpGroups = eqpGroups;

            SetValidEqpIDList();
            //SetEqpStatuDic();
            SetProdColors();

            var planList = GetPlanData(targetEqpPattern);
            FillData_EqpPlan(planList, viewMode, isFilterDownEqp, true);

            //no plan eqp
            var idlePlanList = GetPlanData_OnlyIDLE(planList);
            FillData_EqpPlan(idlePlanList, viewMode, isFilterDownEqp, false);
        }

#if false //LoadHistory 

        //상태종료시간 산출을 위해 RES_CODE별, RES_DATE순서로 수집
        private Dictionary<string, List<DataRow>> CollectionPlanData(DataView dv)
        {
            Dictionary<string, List<DataRow>> items = new Dictionary<string, List<DataRow>>();

            foreach (DataRowView drv in dv)
            {
                var row = drv.Row;
                string key = row.GetString("EQP_ID");

                List<DataRow> list;
                if (items.TryGetValue(key, out list) == false)
                    items.Add(key, list = new List<DataRow>());

                list.Add(drv.Row);
            }

            return items;
        }

        private void FillData_LoadHist(DataView dvPlan, string targetShopID, string targetEqpID, bool showDownEqp)
        {
            //var runWipList = GetRunWipList(targetShopID);

            string factoryStartTime = ShopCalendar.StartTime.ToString().Replace(":", "");
            HashSet<string> sameValueContainer = new HashSet<string>();
            Dictionary<string, List<DataRow>> datas = CollectionPlanData(dvPlan);
                        
            var fromTime = this.FromTime;
            var toTime = this.ToTime;

            foreach (List<DataRow> planDataRows in datas.Values)
            {
                int rowCount = planDataRows.Count;

                for (int j = 0; j < rowCount; j++)
                {
                    DataRow row = planDataRows[j];
                    DataRow nextRow = j < (rowCount - 1) ? planDataRows[j + 1] : null;

                    string eqpID = row.GetString("EQP_ID");

                    EqpMaster.Eqp eqp;
                    if (TryGetValidEqp(eqpID, out eqp) == false)
                    {
                        continue;
                    }

                    DateTime targetDate = DateUtility.DbToDate(row.GetString("TARGET_DATE"));

                    //if (selectedEqpList.Contains(eqpID) == false)
                    //{
                    //    continue;
                    //}

                    string[] data = SchedOut.LoadInfo.DetectNoise(SchedOut.Split(row), targetDate);

                    for (int k = 0; k < data.Length; k++)
                    {
                        var loadInfo = PackedTable.Split<SchedOut.LoadInfo>(data[k]);

                        string strStartTime = loadInfo.StartTime;
                                                
                        string shopID = loadInfo.ShopID;
                        string productID = loadInfo.ProductID;                        
                        string productVersion = loadInfo.ProductVersion;
                        string ownerType = loadInfo.OwnerType;

                        string processID = loadInfo.ProcessID;
                        string stepID = loadInfo.StepID;
                        string stdStep = stepID;

                        string toolID = loadInfo.ProcessID;                        

                        string lotID = loadInfo.LotID;
                        EqpState state = Enums.ParseEqpState(loadInfo.State);

                        if (string.IsNullOrEmpty(targetEqpID) == false && LikeUtility.Like(eqpID, targetEqpID) == false)
                            continue;

                        //상태시작시간                        
                        DateTime startTime = SchedOut.LHStateTime(targetDate, loadInfo.StartTime);
                        if (startTime >= toTime)
                            continue;

                        int qty = CommonHelper.ToInt32(loadInfo.Qty);
                        if (state == EqpState.SETUP)
                            qty = 0;

                        //현재데이터값
                        string nowValue = eqpID + ";" + state + ";" + processID + ";" + stepID + ";" + lotID + ";" + qty.ToString();
                        if (loadInfo.StartTime.CompareTo(factoryStartTime) == 0 && sameValueContainer.Contains(nowValue))
                            qty = 0;

                        sameValueContainer.Add(nowValue);

                        //상태종료시간
                        DateTime endTime = toTime;

                        //이전 OutTime에 현재 InTime 넣어주기
                        TryGetEndTime(data, targetDate, startTime, stepID, loadInfo.StartTime, k, ref endTime);

                        //IDLE과 IDLERUN을 없애기 위한 Merge
                        if (k < data.Length - 1)
                        {
                            var nextLoadInfo = PackedTable.Split<SchedOut.LoadInfo>(data[k + 1]);
                            endTime = SchedOut.LHStateTime(targetDate, nextLoadInfo.StartTime);
                        }
                        else if (nextRow != null)
                        {
                            string nextEqpID = nextRow.GetString("EQP_ID");
                            DateTime nextTargetDate = DateUtility.DbToDate(nextRow.GetString("TARGET_DATE"));

                            string[] nextDataTemp = PackedTable.Split(nextRow);
                            string[] nextData = SchedOut.LoadInfo.DetectNoise(nextDataTemp, nextTargetDate);

                            if (nextData.Length > 0)
                            {
                                var nextLoadInfo = PackedTable.Split<SchedOut.LoadInfo>(nextData[0]);

                                //공장가동 시간(06시)의 전날 자료 무시(이전 데이터와 동일 데이터)
                                if (nextLoadInfo.StartTime.CompareTo(factoryStartTime) == 0)
                                {
                                    string nextValue = nextEqpID + ";" + nextLoadInfo.State + ";" + nextLoadInfo.ProcessID + ";" + nextLoadInfo.StepID + ";" + nextLoadInfo.LotID + ";" + nextLoadInfo.Qty.ToString();

                                    if (sameValueContainer.Contains(nextValue) && nextData.Length > 1)
                                        nextLoadInfo = PackedTable.Split<SchedOut.LoadInfo>(nextData[1]);
                                }

                                string endDate2 = DateHelper.DateToString(nextTargetDate) + nextLoadInfo.StartTime;
                                endTime = ShopCalendar.AdjustSectionDateTime(DateHelper.StringToDateTime(endDate2));
                            }
                        }

                        if (endTime <= fromTime)
                            continue;

                        //ProcessedTime is zoro
                        if (startTime >= endTime)
                            continue;

                        //IDLE, IDLERUN
                        if ((state == EqpState.IDLE || state == EqpState.IDLERUN))
                            continue;

                        string layer = GetLayer(shopID, stepID);
                        string origLotID = lotID;

                                                
                        DataRow dispatchingInfo = FindDispInfo(eqpID, startTime);

                        AddItem(eqpID, 
                                layer, 
                                lotID, 
                                origLotID, 
                                productID, 
                                productVersion, 
                                ownerType, 
                                processID, 
                                stepID,
                                toolID,
                                startTime, 
                                endTime, 
                                qty,
                                state,
                                eqp, 
                                dispatchingInfo);

                        //AddVisibleItem(eqpID);
                    }
                }
            }
        }

#else

        private void FillData_EqpPlan(
            List<EqpPlan> planList,
            ViewMode viewMode,
            bool isFilterDownEqp,
            bool isFilterIDLE)
        {
            if (planList == null)
                return;
            
            var stepInfos = GetPlanStepList(planList);

            var fromTime = this.FromTime;
            var toTime = this.ToTime;

            foreach (var item in planList)
            {
                EqpState state = Enums.ParseEqpState(item.EQP_STATUS);
                if (IsMatched_Plan(item,  state, isFilterIDLE) == false)
                    continue;

                string shopID = item.SHOP_ID;
                string eqpID = item.EQP_ID;

                EqpMaster.Eqp eqp;
                if (TryGetValidEqp(eqpID, out eqp) == false)
                    continue;

                string subEqpID = item.SUB_EQP_ID;
                int? subEqpCount = item.SUB_EQP_COUNT;
                string eqpGoup = eqp.EqpGroup;
                string productID = StringHelper.ToSafeString(item.PRODUCT_ID);
                string productVersion = StringHelper.ToSafeString(item.PRODUCT_VERSION);
                string ownerType = StringHelper.ToSafeString(item.OWNER_TYPE);

                string processID = StringHelper.ToSafeString(item.PROCESS_ID);
                string stepID = StringHelper.ToSafeString(item.STEP_ID);

                string toolID = StringHelper.ToSafeString(item.TOOL_ID);
                string lotID = StringHelper.ToSafeString(item.LOT_ID);
               
                DateTime startTime = item.START_TIME.GetValueOrDefault(toTime);
                DateTime endTime = item.END_TIME.GetValueOrDefault(toTime);

                if (startTime < fromTime)
                    startTime = fromTime;

                int qty = (int)item.UNIT_QTY;
                if (state == EqpState.SETUP)
                    qty = 0;

                string layer = GetLayer(shopID, stepID);
                string origLotID = lotID;
                string wipInitRun = item.WIP_INIT_RUN;

                DataRow dispInfo = FindDispInfo(eqpID, subEqpID, state, startTime, endTime);

                string detailEqpID = GetDetailEqpID(eqpID, subEqpID);

                int lotPriority = item.LOT_PRIORITY;
                string eqpRecipe = item.IS_EQP_RECIPE;
                string stateInfo = item.EQP_STATUS_INFO;

                List<string> stepList = FindPlanStepList(stepInfos, detailEqpID, stepID, state);

                foreach (string planStepID in stepList)
                {
                    string planLayer = GetLayer(shopID, planStepID);

                    string key = CommonHelper.CreateKey(detailEqpID, planLayer);
                    if (viewMode == ViewMode.EQPGROUP)
                        key = CommonHelper.CreateKey(detailEqpID, eqpGoup);

                    AddItem(key,
                            eqp,
                            subEqpID,
                            subEqpCount,
                            planLayer,
                            lotID,
                            origLotID,
                            wipInitRun,
                            productID,
                            productVersion,
                            ownerType,
                            processID,
                            planStepID,
                            toolID,
                            startTime,
                            endTime,
                            qty,
                            state,                            
                            dispInfo,
                            lotPriority,
                            eqpRecipe,
                            stateInfo
                            );
                }
            }

            //BUSY, SETUP, PM이 전혀 없는 설비 제외
            FilterDownEqp(isFilterDownEqp);
        }

#endif

        private Dictionary<string, List<string>> GetPlanStepList(List<EqpPlan> planList)
        {            
            Dictionary<string, List<string>> infos = new Dictionary<string, List<string>>();

            bool isFilterIDLE = true;
            foreach (var item in planList)
            {
                EqpState state = Enums.ParseEqpState(item.EQP_STATUS);

                if (state == EqpState.DOWN || state == EqpState.PM)
                    continue;

                if (IsMatched_Plan(item, state, isFilterIDLE) == false)
                    continue;                

                string eqpID = item.EQP_ID;

                EqpMaster.Eqp eqp;
                if (TryGetValidEqp(eqpID, out eqp) == false)
                    continue;

                string shopID = item.SHOP_ID;
                string subEqpID = item.SUB_EQP_ID;
                string detailEqpID = GetDetailEqpID(eqpID, subEqpID);

                string stepID = StringHelper.ToSafeString(item.STEP_ID);

                List<string> list;
                if(infos.TryGetValue(detailEqpID, out list) == false)
                {
                    list = new List<string>();
                    infos.Add(detailEqpID, list);
                }

                if (list.Contains(stepID))
                    continue;

                list.Add(stepID);
            }

            return infos;
        }

        private bool IsMatched_Plan(EqpPlan item, EqpState state, bool isFilterIDLE)
        {
            var fromTime = this.FromTime;
            var toTime = this.ToTime;

            //상태시작시간                        
            DateTime startTime = item.START_TIME.GetValueOrDefault(toTime);
            if (startTime >= toTime)
                return false;

            //상태종료시간
            DateTime endTime = item.END_TIME.GetValueOrDefault(toTime);
            if (endTime <= fromTime)
                return false;

            //PlanStartTime보다 EqpPlanStart가 작을 경우 (초기Run재공)
            if (startTime < fromTime)
                startTime = fromTime;

            //PlanEndTime보다 Lot의 EndTime이 작을 경우
            if (endTime > toTime)
                endTime = toTime;

            //ProcessedTime is zero
            if (startTime >= endTime)
                return false;

            if (isFilterIDLE)
            {
                //IDLE, IDLERUN
                if ((state == EqpState.IDLE || state == EqpState.IDLERUN))
                    return false;
            }

            return true;
        }

        private List<EqpPlan> GetPlanData_OnlyIDLE(List<EqpPlan> planList)
        {
            if (planList == null || planList.Count == 0)
                return null;

            List<EqpPlan> list = new List<EqpPlan>();

            var groups = planList.GroupBy(t => new Tuple<string, string>(t.EQP_ID, t.SUB_EQP_ID));
            foreach (var it in groups)
            {
                var find = it.FirstOrDefault(t => IsIDLE(t.EQP_STATUS) == false);
                if (find == null)
                {
                    var sample = it.FirstOrDefault();
                    if (sample != null)
                        list.Add(sample);
                }
            }

            return list;
        }

        private bool IsIDLE(string eqpStatus)
        {
            if (string.IsNullOrEmpty(eqpStatus))
                return false;

            EqpState state = Enums.ParseEqpState(eqpStatus);
            if (state == EqpState.IDLE || state == EqpState.IDLERUN)
                return true;

            return false;
        }

        private List<string> FindPlanStepList(Dictionary<string, List<string>> infos, string detailEqpID, string stepID, EqpState state)
        {
            List<string> list = new List<string>();

            if (state == EqpState.DOWN || state == EqpState.PM)
            {
                if (infos != null && infos.Count > 0)
                {
                    List<string> stepList;
                    if (detailEqpID != null && infos.TryGetValue(detailEqpID, out stepList))
                    {
                        list.AddRange(stepList);
                    }
                }
            }

            if(list.Count == 0)
                list.Add(stepID);

            return list;
        }

        private string GetDetailEqpID(string eqpID, string subEqpID = null)
        {
            if (string.IsNullOrEmpty(subEqpID))
                return eqpID;

            return string.Format("{0}-{1}", eqpID ?? "null", subEqpID);
        }

        private void FilterDownEqp(bool isFilterDownEqp)
        {
            if (isFilterDownEqp == false)
                return;
                        
            var infos = this.GanttInfos;
            var keys = infos.Keys.ToList();

            foreach (string key in keys)
            {
                GanttInfo info;
                if (infos.TryGetValue(key, out info) == false)
                    continue;

                bool isRemove = info.Items == null || info.Items.Count == 0;
                if (isRemove == false)
                {
                    bool existNonDown = false;
                    foreach (var barList in info.Items.Values)
                    {                        
                        //BUSY, SETUP, PM이 전혀 없는 설비 제외
                        var find = barList.Find(t => t.State == EqpState.SETUP
                            || t.State == EqpState.BUSY || t.State == EqpState.PM);

                        if (find != null)
                        {
                            existNonDown = true;
                            break;
                        }
                    }

                    if (existNonDown == false)
                        isRemove = true;
                }

                if(isRemove)
                    infos.Remove(key);
            }
        }
        
        private DataRow FindDispInfo(string eqpID, string subEqpID, EqpState state, DateTime startTime, DateTime endTime)
        {
            string eqpKey = CommonHelper.CreateKey(eqpID, subEqpID);

            DataRow find;
            if (this.DispInfos.TryGetValue(eqpKey, startTime, out find))
                return find;

            //AheadSetup인 경우 EndTime 기준으로 추가 체크
            if(state == EqpState.SETUP)
            {
                if (this.DispInfos.TryGetValue(eqpKey, endTime, out find))
                    return find;
            }

            return null;
        }
        
        protected override void ClearData()
        {            
            base.ClearData();

            this.GanttInfos.Clear();
        }

        private void AddItem(
            string key,
            EqpMaster.Eqp eqpInfo,
            string subEqpID,
            int? subEqpCount,
            string layer,
            string lotID,
            string origLotID,
            string wipInitRun,
            string productID,
            string productVersion,
            string ownerID,
            string processID,
            string stepID,
            string toolID,
            DateTime startTime,
            DateTime endTime,
            int inQty,
            EqpState state,                      
            DataRow dispInfo,
            int lotPriority,
            string eqpRecipe,
            string stateInfo,
            bool isGhostBar = false)
        {            
            string shopID = eqpInfo.ShopID;
            string eqpGroup = eqpInfo.EqpGroup;
            string eqpID = eqpInfo.EqpID;

            GanttInfo info;
            if (this.GanttInfos.TryGetValue(key, out info) == false)
                this.GanttInfos.Add(key, info = new GanttInfo(eqpGroup, eqpID, subEqpID, subEqpCount, layer));

            int sortSeq = GetSortSeq(shopID, stepID);

            //SortSeq MIN 기준으로 변경
            info.SortSeq = Math.Min(info.SortSeq, sortSeq);
            
            GanttBar bar = new GanttBar(eqpGroup,
                                        eqpID,
                                        subEqpID,
                                        layer,
                                        lotID,
                                        origLotID,
                                        wipInitRun,
                                        productID,
                                        productVersion,
                                        ownerID,
                                        processID,
                                        stepID,
                                        toolID,
                                        startTime,
                                        endTime,
                                        inQty,
                                        state,
                                        eqpInfo,
                                        dispInfo,
                                        lotPriority,
                                        eqpRecipe,
                                        stateInfo,
                                        isGhostBar);

            var barKey = state != EqpState.DOWN ? bar.BarKey : "DOWN";

            if (barKey != string.Empty)
            {
                //SETUP 다음 BUSY로 이어진 BUSY Bar는 SETUP과 동일한 값 설정 하도록 함.
                if(bar.DispatchingInfo == null && bar.State == EqpState.BUSY)
                    bar.DispatchingInfo = GetDispInfoBySetup(info, barKey);
                
                info.AddItem(barKey, bar, this.IsProductInBarTitle);
            }

            //collect job change 
            if (state == EqpState.SETUP)
                AddJobChange(startTime);
        }

        private DataRow GetDispInfoBySetup(GanttInfo info, string barKey)
        {            
            BarList barList;
            if (info.Items.TryGetValue(barKey, out barList) == false)
                return null;

            var prev = barList.LastOrDefault() as GanttBar;
            if (prev != null && prev.State == EqpState.SETUP)
                return prev.DispatchingInfo;

            return null;
        }

        public List<EqpGantt.GanttInfo> Expand(bool showLayerBar, EqpGantt.ViewMode selectViewMode, bool isDefault = true)
        {
            var infos = this.GanttInfos.Values.ToList();

            if (showLayerBar)
            {
                if (selectViewMode != ViewMode.EQPGROUP)
                    ExpandLayer(infos);
            }

            foreach (GanttInfo info in this.GanttInfos.Values)
            {
                info.Expand(isDefault);
                info.LinkBar(this, isDefault);
            }

            return infos;
        }

        private void ExpandLayer(List<EqpGantt.GanttInfo> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var info = list[i];

                string eqpGroup = info.EqpGroup;
                string eqpID = info.EqpID;
                string subEqpID = info.SubEqpID;
                string layer = info.Layer;

                foreach (BarList barList in info.Items.Values)
                {                    
                    foreach (GanttBar bar in barList)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            EqpGantt.GanttInfo info2 = list[j];

                            bool condition = info2.Layer != layer && info2.EqpID == eqpID && info2.SubEqpID == subEqpID && bar.IsGhostBar == false && bar.State != EqpState.DOWN;

                            if (condition)
                            {
                                var ghostBar = new GanttBar(eqpGroup,
                                                            eqpID,
                                                            subEqpID,
                                                            layer,
                                                            bar.LotID,
                                                            bar.OrigLotID,
                                                            bar.WipInitRun,
                                                            bar.ProductID,
                                                            bar.ProductVersion,
                                                            bar.OwnerType,
                                                            bar.ProcessID,
                                                            bar.StepID,
                                                            bar.ToolID,
                                                            bar.StartTime,
                                                            bar.EndTime,
                                                            0,
                                                            (bar.State == EqpState.PM || bar.State == EqpState.DOWN) ? bar.State : EqpState.IDLERUN,
                                                            bar.EqpInfo,
                                                            bar.DispatchingInfo,
                                                            bar.LotPriority,
                                                            bar.EqpRecipe,
                                                            bar.StateInfo,
                                                            true);

                                info2.AddItem("ghost", ghostBar, this.IsProductInBarTitle);
                            }
                        }
                    }
                }

                if (info.Layer == "PM")
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
        }

        public class GanttInfo : GanttItem
        {
            public string EqpGroup { get; set; }
            public string EqpID { get; set; }
            public string SubEqpID { get; set; }
            public int? SubEqpCount { get; set; }
            public string Layer { get; set; }

            public int SortSeq { get; set; }

            public GanttInfo(string eqpGroup, string eqpID, string subEqpID, int? subEqpCount, string layer, int sortSeq = int.MaxValue)
                : base()
            {
                this.EqpGroup = eqpGroup;
                this.EqpID = eqpID;
                this.SubEqpID = subEqpID;
                this.SubEqpCount = subEqpCount;
                
                this.Layer = layer;

                this.SortSeq = sortSeq;
            }

            public override void AddLinkedNode(Bar bar, LinkedBarNode lnkBarNode)
            {
                base.AddLinkedNode((bar as GanttBar).BarKey, lnkBarNode);
            }

            protected override bool CheckConflict(bool isDefault, Bar currentBar, Bar prevBar)
            {
                return isDefault && (currentBar as GanttBar).BarKey != (prevBar as GanttBar).BarKey;
            }

            public void AddItem(string key, GanttBar bar, bool isProductInBarTitle, bool isSplitA = false)
            {
                BarList list;
                if (this.Items.TryGetValue(key, out list) == false)
                {
                    this.Items.Add(key, list = new BarList());
                    list.Add(bar);
                    return;
                }
                
                foreach (GanttBar it in list)
                {
                    if (it.WipInitRun != bar.WipInitRun)
                        continue;

                    if (it.OwnerType != bar.OwnerType)
                        continue;

                    if (isProductInBarTitle)
                    {
                        if (it.ProductID + it.ProductVersion != bar.ProductID + bar.ProductVersion || it.State != bar.State)
                            continue;
                    }
                    else
                    {
                        if (it.LotID != bar.LotID || it.State != bar.State)
                            continue;                        
                    }

                    if (it.IsShiftSplit || (bar.IsShiftSplit && isSplitA == false) && it.State.Equals(bar.State))
                        return;

                    if (it.Merge(bar))
                        return;
                }

                list.Add(bar);
            }

            public Bar GetBarItem(string key, string lotID)
            {
                BarList list;
                if (this.Items.TryGetValue(key, out list) == false)
                    return null;

                return list.FindLast(t => (t as GanttBar).LotID == lotID);
            }

        }

        public enum SortOptions
        {
            EQP_GROUP,
            EQP,
            LAYER,
        }

        public class CompareGanttInfo : IComparer<GanttInfo>
        {
            private SortOptions[] SortList { get; set; }
            private GanttType GanttType { get; set; }
            private EqpMaster EqpMst { get; set; }
            private string TargetShopID { get; set; }

            public CompareGanttInfo(GanttType gType, EqpMaster eqpMst, string targetShopID, params SortOptions[] sortList)
            {
                this.SortList = sortList;
                this.GanttType = gType;
                this.EqpMst = eqpMst;
                this.TargetShopID = targetShopID;
            }

            public int Compare(GanttInfo x, GanttInfo y)
            {
                int cmp = 0;
                foreach (var sort in SortList)
                {
                    if (cmp != 0)
                        break;
                    
                    cmp = Compare(x, y, sort);                        
                }

                return cmp;
            }

            private int Compare(GanttInfo x, GanttInfo y, SortOptions sort)
            {
                if (sort == SortOptions.EQP_GROUP)
                {
                    int seq_x = this.EqpMst.GetDspGroupSeq(this.TargetShopID, x.EqpGroup);
                    int seq_y = this.EqpMst.GetDspGroupSeq(this.TargetShopID, y.EqpGroup);

                    int cmp = seq_x.CompareTo(seq_y);
                    if(cmp == 0)
                    {
                        cmp = string.Compare(x.EqpGroup, y.EqpGroup);
                    }

                    return cmp;
                }        
                
                if (sort == SortOptions.EQP)
                {
                    int seq_x = this.EqpMst.GetEqpSeq(x.EqpID);
                    int seq_y = this.EqpMst.GetEqpSeq(y.EqpID);

                    int cmp = seq_x.CompareTo(seq_y);
                    if (cmp == 0)
                    {
                        cmp = string.Compare(x.EqpID, y.EqpID);
                    }

                    //SubEqpID
                    if(cmp == 0)
                        cmp = string.Compare(x.SubEqpID, y.SubEqpID); 

                    return cmp;
                }
                
                if (sort == SortOptions.LAYER)
                {
                    int xSeq = x.SortSeq;
                    int ySeq = y.SortSeq;

                    if (xSeq == ySeq)
                    {
                        xSeq = x.Layer.CompareTo(y.Layer);
                        ySeq = y.Layer.CompareTo(x.Layer);
                    }

                    return xSeq.CompareTo(ySeq);
                }

                return 0;
            }
        }
    }
}