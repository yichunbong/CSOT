using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.UserInterface;
using CSOT.Lcd.UserInterface.DataMappings;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.Projects;
using CSOT.Lcd.UserInterface.Common;
using DevExpress.XtraPivotGrid;
using Mozart.Studio.UIComponents;
using Mozart.Studio.TaskModel.Utility;
using Mozart.Studio.Application;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.Scheduling;
using CSOT.Lcd.Scheduling.Outputs;
using DevExpress.XtraCharts;
using CSOT.Lcd.Scheduling.Inputs;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class StepWipView : XtraPivotGridControlView
    {
        private const string _pageID = "StepWipView";
        private IExperimentResultItem _result;
        private ResultDataContext _resultCtx;

        private Dictionary<string, ResultItem> _dict;
        private SortedSet<string> _dayHourList; 
        private IVsApplication _application;
        private List<StdStep> _stdStepList;

        private bool _isWideMode = true;
        private Dictionary<Control, Point> _widePosition;
        private Dictionary<Control, Point> _narrowPosition;
        private Dictionary<string, int> _stepIndexs;
                
        private DateTime QueryStartTime
        {
            get { return this.StartDatePicker.DateTime; }
        }

        private DateTime QueryEndTime
        {
            get { return this.StartDatePicker.DateTime.AddHours(Convert.ToInt32(this.EndCountPicker.Value) * ShopCalendar.ShiftHours); }
        }

        private string TargetAreaID
        {
            get
            { return this.areaComboBox.Text; }
        }
        
        private string TargetShopID
        {
            get
            {
                return this.ShopIDComboBox.Text;
            }
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

        private bool UseOrigProdVer
        {
            get
            {
                if (this.chkOrigProdVer.Checked == true)
                    return true;
                return false;
            }
        }

        public StepWipView()
        {
            InitializeComponent();
            InitMyComponent();
        }

        public StepWipView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
            InitMyComponent();
        }

        private void InitMyComponent()
        {
            _widePosition = new Dictionary<Control, Point>();
            _narrowPosition = new Dictionary<Control, Point>();

            this.SetTaggedControlPosition(this);
        }

        private void SetTaggedControlPosition(Control c)
        {
            if (c.Tag != null)
                _widePosition[c] = c.Location;

            foreach (Control child in c.Controls)
                this.SetTaggedControlPosition(child);
        }

        protected override void LoadDocument()
        {
            var item = (IMenuDocItem)this.Document.ProjectItem;

            _result = (IExperimentResultItem)item.Arguments[0];
            _application = (IVsApplication)GetService(typeof(IVsApplication));
            _resultCtx = _result.GetCtx<ResultDataContext>();

            Globals.InitFactoryTime(_result.Model);

            SetControl();
        }

        #region SetControl

        private void SetControl()
        {
            SetAreaIDComboBox();
            SetShopIDComboBox(this.TargetAreaID);
            SetStepIDCombo(this.TargetShopID);                
                        
            this.StartDatePicker.Properties.EditMask = "yyyy-MM-dd HH:mm:ss";
            this.StartDatePicker.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.StartDatePicker.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;
            this.StartDatePicker.DateTime = _result.StartTime;

            this.EndCountPicker.Value = Globals.GetResultPlanPeriod(_result) * 2;                  
        }

        private void SetAreaIDComboBox()
        {
            this.areaComboBox.Items.Clear();

            SortedSet<string> list = new SortedSet<string>();

            var stepWip = _resultCtx.StepWip;
            foreach (StepWip item in stepWip)
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

        private void SetShopIDComboBox(string targetAreaID)
        {
            this.ShopIDComboBox.Items.Clear();

            if (StringHelper.Equals(StepWipData.TFT, targetAreaID))
                this.ShopIDComboBox.Items.Add(Consts.ALL);

            SortedSet<string> list = new SortedSet<string>();

            foreach (StepWip item in _resultCtx.StepWip)
            {
                if (StringHelper.Equals(item.AREA_ID, targetAreaID) == false)
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

        private void SetStepIDCombo(string targetShopID)
        {
            this.StepComboBox.Items.Clear();
            this.StepComboBox.Items.Add(Consts.ALL);

            DateTime maxDate = DateTime.MinValue;

            var stepWip = _resultCtx.StepWip;

            SortedSet<string> stepList = new SortedSet<string>();
            foreach (StepWip it in stepWip)
            {
                if (string.IsNullOrEmpty(targetShopID) == false)
                {
                    if (targetShopID != Consts.ALL)
                    {
                        if (it.SHOP_ID != targetShopID)
                            continue;
                    }
                }

                if (stepList.Contains(it.STD_STEP_ID) == false)
                    stepList.Add(it.STD_STEP_ID);
            }

            foreach (string stepID in stepList)
            {
                this.StepComboBox.Items.Add(stepID);
            }

            this.StepComboBox.SelectedIndex = 0;
        }

        #endregion

        #region BindData

        private void Query(DataSet layoutDs = null, string activerFilter = null)
        {            
            LoadData();

            BindData();

            SetDefaultLayOut(layoutDs, activerFilter);
        }

        private void SetDefaultLayOut(DataSet layoutDs = null, string activerFilter = null)
        {
            var grid = this.pivotGridControl1;
            if (layoutDs != null)
            {
                PivotGridLayoutHelper.ApplyLayOutFromPivotGrid(grid, layoutDs);
            }
            else
            {
                string dirPath = string.Format("{0}\\DefaultLayOut", _application.ApplicationPath);
                string fileName = string.Format("{0}.xml", _pageID);

                PivotGridLayoutHelper.LoadXml(grid, dirPath, fileName);
            }

            grid.ActiveFilterString = activerFilter;

            ShowTotal(this.pivotGridControl1);
        }

        private void SetDateRanges()
        {
            _dayHourList = new SortedSet<string>();

            //차트 간격 변경
            float interval = 1f;

            DateTime st = this.QueryStartTime;
            DateTime et = this.QueryEndTime;

            //add PlanStartTime
            _dayHourList.Add(GetDateString(st));

            int baseMinute = ShopCalendar.StartTime.Minutes;
            DateTime baseT = DateHelper.GetRptDate_1Hour(st, baseMinute);

            for (DateTime t = baseT; t < et; t = t.AddHours(interval))
            {
                string str = GetDateString(t);
                _dayHourList.Add(str);
            }
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
        
        private void LoadData()
        {
            _dict = new Dictionary<string, ResultItem>();

            string areaID = this.TargetAreaID;
            string shopID = this.TargetShopID;
            bool isFirst = true;

            SetDateRanges();

            _stdStepList = GetStdStepList(areaID);            

            bool isOnlyMainStep = this.ShowSubStep == false;
            bool useOrigProdVer = this.UseOrigProdVer;

            var stepWip = _resultCtx.StepWip;
            foreach (var item in stepWip)
            {
                if (areaID != item.AREA_ID)
                    continue;

                if (item.SHOP_ID != shopID && shopID != Consts.ALL)
                    continue;

                if (item.PLAN_DATE < this.QueryStartTime)
                    continue;

                if (item.PLAN_DATE >= this.QueryEndTime)
                    continue;

                if (this.StepComboBox.Text != Consts.ALL && !item.STD_STEP_ID.Contains(this.StepComboBox.Text.ToString()))
                    continue;

                string stepID = item.STEP_ID;
                string stdStepID = item.STD_STEP_ID;
                int stepSeq = item.STD_STEP_SEQ;

                if (isOnlyMainStep)
                {
                    var stdStep = FindMainStep(item.SHOP_ID, stdStepID);
                    if (stdStep == null)
                        continue;

                    stepID = stdStep.STEP_ID;
                    stdStepID = stdStep.STEP_ID;
                    stepSeq = stdStep.STEP_SEQ;
                }

                string productVersion = item.PRODUCT_VERSION;
                if (useOrigProdVer)
                    productVersion = item.ORIG_PRODUCT_VERSION;

                if (isFirst)
                {
                    foreach (string date in _dayHourList)
                    {
                        ResultItem padding;

                        string dateString = GetDateString(date);

                        string k = item.SHOP_ID + item.PRODUCT_ID + productVersion + item.OWNER_TYPE + stdStepID + dateString;

                        if (_dict.TryGetValue(k, out padding) == false)
                        {
                            padding = new ResultItem();

                            padding.LINE_ID = item.SHOP_ID;
                            padding.STD_STEP = item.STD_STEP_ID;
                            padding.STEP_SEQ = item.STD_STEP_SEQ;

                            padding.PROD_ID = item.PRODUCT_ID;
                            padding.PROD_VER = productVersion;
                            padding.OWNER_TYPE = item.OWNER_TYPE;
                            
                            padding.DATE_INFO = dateString;

                            padding.WAIT_QTY = 0;
                            padding.RUN_QTY = 0;

                            _dict.Add(k, padding);
                        }
                    }

                    isFirst = false;
                }


                ResultItem ri;
                
                DateTime planDate = item.PLAN_DATE;
                DateTime shift = ShopCalendar.ShiftStartTimeOfDayT(planDate);
                
                string dateStr2 = GetDateString(planDate);

                string key = item.SHOP_ID + item.PRODUCT_ID + productVersion + item.OWNER_TYPE + stdStepID + dateStr2;

                if (_dict.TryGetValue(key, out ri) == false)
                {
                    
                    ri = new ResultItem();

                    ri.LINE_ID = item.SHOP_ID;                    
                    ri.PROD_ID = item.PRODUCT_ID;
                    ri.PROD_VER = productVersion;
                    ri.OWNER_TYPE = item.OWNER_TYPE;                    
                    ri.STD_STEP = item.STD_STEP_ID;
                    ri.STEP_SEQ = item.STD_STEP_SEQ;
                    ri.DATE_INFO = dateStr2;

                    _dict.Add(key, ri);
                }

                ri.WAIT_QTY += Convert.ToSingle(item.WAIT_QTY);
                ri.RUN_QTY += Convert.ToSingle(item.RUN_QTY);
            }
        }

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

        private StdStep FindMainStep(string shopID, string stdStepID)
        {
            var list = _stdStepList;

            int index = list.FindIndex(t => t.SHOP_ID == shopID && t.STEP_ID == stdStepID);
            if (index < 0)
                return null;

            int count = list.Count;
            for (int i = index; i < count; i++)
            {
                var stdStep = list[i];
                if (StringHelper.Equals(stdStep.STEP_TYPE, "MAIN"))
                    return stdStep;
            }

            return null;
        }

        private void BindData()
        {
            var dt = CreateDataViewTable();

            FillData(dt);                        
            DrawGrid(dt);
        }

        private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        {
            XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();

            dt.AddColumn(StepWipData.SHOP_ID, StepWipData.SHOP_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(StepWipData.STD_STEP, StepWipData.STD_STEP, typeof(string), PivotArea.RowArea, null, null);

            dt.AddColumn(StepWipData.PRODUCT_ID, StepWipData.PRODUCT_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(StepWipData.PRODUCT_VERSION, StepWipData.PRODUCT_VERSION, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(StepWipData.OWNER_TYPE, StepWipData.OWNER_TYPE, typeof(string), PivotArea.RowArea, null, null);
                                   
            dt.AddColumn(StepWipData.TARGET_DATE, StepWipData.TARGET_DATE, typeof(string), PivotArea.ColumnArea, null, null);

            dt.AddColumn(StepWipData.WAIT_QTY, StepWipData.WAIT_QTY, typeof(float), PivotArea.DataArea, null, null);
            dt.AddColumn(StepWipData.RUN_QTY, StepWipData.RUN_QTY, typeof(float), PivotArea.DataArea, null, null);
            dt.AddColumn(StepWipData.TOTAL_QTY, StepWipData.TOTAL_QTY, typeof(float), PivotArea.DataArea, null, null);

            dt.Columns[StepWipData.TOTAL_QTY].DefaultValue = 0.0f;

            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns[StepWipData.SHOP_ID],
                        dt.Columns[StepWipData.STD_STEP],
                        dt.Columns[StepWipData.PRODUCT_ID],
                        dt.Columns[StepWipData.PRODUCT_VERSION],
                        dt.Columns[StepWipData.OWNER_TYPE],                        
                        dt.Columns[StepWipData.TARGET_DATE]
                    }
                );

            return dt;
        }

        private void FillData(XtraPivotGridHelper.DataViewTable dt)
        {
            string targetAreaID = this.TargetAreaID;
            bool isOnlyMainStep = this.ShowSubStep == false;

            var modelContext = _result.GetCtx<ModelDataContext>();
            var stdStepList = _stdStepList;
            if (isOnlyMainStep)
                stdStepList = stdStepList.FindAll(t => StringHelper.Equals(t.STEP_TYPE, "MAIN"));

            var stepWip = _dict.Values;

            var query = from ps in stdStepList
                        join p in stepWip
                        on ps.SHOP_ID + ps.STEP_ID equals p.LINE_ID + p.STD_STEP into temp
                        from tp in temp.DefaultIfEmpty()
                        select new
                        {
                            SHOP_ID = tp != null ? tp.LINE_ID : ps.SHOP_ID,
                            STD_STEP_ID = tp != null ? tp.STD_STEP : ps.STEP_ID,
                            PROD_ID = tp != null ? tp.PROD_ID : null,
                            PROD_VER = tp != null ? tp.PROD_VER : null,
                            OWNER_TYPE = tp != null ? tp.OWNER_TYPE : null,                            
                            DATE_INFO = tp != null ? tp.DATE_INFO : null,
                            WAIT_QTY = tp != null ? tp.WAIT_QTY : 0,
                            RUN_QTY = tp != null ? tp.RUN_QTY : 0,
                            STD_STEP_SEQ = tp != null ? tp.STEP_SEQ : ps.STEP_SEQ,
                        };

            _stepIndexs = new Dictionary<string, int>();

            var table = dt.DataTable;

            foreach (var item in query)
            {               
                if (item.PROD_ID == null)
                    continue;
                
                table.Rows.Add(item.SHOP_ID,
                               item.STD_STEP_ID,
                               item.PROD_ID,
                               item.PROD_VER,
                               item.OWNER_TYPE,
                               item.DATE_INFO,
                               item.WAIT_QTY,
                               item.RUN_QTY,
                               item.WAIT_QTY + item.RUN_QTY);

                string stepKey = item.STD_STEP_ID;
                _stepIndexs[stepKey] = item.STD_STEP_SEQ;
            }
        }

        #endregion

        #region DrawGrid

        private void DrawGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            this.pivotGridControl1.BeginUpdate();

            this.pivotGridControl1.ClearPivotGridFields();
            this.pivotGridControl1.CreatePivotGridFields(dt);

            this.pivotGridControl1.DataSource = dt.DataTable;

            this.pivotGridControl1.OptionsView.ShowFilterHeaders = true;

            for (int i = 0; i < this.pivotGridControl1.Fields.Count; i++)
            {
                if (this.pivotGridControl1.Fields[i].FieldName == "WAIT_QTY" || this.pivotGridControl1.Fields[i].FieldName == "RUN_QTY")
                    this.pivotGridControl1.Fields[i].Area = PivotArea.FilterArea;
            }
            pivotGridControl1.CustomCellDisplayText += pivotGridControl1_CellDisplayText;

            pivotGridControl1.Fields[StepWipData.TOTAL_QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[StepWipData.TOTAL_QTY].CellFormat.FormatString = "#,##0";

            pivotGridControl1.Fields[StepWipData.RUN_QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[StepWipData.RUN_QTY].CellFormat.FormatString = "#,##0";

            pivotGridControl1.Fields[StepWipData.WAIT_QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[StepWipData.WAIT_QTY].CellFormat.FormatString = "#,##0";

            this.pivotGridControl1.Fields[StepWipData.STD_STEP].SortMode = PivotSortMode.Custom;

            this.pivotGridControl1.EndUpdate();
            pivotGridControl1.BestFitColumnArea();
            
        }

        #endregion

        #region DrawChart

        private void ShowTotal(PivotGridControl pivot, bool isCheck = false)
        {
            pivot.OptionsView.ShowRowTotals = false;
            pivot.OptionsView.ShowRowGrandTotals = false;
            pivot.OptionsView.ShowColumnTotals = isCheck;
            pivot.OptionsView.ShowColumnGrandTotals = isCheck;
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

        private void FillChart(PivotGridControl pivotGrid, List<int> rows)
        {
            XYDiagram xyDiagram = (XYDiagram)this.chartControl.Diagram;
            if (xyDiagram != null)
            {
                xyDiagram.AxisX.Label.Angle = 45;
                xyDiagram.AxisX.Label.ResolveOverlappingOptions.AllowHide = false;
                xyDiagram.AxisX.NumericScaleOptions.AutoGrid = false;
                xyDiagram.EnableAxisXScrolling = false;
                xyDiagram.EnableAxisYScrolling = true;
                xyDiagram.AxisX.Label.Font = new System.Drawing.Font("Tahoma", 7F);
            }

            this.chartControl.Series.Clear();

            var dataAreaList = GetDataAreaList(pivotGrid);

            DataTable dataViewTable = (DataTable)pivotGrid.DataSource;
            var colNameList = GetColNameList(pivotGrid);

            int i = 0;
            foreach (int rowIdx in rows)
            {
                var valueList = GetRowValueList(pivotGrid, rowIdx);
                DataView dv = SummaryData(dataViewTable, colNameList, valueList);

                string seriesName = StringHelper.ConcatKey(valueList.ToArray());

                this.chartControl.Series.Add(seriesName, ViewType.Line);
                this.chartControl.Series[i].ArgumentDataMember = StepWipData.TARGET_DATE;
                this.chartControl.Series[i].ValueDataMembers.AddRange(dataAreaList);

                this.chartControl.Series[i].DataSource = dv;
                this.chartControl.Series[i].LegendText = string.IsNullOrEmpty(seriesName) ? "Total" : seriesName;
                i++;
            }
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

                string dayHour = row.GetString(StepWipData.TARGET_DATE);
                pks[keyColCount] = dayHour;

                DataRow findRow = result.Rows.Find(pks);
                if (findRow == null)
                    continue;

                int waitQty = row.GetInt32(StepWipData.WAIT_QTY);
                if (waitQty > 0)
                    findRow[StepWipData.WAIT_QTY] = findRow.GetInt32(StepWipData.WAIT_QTY) + waitQty;

                int runQty = row.GetInt32(StepWipData.RUN_QTY);
                if (runQty > 0)
                    findRow[StepWipData.RUN_QTY] = findRow.GetInt32(StepWipData.RUN_QTY) + runQty;

                int totalQty = row.GetInt32(StepWipData.TOTAL_QTY);
                if (totalQty > 0)
                    findRow[StepWipData.TOTAL_QTY] = findRow.GetInt32(StepWipData.TOTAL_QTY) + totalQty;
            }

            return new DataView(result, string.Empty, StepWipData.TARGET_DATE, DataViewRowState.CurrentRows);
        }

        private DataTable CreateSummaryTable(List<string> colNameList)
        {
            DataTable result = new DataTable();

            List<DataColumn> pkList = new List<DataColumn>();
            foreach (var colName in colNameList)
            {
                var col = result.Columns.Add(colName, typeof(string));
                pkList.Add(col);
            }

            var tcol = result.Columns.Add(StepWipData.TARGET_DATE, typeof(string));
            pkList.Add(tcol);

            result.Columns.Add(StepWipData.WAIT_QTY, typeof(int));
            result.Columns.Add(StepWipData.RUN_QTY, typeof(int));
            result.Columns.Add(StepWipData.TOTAL_QTY, typeof(int));
            

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

                row[StepWipData.TARGET_DATE] = dayHour;
                row[StepWipData.WAIT_QTY] = 0;
                row[StepWipData.RUN_QTY] = 0;
                row[StepWipData.TOTAL_QTY] = 0;

                result.Rows.Add(row);
            }
        }

        #endregion

        #region Internal Class : ResultItem
        internal class ResultItem
        {
            public string LINE_ID;
            public string PROD_ID;
            public string PROD_VER;
            public string OWNER_TYPE;
            public string STD_STEP;
            public int STEP_SEQ;
            public string DATE_INFO;
            public float WAIT_QTY;
            public float RUN_QTY;
        }
        #endregion

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

        private void btnQuery_Click(object sender, EventArgs e)
        {
            var grid = this.pivotGridControl1;
            var layoutDs = PivotGridLayoutHelper.GetLayOutFromPivotGrid(grid);
            var activerFilter = this.pivotGridControl1.ActiveFilterString;

            Query(layoutDs, activerFilter);
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

        private void ShopComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetStepIDCombo(this.TargetShopID);
        }

        private void StepWipView_SizeChanged(object sender, EventArgs e)
        {
            if (this._isWideMode && this.Width <= 923)
            {
                this.ChangeTagControlPosition(_narrowPosition);
                this._isWideMode = false;
            }
            else if (!this._isWideMode && this.Width > 923)
            {
                this.ChangeTagControlPosition(_widePosition);
                this._isWideMode = true;
            }
        }

        private void ChangeTagControlPosition(Dictionary<Control, Point> dic)
        {
            foreach (var c in dic)
            {
                var control = c.Key;
                var point = c.Value;

                control.Location = point;
            }
        }

        private void pivotGridControl1_CellDoubleClick(object sender, PivotCellEventArgs e)
        {            
            string targetAreaID = this.TargetAreaID;
            string targetTime = pivotGridControl1.GetFieldValue(e.ColumnField, e.ColumnIndex) as string;

            ShowStepWipSnapshot(targetAreaID, targetTime);
        }

        private void ShowStepWipSnapshot(string targetAreaID, string targetTime)
        {
            var view = new StepWipSnapshotView(this.ServiceProvider, _result, targetAreaID, targetTime);

            var dialog = new Common.PopUpForm(view);
            dialog.StartPosition = FormStartPosition.CenterParent;            
            dialog.Show();
        }

        private void pivotGridControl1_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        {
            if (e.Field.FieldName != StepWipData.STD_STEP)
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

        private void pivotGridControl1_CellSelectionChanged(object sender, EventArgs e)
        {
            PivotGridCells cells = pivotGridControl1.Cells;
            DevExpress.XtraPivotGrid.Selection.IMultipleSelection selList = cells.MultiSelection;

            List<int> rows = new List<int>();
            foreach (Point pt in selList.SelectedCells)
            {
                if (rows.Contains(pt.Y) == false)
                    rows.Add(pt.Y);
            }

            FillChart((PivotGridControl)sender, rows);
        }

        private void areaComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            SetShopIDComboBox(this.TargetAreaID);
        }

        private void pivotGridControl1_CellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
        {
            if (e.GetFieldValue(e.DataField) != null && e.GetFieldValue(e.DataField).ToString() == "0")
            {
                e.DisplayText = string.Empty;
            }
        }
    }
}
