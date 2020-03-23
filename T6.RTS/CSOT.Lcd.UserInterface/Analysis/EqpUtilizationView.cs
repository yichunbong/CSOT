using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraCharts;
using DevExpress.XtraPivotGrid;

using Mozart.Text;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserInterface;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class EqpUtilizationView : XtraPivotGridControlView
    {
        #region Variables
        private IExperimentResultItem _result;

        private DataTable _dtEquipment;
        private DataTable _dtLoadStat;

        DataTable _eqpInfoDT;

        private DataTable _dtChart;
        private int _chartIndex;

        private bool _isShift = false;

        private Dictionary<string, List<string>> _eqpGrpsInAreaDic;
        private Dictionary<string, string> _areaByEqpGrpDic;
        private List<string> _selectedEqpGrpInAreaList;

        private Dictionary<string, EuData.GroupUtilInfo> _UtilInfos;

        private float _minBusyValue;
        private float _maxBusyValue;

        private Dictionary<string, Color> _originalSeriesColors;

        private int _lastSelectedSeriesIndex = -1;

        private ColorGenerator _colorGenerator;

        private Dictionary<string, string> _eqpDic;
        private Dictionary<string, List<string>> _eqpGroupDic;

        List<string> _chartRsltList;
        #endregion Variables

        #region Properties

        string SelectedShopID
        {
            get { return this.shopIdComboBoxEdit.SelectedItem != null ? this.shopIdComboBoxEdit.SelectedItem.ToString() : "Blank"; }
        }

        bool IsAllAreaSelected
        {
            get
            {
                int totalCnt = this.areaChkBoxEdit.Properties.Items.Count;
                if (totalCnt <= 0)
                    return true;

                if (totalCnt == this.areaChkBoxEdit.Properties.Items.GetCheckedValues().Count)
                    return true;

                return false;
            }
        }

        List<string> SelectedEqpGrpInAreaList
        {
            get
            {
                if (_selectedEqpGrpInAreaList != null)
                    return _selectedEqpGrpInAreaList;

                _selectedEqpGrpInAreaList = new List<string>();

                foreach (string area in this.areaChkBoxEdit.Properties.Items.GetCheckedValues())
                {
                    List<string> eqpGrpList;
                    if (_eqpGrpsInAreaDic.TryGetValue(area, out eqpGrpList) == false)
                        continue;

                    _selectedEqpGrpInAreaList.AddRange(eqpGrpList);
                }

                return _selectedEqpGrpInAreaList;
            }
        }

        public bool IncludeJobChgOnUtil
        {
            get { return this.includeJobChgOnUtil.Checked; }
        }

        public bool IsUseUtilLowerLimit
        {
            get { return this.utilLowerLimitCondChkBox.Checked; }
        }

        public int UtilConditionVal
        {
            get { return (int)this.utilConditionValSpinEdit.Value; }
        }

        public DateTime FromDate
        {
            get { return this.fromDateEdit.DateTime.Date; }
        }

        public DateTime ToDate
        {
            get
            {
                DateTime dt = this.toDateEdit.DateTime;
                return new DateTime(dt.Year, dt.Month, dt.Day).AddDays(1).AddSeconds(-1);
            }
        }
        #endregion Properties

        #region Constructor
        public EqpUtilizationView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }
        #endregion Constructor

        #region Methods
        protected override void LoadDocument()
        {
            InitializeData();

            SetControls();
        }

        private void InitializeData()
        {
            var item = (IMenuDocItem)this.Document.ProjectItem;
            _result = (IExperimentResultItem)item.Arguments[0];

            Globals.InitFactoryTime(_result.Model);

            //base.SetWaitDialogCaption(DataConsts.In_StdStep);
            //_dtStdStep = _result.LoadInput(DataConsts.In_StdStep);

            base.SetWaitDialogCaption(EuData.DATA_TABLE_1);
            _dtEquipment = _result.LoadInput(EuData.DATA_TABLE_1);

            base.SetWaitDialogCaption(EuData.DATA_TABLE_2);
            _dtLoadStat = _result.LoadOutput(EuData.DATA_TABLE_2);
        }

        private void SetControls()
        {
            expandablePanel1.Text = EuData.TITLE;
            //label2.Text = EuData.QUERY_RANGE;

            dockPanel1.Text = EuData.CHART_TITLE;

            // ShopID ComboBox
            ComboHelper.AddDataToComboBox(this.shopIdComboBoxEdit, _result,
                SimInputData.InputName.Eqp, SimInputData.EqpSchema.SHOP_ID, false);

            
            SetAreaControl();

            this.fromDateEdit.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.fromDateEdit.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;
            this.toDateEdit.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.toDateEdit.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            this.fromDateEdit.DateTime = _result.StartTime;//.Date; new DateTime(PlanStartTime.Year, PlanStartTime.Month, 1);
            this.toDateEdit.DateTime = this.fromDateEdit.DateTime.AddDays(Globals.GetResultPlanPeriod(_result) - 1);//  this.fromDateEdit.DateTime.AddMonths(1).AddSeconds(-1);
        }

        private void SetAreaControl()
        {
            _selectedEqpGrpInAreaList = null;
            _eqpGrpsInAreaDic = new Dictionary<string, List<string>>();
            _areaByEqpGrpDic = new Dictionary<string, string>();

            var modelContext = this._result.GetCtx<ModelDataContext>();

            this._eqpInfoDT = Globals.GetConsInfo(modelContext);

            //string filter = string.Format("{0} = 'AREA_INFO'", EqpGanttChartData.Const.Schema.CATEGORY);

            //DataRow[] drs = this._eqpInfoDT.Select(filter);

            if (this._eqpInfoDT != null)
            {
                List<string> eqpGrpsAllInAreaList = new List<string>();

                foreach (DataRow row in _eqpInfoDT.Rows)
                {
                    SimInputData.Const configConst = new SimInputData.Const(row);

                    if (this.areaChkBoxEdit.Properties.Items.Contains(configConst.Code) == false)
                        this.areaChkBoxEdit.Properties.Items.Add(configConst.Code);

                    string[] eqpGrps = configConst.Description.Split('@');
                    foreach (string eqpGrp in eqpGrps)
                    {
                        if (eqpGrpsAllInAreaList.Contains(eqpGrp) == false)
                            eqpGrpsAllInAreaList.Add(eqpGrp);

                        List<string> eqpGrpList;
                        if (_eqpGrpsInAreaDic.TryGetValue(configConst.Code, out eqpGrpList) == false)
                            _eqpGrpsInAreaDic.Add(configConst.Code, eqpGrpList = new List<string>());

                        if (eqpGrpList.Contains(eqpGrp) == false)
                            eqpGrpList.Add(eqpGrp);

                        if (_areaByEqpGrpDic.ContainsKey(eqpGrp) == false)
                            _areaByEqpGrpDic.Add(eqpGrp, configConst.Code);
                    }
                }


                //string areaAdd = this.areaChkBoxEdit.Properties.Items.Count > 0 ? "OTHERS" : Consts.ALL;

                //if (this.areaChkBoxEdit.Properties.Items.Contains(areaAdd) == false)
                //    this.areaChkBoxEdit.Properties.Items.Add(areaAdd);

                //var eqpGrpInEqpList = modelContext.Eqp.Select(x => x.EQP_GROUP_ID).Distinct();
                //foreach (var eqpGrp in eqpGrpInEqpList)
                //{
                //    if (eqpGrpsAllInAreaList.Contains(eqpGrp) == false)
                //    {
                //        List<string> eqpGrpList;
                //        if (_eqpGrpsInAreaDic.TryGetValue(areaAdd, out eqpGrpList) == false)
                //            _eqpGrpsInAreaDic.Add(areaAdd, eqpGrpList = new List<string>());

                //        if (eqpGrpList.Contains(eqpGrp) == false)
                //            eqpGrpList.Add(eqpGrp);

                //        if (_areaByEqpGrpDic.ContainsKey(eqpGrp) == false)
                //            _areaByEqpGrpDic.Add(eqpGrp, areaAdd);
                //    }
                //}

                if (this.areaChkBoxEdit.Properties.Items.Count > 0)
                    this.areaChkBoxEdit.CheckAll();
            }
        }

        private void ClearData()
        {
            if (_UtilInfos == null)
                _UtilInfos = new Dictionary<string, EuData.GroupUtilInfo>();
            else
                _UtilInfos.Clear();

            //_totalUtilInfo = null;

            this.pivotGridControl1.DataSource = null;

            _minBusyValue = 100;
            _maxBusyValue = 0;

            _colorGenerator = new ColorGenerator();
            _lastSelectedSeriesIndex = -1;
        }

        private void BindData()
        {
            _eqpDic = new Dictionary<string, string>();
            _eqpGroupDic = new Dictionary<string, List<string>>();

            foreach (DataRow row in _dtEquipment.Rows)
            {
                EuData.Eqp eqp = new EuData.Eqp(row);

                if (this.SelectedShopID != eqp.ShopID)
                    continue;

                if (this.IsAllAreaSelected == false && this.SelectedEqpGrpInAreaList.Contains(eqp.DspEqpGroup) == false)
                    continue;

                string area = Consts.NULL_ID;
                _areaByEqpGrpDic.TryGetValue(eqp.DspEqpGroup, out area);
                area = string.IsNullOrEmpty(area) ? Consts.NULL_ID : area;

                string info = CommonHelper.CreateKey(eqp.ShopID, area, eqp.DspEqpGroup);
                _eqpDic[eqp.EqpID] = info;

                List<string> list;

                if (_eqpGroupDic.TryGetValue(eqp.DspEqpGroup, out list) == false)
                    _eqpGroupDic[eqp.DspEqpGroup] = list = new List<string>();

                if (list.Contains(eqp.EqpID) == false)
                    list.Add(eqp.EqpID);
            }

            foreach (DataRow srow in _dtLoadStat.Rows)
            {
                EuData.LoadStat loadStat = new EuData.LoadStat(srow);

                DateTime date = new DateTime(loadStat.TargetDate.Year, loadStat.TargetDate.Month, loadStat.TargetDate.Day);

                string info;
                if (_eqpDic.TryGetValue(loadStat.EqpID, out info) == false)
                    continue;

                if (date < this.FromDate || date > this.ToDate)
                    continue;

                string shopID = info.Split('@')[0];
                string area = info.Split('@')[1];
                string eqpGrpID = info.Split('@')[2];

                if (area == "-")
                    continue;

                EuData.GroupUtilInfo groupInfo;
                if (_UtilInfos.TryGetValue(info, out groupInfo) == false)
                    _UtilInfos[info] = groupInfo = new EuData.GroupUtilInfo(shopID, area, eqpGrpID);

                groupInfo.AddData(loadStat.EqpID, date, loadStat.Setup, loadStat.Busy, loadStat.IdleRun, loadStat.Idle, loadStat.PM, loadStat.Down, _isShift);

                string info2 = CommonHelper.CreateKey(shopID, area, EuData.FOR_AREA);

                EuData.GroupUtilInfo groupInfo2;
                if (_UtilInfos.TryGetValue(info2, out groupInfo2) == false)
                    _UtilInfos[info2] = groupInfo2 = new EuData.GroupUtilInfo(shopID, area, EuData.FOR_AREA);

                groupInfo2.AddData(loadStat.EqpID, date, loadStat.Setup, loadStat.Busy, loadStat.IdleRun, loadStat.Idle, loadStat.PM, loadStat.Down, _isShift);
            }
        }

        private DataTable CreateGridTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(EuData.AREA);
            dt.Columns.Add(EuData.SHOP_ID);
            dt.Columns.Add(EuData.EQP_GROUP_ID);
            //dt.Columns.Add(EuData.AVG, typeof(float));

            for (DateTime date = FromDate; date < ToDate; date = date.AddDays(1))
            {
                string dateCol = date.ToString("MM'/'dd");
                dt.Columns.Add(dateCol, typeof(float));
            }

            return dt;
        }

        private DataTable CreateChartTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(EuData.AREA);
            dt.Columns.Add(EuData.SHOP_ID);
            dt.Columns.Add(EuData.EQP_GROUP_ID);
            dt.Columns.Add(EuData.DATE);
            dt.Columns.Add(EuData.UTILIZATION, typeof(float));

            return dt;
        }

        private void FillPivotGrid()
        {
            _dtChart = CreateChartTable();

            XtraPivotGridHelper.DataViewTable rsltTable = CreateDataViewTable();

            FillData(rsltTable);

            DrawPivotGrid(rsltTable);
        }

        private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        {
            XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();

            dt.AddColumn(EuData.AREA, EuData.AREA, typeof(string), PivotArea.RowArea,
                DevExpress.Data.PivotGrid.PivotSummaryType.Average, null);
            dt.AddColumn(EuData.SHOP_ID, EuData.SHOP_ID, typeof(string), PivotArea.RowArea,
                DevExpress.Data.PivotGrid.PivotSummaryType.Average, null);
            dt.AddColumn(EuData.EQP_GROUP_ID, EuData.EQP_GROUP_ID, typeof(string), PivotArea.FilterArea,
                DevExpress.Data.PivotGrid.PivotSummaryType.Average, null);
            dt.AddColumn(EuData.DATE, EuData.DATE, typeof(string), PivotArea.ColumnArea,
                DevExpress.Data.PivotGrid.PivotSummaryType.Average, null);
            dt.AddColumn(EuData.UTILIZATION, EuData.UTILIZATION, typeof(float), PivotArea.DataArea,
                DevExpress.Data.PivotGrid.PivotSummaryType.Average, null);

            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns[EuData.AREA],
                        dt.Columns[EuData.SHOP_ID],
                        dt.Columns[EuData.EQP_GROUP_ID],
                        dt.Columns[EuData.DATE]
                    }
                );

            return dt;
        }

        private XtraPivotGridHelper.DataViewTable FillData(XtraPivotGridHelper.DataViewTable dt)
        {
            _chartRsltList = new List<string>();

            List<DataRow> rowList = new List<DataRow>();
            List<DataRow> chartRowList = new List<DataRow>();

            foreach (var pair in _UtilInfos.OrderBy(t => t.Key))
            {
                string shopID = pair.Key.Split('@')[0];
                string area = pair.Key.Split('@')[1];
                string eqpGroup = pair.Key.Split('@')[2];

                EuData.GroupUtilInfo groupInfo = pair.Value;

                if (groupInfo == null)
                    continue;

                for (DateTime date = FromDate; date < ToDate; date = date.AddDays(1))
                {
                    float busy = (float)Math.Round(groupInfo.GetAvgBusy(date), 1);
                    float setup = (float)Math.Round(groupInfo.GetAvgSetup(date), 1);

                    if (this.includeJobChgOnUtil.Checked)
                        busy = busy + setup;

                    if (busy < _minBusyValue)
                        _minBusyValue = busy;
                    if (busy > _maxBusyValue)
                        _maxBusyValue = busy;

                    if (this.IsUseUtilLowerLimit && busy <= this.UtilConditionVal)
                        continue;

                    string dateVal = date.ToString("yyyy'/'MM'/'dd");

                    DataRow chartRow = _dtChart.NewRow();

                    chartRow[EuData.SHOP_ID] = shopID;
                    chartRow[EuData.AREA] = area;
                    chartRow[EuData.EQP_GROUP_ID] = eqpGroup;
                    chartRow[EuData.DATE] = dateVal;
                    chartRow[EuData.UTILIZATION] = busy;

                    _dtChart.Rows.Add(chartRow);

                    string item = CommonHelper.CreateKey(shopID, area, eqpGroup);
                    if (_chartRsltList.Contains(item) == false)
                        _chartRsltList.Add(item);


                    if (eqpGroup != EuData.FOR_AREA)
                    {
                        DataRow newRow = dt.DataTable.NewRow();
                        newRow[EuData.SHOP_ID] = shopID;
                        newRow[EuData.AREA] = area;
                        newRow[EuData.EQP_GROUP_ID] = eqpGroup;
                        newRow[EuData.DATE] = dateVal;
                        newRow[EuData.UTILIZATION] = busy;

                        dt.DataTable.Rows.Add(newRow);

                        rowList.Add(newRow);
                    }
                }
            }

            return dt;
        }

        private void DrawPivotGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            this.pivotGridControl1.BeginUpdate();

            this.pivotGridControl1.ClearPivotGridFields();
            this.pivotGridControl1.CreatePivotGridFields(dt);

            this.pivotGridControl1.DataSource = dt.DataTable;

            this.pivotGridControl1.OptionsView.ShowRowTotals = true;
            this.pivotGridControl1.OptionsView.ShowRowGrandTotals = true;
            this.pivotGridControl1.OptionsView.ShowColumnTotals = true;
            this.pivotGridControl1.OptionsView.ShowColumnGrandTotals = true;

            this.pivotGridControl1.Fields[EuData.UTILIZATION].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.pivotGridControl1.Fields[EuData.UTILIZATION].CellFormat.FormatString = "###,##0.0";

            this.pivotGridControl1.EndUpdate();
        }

        private void DrawChart(int chartIndex = 0)
        {
            _chartIndex = chartIndex;

            this.chartControl1.Series.Clear();
            _originalSeriesColors = new Dictionary<string, Color>();

            List<string> chartRsltList = new List<string>();
            if (chartIndex == 0)
                chartRsltList = _chartRsltList.Where(x => x.Contains(EuData.FOR_AREA)).ToList();
            else if (chartIndex == 1)
                chartRsltList = _chartRsltList.Where(x => x.Contains(EuData.FOR_AREA) == false).ToList();

            foreach (string item in chartRsltList)
            {
                string key = chartIndex == 0 ? item.Split('@')[1] : item.Split('@')[2];

                Series series = new Series(key, DevExpress.XtraCharts.ViewType.Line);

                this.chartControl1.Series.Add(series);

                series.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Auto;
                series.ArgumentDataMember = EuData.DATE;
                series.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
                series.ValueDataMembers.AddRange(new string[] { EuData.UTILIZATION });
                series.CrosshairLabelPattern = "{S}({A}) : {V:##0.0}%";

                (series.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
                (series.View as LineSeriesView).LineMarkerOptions.Size = 9;

                string filter = chartIndex == 0 ? string.Format("{0} = '{1}' AND {2} = '{3}'", EuData.AREA, key,
                    EuData.EQP_GROUP_ID, EuData.FOR_AREA)
                    : string.Format("{0} = '{1}'", EuData.EQP_GROUP_ID, key);
                DataView view = new DataView(_dtChart, filter, null, DataViewRowState.CurrentRows);

                series.DataSource = view;

                Color color = _colorGenerator.GetColor(key);
                series.View.Color = color;

                if (_originalSeriesColors.ContainsKey(key) == false)
                    _originalSeriesColors[key] = color;
            }

            if (_minBusyValue > _maxBusyValue)
                _maxBusyValue = _minBusyValue;

            this.chartControl1.Legend.MarkerSize = new Size(20, 20);
        }

        #endregion Methods

        #region Events

        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            ClearData();

            BindData();

            FillPivotGrid();

            //DrawChart();

            _selectedEqpGrpInAreaList = null;

            this.Cursor = Cursors.Default;
        }

        private void utilLowerLimitCondChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.utilLowerLimitCondChkBox.Checked)
                this.utilConditionValSpinEdit.Enabled = true;
            else
                this.utilConditionValSpinEdit.Enabled = false;
        }

        private void pivotGridControl1_CellClick(object sender, PivotCellEventArgs e)
        {
            if (this.chartControl1.Series == null || this.chartControl1.Series.Count == 0 || _chartIndex > 1)
            {
                return;
            }

            string key = string.Empty;
            PivotGridField field = null;
            if (_chartIndex == 0)
            {
                field = e.GetRowFields().Where(x => x.FieldName == EuData.AREA).FirstOrDefault();
                if (field == null)
                    return;
            }
            else if (_chartIndex == 1)
            {
                field = e.GetRowFields().Where(x => x.FieldName == EuData.EQP_GROUP_ID).FirstOrDefault();
                if (field == null)
                    return;
            }

            key = e.GetFieldValue(field, e.RowIndex).ToString();

            int selectedIndex = -1;
            for (int i = 0; i < this.chartControl1.Series.Count; i++)
            {
                Series series = this.chartControl1.Series[i];

                if (series.Name == key)
                {
                    series.View.Color = _originalSeriesColors[key];
                    ((LineSeriesView)series.View).LineStyle.Thickness = 5;
                    selectedIndex = i;
                    series.CrosshairEnabled = DevExpress.Utils.DefaultBoolean.True;
                    (series.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
                }
                else
                {
                    series.View.Color = Color.WhiteSmoke;
                    series.CrosshairEnabled = DevExpress.Utils.DefaultBoolean.False;
                }
            }

            if (selectedIndex < this.chartControl1.Series.Count - 1 && selectedIndex >= 0)
            {
                if (_lastSelectedSeriesIndex >= 0)
                {
                    Series last = (Series)this.chartControl1.Series.Last();
                    this.chartControl1.Series.Remove(last);
                    this.chartControl1.Series.Insert(_lastSelectedSeriesIndex, last);
                }

                int index = _lastSelectedSeriesIndex >= 0 && selectedIndex >= _lastSelectedSeriesIndex ? selectedIndex + 1 : selectedIndex;

                Series current = this.chartControl1.Series[index];
                this.chartControl1.Series.RemoveAt(index);
                this.chartControl1.Series.Add(current);
                _lastSelectedSeriesIndex = index;
                current.View.Color = _originalSeriesColors[key];
            }
        }

        private void pivotGridControl1_CellDoubleClick(object sender, PivotCellEventArgs e)
        {
            List<EuData.GroupUtilInfo> groupInfoList = new List<EuData.GroupUtilInfo>();

            string dockPanel1Name = string.Empty;
            PivotGridField field = null;
            if (_chartIndex == 0)
            {
                field = e.GetRowFields().Where(x => x.FieldName == EuData.SHOP_ID).FirstOrDefault();
                if (field == null)
                    return;

                string shopID = e.GetFieldValue(field, e.RowIndex).ToString();

                field = e.GetRowFields().Where(x => x.FieldName == EuData.AREA).FirstOrDefault();
                if (field == null)
                    return;

                string area = e.GetFieldValue(field, e.RowIndex).ToString();

                dockPanel1Name = string.Format("< AREA : '{0}' >", area);

                List<string> eqpGrpList;
                if (_eqpGrpsInAreaDic.TryGetValue(area, out eqpGrpList) == false || eqpGrpList.Count == 0)
                {
                    MessageBox.Show(string.Format("EqpGroup Info is not existed in '{0}'.", area));
                    return;
                }

                foreach (string eqpGroupID in eqpGrpList)
                {
                    string item = CommonHelper.CreateKey(shopID, area, eqpGroupID);

                    EuData.GroupUtilInfo info;
                    if (_UtilInfos.TryGetValue(item, out info) == false)
                        continue;

                    groupInfoList.Add(info);
                }

                if (groupInfoList.Count == 0)
                {
                    MessageBox.Show(string.Format("EqpGroup Data is not existed in '{0}'.", area));
                    return;
                }
            }
            else if (_chartIndex == 1)
            {
                string item = string.Empty;

                field = e.GetRowFields().Where(x => x.FieldName == EuData.SHOP_ID).FirstOrDefault();
                if (field == null)
                    return;

                string shopID = e.GetFieldValue(field, e.RowIndex).ToString();

                field = e.GetRowFields().Where(x => x.FieldName == EuData.EQP_GROUP_ID).FirstOrDefault();
                if (field == null)
                    return;

                string eqpGroupID = e.GetFieldValue(field, e.RowIndex).ToString();

                dockPanel1Name = string.Format("< EQP GROUP : '{0}' >", eqpGroupID);

                item = e.GetFieldValue(field, e.RowIndex).ToString();

                string area = string.Empty;
                field = e.GetRowFields().Where(x => x.FieldName == EuData.AREA).FirstOrDefault();
                if (field == null)
                {
                    if (_areaByEqpGrpDic.TryGetValue(eqpGroupID, out area) == false)
                        return;
                }
                else
                {
                    area = e.GetFieldValue(field, e.RowIndex).ToString();
                }

                item = CommonHelper.CreateKey(shopID, area, eqpGroupID);

                EuData.GroupUtilInfo info;
                if (_UtilInfos.TryGetValue(item, out info) == false)
                {
                    MessageBox.Show(string.Format("{0} is not existed.", item));
                    return;
                }

                groupInfoList.Add(info);
            }

            EqpUtilizationViewPopup popup = new EqpUtilizationViewPopup(this, groupInfoList, dockPanel1Name);
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.ShowDialog();
        }

        private void pivotGridControl1_FieldAreaChanged(object sender, PivotFieldEventArgs e)
        {
            bool isArea = true;
            bool isEqpGrp = true;
            foreach (PivotGridField field in this.pivotGridControl1.Fields)
            {
                if ((field.FieldName == EuData.SHOP_ID && field.Area != PivotArea.RowArea) ||
                    (field.FieldName == EuData.UTILIZATION && field.Area != PivotArea.DataArea))
                {
                    isArea = false;
                    isEqpGrp = false;
                }
                if (field.FieldName == EuData.AREA && field.Area != PivotArea.RowArea)
                {
                    isArea = false;
                }
                if (field.FieldName == EuData.EQP_GROUP_ID && field.Area != PivotArea.RowArea)
                {
                    isEqpGrp = false;
                }
                if (field.FieldName == EuData.EQP_GROUP_ID && field.Area == PivotArea.RowArea)
                {
                    isArea = false;
                }
            }

            if (isArea)
                _chartIndex = 0;
            else if (isEqpGrp)
                _chartIndex = 1;
            else
            {
                _chartIndex = 2;
                this.chartControl1.Series.Clear();
            }
        }
        #endregion Events

        #region Comparer
        public class EqpGroupSequenceComparer : IComparer<string>
        {
            private List<string> _sequenceList;

            public EqpGroupSequenceComparer(List<string> sequenceList)
            {
                _sequenceList = sequenceList;
            }

            public int Compare(string x, string y)
            {
                int indexX = _sequenceList.IndexOf(x);
                int indexY = _sequenceList.IndexOf(y);

                if (indexX < 0)
                    indexX = int.MaxValue;

                if (indexY < 0)
                    indexY = int.MaxValue;

                return indexX.CompareTo(indexY);
            }
        }
        #endregion
    }

}
