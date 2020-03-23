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
using CSOT.Lcd.UserInterface.Common;
using DevExpress.XtraPivotGrid;
using CSOT.Lcd.UserInterface.DataMappings;
using Mozart.Studio.UIComponents;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.Utility;
using CSOT.Lcd.Scheduling;
using CSOT.UserInterface.Utils;
using Mozart.Studio.Application;
using DevExpress.XtraCharts;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Inputs;

namespace CSOT.Lcd.UserInterface.Analysis
{
    /// <summary>
    /// 
    /// </summary>
    public partial class StepMoveView : XtraPivotGridControlView
    {
        private string _dirPath;
        private string _fileName;

        private const string _pageID = "StepMove";

        private IVsApplication _application;
        private IExperimentResultItem _result;
        private ResultDataContext _resultCtx;
                
        private SortedSet<string> _dayHourList;        
        private Dictionary<string, int> _stepIndexs;
        private List<StdStep> _stdStepList;

        private string TargetAreaID
        {
            get{ return this.areaComboBox.Text; }
        }
        
        private string TargetShopID
        {
            get { return this.ShopIDComboBox.Text; }
        }

        private string TargetStdStep
        {
            get { return this.StepComboBox.Text; }
        }

        private bool ShowSubStep
        {
            get
            {
                if (this.isShowSubStep.Checked == true)
                    return true;
                return false;
            }
        }

        private DateTime QueryStartTime
        {
            get { return this.StartDatePicker.DateTime; }
        }

        private DateTime QueryEndTime
        {
            get 
            { 
                return this.StartDatePicker.DateTime.AddHours(Convert.ToInt32(this.EndCountPicker.Value) * ShopCalendar.ShiftHours); 
            }
        }

        public StepMoveView()
        {
            InitializeComponent();
        }

        public StepMoveView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();

            _dirPath = string.Format("{0}\\DefaultLayOut", Application.StartupPath);
            _fileName = string.Format("{0}.xml", _pageID);
        }

        protected override void LoadDocument()
        {                      
            if (_result == null)
            {
                var item = (IMenuDocItem)this.Document.ProjectItem;
                _result = (IExperimentResultItem)item.Arguments[0];
            }

            _application = (IVsApplication)GetService(typeof(IVsApplication));
            _resultCtx = _result.GetCtx<ResultDataContext>();

            Globals.InitFactoryTime(_result.Model);

            SetControl();
        }

        #region SetControl

        private void SetControl()
        {
            expandablePanel1.Text = StepMoveData.TITLE;

            SetAreaIDComboBox();
            
            SetShopIDComboBox(this.TargetAreaID);

            SetStepIDCombo(this.TargetShopID);

            ShowTotal(this.pivotGridControl1);

            int baseMinute = ShopCalendar.StartTime.Minutes;

            this.StartDatePicker.Properties.EditMask = "yyyy-MM-dd HH:mm:ss";
            this.StartDatePicker.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.StartDatePicker.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;
            this.StartDatePicker.DateTime = DateHelper.GetRptDate_1Hour(_result.StartTime, baseMinute);
            this.EndCountPicker.Value = Globals.GetResultPlanPeriod(_result) * 2;
        }

        private void SetAreaIDComboBox()
        {
            this.ShopIDComboBox.Items.Clear();

            SortedSet<string> list = new SortedSet<string>();

            var stepMove = _resultCtx.StepMove;

            foreach (StepMove item in stepMove)
            {
                if (list.Contains(item.AREA_ID))
                    continue;

                list.Add(item.AREA_ID);
            }

            foreach (string areaID in list.Reverse())
            {
                if (this.areaComboBox.Items.Contains(areaID))
                    continue;

                this.areaComboBox.Items.Add(areaID);
            }

            if (this.areaComboBox.Items.Count > 0)
                this.areaComboBox.SelectedIndex = 0;
        }
         
