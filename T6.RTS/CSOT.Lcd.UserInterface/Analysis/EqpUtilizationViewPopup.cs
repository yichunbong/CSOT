using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraCharts;
using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.Lcd.UserInterface.DataMappings;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class EqpUtilizationViewPopup : Form
    {
        #region Variables
        private List<EuData.GroupUtilInfo> _groupInfoList;
        private EqpUtilizationView _parent;

        private float _minBusyValue;
        private float _maxBusyValue;
        private double _avgBusyValue;

        private Dictionary<string, Color> _originalSeriesColors;

        private DataTable _dtChart;
        private DataTable _dtBusy;
        private DataTable _dtIdle;
        private int _lastSelectedSeriesIndex = -1;

        ColorGenerator _colorGenerator;
        #endregion Variables

        #region Constructor
        public EqpUtilizationViewPopup(EqpUtilizationView parent, List<EuData.GroupUtilInfo> groupInfoList, string dockPanel1Name)
        {
            InitializeComponent();

            //this.Text = string.Format(this.Text, groupInfo.EqpGroup);

            this._parent = parent;
                        
            // 장비 그룹별 Total Buby를 판단 -> 장비별로는 0 일 수 있음
            groupInfoList = _parent.IsUseUtilLowerLimit ?
                groupInfoList.Where(x => x.GetTotalBusy(_parent.FromDate, _parent.ToDate) > _parent.UtilConditionVal).ToList()
                : groupInfoList;

            this._groupInfoList = groupInfoList;

            dockPanel1.Text = dockPanel1Name;   // string.Format(dockPanel1.Text, "k");
            dockPanel2.Text = EuData.POPUP_CHART_TITLE;

            InitializeData();

            FillGrid();
            DrawChart();
            FillSummary();
            DrawBusyPieChart();
            DrawIdlePieChart();
        }
        #endregion Constructor

        #region Methods
        private void InitializeData()
        {
            _minBusyValue = 100;
            _maxBusyValue = 0;
            _avgBusyValue = 0;

            _colorGenerator = new ColorGenerator();
        }

        private void FillSummary()
        {
            double days = Math.Round((_parent.ToDate - _parent.FromDate).TotalDays, 3);

            int eqpCnt = 0;
            float cnt = 0;
            float totalRun = 0;
            float totalIdel = 0;
            Tuple<float, DateTime> minBusy = new Tuple<float, DateTime>(100, DateTime.MinValue);
            Tuple<float, DateTime> maxBusy = new Tuple<float, DateTime>(0, DateTime.MinValue);
            foreach (EuData.GroupUtilInfo info in _groupInfoList)
            {
                cnt++;

                eqpCnt += info.GetEqpCount();

                totalRun += info.GetAvgRun(days);
                totalIdel += info.GetAvgIdleSum(days);

                Tuple<float, DateTime> curMinBusy = info.GetMinBusy(_parent.FromDate, _parent.ToDate);
                if (curMinBusy.Item1 < minBusy.Item1)
                    minBusy = curMinBusy;

                Tuple<float, DateTime> curMaxBusy = info.GetMaxBusy(_parent.FromDate, _parent.ToDate);
                if (curMaxBusy.Item1 > maxBusy.Item1)
                    maxBusy = curMaxBusy;
            }

            float avgRun = cnt > 0 ? totalRun / cnt : 0;
            float avgIdel = cnt > 0 ? totalIdel / cnt : 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("+ Date Range : {0} ~ {1}", _parent.FromDate.ToString("yyyy.MM.dd"), _parent.ToDate.ToString("yyyy.MM.dd")));
            sb.AppendLine(string.Format("+ Util. Days : {0}(days)", days));     // _groupInfoList.GetWorkedDays()
            sb.AppendLine(string.Format("+ No. of Eqp : {0}", eqpCnt));

            double avg = Math.Round(_avgBusyValue, 1);
            sb.AppendLine(string.Format("+ Avg. Util Rate : {0:n2}%", avg));
            //sb.AppendLine(string.Format("+ Avg. Util Rate : {0}%", Math.Round(avgRun, 1)));
            
            double min = Math.Round(_minBusyValue, 1);
            double max = Math.Round(_maxBusyValue, 1);

            sb.AppendLine(string.Format("+ Lowest: {0}%", min));
            sb.AppendLine(string.Format("+ Highest: {0}%", max));
            //sb.AppendLine(string.Format("+ Lowest: {0}%({1})", Math.Round(minBusy.Item1, 1), minBusy.Item2.ToString("yyyy.MM.dd")));
            //sb.AppendLine(string.Format("+ Highest: {0}%({1})", Math.Round(maxBusy.Item1, 1), maxBusy.Item2.ToString("yyyy.MM.dd")));
            sb.AppendLine(string.Format("+ Unused : {0}%", 100.0 - avg));

            this.memoEdit1.Text = sb.ToString();
        }

        private void FillGrid()
        {
            DataTable resultTable = CreateTable();
            _dtChart = CreateChartTable();

            List<float> avgList = new List<float>();
            Dictionary<string, List<float>> utilByDateDic = new Dictionary<string,List<float>>();

            foreach (EuData.GroupUtilInfo groupUnitInfo in _groupInfoList)
            {
                foreach (var pair in groupUnitInfo.Infos)
                {
                    string eqpID = pair.Key;
                    EuData.UtilInfo eqpInfo = pair.Value;

                    DataRow row = resultTable.NewRow();
                    row[EuData.POPUP_EQP_ID] = eqpID;

                    var fromDate = _parent.FromDate;
                    var toDate = _parent.ToDate;
                    
                    for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                    {
                        DataRow chartRow = _dtChart.NewRow();
                        string dateString = date.ToString("yyyy'/'MM'/'dd");
                        float dataValue = 0;

                        EuData.UtilData data;
                        if (eqpInfo.Info.TryGetValue(date, out data))
                        {
                            dataValue = data.AvgBusy;

                            if (this._parent.IncludeJobChgOnUtil)
                                dataValue += data.AvgSetup;

                            row[dateString] = Math.Round(dataValue, 1);

                            if (dataValue < _minBusyValue)
                                _minBusyValue = dataValue;
                            if (dataValue > _maxBusyValue)
                                _maxBusyValue = dataValue;

                            //chartRow = _dtChart.NewRow();
                            chartRow[EuData.POPUP_EQP_ID] = eqpID;
                            chartRow[EuData.DATE] = dateString;
                            chartRow[EuData.UTILIZATION] = dataValue;
                        }
                        else
                        {
                            chartRow[EuData.POPUP_EQP_ID] = eqpID;
                            chartRow[EuData.DATE] = dateString;
                            chartRow[EuData.UTILIZATION] = 0;
                        }

                        List<float> rateList;
                        if (utilByDateDic.TryGetValue(dateString, out rateList) == false)
                            utilByDateDic.Add(dateString, rateList = new List<float>());

                        rateList.Add(dataValue);

                        _dtChart.Rows.Add(chartRow);
                    }

                    float avg = eqpInfo.GetAvgBusy((_parent.ToDate - _parent.FromDate).TotalDays);

                    if (this._parent.IncludeJobChgOnUtil)
                        avg += eqpInfo.GetAvgSetup((_parent.ToDate - _parent.FromDate).TotalDays);

                    row[EuData.POPUP_AVG] = Math.Round(avg, 1);

                    avgList.Add(avg);

                    resultTable.Rows.Add(row);
                }
            }            

            DataRow totalRow = resultTable.NewRow();
            totalRow[EuData.POPUP_EQP_ID] = EuData.POPUP_AVG;
            float sum = 0;
            for (DateTime date = _parent.FromDate; date < _parent.ToDate; date = date.AddDays(1))
            {
                float busy = 0;

                foreach (EuData.GroupUtilInfo groupUnitInfo in _groupInfoList)
                {
                    busy = groupUnitInfo.GetAvgBusy(date);

                    if (this._parent.IncludeJobChgOnUtil)
                        busy += groupUnitInfo.GetAvgSetup(date);
                }

                if (busy < _minBusyValue)
                    _minBusyValue = busy;
                if (busy > _maxBusyValue)
                    _maxBusyValue = busy;

                sum += busy;

                string dateString = date.ToString("yyyy'/'MM'/'dd");

                float avgRateByDate = 0.00f;
                List<float> utilList;
                if (utilByDateDic.TryGetValue(dateString, out utilList))
                    avgRateByDate = utilList.Average();

                totalRow[dateString] = Math.Round(avgRateByDate, 1);
                //totalRow[dateString] = Math.Round(busy, 1);

                DataRow totalChartRow = _dtChart.NewRow();
                totalChartRow[EuData.POPUP_EQP_ID] = EuData.POPUP_AVG;
                totalChartRow[EuData.DATE] = dateString;
                totalChartRow[EuData.UTILIZATION] = Math.Round(avgRateByDate, 1);
                //totalChartRow[EuData.UTILIZATION] = Math.Round(busy, 1);
                _dtChart.Rows.Add(totalChartRow);
            }

            //double totalAvg = Math.Round(sum / (_parent.ToDate - _parent.FromDate).TotalDays, 1);
            double totalAvg = Math.Round(avgList.Average(), 1);

            totalRow[EuData.POPUP_AVG] = totalAvg;
            resultTable.Rows.Add(totalRow);

            _avgBusyValue = totalAvg;

            //DataView view = new DataView(resultTable, "", EuData.POPUP_EQP_ID, DataViewRowState.CurrentRows);

            this.gridControl1.DataSource = resultTable;
            DecorateTable();
        }

        private void DecorateTable()
        {
            //틀고정
            this.gridView1.Columns[EuData.POPUP_EQP_ID].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            this.gridView1.Columns[EuData.POPUP_AVG].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            //배경색
            //this.gridView1.Columns[EqpUtilizationData.POPUP_EQP_ID].AppearanceCell.BackColor = Color.LightYellow;
            this.gridView1.Columns[EuData.POPUP_AVG].AppearanceCell.BackColor = Color.FromArgb(204, 255, 195);

            foreach (DevExpress.XtraGrid.Columns.GridColumn col in this.gridView1.Columns)
            {
                if (col.FieldName == EuData.POPUP_EQP_ID)
                    continue;

                col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
                col.DisplayFormat.FormatString = "###,##0.0";
            }

            //너비
            this.gridView1.BestFitColumns();
        }

        private DataTable CreateTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(EuData.POPUP_EQP_ID);
            dt.Columns.Add(EuData.POPUP_AVG, typeof(float));
            
            for (DateTime date = _parent.FromDate; date < _parent.ToDate; date = date.AddDays(1))
            {
                string dateCol = date.ToString("yyyy'/'MM'/'dd");
                dt.Columns.Add(dateCol, typeof(float));
            }

            return dt;
        }

        private DataTable CreateChartTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(EuData.POPUP_EQP_ID);
            dt.Columns.Add(EuData.DATE);
            dt.Columns.Add(EuData.UTILIZATION, typeof(float));

            return dt;
        }

        private DataTable CreateBusyTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("NAME");
            dt.Columns.Add("VALUE", typeof(float));

            return dt;
        }

        private void DrawSingle()
        {
            DataRow selected = gridView1.GetFocusedDataRow();

            if (selected == null)
                return;

            string eqpID = selected[EuData.POPUP_EQP_ID].ToString();

            this.chartControl1.Series.Clear();

            //_originalSeriesColors = new Dictionary<string, Color>();

            //List<string> seriesList = new List<string>(_groupInfo.Infos.Keys);
            //seriesList.Add(EqpUtilizationData.POPUP_TOTAL);


            Series series = new Series(eqpID, DevExpress.XtraCharts.ViewType.Line);
            series.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Auto;
            series.LabelsVisibility = DevExpress.Utils.DefaultBoolean.False;

            series.ArgumentDataMember = EuData.DATE;
            series.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
            series.ValueDataMembers.AddRange(new string[] { EuData.UTILIZATION });
            series.CrosshairLabelPattern = "{S}({A}) : {V}%";


            (series.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            (series.View as LineSeriesView).LineMarkerOptions.Size = 9;

            string filter = string.Format("{0} = '{1}'", EuData.POPUP_EQP_ID, eqpID);
            DataView view = new DataView(_dtChart, filter, null, DataViewRowState.CurrentRows);
            series.DataSource = view;

            Color color = eqpID == EuData.POPUP_AVG ? Color.Red : _colorGenerator.GetColor(eqpID);
            series.View.Color = color;
            _originalSeriesColors[eqpID] = color;

           
            this.chartControl1.Series.Add(series);

            XYDiagram diag = (XYDiagram)this.chartControl1.Diagram;
            diag.AxisY.VisualRange.SetMinMaxValues(_minBusyValue, _maxBusyValue);
            this.chartControl1.Legend.Visible = false;
            //this.chartControl1.Legend.MarkerSize = new Size(20, 20);
        }

        private void DrawChart()
        {
            this.chartControl1.Series.Clear();
            _originalSeriesColors = new Dictionary<string, Color>();

            List<string> seriesList = new List<string>();

            foreach (EuData.GroupUtilInfo groupInfo in _groupInfoList)
            {
                seriesList.AddRange(groupInfo.Infos.Keys);
            }

            seriesList.Add(EuData.POPUP_AVG);

            this.chartControl1.CacheToMemory = true;

            int count = 0;

            //if (seriesList.Count > 30)
            //{
                
            //    return;
            //}

            foreach (string eqpID in seriesList)
            {
                if (count >= 30)
                    break;

                Series series = new Series(eqpID, DevExpress.XtraCharts.ViewType.Line);
                series.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Auto;
                series.LabelsVisibility = DevExpress.Utils.DefaultBoolean.False;

                series.ArgumentDataMember = EuData.DATE;
                series.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
                series.ValueDataMembers.AddRange(new string[] { EuData.UTILIZATION });
                series.CrosshairLabelPattern = "{S}({A}) : {V}%";


                (series.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
                (series.View as LineSeriesView).LineMarkerOptions.Size = 9;

                string filter = string.Format("{0} = '{1}'", EuData.POPUP_EQP_ID, eqpID);
                DataView view = new DataView(_dtChart, filter, null, DataViewRowState.CurrentRows);
                series.DataSource = view;

                Color color = eqpID == EuData.POPUP_AVG ? Color.Red : _colorGenerator.GetColor(eqpID);
                series.View.Color = color;
                _originalSeriesColors[eqpID] = color;

                count += 1;

                this.chartControl1.Series.Add(series);
            }

            XYDiagram diag = (XYDiagram)this.chartControl1.Diagram;
            diag.AxisY.VisualRange.SetMinMaxValues(_minBusyValue, _maxBusyValue);
            this.chartControl1.Legend.Visible = false;
            //this.chartControl1.Legend.MarkerSize = new Size(20, 20);
        }

        private void EmphasisEqpID()
        {
            DataRow selected = gridView1.GetFocusedDataRow();

            if (selected == null)
                return;

            string eqpID = selected[EuData.POPUP_EQP_ID].ToString();

            int selectedIndex = -1;
            for (int i = 0; i < this.chartControl1.Series.Count; i++)
            {
                Series series = this.chartControl1.Series[i];

                if (series.Name == eqpID)
                {
                    series.View.Color = _originalSeriesColors[eqpID];
                    ((LineSeriesView)series.View).LineStyle.Thickness = 5;
                    selectedIndex = i;
                    series.CrosshairEnabled = DevExpress.Utils.DefaultBoolean.True;
                    (series.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
                }
                else
                {
                    series.View.Color = Color.WhiteSmoke;
                    ((LineSeriesView)series.View).LineStyle.Thickness = 2;
                    series.CrosshairEnabled = DevExpress.Utils.DefaultBoolean.False;
                    (series.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.False;
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
                current.View.Color = _originalSeriesColors[eqpID];
            }
        }

        private void DrawBusyPieChart()
        {
            double days = (_parent.ToDate - _parent.FromDate).TotalDays;

            float cnt = 0.0f;
            float totalBusy = 0.0f;
            float totalIdelRun = 0.0f;
            float totalSetup = 0.0f;
            foreach (EuData.GroupUtilInfo info in _groupInfoList)
            {
                cnt++;

                totalBusy += info.GetAvgBusy(days);
                totalIdelRun += info.GetAvgIdleRun(days);
                totalSetup += info.GetAvgSetup(days);
            }

            float busyRate = cnt > 0 ? totalBusy / cnt : 0;
            float idleRunRate = cnt > 0 ? totalIdelRun / cnt : 0;
            float setupRate = 0.0f;

            if (this._parent.IncludeJobChgOnUtil)
                setupRate = cnt > 0 ? totalSetup / cnt : 0;

            _dtBusy = CreateBusyTable();

            DataRow busyRow = _dtBusy.NewRow();
            busyRow["NAME"] = "BUSY";
            busyRow["VALUE"] = Math.Round(busyRate / (busyRate + idleRunRate + setupRate) * 100, 1);
            _dtBusy.Rows.Add(busyRow);

            DataRow idleRunRow = _dtBusy.NewRow();
            idleRunRow["NAME"] = "IDLERUN";
            idleRunRow["VALUE"] = Math.Round(idleRunRate / (busyRate + idleRunRate + setupRate) * 100, 1);
            _dtBusy.Rows.Add(idleRunRow);

            if (this._parent.IncludeJobChgOnUtil)
            {
                DataRow setupRow = _dtBusy.NewRow();
                setupRow["NAME"] = "SETUP";
                setupRow["VALUE"] = Math.Round(setupRate / (busyRate + idleRunRate + setupRate) * 100, 1);
                _dtBusy.Rows.Add(setupRow);
            }

            Series series = new Series("RATIO", ViewType.Pie);
            chartControl2.Series.Add(series);
            series.ArgumentDataMember = "NAME";
            series.ValueScaleType = ScaleType.Numerical;
            series.ValueDataMembers.AddRange(new string[] { "VALUE" });
            series.DataSource = _dtBusy;
            series.LegendTextPattern = "{A}";
            chartControl2.Legend.Visible = false;
            chartControl2.Legend.TextVisible = true;
            chartControl2.Legend.AlignmentVertical = LegendAlignmentVertical.Center;
            series.Label.TextPattern = "{A}\r\n{V}%";
            series.Label.TextColor = Color.Black;
            (series.Label as PieSeriesLabel).Position = PieSeriesLabelPosition.TwoColumns;
            series.Label.ResolveOverlappingMode = ResolveOverlappingMode.Default;

            ChartTitle title = new ChartTitle();
            title.Text = "Used Rate";
            title.Font = new System.Drawing.Font("Arial", 10f);
            title.TextColor = Color.Black;
            chartControl2.Titles.Add(title);
        }

        private void DrawIdlePieChart()
        {
            double days = (_parent.ToDate - _parent.FromDate).TotalDays;

            float cnt = 0.0f;
            float totalSetup = 0.0f;
            float totalPm = 0.0f;
            float totalDown = 0.0f;
            float totalIdel = 0.0f;
            foreach (EuData.GroupUtilInfo info in _groupInfoList)
            {
                cnt++;

                totalSetup += info.GetAvgSetup(days);
                totalPm += info.GetAvgPM(days);
                totalDown += info.GetAvgDown(days);
                totalIdel += info.GetAvgIdle(days);
            }

            float setupRate = cnt > 0 ? totalSetup / cnt : 0;
            float pmRate = cnt > 0 ? totalPm / cnt : 0;
            float downRate = cnt > 0 ? totalDown / cnt : 0;
            float idleRate = cnt > 0 ? totalIdel / cnt : 0;

            if (this._parent.IncludeJobChgOnUtil)
                setupRate = 0.0f;

            _dtIdle = CreateBusyTable();

            if (this._parent.IncludeJobChgOnUtil == false)
            {
                DataRow setupRow = _dtIdle.NewRow();
                setupRow["NAME"] = "SETUP";
                setupRow["VALUE"] = Math.Round(setupRate / (setupRate + pmRate + downRate + idleRate) * 100, 1);
                _dtIdle.Rows.Add(setupRow);
            }

            DataRow pmRow = _dtIdle.NewRow();
            pmRow["NAME"] = "PM";
            pmRow["VALUE"] = Math.Round(pmRate / (setupRate + pmRate + downRate + idleRate) * 100, 1);
            _dtIdle.Rows.Add(pmRow);

            DataRow downRow = _dtIdle.NewRow();
            downRow["NAME"] = "DOWN";
            downRow["VALUE"] = Math.Round(downRate / (setupRate + pmRate + downRate + idleRate) * 100, 1);
            _dtIdle.Rows.Add(downRow);

            DataRow idleRow = _dtIdle.NewRow();
            idleRow["NAME"] = "IDLE";
            idleRow["VALUE"] = Math.Round(idleRate / (setupRate + pmRate + downRate + idleRate) * 100, 1);
            _dtIdle.Rows.Add(idleRow);

            Series series = new Series("RATIO", ViewType.Pie);
            chartControl3.Series.Add(series);
            series.ArgumentDataMember = "NAME";
            series.ValueScaleType = ScaleType.Numerical;
            series.ValueDataMembers.AddRange(new string[] { "VALUE" });
            series.DataSource = _dtIdle;
            series.LegendTextPattern = "{A}";
            chartControl3.Legend.Visible = false;
            chartControl3.Legend.TextVisible = true;
            chartControl3.Legend.AlignmentVertical = LegendAlignmentVertical.Center;
            series.Label.TextPattern = "{A}\r\n{V}%";
            series.Label.TextColor = Color.Black;
            (series.Label as PieSeriesLabel).Position = PieSeriesLabelPosition.TwoColumns;
            series.Label.ResolveOverlappingMode = ResolveOverlappingMode.Default;

            ChartTitle title = new ChartTitle();
            title.Text = "Unused Rate";
            title.Font = new System.Drawing.Font("Arial", 10f);
            title.TextColor = Color.Black;
            chartControl3.Titles.Add(title);
        }
        #endregion Methods

        #region Events
        private void gridView1_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (gridView1.Columns.Count == 0)
                return;

            string eqpID = gridView1.GetRowCellDisplayText(e.RowHandle, gridView1.Columns[EuData.POPUP_EQP_ID]);

            if (eqpID != EuData.POPUP_AVG)
                return;

            e.Appearance.BackColor = Color.FromArgb(204, 255, 195);
        }

        private void gridView1_Click(object sender, EventArgs e)
        {
            //EmphasisEqpID();

            DrawSingle();
        }
        #endregion Events
    }
}
