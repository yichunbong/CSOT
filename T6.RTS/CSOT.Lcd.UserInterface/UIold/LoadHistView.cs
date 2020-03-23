using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.Application;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.Scheduling;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Text;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.UserInterface.DataMappings;
using Mozart.Collections;
using DevExpress.XtraEditors.Controls;
using CSOT.Lcd.UserInterface.Gantts;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class LoadHistView : XtraUserControlView
    {
        private const string _pageID = "LotHistView";

        private IVsApplication _application;
        private IExperimentResultItem _result;
        private ResultDataContext _resultCtx;

        protected Dictionary<string, EqpMaster.Eqp> ValidEqps { get; set; }
        protected Dictionary<string, EqpGanttChartData.StdStep> StdSteps { get; set; }
        protected DoubleDictionary<string, DateTime, DataRow> DispInfos { get; set; }


        private EqpMaster EqpMgr = new EqpMaster();

        private DateTime PlanStartTime { get; set; }
        private DateTime StartDate { get { return dateEdit1.DateTime; } }
        private DateTime EndDate { get { return this.StartDate.AddHours(ShopCalendar.ShiftHours * Convert.ToInt32(fromShiftComboBox.Value)); } }

        private string TargetShopID { get { return this.ShopIDComboBox.Text; } }


        private List<string> SelectedEqpGroups
        {
            get
            {
                List<string> eqpGroupList = new List<string>();

                foreach (CheckedListBoxItem item in this.EqpGroupsCheckedBox.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                        eqpGroupList.Add(item.ToString());
                }

                return eqpGroupList;
            }
        }

        private string EqpIdPattern { get { return this.eqpIDtextBox.Text; } }

        public LoadHistView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }


        protected override void LoadDocument()
        {
            var item = (IMenuDocItem)this.Document.ProjectItem;

            _result = (IExperimentResultItem)item.Arguments[0];
            _application = (IVsApplication)GetService(typeof(IVsApplication));
            _resultCtx = _result.GetCtx<ResultDataContext>();

            Globals.InitFactoryTime(_result.Model);

            this.EqpMgr.LoadEqp(_result);

            
            SetValidEqpIDList();

            PrepareData_StdStep();
            PrepareDispatchingData();

            SetControl();

        }

        private DataTable CreateResultTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(ColName.EqpID);
            dt.Columns.Add(ColName.ChamberID);
            dt.Columns.Add(ColName.EqpGroup);
            dt.Columns.Add(ColName.Status);
            dt.Columns.Add(ColName.LotID);
            dt.Columns.Add(ColName.ProductID);
            dt.Columns.Add(ColName.ProductVer);
            dt.Columns.Add(ColName.StartTime);
            dt.Columns.Add(ColName.EndTime);

            return dt;
        }


        private void SetControl()
        {
            this.fromShiftComboBox.Value = Globals.GetResultPlanPeriod(_result) * ShopCalendar.ShiftCount;
            this.PlanStartTime = _result.StartTime;

            this.dateEdit1.DateTime = PlanStartTime;
            this.dateEdit1.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.dateEdit1.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            this.ShopIDComboBox.SelectedIndex = 0;



            ComboHelper.AddDataToComboBox(this.ShopIDComboBox, _result, EqpGanttChartData.STD_STEP_TABLE_NAME,
               EqpGanttChartData.Eqp.Schema.SHOP_ID, false);
            
            BindChkListEqpGroup(EqpGroupsCheckedBox.Properties.Items, this.TargetShopID);

            if (this.EqpGroupsCheckedBox.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in EqpGroupsCheckedBox.Properties.Items)
                    item.CheckState = CheckState.Checked;
            }

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
            if (this.DispInfos == null)
                this.DispInfos = new DoubleDictionary<string, DateTime, DataRow>();

            string filter = string.Empty;
            var dispatchDt = _result.LoadOutput(EqpGanttChartData.EQP_DISPATCH_LOG_TABLE_NAME, filter);

            foreach (DataRow row in dispatchDt.Rows)
            {
                string eqpID = row.GetString(EqpGanttChartData.EqpDispatchLog.Schema.EQP_ID);
                string timeTemp = row.GetString(EqpGanttChartData.EqpDispatchLog.Schema.DISPATCH_TIME);

                if (string.IsNullOrEmpty(timeTemp))
                    continue;

                DateTime dispatchingTime = timeTemp.DbToDateTime();

                Dictionary<DateTime, DataRow> dic;
                if (DispInfos.TryGetValue(eqpID, out dic) == false)
                    DispInfos[eqpID] = dic = new Dictionary<DateTime, DataRow>();

                DataRow info;
                if (dic.TryGetValue(dispatchingTime, out info) == false)
                    dic[dispatchingTime] = info = row;
            }
        }

        protected void SetValidEqpIDList(string selectedShopId = null, string eqpPattern = null)
        {
            this.ValidEqps = new Dictionary<string, EqpMaster.Eqp>();

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
                    if (StringHelper.Equals(targetShopID, "ARRAY"))
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
            if (this.SelectedEqpGroups.Count > 0)
            {
                foreach (string eqpGrpID in this.SelectedEqpGroups)
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


        private void queryButton_Click(object sender, EventArgs e)
        {
            var loadHistDt = _result.LoadOutput("LoadHistory");

            string filter = string.Empty;

            DataView dv = GetPlanData();

            SchedOut.LoadInfo info = new SchedOut.LoadInfo();


            DataTable result = CreateResultTable();

            FillData_LoadHist(result, dv, this.TargetShopID, this.EqpIdPattern, true);

            this.gridControl.DataSource = result;
            this.gridView.BestFitColumns();



        }

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

        private void FillData_LoadHist(DataTable result, DataView dvPlan, string targetShopID, string targetEqpID, bool showDownEqp)
        {
            //var runWipList = GetRunWipList(targetShopID);

            string factoryStartTime = ShopCalendar.StartTime.ToString().Replace(":", "");
            HashSet<string> sameValueContainer = new HashSet<string>();
            Dictionary<string, List<DataRow>> datas = CollectionPlanData(dvPlan);

            var fromTime = this.StartDate;
            var toTime = this.EndDate;

            foreach (List<DataRow> planDataRows in datas.Values)
            {
                int rowCount = planDataRows.Count;

                for (int j = 0; j < rowCount; j++)
                {
                    DataRow row = planDataRows[j];
                    DataRow nextRow = j < (rowCount - 1) ? planDataRows[j + 1] : null;

                    string eqpID = row.GetString(ColName.EqpID);
                    string chamberID = row.GetString(ColName.ChamberID);
                    string eqpGrp = row.GetString(ColName.EqpGroup);

                    EqpMaster.Eqp eqp;
                    if (TryGetValidEqp(eqpID, out eqp) == false)
                        continue;
                   
                    
                    DateTime targetDate = DateUtility.DbToDate(row.GetString("TARGET_DATE"));


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

                        ////IDLE, IDLERUN
                        //if ((state == EqpState.IDLE || state == EqpState.IDLERUN))
                        //    continue;

                        string layer = GetLayer(shopID, stepID);
                        string origLotID = lotID;

                        DataRow dispatchingInfo = FindDispInfo(eqpID, startTime);

                        AddItem(result,
                               eqpID,
                               eqpGrp,
                               chamberID,
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
                              eqpID,
                              dispatchingInfo);

                    }
                }
            }
        }

        private void AddItem(DataTable result, string eqpID, string eqpGrp, string chamberID, string layer, string lotID, string origLotID, string productID, string productVersion, string ownerType, string processID, string stepID, string toolID, DateTime startTime, DateTime endTime, int qty, EqpState state, string eqpID_2, DataRow dispatchingInfo)
        {
            DataRow row = result.NewRow();

            row[ColName.EqpID] = eqpID;
            row[ColName.ChamberID] = chamberID;
            row[ColName.EqpGroup] = eqpGrp;
            row[ColName.LotID] = lotID;
            row[ColName.ProductID] = productID;
            row[ColName.ProductVer] = productVersion;
            row[ColName.Status] = state.ToString();
            row[ColName.StartTime] = DateHelper.Format(startTime);
            row[ColName.EndTime] = DateHelper.Format(endTime);

            result.Rows.Add(row);
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

        private DataRow FindDispInfo(string eqpID, DateTime startTime)
        {
            DataRow find;
            if (this.DispInfos.TryGetValue(eqpID, startTime, out find))
                return find;

            return null;
        }

    }
}
