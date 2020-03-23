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
using Mozart.Collections;
using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.Lcd.UserInterface.DataMappings;
using DevExpress.XtraPivotGrid;
using DevExpress.XtraCharts;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class BalanceTableView : XtraPivotGridControlView
    {
        #region Class Variables

        private IExperimentResultItem _result;
        private List<string> _targetShiftList;
        private Dictionary<string, CellInfo> _resultDict;
        private Dictionary<string, string> _bomDict;
        private Dictionary<string, string> _bomReverseDict;
        private List<string> _categories;
        private ColorGenerator _colorGenerator;
        private List<string> _demandList;

        public bool ShowOutBalance
        { get { return this.showOutInfoCheck.Checked; } }

        public bool ShowCellOutID
        { get { return this.showCellOutIDCheck.Checked; } }

        #endregion

        #region 생성자
        public BalanceTableView()
        {
            InitializeComponent();
        }

        public BalanceTableView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }
        #endregion



        #region InitData

        protected override void LoadDocument()
        {
            // 진입점

            var item = (IMenuDocItem)this.Document.ProjectItem;
            _result = (IExperimentResultItem)item.Arguments[0];

            if (_result == null)
                return;

            Globals.InitFactoryTime(_result.Model);

            // 0. Set Control
            SetControl();
        }
        #endregion

        #region Set Control / Data
        private void SetControl()
        {
            this.startDatePicker.Properties.EditMask = "yyyy-MM-dd HH:mm:ss";
            this.startDatePicker.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.startDatePicker.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;
            this.startDatePicker.DateTime = _result.StartTime;

            this.endCountPicker.Value = Globals.GetResultPlanPeriod(_result) * 3;
        }


        private void SetData()
        {
            #region Target Shift Range 생성
            // TARGET SHIFT List 생성
            _targetShiftList = new List<string>();

           
            DateTime startTime = _result.StartTime;
            int shiftEndCount = Convert.ToInt32(this.endCountPicker.Value);

            startTime = this.startDatePicker.DateTime;

            for (int i = 0; i < shiftEndCount; i++)
            {
                string shift = startTime.AddHours(i * ShopCalendar.ShiftHours).ToString("yyyy-MM-dd HH");
                _targetShiftList.Add(shift);
            }

            _targetShiftList.Sort();

            _targetShiftList.Insert(0, BalanceTableViewData.INITIAL_SHIFT);

            #endregion

            #region InnerBom

            _resultDict = new Dictionary<string, CellInfo>();
            _bomDict = new Dictionary<string, string>();
            _bomReverseDict = new Dictionary<string, string>();

            string filter = string.Format("STEP_ID = '{0}' OR STEP_ID = '{1}'", BalanceTableViewData.NEXT_STEP, BalanceTableViewData.CELL_STEP);
            DataTable innerBomTable = _result.LoadInput(BalanceTableViewData.INNER_BOM_DATA_TABLE, filter);

            foreach (DataRow iRow in innerBomTable.Rows)
            {
                BalanceTableViewData.InnerBom bom = new BalanceTableViewData.InnerBom(iRow);

                if (bom.StepID == BalanceTableViewData.NEXT_STEP)
                {
                    CellInfo cinfo;
                    if (_resultDict.TryGetValue(bom.ToProductID, out cinfo) == false)
                    {
                        cinfo = new CellInfo(bom.ToProductID);
                        _resultDict.Add(bom.ToProductID, cinfo);
                    }

                    if (bom.FromProductID.StartsWith("C"))
                        cinfo.CFProductID = bom.FromProductID;
                    else if (bom.FromProductID.StartsWith("T"))
                        cinfo.TFProductID = bom.FromProductID;
                    else
                        continue;

                    if (cinfo.QtyDict == null)
                        cinfo.InitDict(_targetShiftList);
                }
                if (!_bomDict.Keys.Contains(bom.FromProductID))
                    _bomDict.Add(bom.FromProductID, bom.ToProductID);

            }

            #endregion

            #region Demand

            DataTable demandTable = _result.LoadInput(BalanceTableViewData.DEMAND_DATA_TABLE);

            _demandList = new List<string>();

            foreach (DataRow dRow in demandTable.Rows)
            {
                BalanceTableViewData.Demand demand = new BalanceTableViewData.Demand(dRow);

                if (_demandList.Contains(demand.ProductID) == false)
                    _demandList.Add(demand.ProductID);
            }

            #endregion 

            _categories = new List<string>();
            _categories.Add(CellInfo.TFT_DONE);
            _categories.Add(CellInfo.TFT_WIP);
            _categories.Add(CellInfo.CF_DONE);
            _categories.Add(CellInfo.CF_WIP);
            _categories.Add(CellInfo.CELL_IN_TARGET);
            _categories.Add(CellInfo.CELL_IN_PLAN);
            _categories.Add(CellInfo.CELL_IN_BALANCE);
            _categories.Add(CellInfo.CELL_OUT_TARGET);
            _categories.Add(CellInfo.CELL_OUT_PLAN);
            _categories.Add(CellInfo.CELL_OUT_BALANCE);
            
        }
        #endregion



        #region Load Data
        private void LoadData()
        {
            #region Wip

            string filter = string.Format("{0} = '{1}' OR {0} = '{2}'", BalanceTableViewData.Wip.Schema.STEP_ID, BalanceTableViewData.BANK_STEP, BalanceTableViewData.NEXT_STEP); 

            DataTable wipTable = _result.LoadInput(BalanceTableViewData.WIP_DATA_TABLE, filter);

            foreach (DataRow wRow in wipTable.Rows)
            {
                BalanceTableViewData.Wip wip = new BalanceTableViewData.Wip(wRow);

                if(wip.ProductID == "CE0002")
                    Console.WriteLine("a");

                if (wip.StepID == BalanceTableViewData.BANK_STEP)
                {
                    string cellID = GetToProductID(wip.ProductID);

                    CellInfo cinfo;

                    if (_resultDict.TryGetValue(cellID, out cinfo))
                    {
                        cinfo.AddQty(wip.ProductID.StartsWith("C") ? CellInfo.CF_WIP : CellInfo.TFT_WIP, BalanceTableViewData.INITIAL_SHIFT, wip.Qty);
                    }
                }
                else if (wip.StepID == BalanceTableViewData.NEXT_STEP)
                {
                    CellInfo cinfo;

                    if (_resultDict.TryGetValue(wip.ProductID, out cinfo))
                    {
                        cinfo.AddQty(CellInfo.CF_WIP, BalanceTableViewData.INITIAL_SHIFT, wip.Qty);
                        cinfo.AddQty(CellInfo.TFT_WIP, BalanceTableViewData.INITIAL_SHIFT, wip.Qty);
                    }
                }
            }


            #endregion 

            #region StepTarget

            filter = string.Format("{0} = '{1}'", BalanceTableViewData.StepTarget.Schema.STEP_ID, BalanceTableViewData.NEXT_STEP);

            DataTable stepTargetTable = _result.LoadOutput(BalanceTableViewData.STEP_TARGET_DATA_TABLE, filter);

            foreach (DataRow sRow in stepTargetTable.Rows)
            {
                BalanceTableViewData.StepTarget st = new BalanceTableViewData.StepTarget(sRow);

                if(st.ProductID == "CE0011")
                    Console.WriteLine("a");

                CellInfo cinfo;

                if (_resultDict.TryGetValue(st.ProductID, out cinfo))
                {
                    cinfo.AddQty(CellInfo.CELL_IN_TARGET, st.TargetDate, st.InTargetQty);
                    cinfo.AddQty(CellInfo.CELL_OUT_TARGET, st.TargetDate, st.OutTargetQty);
                }
            }
            #endregion 

            #region EqpPlan

            filter = string.Format("{0} = '{1}' OR {0} = '{2}'", BalanceTableViewData.EqpPlan.Schema.STEP_ID, BalanceTableViewData.BANK_STEP, BalanceTableViewData.NEXT_STEP); 

            DataTable eqpPlanTable = _result.LoadOutput(BalanceTableViewData.EQP_PLAN_DATA_TABLE, filter);

            foreach (DataRow eRow in eqpPlanTable.Rows)
            {
                BalanceTableViewData.EqpPlan ep = new BalanceTableViewData.EqpPlan(eRow);

                if(ep.ProductID == "TF0007")
                    Console.WriteLine("a");

                string shiftIn = ShopCalendar.ShiftStartTimeOfDayT(ep.StartTime).ToString("yyyy-MM-dd HH");
                string shiftOut = ShopCalendar.ShiftStartTimeOfDayT(ep.EndTime).ToString("yyyy-MM-dd HH");

                if (ep.StepID == BalanceTableViewData.BANK_STEP)
                {
                    string cellID = GetToProductID(ep.ProductID);

                    CellInfo cinfo;

                    if (_resultDict.TryGetValue(cellID, out cinfo))
                    {
                        cinfo.AddQty(ep.ProductID.StartsWith("C") ? CellInfo.CF_DONE : CellInfo.TFT_DONE, shiftIn, ep.OutTargetQty);
                    }
                }

                else if (ep.StepID == BalanceTableViewData.NEXT_STEP)
                {
                    CellInfo cinfo;

                    if (_resultDict.TryGetValue(ep.ProductID, out cinfo))
                    {
                        cinfo.AddQty(CellInfo.CELL_IN_PLAN, shiftIn, ep.OutTargetQty);
                        cinfo.AddQty(CellInfo.CELL_OUT_PLAN, shiftOut, ep.OutTargetQty);
                    }
                }
            }
            #endregion 
        }
        #endregion

        #region Process Data
        private void ProcessData()
        {
            foreach (CellInfo cinfo in _resultDict.Values)
            {
                float cumBalance = 0.0f;
                float cumOutBalance = 0.0f;

                for(int i = 1; i < _targetShiftList.Count; i++)
                {
                    string prevShift = _targetShiftList[i - 1];
                    string currShift = _targetShiftList[i];

                    // TFT 재고 계산
                    float tftWip =    cinfo.QtyDict[CellInfo.TFT_WIP][prevShift] 
                                    + cinfo.QtyDict[CellInfo.TFT_DONE][currShift] 
                                    - cinfo.QtyDict[CellInfo.CELL_IN_PLAN][currShift];
                    cinfo.QtyDict[CellInfo.TFT_WIP][currShift] = tftWip;

                    // CF 재고 계산
                    float cfWip = cinfo.QtyDict[CellInfo.CF_WIP][prevShift]
                                    + cinfo.QtyDict[CellInfo.CF_DONE][currShift]
                                    - cinfo.QtyDict[CellInfo.CELL_IN_PLAN][currShift];
                    cinfo.QtyDict[CellInfo.CF_WIP][currShift] = cfWip;

                    // BALANCE 계산
                    float currentBalance = cinfo.QtyDict[CellInfo.CELL_IN_PLAN][currShift] - cinfo.QtyDict[CellInfo.CELL_IN_TARGET][currShift];
                    cumBalance += currentBalance;

                    cinfo.QtyDict[CellInfo.CELL_IN_BALANCE][currShift] = cumBalance;

                    float currentOutBalance = cinfo.QtyDict[CellInfo.CELL_OUT_PLAN][currShift] - cinfo.QtyDict[CellInfo.CELL_OUT_TARGET][currShift];
                    cumOutBalance += currentOutBalance;

                    cinfo.QtyDict[CellInfo.CELL_OUT_BALANCE][currShift] = cumOutBalance;
                }
            }
        }
        #endregion 

        #region Bind Grid

        private void Query()
        {
            _colorGenerator = new ColorGenerator();

            SetData();

            LoadData();

            ProcessData();

            XtraPivotGridHelper.DataViewTable dt = CreateDataViewTable();
            //_dtChart = CreateChartTable();

            FillData(dt);

            // 4. Draw Grid
            DrawGrid(dt);
        }

        private void DrawGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            this.pivotGridControl1.BeginUpdate();

            this.pivotGridControl1.ClearPivotGridFields();
            this.pivotGridControl1.CreatePivotGridFields(dt);

            this.pivotGridControl1.DataSource = dt.DataTable;
            formatGrid();

            ShowTotal(this.pivotGridControl1, false);
            this.pivotGridControl1.Fields[BalanceTableViewData.CATEGORY].SortMode = PivotSortMode.Custom;
            this.pivotGridControl1.Fields[BalanceTableViewData.TARGET_DATE].SortMode = PivotSortMode.Custom;

            this.pivotGridControl1.EndUpdate();

            this.pivotGridControl1.BestFit();

            pivotGridControl1.Fields[BalanceTableViewData.QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[BalanceTableViewData.QTY].CellFormat.FormatString = "#,##0";
        }

        private void formatGrid()
        {
            pivotGridControl1.CustomDrawCell += this.pivotGridControl1_CustomDrawCell;
            pivotGridControl1.CustomFieldSort += this.pivotGridControl1_CustomFieldSort;
            pivotGridControl1.CustomCellDisplayText += pivotGridControl1_CellDisplayText;
            pivotGridControl1.FocusedCellChanged += this.pivotGridControl1_FocusedCellChanged;

            if(this.ShowCellOutID == false)
                pivotGridControl1.Fields[BalanceTableViewData.CELL_OUT_PRODUCT].Visible = false;
        }

        private void FillData(XtraPivotGridHelper.DataViewTable dt)
        {
            foreach (CellInfo cinfo in _resultDict.Values)
            {
                foreach (string category in cinfo.QtyDict.Keys)
                {
                    Dictionary<string, float> dict = cinfo.QtyDict[category];
                    foreach (KeyValuePair<string, float> pair in dict)
                    {
                        if (this.ShowOutBalance == false && category.Contains("Out-"))
                            continue;

                        string cellOutID = "-";

                        if (_bomDict.ContainsKey(cinfo.CellID))
                            cellOutID = _bomDict[cinfo.CellID];

                        if (_demandList.Contains(cellOutID) == false)
                            continue;

                        dt.DataTable.Rows.Add(
                            cellOutID,
                            cinfo.CellID,
                            cinfo.TFProductID + "/\n" + cinfo.CFProductID,
                            category,
                            pair.Key,
                            pair.Value
                        );
                    }
                }
            }
        }

        private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        {
            XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();

            dt.AddColumn(BalanceTableViewData.CELL_OUT_PRODUCT, BalanceTableViewData.CELL_OUT_PRODUCT, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(BalanceTableViewData.CELL_PRODUCT, BalanceTableViewData.CELL_PRODUCT, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(BalanceTableViewData.FROM_PRODUCT_ID, BalanceTableViewData.FROM_PRODUCT_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(BalanceTableViewData.CATEGORY, BalanceTableViewData.CATEGORY, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(BalanceTableViewData.TARGET_DATE, BalanceTableViewData.TARGET_DATE, typeof(string), PivotArea.ColumnArea, null, null);
            dt.AddColumn(BalanceTableViewData.QTY, BalanceTableViewData.QTY, typeof(float), PivotArea.DataArea, null, null);

            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns[BalanceTableViewData.CELL_OUT_PRODUCT],
                        dt.Columns[BalanceTableViewData.CELL_PRODUCT],
                        dt.Columns[BalanceTableViewData.FROM_PRODUCT_ID],
                        dt.Columns[BalanceTableViewData.CATEGORY],
                        dt.Columns[BalanceTableViewData.TARGET_DATE]
                    }
                );
            return dt;
        }

        #endregion 

        #region Chart

        private void DrawChart(string prodID)
        {
            CellInfo info;
            
            if (_resultDict.TryGetValue(prodID, out info) == false)
                return;

            this.chartControl1.Series.Clear();

            DataTable dt = CreateChartTable();

            FillChartTable(info, dt);

            GenerateSeries(dt, "IN-TARGET");
            GenerateSeries(dt, "IN-PLAN");

        }

        private void FillChartTable(CellInfo item, DataTable dt)
        {
            List<string> dates = _targetShiftList;

            float tarQty = 0.0f;
            float planQty = 0.0f;

            foreach (string date in dates)
            {
                // TARGET
                float qty1 = 0.0f;

                item.QtyDict.TryGetValue(CellInfo.CELL_IN_TARGET,date, out qty1);
                tarQty += qty1;

                DataRow chartRow = dt.NewRow();

                chartRow[TargetPlanCompareData.CATEGORY] = "IN-TARGET";
                chartRow[TargetPlanCompareData.TARGET_DATE] = date;
                chartRow[TargetPlanCompareData.QTY] = tarQty;

                dt.Rows.Add(chartRow);


                // TARGET
                float qty2 = 0.0f;
                item.QtyDict.TryGetValue(CellInfo.CELL_IN_PLAN, date, out qty2);
                planQty += qty2;

                DataRow chartRow2 = dt.NewRow();

                chartRow2[TargetPlanCompareData.CATEGORY] = "IN-PLAN";
                chartRow2[TargetPlanCompareData.TARGET_DATE] = date;
                chartRow2[TargetPlanCompareData.QTY] = planQty;

                dt.Rows.Add(chartRow2);
            }
        }

        private DataTable CreateChartTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(TargetPlanCompareData.CATEGORY);
            dt.Columns.Add(TargetPlanCompareData.TARGET_DATE);
            dt.Columns.Add(TargetPlanCompareData.QTY, typeof(float));

            return dt;
        }


        private void GenerateSeries(DataTable dt, string category)
        {
            Series series = new Series(category, DevExpress.XtraCharts.ViewType.Line);
            this.chartControl1.Series.Add(series);

            series.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Auto;
            series.ArgumentDataMember = TargetPlanCompareData.TARGET_DATE;
            series.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
            series.ValueDataMembers.AddRange(new string[] { TargetPlanCompareData.QTY });
            series.CrosshairLabelPattern = "{S}({A}) : {V:##0.0}";
            (series.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            (series.View as LineSeriesView).LineMarkerOptions.Size = 9;

            string filter = string.Format("{0} = '{1}'", TargetPlanCompareData.CATEGORY, category);
            DataView view = new DataView(dt, filter, null, DataViewRowState.CurrentRows);
            series.DataSource = view;

            Color color = category == "IN-TARGET" ? Color.CornflowerBlue : Color.MediumVioletRed;
            series.View.Color = color;
        }


        #endregion 

    
        #region Event Handler

        private void pivotGridControl1_FocusedCellChanged(object sender, EventArgs e)
        {
            PivotCellEventArgs c = this.pivotGridControl1.Cells.GetFocusedCellInfo();

            string prodID = string.Empty;

            foreach (PivotGridField r in c.GetRowFields())
            {
                string strFieldName = r.FieldName;
                string strFieldValue = r.GetValueText(c.GetFieldValue(r));

                if (strFieldName == BalanceTableViewData.CELL_PRODUCT)
                    prodID = strFieldValue;
            }

            DrawChart(prodID);
        }


        private void btnQuery_Click(object sender, EventArgs e)
        {
            Query();

        }

        private void pivotGridControl1_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        {
            if (e.Field.FieldName == BalanceTableViewData.CATEGORY)
            {
                if (e.Value1 == null || e.Value2 == null) return;
                e.Handled = true;
                string s1 = e.Value1.ToString();
                string s2 = e.Value2.ToString();

                int ind1 = _categories.IndexOf(s1);
                int ind2 = _categories.IndexOf(s2);

                e.Result = ind1.CompareTo(ind2);
            }

            else if (e.Field.FieldName == BalanceTableViewData.TARGET_DATE)
            {
                if (e.Value1 == null || e.Value2 == null) return;
                e.Handled = true;
                string s1 = e.Value1.ToString();
                string s2 = e.Value2.ToString();

                if (s1 == BalanceTableViewData.INITIAL_SHIFT && s2 == BalanceTableViewData.INITIAL_SHIFT)
                    e.Result = 0;
                else if (s1 == BalanceTableViewData.INITIAL_SHIFT)
                    e.Result = -1;
                else if (s2 == BalanceTableViewData.INITIAL_SHIFT)
                    e.Result = 1;
                else
                    e.Result = s1.CompareTo(s2) ;
            }
        }

        private void pivotGridControl1_CustomDrawCell(object sender, PivotCustomDrawCellEventArgs e)
        {
            int index = 0;
            PivotGridField[] fields = e.GetRowFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Caption == BalanceTableViewData.CATEGORY)
                {
                    index = i; break;
                }
            }

            if (index >= 0 && fields.Length > index)
            {
                if (e.GetFieldValue(fields[index]).ToString().StartsWith(CellInfo.CELL_IN_BALANCE) ||
                    e.GetFieldValue(fields[index]).ToString().StartsWith(CellInfo.CELL_OUT_BALANCE)
                    )
                {
                    e.Appearance.BackColor = Color.PowderBlue;
                    e.Appearance.BackColor2 = Color.PowderBlue;
                    e.Appearance.Options.UseBackColor = true;
                }
                else if (e.GetFieldValue(fields[index]).ToString().StartsWith(CellInfo.CELL_IN_TARGET) ||
                        e.GetFieldValue(fields[index]).ToString().StartsWith(CellInfo.CELL_OUT_TARGET) 
                    )
                {
                    e.Appearance.BackColor = Color.WhiteSmoke;
                    e.Appearance.BackColor2 = Color.WhiteSmoke;
                    e.Appearance.Options.UseBackColor = true;
                }
                else if (e.GetFieldValue(fields[index]).ToString().StartsWith(CellInfo.CELL_IN_PLAN) ||
                    e.GetFieldValue(fields[index]).ToString().StartsWith(CellInfo.CELL_OUT_PLAN)
                )
                {
                    e.Appearance.BackColor = Color.WhiteSmoke;
                    e.Appearance.BackColor2 = Color.WhiteSmoke;
                    e.Appearance.Options.UseBackColor = true;
                }
            }

            index = 0;

            // 색상 변경 : 특정 Data Cell : 색상 처리
            if (e.DataField.FieldName == BalanceTableViewData.QTY && (decimal)e.GetFieldValue(e.DataField) < 0)
            {
                e.Appearance.Options.UseBackColor = true;
                e.Appearance.DrawBackground(e.GraphicsCache, e.Bounds);
                e.Appearance.DrawString(e.GraphicsCache, e.DisplayText, e.Bounds, e.Appearance.Font, Brushes.Red, e.Appearance.GetStringFormat());
                e.Handled = true;
            }
        }

        private void pivotGridControl1_CellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
        {
            if (e.GetFieldValue(e.DataField) != null && e.GetFieldValue(e.DataField).ToString() == "0")
            {
                e.DisplayText = string.Empty;
            }
        }

        #endregion

        #region Helper Function

        private string GetToProductID(string fromProductID)
        {
            if (_bomDict.ContainsKey(fromProductID))
                return _bomDict[fromProductID];

            return string.Empty;
        }

        private List<string> GetFromProductID(string toProductID)
        {
            List<string> list = new List<string>();

            foreach (KeyValuePair<string, string> pair in _bomDict)
            {
                if (pair.Value == toProductID)
                    list.Add(pair.Key);
            }

            return list;
        }

        private void ShowTotal(PivotGridControl pivot, bool isCheck = true)
        {
            pivot.OptionsView.ShowRowTotals = false;
            pivot.OptionsView.ShowRowGrandTotals = false;
            pivot.OptionsView.ShowColumnTotals = isCheck;
            pivot.OptionsView.ShowColumnGrandTotals = isCheck;
        }

        #endregion

        #region Inner Class

        public class CellInfo
        {
            public string CellID;
            public string TFProductID;
            public string CFProductID;

            public static string TFT_DONE = "TFT Out";
            public static string CF_DONE = "CF Out";  // 완성
            public static string TFT_WIP = "TFT Stock";  // 재고 
            public static string CF_WIP = "CF Stock";
            public static string CELL_IN_TARGET = "CELL In-Target";
            public static string CELL_IN_PLAN = "CELL In-Plan";
            public static string CELL_IN_BALANCE = "CELL In-Balance";
            public static string CELL_OUT_TARGET = "CELL Out-Target";
            public static string CELL_OUT_PLAN = "CELL Out-Plan";
            public static string CELL_OUT_BALANCE = "CELL Out-Balance";
            

            // category, target date, qty
            public DoubleDictionary<string, string, float> QtyDict;

            public CellInfo(string cellID)
            {
                this.CellID = cellID;
            }

            public void InitDict(List<string> shifts)
            {
                QtyDict = new DoubleDictionary<string, string, float>();

                GenerateDict(TFT_DONE, shifts);
                GenerateDict(CF_DONE, shifts);
                GenerateDict(TFT_WIP, shifts);
                GenerateDict(CF_WIP, shifts);
                GenerateDict(CELL_IN_TARGET, shifts);
                GenerateDict(CELL_IN_PLAN, shifts);
                GenerateDict(CELL_IN_BALANCE, shifts);
                GenerateDict(CELL_OUT_TARGET, shifts);
                GenerateDict(CELL_OUT_PLAN, shifts);
                GenerateDict(CELL_OUT_BALANCE, shifts);
            }

            private void GenerateDict(string category, List<string> shifts)
            {
                 foreach (string shift in shifts)
                {
                    QtyDict.Add(category, shift, 0.0f);
                }
            }

            public void AddQty(string category, string shift, float qty)
            {
                float curQty = 0.0f;

                if (QtyDict.TryGetValue(category, shift, out curQty))
                {
                    curQty += qty;
                    QtyDict[category][shift] = curQty;
                }
            }
        }

        #endregion 

        private void dockPanel1_Click(object sender, EventArgs e)
        {

        }


    }
}
