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

using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserInterface;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class InOutCompareView : XtraPivotGridControlView
    {
        private IExperimentResultItem _result;
        private DateTime _planStartTime;

        private List<string> _selectedProductList;
        private List<string> _selectedInOutList;

        Dictionary<string, IocvData.ResultPivot> _rsltPivotDic;
        Dictionary<string, IocvData.ResultChart> _rsltChartDic;

        string SelectedShopID
        {
            get { return this.shopIdComboBoxEdit.SelectedItem != null ? this.shopIdComboBoxEdit.SelectedItem.ToString() : "Blank"; }
        }

        bool IsAllProductSeletected
        {
            get
            {
                return this.prodIdCheckedComboBoxEdit.Properties.Items.GetCheckedValues().Count
                    == this.prodIdCheckedComboBoxEdit.Properties.Items.Count;
            }
        }

        List<string> SelectedProductList
        {
            get
            {
                if (_selectedProductList != null)
                    return _selectedProductList;

                _selectedProductList = new List<string>();

                if (this.prodIdCheckedComboBoxEdit.Properties.Items.GetCheckedValues().Count <= 0)
                {
                    foreach (object prodID in this.prodIdCheckedComboBoxEdit.Properties.Items)
                        _selectedProductList.Add(prodID.ToString());

                    return _selectedProductList;
                }

                foreach (var checkedProdID in this.prodIdCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
                {
                    if (_selectedProductList.Contains(checkedProdID.ToString()) == false)
                        _selectedProductList.Add(checkedProdID.ToString());
                }

                return _selectedProductList;
            }
        }

        private DateTime FromTime
        {
            get
            {
                int iShift = this.shiftComboBoxEdit.SelectedIndex + 1;
                DateTime dt = ShopCalendar.GetShiftStartTime(this.fromDateEdit.DateTime.Date, iShift);

                //dt = dt.AddMinutes(-dt.Minute).AddSeconds(-dt.Second);

                return dt;
            }
        }
        
        private DateTime ToTime
        {
            get
            {
                return FromTime.AddDays((double)this.dayShiftSpinEdit.Value);
            }
        }


        bool IsAllInOutSeletected
        {
            get
            {
                return this.inOutChkBoxEdit.Properties.Items.GetCheckedValues().Count == this.inOutChkBoxEdit.Properties.Items.Count;
            }
        }

        List<string> SelectedInOutList
        {
            get
            {
                if (_selectedInOutList != null)
                    return _selectedInOutList;

                _selectedInOutList = new List<string>();

                if (this.inOutChkBoxEdit.Properties.Items.GetCheckedValues().Count <= 0)
                {
                    foreach (object item in this.inOutChkBoxEdit.Properties.Items)
                        _selectedInOutList.Add(item.ToString());

                    return _selectedInOutList;
                }

                foreach (var checkedItem in this.inOutChkBoxEdit.Properties.Items.GetCheckedValues())
                {
                    if (_selectedInOutList.Contains(checkedItem.ToString()) == false)
                        _selectedInOutList.Add(checkedItem.ToString());
                }

                return _selectedInOutList;
            }
        }

        string SelectedArrayInStepID
        {
            get
            {
                return this.arrayInTxtEdit.EditValue.ToString();
            }
        }

        string SelectedArrayOutStepID
        {
            get
            {
                return this.arrayOutTxtEdit.EditValue.ToString();
            }
        }

        string SelectedCfInStepID
        {
            get
            {
                return this.cfInTxtEdit.EditValue.ToString();
            }
        }

        string SelectedCfOutStepID
        {
            get
            {
                return this.cfOutTxtEdit.EditValue.ToString();
            }
        }

        string SelectedCellInStepID
        {
            get
            {
                return "1000";
            }
        }
        
        public InOutCompareView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {
            InitializeBase();

            InitializeControl();
        }
        
        private void InitializeBase()
        {
            var item = (IMenuDocItem)this.Document.ProjectItem;
            _result = (IExperimentResultItem)item.Arguments[0];

            if (_result == null)
                return;

            Globals.InitFactoryTime(_result.Model);

            _planStartTime = _result.StartTime;
        }

        private void InitializeControl()
        {
            var modelContext = this._result.GetCtx<ModelDataContext>();

            // ShopID ComboBox
            this.shopIdComboBoxEdit.Properties.Items.Add("ARRAY");
            //this.shopIdComboBoxEdit.Properties.Items.Add("CF");
            //this.shopIdComboBoxEdit.Properties.Items.Add("CELL");

            if (this.shopIdComboBoxEdit.Properties.Items.Contains("ARRAY"))
                this.shopIdComboBoxEdit.SelectedIndex = this.shopIdComboBoxEdit.Properties.Items.IndexOf("ARRAY");
            
            // ProductID CheckComboBox
            this.prodIdCheckedComboBoxEdit.Properties.Items.Clear();
            
            var prodIDs = (from a in modelContext.Product
                           select new { PRODUCT_ID = a.PRODUCT_ID })
                           .Distinct().OrderBy(x => x.PRODUCT_ID);

            foreach (var item in prodIDs)
                this.prodIdCheckedComboBoxEdit.Properties.Items.Add(item.PRODUCT_ID.ToString());

            this.prodIdCheckedComboBoxEdit.CheckAll();

            //DateEdit Controls
            this.fromDateEdit.DateTime = ShopCalendar.SplitDate(_planStartTime);
            ComboHelper.ShiftName(this.shiftComboBoxEdit, _planStartTime);

            // InOut Check Control
            this.inOutChkBoxEdit.Properties.Items.Add("IN");
            this.inOutChkBoxEdit.Properties.Items.Add("OUT");
            this.inOutChkBoxEdit.CheckAll();
                        
            // InOut StepID Info Contols
            this.arrayInTxtEdit.EditValue = "1200";
            var arrayInStepInfo = modelContext.StdStep.Where(x => x.SHOP_ID == "ARRAY" && x.STEP_SEQ > 0 &&
                x.STEP_DESC.Trim() == "Dense Unpacker + DCLN").FirstOrDefault();
            if (arrayInStepInfo != null)
                this.arrayInTxtEdit.EditValue = arrayInStepInfo.STEP_ID;

            this.arrayOutTxtEdit.EditValue = "9900";
            var arrayOutStepInfo = modelContext.StdStep.Where(x => x.SHOP_ID == "ARRAY" && x.STEP_DESC == "Shipping").LastOrDefault();
            if (arrayOutStepInfo != null)
                this.arrayOutTxtEdit.EditValue = arrayOutStepInfo.STEP_ID;

            //this.cfInTxtEdit.EditValue = "F100-00";
            //var cfInStepInfo = modelContext.StdStep.Where(x => x.SHOP_ID == "CF" && x.STEP_DESC == "Unpacker").FirstOrDefault();
            //if (cfInStepInfo != null)
            //    this.cfInTxtEdit.EditValue = cfInStepInfo.STEP_ID;

            //this.cfOutTxtEdit.EditValue = "F110-00";
            //var cfOutStepInfo = modelContext.StdStep.Where(x => x.SHOP_ID == "CF" && x.STEP_DESC == "Shipping").LastOrDefault();
            //if (cfOutStepInfo != null)
            //    this.cfOutTxtEdit.EditValue = cfOutStepInfo.STEP_ID;
        }
        
        private void queryBtn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            Query();

            _selectedProductList = null;
            _selectedInOutList = null;

            this.Cursor = Cursors.Default;
        }

        private void Query()
        {
            ProcessData();

            XtraPivotGridHelper.DataViewTable dt = CreateDataViewTable();

            FillPivotData(dt);

            DrawPivotGrid(dt);

            DataTable dtChart = GetChartSchema();

            dtChart = FillChartData(dtChart);

            DrawChart(dtChart);
        }

        private void ProcessData()
        {
            _rsltPivotDic = new Dictionary<string, IocvData.ResultPivot>();
            _rsltChartDic = new Dictionary<string, IocvData.ResultChart>();

            var modelContext = this._result.GetCtx<ModelDataContext>();

            // ACT_DATE : string 형식 (20161215 180000 위와 같이 데이터 들어옴)
            //var dtAct = modelContext.InOutAct.Where(x => x.SHOP_ID == this.SelectedShopID
            //    & this.FromTime <= x.ACT_DATE.Replace(" ", "").DbToDateTime()
            //    & x.ACT_DATE.Replace(" ", "").DbToDateTime() < this.ToTime)
            //    .Where(x => this.IsAllProductSeletected ? true : this.SelectedProductList.Contains(x.PRODUCT_ID))
            //    .Where(x => this.IsAllInOutSeletected ? true : this.SelectedInOutList.Contains(x.IN_OUT.ToUpper()))
            //    .Where(x => x.ACT_QTY > 0)
            //    .GroupBy(x => new {
            //        SHOP_ID = x.SHOP_ID,
            //        IN_OUT_FLAG = x.IN_OUT.ToUpper() == IocvData.Consts.InFlag ? IocvData.Consts.InFlag :
            //            IocvData.Consts.OutFlag,
            //        TARGET_DATE = x.ACT_DATE.Replace(" ", "").DbToDateTime(),
            //        PROD_ID = x.PRODUCT_ID,
            //        PROD_VER = x.PRODUCT_VERSION
            //        })
            //    .Select(g => new
            //        {
            //            SHOP_ID = g.Key.SHOP_ID,
            //            IN_OUT_FLAG = g.Key.IN_OUT_FLAG,
            //            TARGET_DATE = g.Key.TARGET_DATE,
            //            PROD_ID = g.Key.PROD_ID,
            //            PROD_VER = g.Key.PROD_VER,
            //            DEPT = IocvData.Consts.ACT,
            //            QTY = g.Sum(s => s.ACT_QTY)
            //        });

            //foreach (var row in dtAct)
            //{
            //    AddResult(row.SHOP_ID, row.IN_OUT_FLAG, row.TARGET_DATE, row.PROD_ID, row.PROD_VER,
            //        IocvData.Consts.ACT, row.QTY);
            //}
            
            // PLAN_DATE : DateTime 형식 (2016-12-08 00:00:00 위와 같이 데이터 들어옴)
            var dtPlan = modelContext.InOutPlan.Where(x => x.SHOP_ID == this.SelectedShopID)
                .Where(x => this.IsAllProductSeletected ? true : this.SelectedProductList.Contains(x.PRODUCT_ID))
                .Where(x => this.IsAllInOutSeletected ? true : this.SelectedInOutList.Contains(x.IN_OUT.ToUpper()))
                .Where(x => x.PLAN_QTY > 0)
                .Select(x => new
                    {
                        SHOP_ID = x.SHOP_ID,
                        IN_OUT_FLAG = x.IN_OUT.Trim().ToUpper() == IocvData.Consts.InFlag ? IocvData.Consts.InFlag :
                            IocvData.Consts.OutFlag,
                        TARGET_DATE = x.PLAN_DATE == x.PLAN_DATE.Date ?
                            ShopCalendar.StartTimeOfDay(x.PLAN_DATE) ://x.PLAN_DATE.AddHours(ShopCalendar.StartTime.Hours)
                            x.PLAN_DATE,      // Data가 00:00:00 형식에 대한 예외처리
                        PROD_ID = x.PRODUCT_ID,
                        //PROD_VER = x.PRODUCT_VERSION,
                        DEPT = IocvData.Consts.PLAN,
                        QTY = (double)x.PLAN_QTY
                    })           
                .Where(x => this.FromTime <= x.TARGET_DATE & x.TARGET_DATE < this.ToTime);

            foreach (var row in dtPlan)
            {
                AddResult(row.SHOP_ID, 
                    row.IN_OUT_FLAG, 
                    row.TARGET_DATE, 
                    row.PROD_ID,
                    "-",//row.PROD_VER,
                    IocvData.Consts.PLAN, row.QTY);
            }

            string filter = Globals.CreateFilter(string.Empty, SmcvData.Schema.SHOP_ID, "=", this.SelectedShopID);
            filter = Globals.CreateFilter(filter, SimResultData.StepMoveSchema.TARGET_DATE, ">=", this.FromTime.ToString(), "AND");
            filter = Globals.CreateFilter(filter, SimResultData.StepMoveSchema.TARGET_DATE, "<", this.ToTime.ToString(), "AND");

            DataTable dtStepMove = _result.LoadOutput(SimResultData.OutputName.StepMove, filter);

            foreach (DataRow row in dtStepMove.Rows)
            {
                SimResultData.StepMoveInfo info = new SimResultData.StepMoveInfo(row);

                if (this.IsAllProductSeletected == false && this.SelectedProductList.Contains(info.ProductID) == false)
                    continue;
                                
                string inStepID = this.SelectedShopID == "ARRAY" ? this.SelectedArrayInStepID :
                    (this.SelectedShopID == "CF" ? this.SelectedCfInStepID : this.SelectedCellInStepID);
                string outStepID = this.SelectedShopID == "ARRAY" ? this.SelectedArrayOutStepID :
                    (this.SelectedShopID == "CF" ? this.SelectedCfOutStepID : "--");

                string inOutFlag = string.Empty;
                if (info.StepID == inStepID)
                    inOutFlag = IocvData.Consts.InFlag;
                else if (info.StepID == outStepID)
                    inOutFlag = IocvData.Consts.OutFlag;
                else
                    continue;
                                
                if (this.IsAllInOutSeletected == false && this.SelectedInOutList.Contains(inOutFlag) == false)
                    continue;

                double qty = inOutFlag == IocvData.Consts.InFlag ? info.InQty : info.OutQty;

                AddResult(info.ShopID, inOutFlag, info.TargetDate, info.ProductID, info.ProductVersion, IocvData.Consts.SIM, qty);
            }
        }

        private void AddResult(string shopID, string inOutFlag, DateTime targetDate, string prodID, string prodVer, string dept, double qty)
        {
            if (qty <= 0)
                return;

            //StartTimeOfDay 확인하기
            string sTargetDate = ShopCalendar.StartTimeOfDayT(targetDate).DbToDateString();

            // Result Of Pivot
            string key = shopID + dept + inOutFlag + sTargetDate + prodID + prodVer;

            IocvData.ResultPivot rsltPivot;
            if (_rsltPivotDic.TryGetValue(key, out rsltPivot) == false)
            {
                _rsltPivotDic.Add(key, rsltPivot = new IocvData.ResultPivot(shopID, inOutFlag, sTargetDate, targetDate, prodID, prodVer, dept));
            }

            rsltPivot.AddQty(qty);

            // Result Of Chart
            string key2 = sTargetDate + "_" + inOutFlag;

            IocvData.ResultChart rsltChart;
            if (_rsltChartDic.TryGetValue(key2, out rsltChart) == false)
            {
                _rsltChartDic.Add(key2, rsltChart = new IocvData.ResultChart(shopID, sTargetDate, targetDate, inOutFlag));
            }

            rsltChart.AddQty(qty, dept);
        }

        #region CreateDataViewTable
        private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        {
            XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();
                        
            dt.AddColumn(IocvData.Schema.SHOP_ID, IocvData.Schema.SHOP_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(IocvData.Schema.IN_OUT_FLAG, IocvData.Schema.IN_OUT_FLAG, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(IocvData.Schema.DEPT, IocvData.Schema.DEPT, typeof(string), PivotArea.RowArea, null, null);

            dt.AddColumn(IocvData.Schema.PROD_ID, IocvData.Schema.PROD_ID, typeof(string), PivotArea.FilterArea, null, null);
            dt.AddColumn(IocvData.Schema.PROD_VER, IocvData.Schema.PROD_VER, typeof(string), PivotArea.FilterArea, null, null);
            
            dt.AddColumn(IocvData.Schema.TARGET_DATE, IocvData.Schema.TARGET_DATE, typeof(string), PivotArea.ColumnArea, null, null);

            dt.AddColumn(IocvData.Schema.QTY, IocvData.Schema.QTY, typeof(double), PivotArea.DataArea, null, null);
           
            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns[IocvData.Schema.SHOP_ID],
                        dt.Columns[IocvData.Schema.IN_OUT_FLAG],
                        dt.Columns[IocvData.Schema.DEPT],
                        dt.Columns[IocvData.Schema.PROD_ID],
                        dt.Columns[IocvData.Schema.PROD_VER],
                        dt.Columns[IocvData.Schema.TARGET_DATE]
                    }
                );

            return dt;
        }
        #endregion

        #region FillPivotData
        private void FillPivotData(XtraPivotGridHelper.DataViewTable dt)
        {            
            foreach (IocvData.ResultPivot item in _rsltPivotDic.Values)
            {
                string prodVer = "-";
                if (item.PROD_VER != null)
                {
                    prodVer = item.PROD_VER;
                }

                dt.DataTable.Rows.Add(
                    item.SHOP_ID,
                    item.IN_OUT_FLAG,
                    item.DEPT,
                    item.PROD_ID,
                    prodVer,
                    item.TARGET_DATE,
                    item.QTY
                );
            }
        }
        #endregion

        #region DrawPivotGrid
        private void DrawPivotGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            this.pivotGridControl1.BeginUpdate();

            this.pivotGridControl1.ClearPivotGridFields();
            this.pivotGridControl1.CreatePivotGridFields(dt);

            this.pivotGridControl1.DataSource = dt.DataTable;
            
            ShowTotal(this.pivotGridControl1);
            //this.pivotGridControl1.Fields[SmcvData.Schema.STEP_ID].SortMode = PivotSortMode.Custom;
            
            pivotGridControl1.Fields[IocvData.Schema.QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[IocvData.Schema.QTY].CellFormat.FormatString = "#,##0";

            //this.pivotGridControl1.BestFitColumnArea();

            this.pivotGridControl1.EndUpdate();
            this.pivotGridControl1.Refresh();
        }

        private void ShowTotal(PivotGridControl pivot)//, bool isCheck)
        {
            pivot.OptionsView.ShowRowTotals = true;
            pivot.OptionsView.ShowRowGrandTotals = true;
            pivot.OptionsView.ShowColumnTotals = true;
            pivot.OptionsView.ShowColumnGrandTotals = true;
        }
        #endregion
        
        private DataTable GetChartSchema()
        {
            DataTable dtChartTable = new DataTable();
            
            dtChartTable.Columns.Add(IocvData.Schema.TARGET_DATE, typeof(string));
            dtChartTable.Columns.Add(IocvData.Schema.DEPT, typeof(string));
            dtChartTable.Columns.Add(IocvData.Schema.QTY, typeof(double));

            //DataColumn[] dcPKeys = new DataColumn[1];

            //dcPKeys[0] = dtChartTable.Columns[SmcvData.Schema.STEP_DESC];
            //dtChartTable.PrimaryKey = dcPKeys;

            return dtChartTable;
        }

        private DataTable FillChartData(DataTable dtChart)
        {
            foreach (var item in _rsltChartDic.Values)
            {
                for (int i=0; i<3; i++)
                {
                    DataRow dRow = dtChart.NewRow();

                    string dept = i == 0 ? IocvData.Consts.ACT : i == 1 ? IocvData.Consts.PLAN : IocvData.Consts.SIM;
                    double qty = i == 0 ? item.ACT_QTY : i == 1 ? item.PLAN_QTY : item.SIM_QTY;

                    dRow[IocvData.Schema.TARGET_DATE] = item.KEY_CHART;
                    dRow[IocvData.Schema.DEPT] = dept;
                    dRow[IocvData.Schema.QTY] = qty;

                    dtChart.Rows.Add(dRow);
                }
            }

            return dtChart;
        }

        private void DrawChart(DataTable dt)
        {
            //if (chartControl1.Diagram != null && ((XYDiagram)chartControl1.Diagram).SecondaryAxesY != null)
            //    ((XYDiagram)chartControl1.Diagram).SecondaryAxesY.Clear();

            chartControl1.Series.Clear();
            
            if (dt == null)
                return;
            
            for (int i = 0; i < 3; i++)
            {
                string seriesName = i == 0 ? IocvData.Consts.ACT : i == 1 ? IocvData.Consts.PLAN : IocvData.Consts.SIM;
 
                ViewType viewType = ViewType.Bar;

                Series series = new Series(seriesName, viewType);
                this.chartControl1.Series.Add(series);
                series.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Auto;
                series.ArgumentDataMember = IocvData.Schema.TARGET_DATE;
                series.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
                series.ValueDataMembers.AddRange(new string[] { IocvData.Schema.QTY });

                if ((series.View as BarSeriesView).BarWidth > 0.3)
                    (series.View as BarSeriesView).BarWidth = 0.3;

                string filter = Globals.CreateFilter(string.Empty, IocvData.Schema.DEPT, "=", seriesName);
                filter = Globals.CreateFilter(filter, IocvData.Schema.QTY, ">", "0", "AND");
                DataView view = new DataView(dt, filter, null, DataViewRowState.CurrentRows);
                series.DataSource = view;

                Color color = i == 0 ? Color.Red : i == 1 ? Color.Green : Color.Blue;
                series.View.Color = color;
            }
        }
    }
}
