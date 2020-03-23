using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraCharts;

using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserInterface;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class ProgressRateView : XtraGridControlView
    {
        private IExperimentResultItem _result;
        private DateTime _planStartTime;

        bool _endLoadDocument = false;

        double _defaultWaitTat;
        double _defaultRunTat;
        private Dictionary<string, PrvData.StepInfo> _stdStepDic;
        private Dictionary<string, string> _mainProcDic;
        private Dictionary<string, PrvData.ProcStepTatInfo> _tatDic;

        private Dictionary<string, PrvData.ResultData> _resultDataDic;
        private Dictionary<string, PrvData.ResultChartData> _resultChartDataDic;
        private List<string> _targetDateInChartList;

        private List<string> _selectedProductList;
        //private Dictionary<string, List<string>> _prodIdsInTatDic;

        string SelectedShopID
        {
            get { return this.shopIdComboBoxEdit.SelectedItem != null ? this.shopIdComboBoxEdit.SelectedItem.ToString() : "Blank"; }
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
                return FromTime.AddHours((double)((int)ShopCalendar.ShiftHours * (int)this.dayShiftSpinEdit.Value));
            }
        }

        bool IsSeletectedAllProduct
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

        int SelectedSectorSize
        {
            get
            {
                return this.sectorSizeSpinEdit.Value > 0 ? (int)this.sectorSizeSpinEdit.Value : 1;
            }
        }

        public ProgressRateView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {
            InitializeBase();

            InitializeControl();

            InitializeData();

            _endLoadDocument = true;
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
            ComboHelper.AddDataToComboBox(this.shopIdComboBoxEdit, _result,
                SimInputData.InputName.StdStep, SimInputData.StdStepSchema.SHOP_ID, false);

            if (this.shopIdComboBoxEdit.Properties.Items.Contains("ARRAY"))
                this.shopIdComboBoxEdit.SelectedIndex = this.shopIdComboBoxEdit.Properties.Items.IndexOf("ARRAY");

            //DateEdit Controls
            this.fromDateEdit.DateTime = ShopCalendar.SplitDate(_planStartTime);
            ComboHelper.ShiftName(this.shiftComboBoxEdit, _planStartTime);

            //// Set ProdInTatDic
            //_prodIdsInTatDic = new Dictionary<string, List<string>>();
            //var dtTat = modelContext.Tat.Where(x => x.PRODUCT_TYPE == "Production")
            //    .Select(x => new { SHOP_ID = x.SHOP_ID, PROD_ID = x.PRODUCT_ID }).Distinct();

            //foreach (var row in dtTat)
            //{
            //    List<string> prodInTatList;
            //    if (_prodIdsInTatDic.TryGetValue(row.SHOP_ID, out prodInTatList) == false)
            //        _prodIdsInTatDic.Add(row.SHOP_ID, prodInTatList = new List<string>());

            //    prodInTatList.Add(row.PROD_ID);
            //}

            // ProductIDs
            SetProdIdCheckBox(); 
        }

        private void SetProdIdCheckBox()
        {
            this.prodIdCheckedComboBoxEdit.Properties.Items.Clear();

            var modelContext = this._result.GetCtx<ModelDataContext>();

            // ProductID CheckComboBox
            this.prodIdCheckedComboBoxEdit.Properties.Items.Clear();

            var prodIDs = modelContext.Product.Where(x => this.SelectedShopID == x.SHOP_ID).Select(x => x.PRODUCT_ID)
                .Distinct();

            //List<string> prodsInTatList;
            //if (_prodIdsInTatDic.TryGetValue(this.SelectedShopID, out prodsInTatList) == false)
            //    prodsInTatList = new List<string>();

            //var seleProds = prodsInTatList.Where(x => prodIDs.Contains(x));

            string filter = Globals.CreateFilter(string.Empty, SimResultData.StepWipSchema.SHOP_ID, "=",
                this.SelectedShopID);
            filter = Globals.CreateFilter(filter, SimResultData.StepWipSchema.TARGET_DATE, ">=",
                this.FromTime.ToString(), "AND");
            filter = Globals.CreateFilter(filter, SimResultData.StepWipSchema.TARGET_DATE, "<",
                this.ToTime.ToString(), "AND");

            DataTable dtStepWip = _result.LoadOutput(SimResultData.OutputName.StepWip, filter);
            var prodsInStepWip = ComboHelper.Distinct(dtStepWip, SimResultData.StepWipSchema.PRODUCT_ID, string.Empty);

            var seleProds = prodIDs.Where(x => prodsInStepWip.Contains(x));

            foreach (var item in seleProds)
                this.prodIdCheckedComboBoxEdit.Properties.Items.Add(item.ToString());

            this.prodIdCheckedComboBoxEdit.CheckAll();
        }

        private void InitializeData()
        {
            var modelContext = this._result.GetCtx<ModelDataContext>();

            _stdStepDic = new Dictionary<string, PrvData.StepInfo>();
            var stdStep = modelContext.StdStep.Where(x => x.STEP_SEQ > 0)
                .OrderBy(x => x.SHOP_ID);//.OrderByDescending(x => x.STEP_SEQ);

            foreach (var step in stdStep)
            {
                if (_stdStepDic.ContainsKey(step.SHOP_ID + step.STEP_ID) == false)
                    _stdStepDic.Add(step.SHOP_ID + step.STEP_ID, new PrvData.StepInfo(step.SHOP_ID, step.STEP_ID,
                        step.STEP_DESC, step.STEP_TYPE, (int)step.STEP_SEQ, step.LAYER_ID));
            }
                        
            try
            {
                _defaultRunTat = Convert.ToDouble(_result.Experiment.GetArgument("DefaultRunTAT"));
                _defaultWaitTat = Convert.ToDouble(_result.Experiment.GetArgument("DefaultWaitTAT"));
            }
            catch
            {
                _defaultWaitTat = 0;
                _defaultRunTat = 0;
            }
            
            _mainProcDic = new Dictionary<string, string>();

            var dtProduct = modelContext.Product;
            foreach (var prod in dtProduct)
            {
                if (_mainProcDic.ContainsKey(prod.SHOP_ID + prod.PRODUCT_ID) == false)
                    _mainProcDic.Add(prod.SHOP_ID + prod.PRODUCT_ID, prod.PROCESS_ID);
            }

            foreach (var prod in dtProduct)
            {
                if (_mainProcDic.ContainsKey(prod.SHOP_ID + prod.PRODUCT_ID) == false)
                    _mainProcDic.Add(prod.SHOP_ID + prod.PRODUCT_ID, prod.PROCESS_ID);
            }

            var dtTat = modelContext.Tat;// modelContext.Tat.Where(x => x.PRODUCT_TYPE == "Production");//.Join(modelContext.ProcStep.Where(x => x.IS_MANDATORY == 'Y'),
                    //t => t.SHOP_ID + t.PROCESS_ID + t.STEP_ID,
                    //p => p.SHOP_ID + p.PROCESS_ID + p.STEP_ID,
                    //(t, p) => new
                    //{
                    //    SHOP_ID = t.SHOP_ID,
                    //    PRODUCT_ID = t.PRODUCT_ID,
                    //    PROCESS_ID = t.PROCESS_ID,
                    //    STEP_ID = t.STEP_ID,
                    //    STEP_SEQ = p.STEP_SEQ,
                    //    RUN_TAT = t.RUN_TAT,
                    //    WAIT_TAT = t.WAIT_TAT
                    //})
                    //.OrderBy(x => x.SHOP_ID + x.PROD_ID + x.PROC_ID).ThenBy(x => x.STEP_SEQ);
            Dictionary<string, PrvData.TatInfo> tatAllDic = new Dictionary<string, PrvData.TatInfo>();
            foreach (var row in dtTat)
            {
                PrvData.TatInfo tatInfo = new PrvData.TatInfo(row.WAIT_TAT, row.RUN_TAT);
                if (tatAllDic.ContainsKey(row.SHOP_ID + row.PRODUCT_ID + row.PROCESS_ID + row.STEP_ID) == false)
                    tatAllDic.Add(row.SHOP_ID + row.PRODUCT_ID + row.PROCESS_ID + row.STEP_ID, tatInfo);
            }
                        
            var procStepMand = modelContext.ProcStep.OrderBy(x => x.SHOP_ID + x.PROCESS_ID).OrderBy(x => x.STEP_SEQ);

            Dictionary<string, List<ProcStep>> procStepDic = new Dictionary<string,List<ProcStep>>();
            foreach (var row in procStepMand)
            {
                string key = CommonHelper.CreateKey(row.SHOP_ID, row.PROCESS_ID);

                List<ProcStep> list;
                if (procStepDic.TryGetValue(key, out list) == false)
                    procStepDic.Add(key, list = new List<ProcStep>());

                list.Add(row);
            }

            List<string> mainProcProdList = new List<string>();
            DataTable dtStepWip = _result.LoadOutput(SimResultData.OutputName.StepWip);
            foreach (DataRow drow in dtStepWip.Rows)
            {
                SimResultData.StepWip stepWip = new SimResultData.StepWip(drow);

                string mainProcID = string.Empty;
                _mainProcDic.TryGetValue(stepWip.ShopID + stepWip.ProductID, out mainProcID);
                if (string.IsNullOrEmpty(mainProcID))
                    continue;

                string item = CommonHelper.CreateKey(stepWip.ShopID, stepWip.ProductID, mainProcID);
                if (mainProcProdList.Contains(item) == false)
                    mainProcProdList.Add(item);
            }

            _tatDic = new Dictionary<string, PrvData.ProcStepTatInfo>();
            foreach (string item in mainProcProdList)
            {
                string shopID = item.Split('@')[0];
                string prodID = item.Split('@')[1];
                string mainProcID = item.Split('@')[2];

                string key = CommonHelper.CreateKey(shopID, mainProcID);
                List<ProcStep> procStepList;
                if (procStepDic.TryGetValue(key, out procStepList) == false)
                    continue;

                double cumTat = 0;
                List<string> keyList = new List<string>();
                foreach (ProcStep procStep in procStepList)
                {
                    double waitTat = _defaultWaitTat;
                    double runTat = _defaultWaitTat;
                    double tat = waitTat + runTat;

                    string key2 = procStep.SHOP_ID + prodID + procStep.PROCESS_ID + procStep.STEP_ID;
                    PrvData.TatInfo tatInfo;
                    if (tatAllDic.TryGetValue(key2, out tatInfo))
                    {
                        waitTat = tatInfo.WaitTat;
                        runTat = tatInfo.RunTat;
                        tat = tatInfo.Tat;
                    }

                    cumTat += tat;
                    
                    PrvData.ProcStepTatInfo psTatInfo;
                    if (_tatDic.TryGetValue(key2, out psTatInfo) == false)
                    {
                        _tatDic.Add(key2, psTatInfo = new PrvData.ProcStepTatInfo(procStep.SHOP_ID, prodID, procStep.PROCESS_ID,
                            procStep.STEP_ID, procStep.STEP_SEQ, waitTat, runTat, cumTat));
                    }

                    if (keyList.Contains(key2) == false)
                        keyList.Add(key2);
                }
                
                double totalTat = cumTat;

                foreach (string k in keyList)
                {
                    PrvData.ProcStepTatInfo psTatInfo;
                    if (_tatDic.TryGetValue(k, out psTatInfo) == false)
                        continue; // 있을 수 없음

                    psTatInfo.SetTotalTat(totalTat);
                }
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            
            Query();

            _selectedProductList = null;
            
            this.Cursor = Cursors.Default;
        }

        private void Query()
        {
            SetResultData();

            SetResultChartData();

            DataTable dtChart = GetChartSchema();

            dtChart = FillChartData(dtChart);

            DrawChart(dtChart);  
        }

        private void SetResultData()
        {
            _resultDataDic = new Dictionary<string, PrvData.ResultData>();

            string filter = Globals.CreateFilter(string.Empty, SimResultData.StepWipSchema.SHOP_ID, "=",
                this.SelectedShopID);
            filter = Globals.CreateFilter(filter, SimResultData.StepWipSchema.TARGET_DATE, ">=",
                this.FromTime.ToString(), "AND");
            filter = Globals.CreateFilter(filter, SimResultData.StepWipSchema.TARGET_DATE, "<",
                this.ToTime.ToString(), "AND");

            DataTable dtStepWip = _result.LoadOutput(SimResultData.OutputName.StepWip, filter);
            //var prodsInStepWip = ComboHelper.Distinct(dtStepWip, SimResultData.StepWipSchema.PRODUCT_ID, string.Empty);
            foreach (DataRow drow in dtStepWip.Rows)
            {
                SimResultData.StepWip stepWip = new SimResultData.StepWip(drow);

                if (this.SelectedProductList.Contains(stepWip.ProductID) == false)
                    continue;

                string mainProcID = string.Empty;
                _mainProcDic.TryGetValue(stepWip.ShopID + stepWip.ProductID, out mainProcID);
                if (string.IsNullOrEmpty(mainProcID))
                    continue;

                PrvData.StepInfo stepInfo;
                _stdStepDic.TryGetValue(stepWip.ShopID + stepWip.StepID, out stepInfo);

                //string[] isWaitRunArr = stepWip.WaitQty * stepWip.RunQty > 0 ? new string[]{"Run", "Wait"}
                //    : stepWip.RunQty > 0 ? new string[]{"Run"} : new string[]{"Wait"};

                //foreach (string isWaitRun in isWaitRunArr)
                //{
                //}
                
                string key = stepWip.ShopID + stepWip.ProductID + mainProcID + stepWip.StepID + stepWip.TargetDate;
                PrvData.ResultData resultData;
                if (_resultDataDic.TryGetValue(key, out resultData) == false)
                {
                    resultData = new PrvData.ResultData(stepWip.ShopID, stepWip.ProductID, mainProcID, stepWip.StepID, stepInfo
                        , stepWip.TargetDate);

                    string key2 = stepWip.ShopID + stepWip.ProductID + mainProcID + stepWip.StepID;
                    PrvData.ProcStepTatInfo tatInfo;
                    _tatDic.TryGetValue(key2, out tatInfo);

                    resultData.SetTatAndProgRate(tatInfo, this.SelectedSectorSize);

                    _resultDataDic.Add(key, resultData);
                }

                resultData.AddQty((int)stepWip.WaitQty, (int)stepWip.RunQty);
            }
        }

        private void SetResultChartData()
        {
            _resultChartDataDic = new Dictionary<string, PrvData.ResultChartData>();
            _targetDateInChartList = new List<string>();

            foreach (PrvData.ResultData rsltData in _resultDataDic.Values)
            {
                // Run
                int rangeMaxRun = rsltData.MaxProgRateRunInRange;

                string sTargetDate = rsltData.TargetDate.ToString("yyyy-MM-dd HH:mm:ss");

                if (_targetDateInChartList.Contains(sTargetDate) == false)
                    _targetDateInChartList.Add(sTargetDate);

                string key = CommonHelper.CreateKey(rsltData.RangeOfRun, sTargetDate);

                if (rangeMaxRun <= 0 )
                { }                

                PrvData.ResultChartData rsltChartData;
                if (_resultChartDataDic.TryGetValue(key, out rsltChartData) == false && rangeMaxRun > 0)
                    _resultChartDataDic.Add(key, rsltChartData = new PrvData.ResultChartData(rsltData.RangeOfRun, sTargetDate));

                if (rsltChartData != null)
                {
                    rsltChartData.AddQty(true, rsltData.RunQty);
                }

                // Wait
                int rangeMaxWait = rsltData.MaxProgRateWaitInRange;

                key = CommonHelper.CreateKey(rsltData.RangeOfWait, sTargetDate);

                rsltChartData = null;
                if (_resultChartDataDic.TryGetValue(key, out rsltChartData) == false && rangeMaxWait > 0)
                    _resultChartDataDic.Add(key, rsltChartData = new PrvData.ResultChartData(rsltData.RangeOfWait, sTargetDate));

                if (rsltChartData != null)
                {
                    rsltChartData.AddQty(false, rsltData.WaitQty);
                }
            }

            _resultChartDataDic = _resultChartDataDic.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
            _targetDateInChartList.Sort();
        }

        private DataTable GetChartSchema()
        {
            DataTable dtChartTable = new DataTable();

            dtChartTable.Columns.Add(PrvData.Schema.RANGE, typeof(string));            
            dtChartTable.Columns.Add(PrvData.Schema.TARGET_DATE, typeof(string));
            dtChartTable.Columns.Add(PrvData.Schema.QTY, typeof(int));

            //DataColumn[] dcPKeys = new DataColumn[1];

            //dcPKeys[0] = dtChartTable.Columns[PrvData.Schema.PROG_RATE];
            //dtChartTable.PrimaryKey = dcPKeys;

            return dtChartTable;
        }

        private DataTable FillChartData(DataTable dtChart)
        {
            foreach (PrvData.ResultChartData data in _resultChartDataDic.Values)
            {
                DataRow dRow = dtChart.NewRow();

                dRow[PrvData.Schema.RANGE] = data.Range;
                dRow[PrvData.Schema.TARGET_DATE] = data.TargetDate;
                dRow[PrvData.Schema.QTY] = data.Qty;

                dtChart.Rows.Add(dRow);
            }

            return dtChart;
        }

        private void DrawChart(DataTable dt)
        {
            chartControl1.Series.Clear();

            if (dt == null)
                return;

            foreach (string targetDate in _targetDateInChartList)
            {
                string seriesName = targetDate;

                ViewType viewType = ViewType.Bar;

                Series series = new Series(seriesName, viewType);
                this.chartControl1.Series.Add(series);
                series.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Qualitative;
                series.ArgumentDataMember = PrvData.Schema.RANGE;
                series.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
                series.ValueDataMembers.AddRange(new string[] { PrvData.Schema.QTY });
                //series.CrosshairLabelPattern = "{S}({A}) : {V:##0.0}%";

                if ((series.View as BarSeriesView).BarWidth >= 0.5)
                    (series.View as BarSeriesView).BarWidth = 0.5;

                string filter = string.Format("{0} = '{1}'", PrvData.Schema.TARGET_DATE, targetDate);
                DataView view = new DataView(dt, filter, null, DataViewRowState.CurrentRows);
                series.DataSource = view;
            }            

            //Color color = Color.Navy;
            //series.View.Color = color;
        }

        private void shopIdComboBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_endLoadDocument == false)
                return;

            // ProductIDs
            SetProdIdCheckBox(); 
        }

        private void fromDateEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_endLoadDocument == false)
                return;

            // ProductIDs
            SetProdIdCheckBox(); 
        }

        private void shiftComboBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_endLoadDocument == false)
                return;

            // ProductIDs
            SetProdIdCheckBox(); 
        }

        private void dayShiftSpinEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_endLoadDocument == false)
                return;

            // ProductIDs
            SetProdIdCheckBox();
        }

        private void popUpDataBtn_Click(object sender, EventArgs e)
        {
            SetResultData();

            DataTable dt = new DataTable();
            dt.Columns.Add(PrvData.Schema.SHOP_ID, typeof(string));
            dt.Columns.Add(PrvData.Schema.TARGET_DATE, typeof(string));
            dt.Columns.Add(PrvData.Schema.PRODUCT_ID, typeof(string));
            dt.Columns.Add(PrvData.Schema.PROCESS_ID, typeof(string));
            dt.Columns.Add(PrvData.Schema.STEP_ID, typeof(string));
            dt.Columns.Add(PrvData.Schema.STEP_DESC, typeof(string));
            dt.Columns.Add(PrvData.Schema.STD_STEP_SEQ, typeof(int));
            dt.Columns.Add(PrvData.Schema.LAYER, typeof(string));
            dt.Columns.Add(PrvData.Schema.IS_RUN, typeof(string));
            dt.Columns.Add(PrvData.Schema.QTY, typeof(int));
            dt.Columns.Add(PrvData.Schema.PROG_RATE, typeof(double));
            dt.Columns.Add(PrvData.Schema.RANGE, typeof(string));
            dt.Columns.Add(PrvData.Schema.WAIT_TAT, typeof(double));
            dt.Columns.Add(PrvData.Schema.RUN_TAT, typeof(double));
            dt.Columns.Add(PrvData.Schema.TAT, typeof(double));
            dt.Columns.Add(PrvData.Schema.CUM_TAT, typeof(double));
            dt.Columns.Add(PrvData.Schema.TOTAL_TAT, typeof(double));

            var rsltAll = _resultDataDic.Values//.Where(x => x.ProgressRateWait * x.ProgressRateRun > 0)
                .OrderBy(x => x.ShopID).ThenBy(x => x.TargetDate)
                .ThenBy(x => x.ProdID + x.ProcID).ThenBy(x => x.StdStepSeq)
                .ThenBy(x => x.ProgressRateWait).ThenBy(x => x.ProgressRateRun);

            foreach (PrvData.ResultData rsltData in rsltAll)
            {
                if (rsltData.ProgressRateWait > 0)
                {
                    DataRow waitDrow = dt.NewRow();

                    waitDrow[PrvData.Schema.SHOP_ID] = rsltData.ShopID;
                    waitDrow[PrvData.Schema.TARGET_DATE] = rsltData.TargetDate.ToString("yyyy-MM-dd HH:mm:ss");
                    waitDrow[PrvData.Schema.PRODUCT_ID] = rsltData.ProdID;
                    waitDrow[PrvData.Schema.PROCESS_ID] = rsltData.ProcID;
                    waitDrow[PrvData.Schema.STEP_ID] = rsltData.StepID;
                    waitDrow[PrvData.Schema.STEP_DESC] = rsltData.StepDesc;
                    waitDrow[PrvData.Schema.STD_STEP_SEQ] = rsltData.StdStepSeq;
                    waitDrow[PrvData.Schema.LAYER] = rsltData.Layer;
                    waitDrow[PrvData.Schema.IS_RUN] = string.Empty;
                    waitDrow[PrvData.Schema.QTY] = rsltData.WaitQty;
                    waitDrow[PrvData.Schema.PROG_RATE] = Math.Round(rsltData.ProgressRateWait,4);
                    waitDrow[PrvData.Schema.RANGE] = rsltData.RangeOfWait;
                    waitDrow[PrvData.Schema.WAIT_TAT] = Math.Round(rsltData.WaitTat, 2);
                    waitDrow[PrvData.Schema.RUN_TAT] = Math.Round(rsltData.RunTat,2);
                    waitDrow[PrvData.Schema.TAT] = Math.Round(rsltData.Tat,2);
                    waitDrow[PrvData.Schema.CUM_TAT] = Math.Round(rsltData.CumTat,2);
                    waitDrow[PrvData.Schema.TOTAL_TAT] = Math.Round(rsltData.TotalTat,2);

                    dt.Rows.Add(waitDrow);
                }

                if (rsltData.ProgressRateRun > 0)
                {
                    DataRow runDrow = dt.NewRow();

                    runDrow[PrvData.Schema.SHOP_ID] = rsltData.ShopID;
                    runDrow[PrvData.Schema.TARGET_DATE] = rsltData.TargetDate.ToString("yyyy-MM-dd HH:mm:ss");
                    runDrow[PrvData.Schema.PRODUCT_ID] = rsltData.ProdID;
                    runDrow[PrvData.Schema.PROCESS_ID] = rsltData.ProcID;
                    runDrow[PrvData.Schema.STEP_ID] = rsltData.StepID;
                    runDrow[PrvData.Schema.STEP_DESC] = rsltData.StepDesc;
                    runDrow[PrvData.Schema.STD_STEP_SEQ] = rsltData.StdStepSeq;
                    runDrow[PrvData.Schema.LAYER] = rsltData.Layer;
                    runDrow[PrvData.Schema.IS_RUN] = "Y";
                    runDrow[PrvData.Schema.QTY] = rsltData.RunQty;
                    runDrow[PrvData.Schema.PROG_RATE] = Math.Round(rsltData.ProgressRateRun, 4);
                    runDrow[PrvData.Schema.RANGE] = rsltData.RangeOfRun;
                    runDrow[PrvData.Schema.WAIT_TAT] = Math.Round(rsltData.WaitTat,2);
                    runDrow[PrvData.Schema.RUN_TAT] = Math.Round(rsltData.RunTat,2);
                    runDrow[PrvData.Schema.TAT] = Math.Round(rsltData.Tat,2);
                    runDrow[PrvData.Schema.CUM_TAT] = Math.Round(rsltData.CumTat,2);
                    runDrow[PrvData.Schema.TOTAL_TAT] = Math.Round(rsltData.TotalTat,2);

                    dt.Rows.Add(runDrow);
                }
            }
            
            SimpleGridPopUp simpleGrid = new SimpleGridPopUp("Progress Rate", dt);
            simpleGrid.SetFormSize(1300, 900);
            simpleGrid.SetFooter(true);
            //simpleGrid.SetColumnDisplayFormat(PrvData.Schema.TARGET_DATE, DevExpress.Utils.FormatType.DateTime, "yyyy-MM-dd HH:mm:ss");
            
            simpleGrid.Show();
            simpleGrid.Focus();
        }
    }
}