        private void SetShopIDComboBox(string areaID)
        {
            this.ShopIDComboBox.Items.Clear();
            if (StringHelper.Equals(StepMoveData.TFT, this.TargetAreaID))
                this.ShopIDComboBox.Items.Add(Consts.ALL);

            SortedSet<string> list = new SortedSet<string>();

            var stepMove = _resultCtx.StepMove;
            foreach (StepMove item in stepMove)
            {
                if (StringHelper.Equals(item.AREA_ID, this.TargetAreaID) == false)
                    continue;
                
                if (list.Contains(item.SHOP_ID))
                    continue;

                list.Add(item.SHOP_ID);
            }

            foreach (string shopID in list)
            {
                if (this.ShopIDComboBox.Items.Contains(shopID))
                    continue;

                this.ShopIDComboBox.Items.Add(shopID);
            }

            if (this.ShopIDComboBox.Items.Count > 0)
                this.ShopIDComboBox.SelectedIndex = 0;
        } 

        private void SetStepIDCombo(string shopID)
        {
            this.StepComboBox.Items.Clear();

            this.StepComboBox.Items.Add(Consts.ALL);
            
            SortedSet<string> stepSet = new SortedSet<string>();

            var stepMove = _resultCtx.StepMove;
            foreach (StepMove item in stepMove)
            {
                if ((shopID == Consts.ALL || item.SHOP_ID == shopID))
                {
                    if (stepSet.Contains(item.STD_STEP_ID) == false)
                        stepSet.Add(item.STD_STEP_ID);
                }
            }

            foreach (string step in stepSet)
            {
                this.StepComboBox.Items.Add(step);
            }

            if (this.StepComboBox.Items.Count > 0)
                this.StepComboBox.SelectedIndex = 0;
        }

        #endregion

        #region BindData

        private void Query()
        {
            SetDateRanges();
            
            BindData();

            LoadDefaultLayOutFromXml();
        }

        private void SetDateRanges()
        {
            _dayHourList = new SortedSet<string>();
            
            float interval = 1f;
            if (this.dayRadioBtn.Checked)
                interval = 24f;
            else if (this.shiftRadioBtn.Checked)
                interval = ShopCalendar.ShiftHours;

            DateTime st = GetPlanDate(this.QueryStartTime);
            DateTime et = this.QueryEndTime;

            int baseMinute = ShopCalendar.StartTime.Minutes;
            DateTime baseT = DateHelper.GetRptDate_1Hour(st, baseMinute);                       
            
            for (DateTime t = baseT; t < et; t = t.AddHours(interval))
            {
                string str = GetDateString(t);

                _dayHourList.Add(str);
            }
        }

        private Dictionary<string, ResultItem> LoadData()
        {

            Dictionary<string, ResultItem> items = new Dictionary<string, ResultItem>();        
            
            string targetShopID = this.TargetShopID;

            bool isFirst = true;
            string targetStdStep = this.TargetStdStep;
            string areaID = this.TargetAreaID;

            _stdStepList = GetStdStepList(areaID);

            //bool isOnlyMainStep = this.ShowSubStep == false;

            var stepMove = _resultCtx.StepMove;
            foreach (StepMove item in stepMove)
            {

                if (areaID != item.AREA_ID)
                    continue;
                
                if (targetShopID != Consts.ALL && item.SHOP_ID != targetShopID)
                    continue;

                if (item.PLAN_DATE < this.QueryStartTime)
                    continue;

                if (item.PLAN_DATE >= this.QueryEndTime)
                    continue;

                if (targetStdStep != Consts.ALL && item.STD_STEP_ID != TargetStdStep)
                    continue;

                string shopID = item.SHOP_ID;
                string productID = item.PRODUCT_ID;
                string productVersion = item.PRODUCT_VERSION;
                string ownerType = item.OWNER_TYPE;
                string stdStep = item.STD_STEP_ID;
                int stepSeq = item.STD_STEP_SEQ;
                string eqpID = item.EQP_ID;
                string eqpGroupID = item.EQP_GROUP_ID;

                if (isFirst)
                {
                    foreach (string dayHour in _dayHourList)
                    {                       
                        string dateString = GetDateString(dayHour);
                        string k = shopID + productID + productVersion + ownerType + stdStep + eqpID + eqpGroupID + dateString;

                        ResultItem padding;
                        if (items.TryGetValue(k, out padding) == false)
                        {
                            padding = new ResultItem(item.SHOP_ID, item.PRODUCT_ID, item.PRODUCT_VERSION, item.OWNER_TYPE, item.STEP_ID, item.STD_STEP_SEQ, item.EQP_ID, item.EQP_GROUP_ID, dateString, 0, 0);

                            items.Add(k, padding);
                        }
                    }

                    isFirst = false;
                }

                DateTime planDate = GetPlanDate(item.PLAN_DATE);
                string dateStr = GetDateString(planDate);

                string key = shopID + productID + productVersion + ownerType + stdStep + eqpID + eqpGroupID + dateStr;

                ResultItem ri = null;
                if (items.TryGetValue(key, out ri) == false)
                    items.Add(key, ri = new ResultItem(shopID, productID, productVersion, ownerType, stdStep, stepSeq, eqpID, eqpGroupID, dateStr));

                ri.UpdateQty((int)item.IN_QTY, (int)item.OUT_QTY);
            }

            return items;
        }

