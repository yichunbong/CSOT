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
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling;
using CSOT.Lcd.Scheduling.Inputs;
using DevExpress.XtraCharts;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class StepWipSnapshotView  : XtraPivotGridControlView
    {
        private const string _pageID = "StepWipSnapshotView";
        private string _dirPath;
        private string _fileName;
        
        private string _targetAreaID;
        private string _targetTime;

        private IVsApplication _application;
        private IExperimentResultItem _result;

        private ResultDataContext _resultCtx;

        private Dictionary<string, ResultItem> _dict;
        private Dictionary<string, int> _stepIndexs;

        private List<StdStep> _stdStepList;

        private string TargetAreaID
        {
            get { return this.areaComboBox.Text; }
        }
        
        private string TargetDate
        {
            get { return this.TimeComboBox.Text; }
        }

        private bool ShowSubStep
        {
            get
            {
                if (this.chkSubStep.Checked == true)
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

        public StepWipSnapshotView()        
        {
            InitializeComponent();           
        }

        public StepWipSnapshotView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();

            _dirPath = string.Format("{0}\\DefaultLayOut", Application.StartupPath);
            _fileName = string.Format("{0}.xml", _pageID);
        }

        public StepWipSnapshotView(IServiceProvider serviceProvider, IExperimentResultItem result, string targetAreaID, string targetTime)
            : base(serviceProvider)
        {
            InitializeComponent();

            _dirPath = string.Format("{0}\\DefaultLayOut", Application.StartupPath);
            _fileName = string.Format("{0}.xml", _pageID);

            _result = result;
            _targetAreaID = targetAreaID;
            _targetTime = targetTime;

            LoadDocument();
            Query();
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
            SetAreaIDComboBox();
            SetTimeCombo();                                       
        }

        private void SetAreaIDComboBox()
        {
            SortedSet<string> list = new SortedSet<string>();

            var stepWip = _resultCtx.StepWip;
            foreach (StepWip item in stepWip)
            {
                if (list.Contains(item.AREA_ID))
                    continue;

                list.Add(item.AREA_ID);
            }

            var control = this.areaComboBox;

            foreach (string areaID in list.Reverse())
            {
                if (control.Items.Contains(areaID))
                    continue;

                control.Items.Add(areaID);
            }

            if (control.Items.Count > 0)
                control.SelectedIndex = 0;

            if (string.IsNullOrEmpty(_targetAreaID) == false)
            {
                if (control.Items.Contains(_targetAreaID))
                    control.SelectedIndex = control.Items.IndexOf(_targetAreaID);
            }
        }

        private void SetTimeCombo()
        {            
            HashSet<string> timeSet = new HashSet<string>();
            foreach (StepWip item in _resultCtx.StepWip)
            {
                timeSet.Add(item.PLAN_DATE.ToString("yyyyMMddHHmm"));
            }

            var control = this.TimeComboBox;

            foreach (string str in timeSet)
                control.Items.Add(str);

            if (control.Items.Count > 0)
                control.SelectedIndex = 0;

            if (string.IsNullOrEmpty(_targetTime) == false)
            {
                if (control.Items.Contains(_targetTime))
                    control.SelectedIndex = control.Items.IndexOf(_targetTime);
            }
        }  
        
        #endregion

        private class ResultItem
        {
            public string AREA_ID;
            public string SHOP_ID;
            public string PROD_ID;
            public string PROD_VER;
            public string OWNER_TYPE;
            public string STEP_ID;
            public string DATE_INFO;
            public float WAIT_QTY;
            public float RUN_QTY;
            public string STD_STEP_ID;
            public int STEP_SEQ;
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
        
        private void LoadData()
        {
            _resultCtx = _result.GetCtx<ResultDataContext>();
            var sw = _resultCtx.StepWip;

            _dict = new Dictionary<string, ResultItem>();
            
            string targetDate = this.TargetDate;
            string areaID = this.TargetAreaID;

            _stdStepList = GetStdStepList(areaID);
            
            bool isOnlyMainStep = this.ShowSubStep == false;
            bool useOrigProdVer = this.UseOrigProdVer;

            foreach (StepWip item in sw)
            {
                if (areaID != item.AREA_ID)
                    continue;
                               
                string dateStr = item.PLAN_DATE.ToString("yyyyMMddHHmm");
                if (dateStr != targetDate)
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
                
                ResultItem ri;
                string key = CommonHelper.CreateKey(item.SHOP_ID, stdStepID, item.PRODUCT_ID, productVersion, item.OWNER_TYPE);
                if (_dict.TryGetValue(key, out ri) == false)
                {                    

                    ri = new ResultItem();

                    ri.AREA_ID = item.AREA_ID;
                    ri.SHOP_ID = item.SHOP_ID;
                    ri.PROD_ID = item.PRODUCT_ID;
                    ri.PROD_VER = productVersion;
                    ri.OWNER_TYPE = item.OWNER_TYPE;

                    ri.STEP_ID = stepID;
                    ri.STD_STEP_ID = stdStepID;
                    ri.STEP_SEQ = stepSeq;

                    ri.DATE_INFO = dateStr;

                    _dict.Add(key, ri);
                }

                ri.WAIT_QTY += Convert.ToSingle(item.WAIT_QTY);
                ri.RUN_QTY += Convert.ToSingle(item.RUN_QTY);
            }
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
            dt.AddColumn(StepWipData.STD_STEP, StepWipData.STD_STEP, typeof(string), PivotArea.ColumnArea, null, null);

            dt.AddColumn(StepWipData.PRODUCT_ID, StepWipData.PRODUCT_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(StepWipData.PRODUCT_VERSION, StepWipData.PRODUCT_VERSION, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(StepWipData.OWNER_TYPE, StepWipData.OWNER_TYPE, typeof(string), PivotArea.RowArea, null, null);            

            dt.AddColumn(StepWipData.WAIT_QTY, StepWipData.WAIT_QTY, typeof(float), PivotArea.DataArea, null, null);
            dt.AddColumn(StepWipData.RUN_QTY, StepWipData.RUN_QTY, typeof(float), PivotArea.DataArea, null, null);
            dt.AddColumn(StepWipData.TOTAL_QTY, StepWipData.TOTAL_QTY, typeof(float), PivotArea.DataArea, null, null);

            dt.Columns[StepWipData.TOTAL_QTY].DefaultValue = 0.0f;

            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns[StepWipData.STD_STEP],
                        dt.Columns[StepWipData.SHOP_ID],
                        dt.Columns[StepWipData.PRODUCT_ID],
                        dt.Columns[StepWipData.PRODUCT_VERSION],
                        dt.Columns[StepWipData.OWNER_TYPE]                        
                    }
                );

            return dt;
        }
                                
        private void FillData(XtraPivotGridHelper.DataViewTable dt)
        {
            string targetAreaID = this.TargetAreaID;
            bool isOnlyMainStep = this.ShowSubStep == false;

            var stdStepList = _stdStepList;
            if (isOnlyMainStep)
                stdStepList = stdStepList.FindAll(t => StringHelper.Equals(t.STEP_TYPE, "MAIN"));

            var stepWip = _dict.Values;

            var query = from ps in stdStepList
                        join p in stepWip
                        on ps.SHOP_ID + ps.STEP_ID equals p.SHOP_ID + p.STD_STEP_ID into temp
                        from tp in temp.DefaultIfEmpty()
                        select new
                        {
                            AREA_ID = tp != null ? tp.AREA_ID : ps.AREA_ID,
                            SHOP_ID = tp != null ? tp.SHOP_ID : ps.SHOP_ID,
                            PROD_ID = tp != null ? tp.PROD_ID : null,
                            PROD_VER = tp != null ? tp.PROD_VER : null,
                            OWNER_TYPE = tp != null ? tp.OWNER_TYPE : null,
                            STD_STEP_ID = tp != null ? tp.STD_STEP_ID : ps.STEP_ID,
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
                               item.WAIT_QTY,
                               item.RUN_QTY,
                               item.WAIT_QTY + item.RUN_QTY);

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

            this.pivotGridControl1.EndUpdate();
            pivotGridControl1.BestFitColumnArea();

            this.pivotGridControl1.Fields[StepWipData.STD_STEP].SortMode = PivotSortMode.Custom;
        }

        private void ShowTotal(PivotGridControl pivot, bool isCheck)
        {
            pivot.OptionsView.ShowRowTotals = false;
            pivot.OptionsView.ShowRowGrandTotals = false;
            pivot.OptionsView.ShowColumnTotals = isCheck;
            pivot.OptionsView.ShowColumnGrandTotals = isCheck;
        }

        #region Event Handlers

        private void btnQuery_Click(object sender, EventArgs e)
        {
            var grid = this.pivotGridControl1;
            var layoutDs = PivotGridLayoutHelper.GetLayOutFromPivotGrid(grid);
            var activerFilter = this.pivotGridControl1.ActiveFilterString;

            Query(layoutDs, activerFilter);
        }

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

            ShowTotal(this.pivotGridControl1, chkViewTotal.Checked);
        }

        private void chkViewTotal_CheckedChanged(object sender, EventArgs e)
        {
            ShowTotal(this.pivotGridControl1, this.chkViewTotal.Checked);
        }

        private void pivotGridControl1_CellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
        {
            if (e.DataField == null)
                return;

            if (e.GetFieldValue(e.DataField) != null && e.GetFieldValue(e.DataField).ToString() == "0")
            {
                e.DisplayText = string.Empty;
            }
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

            int seq;
            if (_stepIndexs.TryGetValue(stepID, out seq))
                return seq;

            return 999;
        }

        #endregion

        #region //Chart
      
        private void FillChart(PivotGridControl pivotGrid, List<int> rows)
        {
            XYDiagram xyDiagram = (XYDiagram)chartControl.Diagram;
            if (xyDiagram != null)
            {
                xyDiagram.AxisX.Label.Angle = 90;

                xyDiagram.AxisX.Label.Staggered = false;
                xyDiagram.AxisX.Label.ResolveOverlappingOptions.AllowHide = false;
                xyDiagram.AxisX.NumericScaleOptions.AutoGrid = false;
                xyDiagram.AxisX.QualitativeScaleOptions.AutoGrid = false;
                xyDiagram.EnableAxisXScrolling = true;
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

                AddSeries(seriesName, ViewType.StackedBar, dataAreaList, dv);

                //if (onlyOne)
                //    AddSeries(seriesName, ViewType.Bar, dataAreaList, dv);
            }
        }

        private void AddSeries(string seriesName, ViewType viewType, string[] dataAreaList, DataView dv)
        {
            int index = chartControl.Series.Add(seriesName, viewType);

            chartControl.Series[index].ArgumentDataMember = StepWipData.STD_STEP;            
            chartControl.Series[index].ValueDataMembers.AddRange(dataAreaList);

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

                string stdStep = row.GetString(StepWipData.STD_STEP);
                pks[keyColCount] = stdStep;

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

            return new DataView(result, string.Empty, string.Empty, DataViewRowState.CurrentRows);
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

            var scol = result.Columns.Add(StepWipData.STD_STEP, typeof(string));
            pkList.Add(scol);

            result.Columns.Add(StepWipData.WAIT_QTY, typeof(int));
            result.Columns.Add(StepWipData.RUN_QTY, typeof(int));
            result.Columns.Add(StepWipData.TOTAL_QTY, typeof(int));

            result.PrimaryKey = pkList.ToArray();

            return result;
        }

        private void FillRowZero(DataTable result, List<string> valueList)
        {
            var stdStepList = _stdStepList;
            if (stdStepList == null || stdStepList.Count == 0)
                return;

            bool isOnlyMainStep = this.ShowSubStep == false;
            if (isOnlyMainStep)
                stdStepList = stdStepList.FindAll(t => StringHelper.Equals(t.STEP_TYPE, "MAIN"));

            int count = valueList.Count;
            foreach (var stdStep in stdStepList.OrderBy(t => t.STEP_SEQ))
            {
                DataRow row = result.NewRow();

                for (int i = 0; i < count; i++)
                    row[i] = valueList[i];

                row[StepMoveData.STD_STEP] = stdStep.STEP_ID;
                row[StepWipData.WAIT_QTY] = 0;
                row[StepWipData.RUN_QTY] = 0;
                row[StepWipData.TOTAL_QTY] = 0;

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
    }
}