        private DateTime GetPlanDate(DateTime t)
        {
            DateTime planDate = t;
            if (this.dayRadioBtn.Checked)
                planDate = ShopCalendar.SplitDate(t);
            else if (this.shiftRadioBtn.Checked)
                planDate = ShopCalendar.ShiftStartTimeOfDayT(t);

            return planDate;
        }

        private string GetDateString(string dayHour)
        {
            DateTime t = DateHelper.StringToDateTime(dayHour);

            return GetDateString(t);
        }

        private string GetDateString(DateTime t)
        {
            string dateString = t.ToString("yyyyMMddHHmm");

            return dateString;
        }

        private void BindData()
        {            
            XtraPivotGridHelper.DataViewTable dt = CreateDataViewTable();
            
            FillData(dt);

            DrawGrid(dt);
        }

        private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        {
            XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();

            dt.AddColumn(StepMoveData.SHOP_ID, StepMoveData.SHOP_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(StepMoveData.STD_STEP, StepMoveData.STD_STEP, typeof(string), PivotArea.RowArea, null, null);

            dt.AddColumn(StepMoveData.PRODUCT_ID, StepMoveData.PRODUCT_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(StepMoveData.PRODUCT_VERSION, StepMoveData.PRODUCT_VERSION, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(StepMoveData.OWNER_TYPE, StepMoveData.OWNER_TYPE, typeof(string), PivotArea.RowArea, null, null);
            
            dt.AddColumn(StepMoveData.EQP_ID, StepMoveData.EQP_ID, typeof(string), PivotArea.FilterArea, null, null);
            dt.AddColumn(StepMoveData.EQP_GROUP_ID, StepMoveData.EQP_GROUP_ID, typeof(string), PivotArea.FilterArea, null, null);
            dt.AddColumn(StepMoveData.TARGET_DATE, StepMoveData.TARGET_DATE, typeof(string), PivotArea.ColumnArea, null, null);
            dt.AddColumn(StepMoveData.IN_QTY, StepMoveData.IN_QTY, typeof(float), PivotArea.DataArea, null, null);
            dt.AddColumn(StepMoveData.OUT_QTY, StepMoveData.OUT_QTY, typeof(float), PivotArea.DataArea, null, null);

            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns[StepMoveData.SHOP_ID],
                        dt.Columns[StepMoveData.STD_STEP],
                        dt.Columns[StepMoveData.PRODUCT_ID],
                        dt.Columns[StepMoveData.PRODUCT_VERSION],
                        dt.Columns[StepMoveData.OWNER_TYPE],                        
                        dt.Columns[StepMoveData.EQP_ID],
                        dt.Columns[StepMoveData.EQP_GROUP_ID],
                        dt.Columns[StepMoveData.TARGET_DATE]
                    }
                );

            return dt;
        }

        private void FillData(XtraPivotGridHelper.DataViewTable dt)
        {
            var items = LoadData();

            string targetAreaID = this.TargetAreaID;
            bool isOnlyMainStep = this.ShowSubStep == false;

            var stdStepList = _stdStepList;
            if (isOnlyMainStep)
                stdStepList = stdStepList.FindAll(t => StringHelper.Equals(t.STEP_TYPE, "MAIN"));

            var stepMove = items.Values;

            var query = from ps in stdStepList
                        join p in stepMove
                        on ps.SHOP_ID + ps.STEP_ID equals p.ShopID + p.StepID into temp
                        from tp in temp.DefaultIfEmpty()
                        select new
                        {
                            SHOP_ID = tp != null ? tp.ShopID : ps.SHOP_ID,
                            STD_STEP_ID = tp != null ? tp.StepID : ps.STEP_ID,
                            PROD_ID = tp != null ? tp.ProductID : null,
                            PROD_VER = tp != null ? tp.ProductVersion : null,
                            OWNER_TYPE = tp != null ? tp.OwnerType : null,                            
                            TIME_INFO = tp != null ? tp.TimeInfo : null,
                            EQP_ID = tp != null ? tp.EqpID : null,
                            EQP_GROUP_ID = tp != null ? tp.EqpGroupID : string.Empty,
                            IN_QTY = tp != null ? tp.InQty : 0,
                            OUT_QTY = tp != null ? tp.OutQty : 0,
                            STD_STEP_SEQ = tp != null ? tp.StepSeq : ps.STEP_SEQ,
                        };
                        
            _stepIndexs = new Dictionary<string, int>();

            foreach (var item in query)
            {
                if (item.PROD_ID == null)
                    continue;

                dt.DataTable.Rows.Add(
                    item.SHOP_ID ?? string.Empty,
                    item.STD_STEP_ID ?? string.Empty,
                    item.PROD_ID ?? string.Empty,
                    item.PROD_VER ?? string.Empty,
                    item.OWNER_TYPE ?? string.Empty,                    
                    item.EQP_ID ?? string.Empty,
                    item.EQP_GROUP_ID ?? string.Empty,
                    item.TIME_INFO,
                    item.IN_QTY,
                    item.OUT_QTY
                );

                string stepKey = item.STD_STEP_ID;
                _stepIndexs[stepKey] = item.STD_STEP_SEQ;
            }
        }

        private void DrawGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            this.pivotGridControl1.BeginUpdate();

            this.pivotGridControl1.ClearPivotGridFields();
            this.pivotGridControl1.CreatePivotGridFields(dt);

            this.pivotGridControl1.DataSource = dt.DataTable;

            this.pivotGridControl1.Fields[StepMoveData.STD_STEP].SortMode = PivotSortMode.Custom;

            pivotGridControl1.CustomCellDisplayText += pivotGridControl1_CellDisplayText;
            this.pivotGridControl1.EndUpdate();

            this.pivotGridControl1.BestFitColumnArea();

            pivotGridControl1.Fields[StepMoveData.OUT_QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[StepMoveData.OUT_QTY].CellFormat.FormatString = "#,##0";

            pivotGridControl1.Fields[StepMoveData.IN_QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[StepMoveData.IN_QTY].CellFormat.FormatString = "#,##0";

            this.pivotGridControl1.Fields[StepMoveData.STD_STEP].SortMode = PivotSortMode.Custom;
        }

        private void ShowTotal(PivotGridControl pivot, bool isCheck = false)
        {
            pivot.OptionsView.ShowRowTotals = false;
            pivot.OptionsView.ShowRowGrandTotals = false;
            pivot.OptionsView.ShowColumnTotals = isCheck;
            pivot.OptionsView.ShowColumnGrandTotals = isCheck;
        }

        #endregion

        private List<StdStep> GetStdStepList(string areaID)
        {
            var modelContext = _result.GetCtx<ModelDataContext>();
            var stdStep = modelContext.StdStep;

            List<StdStep> list = new List<StdStep>();

            var finds = stdStep.Where(t => t.AREA_ID == areaID).OrderBy(t => t.STEP_SEQ);

            if (finds == null || finds.Count() == 0)
                return list;

            List<string> keyList = new List<string>(); 
            foreach (var item in finds)
            {
                string key = item.SHOP_ID + item.STEP_ID;
                if (keyList.Contains(key))
                    continue;
                
                keyList.Add(key);

                list.Add(item);
            }

            return list;
        }

        #region Internal Class : ResultItem

        internal class ResultItem
        {
            public string ShopID { get; private set; }
            public string ProductID { get; private set; }
            public string ProductVersion { get; private set; }
            public string OwnerType { get; private set; }
            public string StepID { get; private set; }
            public int StepSeq { get; private set; }
            public string EqpID { get; private set; }
            public string EqpGroupID { get; private set; }            
            public string TimeInfo { get; set; }

            public int InQty { get; private set; }
            public int OutQty { get; private set; }

            public ResultItem(string shopID, string productID, string productVersion, string ownerType, string stepID, int stepSeq, string eqpID, string eqpGroupID, string timeInfo)
            {
                this.ShopID = shopID;
                this.ProductID = productID;
                this.ProductVersion = productVersion;
                this.OwnerType = ownerType;
                this.StepID = stepID;
                this.StepSeq = stepSeq;
                this.EqpID = eqpID;
                this.EqpGroupID = eqpGroupID;
                this.TimeInfo = timeInfo;
            }

            public ResultItem(string shopID, string productID, string productVersion, string ownerType, string stepID, int stepSeq, string eqpID, string eqpGroupID, string timeInfo, int inQty, int outQty)
            {
                this.ShopID = shopID;
                this.ProductID = productID;
                this.ProductVersion = productVersion;
                this.OwnerType = ownerType;
                this.StepID = stepID;
                this.StepSeq = stepSeq;
                this.EqpID = eqpID;
                this.EqpGroupID = eqpGroupID;
                                
                this.TimeInfo = timeInfo;

                this.InQty = inQty;
                this.OutQty = outQty;
            }

            public void UpdateQty(int inQty, int outQty)
            {
                this.InQty += inQty;
                this.OutQty += outQty;
            }
        }

        #endregion
        
        #region //Chart

        private void FillChart(PivotGridControl pivotGrid, List<int> rows)
        {            
            XYDiagram xyDiagram = (XYDiagram)chartControl.Diagram;
            if (xyDiagram != null)
            {
                xyDiagram.AxisX.Label.Angle = 45;
                xyDiagram.AxisX.Label.ResolveOverlappingOptions.AllowHide = false;
                xyDiagram.AxisX.NumericScaleOptions.AutoGrid = false;
                xyDiagram.EnableAxisXScrolling = false;
                xyDiagram.EnableAxisYScrolling = true;
                xyDiagram.AxisX.Label.Font = new System.Drawing.Font("Tahoma", 7F);                
            }

            chartControl.Series.Clear();

            DataTable dt = (DataTable)pivotGrid.DataSource;
            var colNameList = GetColNameList(pivotGrid);

            var dataAreaList = GetDataAreaList(pivotGrid);

            //bool onlyOne = rows.Count == 1;
            
            foreach (int rowIdx in rows)
            {
                var valueList = GetRowValueList(pivotGrid, rowIdx);
                DataView dv = SummaryData(dt, colNameList, valueList);

                string seriesName = StringHelper.ConcatKey(valueList.ToArray());

                AddSeries(seriesName, ViewType.Line, dataAreaList, dv);

                //if (onlyOne)
                //    AddSeries(seriesName, ViewType.Bar, dataAreaList, dv);
            }
        }

        private void AddSeries(string seriesName, ViewType viewType, string[] dataAreaList, DataView dv)
        {
            int index = chartControl.Series.Add(seriesName, viewType);

            chartControl.Series[index].ArgumentDataMember = StepMoveData.TARGET_DATE;
            chartControl.Series[index].ValueDataMembers.AddRange(dataAreaList);
            //chartControl.Series[index].Label.Visible = false;            

            chartControl.Series[index].DataSource = dv;
            chartControl.Series[index].LegendText = string.IsNullOrEmpty(seriesName) ? "Total" : seriesName;
        }

        private string[] GetDataAreaList(PivotGridControl pivotGrid)
        {
            List<string> list = new List<string>();

            var dataAreaList = pivotGrid.GetFieldsByArea(PivotArea.DataArea);
            foreach (var field in dataAreaList)
            {
                if (field.DataType == typeof(string))
                    continue;

                list.Add(field.FieldName);

                //ValueDataMembers에 복수개을 설정해도 첫번째 Field만 기록됨(그래서 한개만 추가 하도록 함)
                break;
            }

            return list.ToArray();
        }

        private List<string> GetColNameList(PivotGridControl pivotGrid)
        {
            List<string> list = new List<string>();

            List<PivotGridField> rowList = pivotGrid.GetFieldsByArea(PivotArea.RowArea);
            for (int i = 0; i < rowList.Count; i++)
            {  
                string colName = pivotGrid.GetFieldByArea(PivotArea.RowArea, i).ToString();
                list.Add(colName);
            }

            return list;
        }

        private List<string> GetRowValueList(PivotGridControl pivotGrid, int rowindex)
        {
            List<string> list = new List<string>();

            List<PivotGridField> rowList = pivotGrid.GetFieldsByArea(PivotArea.RowArea);
            for (int i = 0; i < rowList.Count; i++)
            {                
                string value = pivotGrid.GetFieldValue(pivotGrid.GetFieldByArea(PivotArea.RowArea, i), rowindex) as string;
                list.Add(value ?? string.Empty);
            }

            return list;
        }

        private DataView SummaryData(DataTable dt, List<string> colNameList, List<string> valueList)
        {
            DataTable result = CreateSummaryTable(colNameList);
            FillRowZero(result, valueList);

            int keyColCount = colNameList.Count;
           
            foreach (DataRow row in dt.Rows)
            {                
                object[] pks = new object[keyColCount + 1];

                for (int i = 0; i < keyColCount; i++)
			    {
                    string colName = colNameList[i];
                    pks[i] = row.GetString(colName);
			    }

                string dayHour = row.GetString(StepMoveData.TARGET_DATE);
                pks[keyColCount] = dayHour; 
                                
                DataRow findRow = result.Rows.Find(pks);
                if (findRow == null)
                    continue;

                int inQty = row.GetInt32(StepMoveData.IN_QTY);
                if (inQty > 0)
                    findRow[StepMoveData.IN_QTY] = findRow.GetInt32(StepMoveData.IN_QTY) + inQty;

                int outQty = row.GetInt32(StepMoveData.OUT_QTY);
                if (outQty > 0)
                    findRow[StepMoveData.OUT_QTY] = findRow.GetInt32(StepMoveData.OUT_QTY) + outQty;
            }

            return new DataView(result, string.Empty, StepMoveData.TARGET_DATE, DataViewRowState.CurrentRows);
        }

        private DataTable CreateSummaryTable(List<string>  colNameList)
        {
            DataTable result = new DataTable();
                        
            List<DataColumn> pkList = new List<DataColumn>();
            foreach (var colName in colNameList)
            {
                var col = result.Columns.Add(colName, typeof(string));
                pkList.Add(col);
            }

            var tcol = result.Columns.Add(StepMoveData.TARGET_DATE, typeof(string));
            pkList.Add(tcol);

            result.Columns.Add(StepMoveData.IN_QTY, typeof(int));
            result.Columns.Add(StepMoveData.OUT_QTY, typeof(int));

            result.PrimaryKey = pkList.ToArray();

            return result;
        }

        private void FillRowZero(DataTable result, List<string> valueList)
        {
            int count = valueList.Count;
            foreach (var dayHour in _dayHourList)
            {
                DataRow row = result.NewRow();
                                
                for (int i = 0; i < count; i++)
                    row[i] = valueList[i];

                row[StepMoveData.TARGET_DATE] = dayHour;
                row[StepMoveData.IN_QTY] = 0;
                row[StepMoveData.OUT_QTY] = 0;

                result.Rows.Add(row);
            }
        }

        #endregion //Chart

        #region Excel Export

        //상위메뉴 Data>ExportToExcel --> Enable
        protected override bool UpdateCommand(Command command)
        {
            bool handled = false;

            switch (command.CommandID)
            {
                case DataCommands.DataExportToExcel:
                    command.Enabled = true;
                    handled = true;
                    break;
            }

            if (handled) return true;
            return base.UpdateCommand(command);
        }

        protected override bool HandleCommand(Command command)
        {
            bool handled = false;

            switch (command.CommandID)
            {
                case DataCommands.DataExportToExcel:
                    {
                        PivotGridExporter.ExportToExcel(this.pivotGridControl1);
                        handled = true;
                        break;
                    }
            }

            if (handled) return true;
            return base.HandleCommand(command);
        }

        #endregion

        private void pivotGridControl1_CellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
        {
            if (e.GetFieldValue(e.DataField) != null && e.GetFieldValue(e.DataField).ToString() == "0")
            {
                e.DisplayText = string.Empty;
            }
        }

        private void pivotGridControl_CellSelectionChanged(object sender, EventArgs e)
        {
            PivotGridCells cells = this.pivotGridControl1.Cells;
            DevExpress.XtraPivotGrid.Selection.IMultipleSelection selList = cells.MultiSelection;

            List<int> rows = new List<int>();
            foreach (Point pt in selList.SelectedCells)
            {
                if (rows.Contains(pt.Y) == false)
                    rows.Add(pt.Y);
            }

            FillChart((PivotGridControl)sender, rows);
        }
                                
        private bool IsSameData(string strFstData, string strSndData, ref string key)
        {
            if (strFstData != strSndData)
                return false;

            key += strFstData;

            return true;
        }
                
        private void pivotGridControl1_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        {
            if (e.Field.FieldName != StepMoveData.STD_STEP)
                return;

            int s1 = ConvertLayerIndex(e.Value1 as string);
            int s2 = ConvertLayerIndex(e.Value2 as string);

            e.Result = s1.CompareTo(s2);

            e.Handled = true;
        }

        private int ConvertLayerIndex(string stepID)
        {
            if (stepID == null)
                return 999;

            int seq = 0;
            if (_stepIndexs.TryGetValue(stepID, out seq))
                return seq;

            return 999;
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            Query();
        }

        private void shopComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetStepIDCombo(this.TargetShopID);                
        }

        private void btnSaveLayOut_Click(object sender, EventArgs e)
        {
            string message = "Do you want to save PivotGridLayOut?";
            DialogResult result = MessageBox.Show(message, "Save Default LayOut", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (result == DialogResult.OK)
            {
                string dirPath = string.Format("{0}\\DefaultLayOut", _application.ApplicationPath);
                string fileName = string.Format("{0}.xml", _pageID);

                PivotGridLayoutHelper.SaveXml(this.pivotGridControl1, dirPath, fileName);
            }
        }

        private void LoadDefaultLayOutFromXml()
        {
            string dirPath = string.Format("{0}\\DefaultLayOut", _application.ApplicationPath);
            string fileName = string.Format("{0}.xml", _pageID);

            PivotGridLayoutHelper.LoadXml(this.pivotGridControl1, dirPath, fileName);
        }

        private void areaComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            SetShopIDComboBox(this.TargetAreaID);
        }

        private void isShowSubStep_CheckedChanged(object sender, EventArgs e)
        {
            Query();
        }
    }
}
