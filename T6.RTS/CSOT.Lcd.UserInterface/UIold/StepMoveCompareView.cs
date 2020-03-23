using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraCharts;

using Mozart.Collections;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class StepMoveCompareView : XtraGridControlView
    {
        private IExperimentResultItem _result;
        private DateTime _planStartTime;

        private DoubleDictionary<string, string, SmcvData.Wip> _wipQtyAllDDic;
        private Dictionary<string, SmcvData.WipDetail> _wipCurStepDic;
        private Dictionary<string, SmcvData.WipDetail> _wipMainStepDic;

        private Dictionary<string, SmcvData.StepInfo> _stepInfoDic;
        private List<string> _maindatoryStepList;

        private Dictionary<string, SmcvData.StepInfo> _mainStepInProcessingDic;
        private Dictionary<string, int> _simMoveChartDic;
        private Dictionary<string, int> _actMoveChartDic;
        private Dictionary<string, int> _wipQtyChartDic;
        private Dictionary<string, int> _wipQtyChartDic2;
        private Dictionary<string, SmcvData.Wip> _wipByProdGridDic;
        private Dictionary<string, SmcvData.Wip> _wipByProdGridDic2;
        private DoubleDictionary<string, string, SmcvData.MoveInfo> _moveGridDDic;

        private List<string> _selectedProductList;

        DataTable _dtMoveGrid;
        DataTable _dtWipGrid;

        bool _fixGridData = false;
        string _argOfCursorInChart;
        
        bool IsTimeConditionHour
        {
            get { return this.hourRadioButton.Checked; }
        }

        bool IsTimeConditionHourWhenQuery
        {
            get;
            set;
        }

        string SelectedShopID
        {
            get { return this.shopIdComboBoxEdit.SelectedItem != null ? this.shopIdComboBoxEdit.SelectedItem.ToString() : "Blank"; }
        }

        bool IsSeletectedAllProduct
        {
            get
            {
                return this.prodIdCheckedComboBoxEdit.Properties.Items.GetCheckedValues().Count
                    == this.prodIdCheckedComboBoxEdit.Properties.Items.Count;
            }
        }
        bool IsSeletectedAllProductWhenQuery
        {
            get;
            set;
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

        List<string> SeletedProductListWhenQuery
        {
            get;
            set;
        }

        private DateTime FromTime
        {
            get
            {
                DateTime dt = this.fromTimeEdit.DateTime;

                //dt = dt.AddMinutes(-dt.Minute).AddSeconds(-dt.Second);

                return dt;
            }
        }

        private DateTime ToTime
        {
            get
            {
                if (this.IsTimeConditionHour)
                    return FromTime.AddHours((double)this.hourShiftSpinEdit.Value);
                else
                    return FromTime.AddHours((double)((int)ShopCalendar.ShiftHours * (int)this.hourShiftSpinEdit.Value));
            }
        }

        private bool IsSelectedInputWip
        {
            get
            {
                return this.wipCondInputBtn.Checked;
            }
        }
        private bool IsSelectedInputWipWhenQuery
        {
            get;
            set;
        }

        private string WipQtyColumnName
        {
            get;
            set;
        }

        public StepMoveCompareView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {
            InitializeBase();

            InitializeControl();

            InitializeData();
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

            // ProductID CheckComboBox
            this.prodIdCheckedComboBoxEdit.Properties.Items.Clear();

            var prodIDs = (from a in modelContext.Product
                           select new { PRODUCT_ID = a.PRODUCT_ID })
                           .Distinct().OrderBy(x => x.PRODUCT_ID);

            foreach (var item in prodIDs)
                this.prodIdCheckedComboBoxEdit.Properties.Items.Add(item.PRODUCT_ID.ToString());

            this.prodIdCheckedComboBoxEdit.CheckAll();

            //DateEdit Controls
            this.fromTimeEdit.DateTime = _planStartTime;
            this.hourShiftSpinEdit.Value = 24;// (decimal)(ShopCalendar.StartTimeOfDayT(_planStartTime).AddDays(1) - _planStartTime).TotalHours;
            this.fromTimeEdit.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.fromTimeEdit.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            // Time Constrols
            this.hourRadioButton.Checked = true;
            this.shiftRadioButton.Checked = false;

            // Wip Condition
            this.wipCondSimTimeBtn.Checked = false;
            this.wipCondInputBtn.Checked = true;
        }

        private void InitializeData()
        {
            var modelContext = this._result.GetCtx<ModelDataContext>();

            var distProduct = modelContext.Product.Select(x => new
                                                            { SHOP_ID = x.SHOP_ID, 
                                                            PRODUCT_ID = x.PRODUCT_ID, 
                                                            PROCESS_ID = x.PROCESS_ID }
                                                        ).Distinct();

            Dictionary<string, string> mainProcByProdDic = distProduct.ToDictionary
                (
                    x => x.SHOP_ID + x.PRODUCT_ID,
                    y => y.PROCESS_ID
                );

            _maindatoryStepList = modelContext.ProcStep.Select(x => x.STEP_ID.ToString()).Distinct().ToList();

            //Dictionary<string, string> mainProcByProdDic = new Dictionary<string, string>();
            //foreach (var row in modelContext.Product)
            //{
            //    string key = row.SHOP_ID + row.PRODUCT_ID;
            //    if (mainProcByProdDic.ContainsKey(key) == false)
            //        mainProcByProdDic.Add(key, row.PROCESS_ID);                    
            //}

            // Process 를 이용한 NextMainStep
            var prodProcStep = modelContext.ProcStep.Where(x => x.STEP_SEQ > 0)
                .Join(distProduct,
                    ps => ps.SHOP_ID + ps.PROCESS_ID, p => p.SHOP_ID + p.PROCESS_ID,
                    (ps, p) => new
                    {
                        SHOP_ID = ps.SHOP_ID,
                        PROD_ID = p.PRODUCT_ID,
                        MAIN_PROC_ID = ps.PROCESS_ID,
                        STEP_ID = ps.STEP_ID,
                        STEP_SEQ = ps.STEP_SEQ,                        
                    })
                .OrderBy(x => x.SHOP_ID).ThenBy(x => x.PROD_ID).ThenBy(x => x.MAIN_PROC_ID).ThenByDescending(x => x.STEP_SEQ);

            string preInfo = string.Empty;
            string nextMainStepID = string.Empty;
            Dictionary<string, string> nextMainStepInProc = new Dictionary<string, string>();
            foreach (var step in prodProcStep)
            {
                if (preInfo != step.SHOP_ID + step.PROD_ID + step.MAIN_PROC_ID)
                    nextMainStepID = string.Empty;

                if (IsMainStep(step.STEP_ID))
                    nextMainStepID = step.STEP_ID;

                if (string.IsNullOrEmpty(nextMainStepID) == false
                    && nextMainStepInProc.ContainsKey(step.SHOP_ID + step.PROD_ID + step.MAIN_PROC_ID + step.STEP_ID) == false)
                {
                    nextMainStepInProc.Add(step.SHOP_ID + step.PROD_ID + step.MAIN_PROC_ID + step.STEP_ID, nextMainStepID);
                }

                preInfo = step.SHOP_ID + step.PROD_ID + step.MAIN_PROC_ID;
            }

            // ALL Step정보
            _stepInfoDic = new Dictionary<string, SmcvData.StepInfo>();

            // StdStep을 이용한 NextMain 정보
            var stdStep = modelContext.StdStep.Where(x => x.STEP_SEQ > 0)
                .OrderBy(x => x.SHOP_ID).OrderByDescending(x => x.STEP_SEQ);
            
            preInfo = string.Empty;
            nextMainStepID = string.Empty;
            Dictionary<string, string> nextMainStepInStdStep = new Dictionary<string, string>();
            foreach (var step in stdStep)
            {
                if (_stepInfoDic.ContainsKey(step.SHOP_ID + step.STEP_ID) == false)
                    _stepInfoDic.Add(step.SHOP_ID + step.STEP_ID, new SmcvData.StepInfo(step.SHOP_ID, step.STEP_ID,
                        step.STEP_DESC, step.STEP_TYPE, (int)step.STEP_SEQ, step.LAYER_ID));

                if (preInfo != step.SHOP_ID)
                    nextMainStepID = string.Empty;

                if (IsMainStep(step.STEP_ID))
                    nextMainStepID = step.STEP_ID;

                if (string.IsNullOrEmpty(nextMainStepID) == false
                    && nextMainStepInStdStep.ContainsKey(step.SHOP_ID + step.STEP_ID) == false)
                {
                    nextMainStepInStdStep.Add(step.SHOP_ID + step.STEP_ID, nextMainStepID);
                }

                preInfo = step.SHOP_ID;
            }

            // Input / Wip 재공 정보
            _wipQtyAllDDic = new DoubleDictionary<string, string, SmcvData.Wip>();
            _wipCurStepDic = new Dictionary<string, SmcvData.WipDetail>();
            _wipMainStepDic = new Dictionary<string, SmcvData.WipDetail>();
            var wips = modelContext.Wip;
            foreach (var wip in wips)
            {
                string key = wip.SHOP_ID + wip.PRODUCT_ID;
                string mainProc = string.Empty;
                string wipProdVer = string.Empty;
                mainProcByProdDic.TryGetValue(key, out mainProc);
               
                //if (wip.PRODUCT_VERSION == null)
                //    wipProdVer = "-";


                nextMainStepID = string.Empty;
                if (string.IsNullOrEmpty(mainProc) == false)
                    nextMainStepInProc.TryGetValue(wip.SHOP_ID + wip.PRODUCT_ID + mainProc + wip.STEP_ID, out nextMainStepID);

                if (string.IsNullOrEmpty(nextMainStepID))
                    nextMainStepInStdStep.TryGetValue(wip.SHOP_ID + wip.STEP_ID, out nextMainStepID);

                if (string.IsNullOrEmpty(nextMainStepID) == false)
                {
                    // Chart 용
                    Dictionary<string, SmcvData.Wip> wipDic;
                    if (_wipQtyAllDDic.TryGetValue(wip.SHOP_ID, out wipDic) == false)
                    {
                        wipDic = new Dictionary<string, SmcvData.Wip>();
                        _wipQtyAllDDic.Add(wip.SHOP_ID, wipDic);

                    }

                    SmcvData.Wip wipInfo;
                    if (wipDic.TryGetValue(wip.SHOP_ID + wip.PRODUCT_ID + nextMainStepID, out wipInfo) == false)
                        wipDic.Add(wip.SHOP_ID + wip.PRODUCT_ID + nextMainStepID,
                            wipInfo = new SmcvData.Wip(wip.SHOP_ID, wip.PRODUCT_ID, nextMainStepID));

                    wipInfo.AddGlassQty(wipProdVer, (int)wip.GLASS_QTY);

                    SmcvData.StepInfo stepInfo;
                    _stepInfoDic.TryGetValue(wip.SHOP_ID + nextMainStepID, out stepInfo);
                    string stepDesc = stepInfo == null ? Consts.NULL_ID : stepInfo.StepDesc;
                    int stepSeq = stepInfo == null ? int.MaxValue : stepInfo.StepSeq;
                    string layer = stepInfo == null ? Consts.NULL_ID : stepInfo.Layer;

                    // Pivot 용
                    string key2 = wip.SHOP_ID + wip.PRODUCT_ID + wipProdVer + wip.PROCESS_ID + nextMainStepID;
                    SmcvData.WipDetail wipDetail;
                    if (_wipMainStepDic.TryGetValue(key2, out wipDetail) == false)
                        _wipMainStepDic.Add(key2, wipDetail = new SmcvData.WipDetail(wip.SHOP_ID, wip.PRODUCT_ID, wipProdVer
                            ,wip.PROCESS_ID, nextMainStepID, stepSeq, stepDesc, layer));

                    wipDetail.AddGlassQty((int)wip.GLASS_QTY);
                }

                SmcvData.StepInfo stepInfo2;
                _stepInfoDic.TryGetValue(wip.SHOP_ID + wip.STEP_ID, out stepInfo2);
                string stepDesc2 = stepInfo2 == null ? Consts.NULL_ID : stepInfo2.StepDesc;
                int stepSeq2 = stepInfo2 == null ? int.MaxValue : stepInfo2.StepSeq;
                string layer2 = stepInfo2 == null ? Consts.NULL_ID : stepInfo2.Layer;

                // Pivot 용
                string key3 = wip.SHOP_ID + wip.PRODUCT_ID + wipProdVer + wip.PROCESS_ID + wip.STEP_ID;
                SmcvData.WipDetail wipDetailInf;
                if (_wipCurStepDic.TryGetValue(key3, out wipDetailInf) == false)
                    _wipCurStepDic.Add(key3, wipDetailInf = new SmcvData.WipDetail(wip.SHOP_ID, wip.PRODUCT_ID, wipProdVer
                        ,wip.PROCESS_ID, wip.STEP_ID, stepSeq2, stepDesc2, layer2));

                wipDetailInf.AddGlassQty((int)wip.GLASS_QTY);
            }

            // Result / StepWip 재공정보
            DataTable dtStepWip = _result.LoadOutput(SimResultData.OutputName.StepWip);

            foreach (DataRow drow in dtStepWip.Rows)
            {
                SimResultData.StepWip row = new SimResultData.StepWip(drow);

                string key0 = row.ShopID + row.ProductID;
                string mainProc = string.Empty;
                mainProcByProdDic.TryGetValue(key0, out mainProc);

                nextMainStepID = string.Empty;
                if (string.IsNullOrEmpty(mainProc) == false)
                    nextMainStepInProc.TryGetValue(row.ShopID + row.ProductID + mainProc + row.StepID, out nextMainStepID);

                if (string.IsNullOrEmpty(nextMainStepID))
                    nextMainStepInStdStep.TryGetValue(row.ShopID + row.StepID, out nextMainStepID);

                int wipQty = (int)row.WaitQty + (int)row.RunQty;

                if (string.IsNullOrEmpty(nextMainStepID) == false)
                {
                    // Chart 용
                    Dictionary<string, SmcvData.Wip> wipDic;
                    string key1 = row.TargetDate.DbToString() + row.ShopID;
                    if (_wipQtyAllDDic.TryGetValue(key1, out wipDic) == false)
                    {
                        wipDic = new Dictionary<string, SmcvData.Wip>();
                        _wipQtyAllDDic.Add(key1, wipDic);
                    }

                    string key2 = row.ShopID + row.ProductID + nextMainStepID;
                    SmcvData.Wip wipInfo;
                    if (wipDic.TryGetValue(key2, out wipInfo) == false)
                        wipDic.Add(key2, wipInfo = new SmcvData.Wip(row.ShopID, row.ProductID, nextMainStepID));
                    
                    //wipInfo.AddGlassQty(row.ProductVersion, wipQty);

                    SmcvData.StepInfo stepInfo;
                    _stepInfoDic.TryGetValue(row.ShopID + nextMainStepID, out stepInfo);
                    string stepDesc = stepInfo == null ? Consts.NULL_ID : stepInfo.StepDesc;
                    int stepSeq = stepInfo == null ? int.MaxValue : stepInfo.StepSeq;
                    string layer = stepInfo == null ? Consts.NULL_ID : stepInfo.Layer;

                    // Pivot 용
                    string key3 = row.TargetDate.DbToString() + row.ShopID + row.ProductID + row + row.ProcessID + nextMainStepID;
                    SmcvData.WipDetail wipDetail;
                    if (_wipMainStepDic.TryGetValue(key3, out wipDetail) == false)
                        _wipMainStepDic.Add(key3, wipDetail = new SmcvData.WipDetail(row.ShopID, row.ProductID, row.ProductVersion
                            , row.ProcessID, nextMainStepID, stepSeq, stepDesc, layer));

                    wipDetail.AddGlassQty(wipQty);
                }

                SmcvData.StepInfo stepInfo2;
                _stepInfoDic.TryGetValue(row.ShopID + row.StepID, out stepInfo2);
                string stepDesc2 = stepInfo2 == null ? Consts.NULL_ID : stepInfo2.StepDesc;
                int stepSeq2 = stepInfo2 == null ? int.MaxValue : stepInfo2.StepSeq;
                string layer2 = stepInfo2 == null ? Consts.NULL_ID : stepInfo2.Layer;

                // Pivot 용
                string key4 = row.TargetDate.DbToString() + row.ShopID + row.ProductID + row.ProductVersion + row.ProcessID + row.StepID;
                SmcvData.WipDetail wipDetailInf;
                if (_wipCurStepDic.TryGetValue(key4, out wipDetailInf) == false)
                    _wipCurStepDic.Add(key4, wipDetailInf = new SmcvData.WipDetail(row.ShopID, row.ProductID, row.ProductVersion, row.ProcessID
                        , row.StepID, stepSeq2, stepDesc2, layer2));

                wipDetailInf.AddGlassQty(wipQty);
            }
        }

        private bool IsMainStep(string stepID)
        {
            if (_maindatoryStepList.Contains(stepID) == false)
                return false;

            return stepID.Contains("00");
        }

        private void SetStepInfo()
        {
            _mainStepInProcessingDic = new Dictionary<string, SmcvData.StepInfo>();

            var modelContext = this._result.GetCtx<ModelDataContext>();

            var allSteps = modelContext.StdStep.Where(x => this.SelectedShopID == Consts.ALL ? true : x.SHOP_ID == this.SelectedShopID)
                .Where(x => x.STEP_SEQ > 0)
                .Where(x => string.IsNullOrEmpty(x.STEP_DESC) == false)
                .OrderBy(x => x.STEP_SEQ);

            //var lastMainStep = allSteps.Where(x => x.STEP_ID.Contains("0-00")).OrderBy(x => x.STEP_SEQ).Last();

            Dictionary<string, int> mainStepDescDic = new Dictionary<string, int>();
            foreach (var step in allSteps)
            {
                // Main 이면
                if (this.IsMainStep(step.STEP_ID))
                {
                    int stepDescCnt = 0;
                    string stepDesc = step.STEP_DESC;
                    if (mainStepDescDic.TryGetValue(step.STEP_DESC, out stepDescCnt))
                    {
                        stepDescCnt++;
                        mainStepDescDic[step.STEP_DESC] = stepDescCnt;

                        stepDesc = stepDesc + " " + stepDescCnt;
                    }
                    else
                    {
                        mainStepDescDic.Add(step.STEP_DESC, 1);
                    }

                    SmcvData.StepInfo mainStepInfo = new SmcvData.StepInfo(step.SHOP_ID,
                        step.STEP_ID, stepDesc, step.STEP_TYPE, (int)step.STEP_SEQ, step.LAYER_ID);

                    if (_mainStepInProcessingDic.ContainsKey(step.STEP_ID) == false)
                        _mainStepInProcessingDic.Add(step.STEP_ID, mainStepInfo);
                }
            }
        }

        private string GetMainStepDesc(string stepID)
        {
            SmcvData.StepInfo stepInfo;
            if (_mainStepInProcessingDic.TryGetValue(stepID, out stepInfo) == false)
                return string.Empty;

            return stepInfo.StepDesc;
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            InitQuery();

            Query();

            _selectedProductList = null;

            this.Cursor = Cursors.Default;
        }

        private void InitQuery()
        {
            this.IsSeletectedAllProductWhenQuery = this.IsSeletectedAllProduct;
            this.SeletedProductListWhenQuery = this.SelectedProductList;
            this.IsSelectedInputWipWhenQuery = this.IsSelectedInputWip;
            this.IsTimeConditionHourWhenQuery = this.IsTimeConditionHour;

            this.WipQtyColumnName = this.IsSelectedInputWipWhenQuery ? SmcvData.Schema.WIP_QTY
                : SmcvData.Schema.WIP_BOH;

            _fixGridData = false;

            _dtWipGrid = null;
            this.wipGridView.Columns.Clear();
        }

        private void Query()
        {
            ProcessData();

            DataTable dtChart = GetChartSchema();

            dtChart = FillChartData(dtChart);

            DrawChart(dtChart);
        }

        private void ProcessData()
        {
            _moveGridDDic = new DoubleDictionary<string, string, SmcvData.MoveInfo>();

            SetStepInfo();

            SetSimMoveInfo();

            SetActMoveInfo();

            SetWip();
        }

        private void DrawChart(DataTable dt)
        {
            if (chartControl1.Diagram != null && ((XYDiagram)chartControl1.Diagram).SecondaryAxesY != null)
                ((XYDiagram)chartControl1.Diagram).SecondaryAxesY.Clear();

            chartControl1.Series.Clear();
            
            if (dt == null)
                return;

            for (int i = 0; i < 4; i++)
            {
                if (this.IsSelectedInputWip && i == 1)
                    continue;

                string seriesName = i == 0 ? this.WipQtyColumnName
                    : i == 1 ? SmcvData.Schema.WIP_EOH
                    : i == 2 ? SmcvData.Schema.SIM_MOVE_QTY
                    : SmcvData.Schema.ACT_MOVE_QTY;

                ViewType viewType = i == 0 || i == 1 ? ViewType.Bar : ViewType.Line;

                Series series = new Series(seriesName, viewType);
                this.chartControl1.Series.Add(series);
                series.ArgumentScaleType = DevExpress.XtraCharts.ScaleType.Auto;
                series.ArgumentDataMember = SmcvData.Schema.STEP_DESC;
                series.ValueScaleType = DevExpress.XtraCharts.ScaleType.Numerical;
                series.ValueDataMembers.AddRange(new string[] { seriesName });
                //series.CrosshairLabelPattern = "{S}({A}) : {V:##0.0}%";
                if (viewType == ViewType.Line)
                {
                    //(series.View as LineSeriesView).MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
                    //(series.View as LineSeriesView).LineMarkerOptions.Size = 1;
                }

                if (i == 0 || i == 1) // Wip
                {
                    if (((XYDiagram)chartControl1.Diagram).SecondaryAxesY == null ||
                        ((XYDiagram)chartControl1.Diagram).SecondaryAxesY.Count == 0)
                    {
                        SecondaryAxisY wipAxisY = new SecondaryAxisY("Wip");
                        ((XYDiagram)chartControl1.Diagram).SecondaryAxesY.Add(wipAxisY);
                    }

                    (series.View as BarSeriesView).BarWidth = this.IsSelectedInputWip ? 0.3 : 0.5;

                    ((BarSeriesView)series.View).AxisY = ((XYDiagram)chartControl1.Diagram).SecondaryAxesY[0];
                }

                DataView view = new DataView(dt, "", null, DataViewRowState.CurrentRows);
                series.DataSource = view;

                Color color = i == 0 ? Color.Gray : i == 1 ? Color.LightGray : i == 2 ? Color.Blue : Color.Red;
                series.View.Color = color;
            }
        }

        private DataTable FillChartData(DataTable dtChart)
        {
            var modelContext = this._result.GetCtx<ModelDataContext>();

            var mainSteps = modelContext.StdStep.Where(x => this.SelectedShopID == Consts.ALL ? true : x.SHOP_ID == this.SelectedShopID
                & x.STEP_SEQ > 0 & this.IsMainStep(x.STEP_ID)).OrderBy(x => x.STEP_SEQ);

            Dictionary<string, int> stepDescDic = new Dictionary<string, int>();

            foreach (var mainStep in mainSteps)
            {
                int simMoveQty = 0;
                _simMoveChartDic.TryGetValue(mainStep.STEP_ID, out simMoveQty);

                int actMoveQty = 0;
                _actMoveChartDic.TryGetValue(mainStep.STEP_ID, out actMoveQty);

                int wipQty = 0;
                _wipQtyChartDic.TryGetValue(mainStep.STEP_ID, out wipQty);

                int wipEndTm = 0;
                _wipQtyChartDic2.TryGetValue(mainStep.STEP_ID, out wipEndTm);

                int stepDescCnt = 0;
                string stepDesc = mainStep.STEP_DESC;
                if (stepDescDic.TryGetValue(mainStep.STEP_DESC, out stepDescCnt))
                {
                    stepDescCnt++;
                    stepDescDic[mainStep.STEP_DESC] = stepDescCnt;

                    stepDesc = stepDesc + " " + stepDescCnt;
                }
                else
                {
                    stepDescDic.Add(mainStep.STEP_DESC, 1);
                }

                DataRow dRow = dtChart.NewRow();

                dRow[SmcvData.Schema.STEP_DESC] = stepDesc;
                dRow[SmcvData.Schema.SIM_MOVE_QTY] = simMoveQty;
                dRow[SmcvData.Schema.ACT_MOVE_QTY] = actMoveQty;
                dRow[this.WipQtyColumnName] = wipQty;

                if (dtChart.Columns.Contains(SmcvData.Schema.WIP_EOH))
                    dRow[SmcvData.Schema.WIP_EOH] = wipEndTm;

                dtChart.Rows.Add(dRow);
            }

            return dtChart;
        }

        private void SetActMoveInfo()
        {
            _actMoveChartDic = new Dictionary<string, int>();

            var modelContext = this._result.GetCtx<ModelDataContext>();

            //var stepMoveAct = modelContext.StepMoveAct.Where(x => this.SelectedShopID == Consts.ALL ? true : x.SHOP_ID == this.SelectedShopID
            //    & this.SelectedProductList.Contains(x.PRODUCT_ID));

            //foreach (var row in stepMoveAct)
            //{
            //    if ((this.FromTime <= row.TARGET_DATE
            //        && row.TARGET_DATE < this.ToTime) == false)
            //        continue;

            //    if (_actMoveChartDic.ContainsKey(row.STEP_ID) == false)
            //        _actMoveChartDic.Add(row.STEP_ID, 0);

            //    _actMoveChartDic[row.STEP_ID] += (int)row.OUT_QTY;

            //     Product Grid 데이터
            //    SmcvData.StepInfo stepInfo;
            //    if (_mainStepInProcessingDic.TryGetValue(row.STEP_ID, out stepInfo))
            //    {
            //        Dictionary<string, SmcvData.MoveInfo> dic;
            //        if (_moveGridDDic.TryGetValue(stepInfo.StepDesc, out dic) == false)
            //            _moveGridDDic.Add(stepInfo.StepDesc, dic = new Dictionary<string, SmcvData.MoveInfo>());

            //        DateTime targetDate = DateTime.MinValue;
            //        if (this.IsTimeConditionHourWhenQuery)
            //            targetDate = ShopCalendar.StartTimeOfDay(row.TARGET_DATE);
            //        else
            //            targetDate = ShopCalendar.ShiftStartTimeOfDayT(row.TARGET_DATE);

            //        string key = CommonHelper.CreateKey(row.SHOP_ID, targetDate.ToString(), row.PRODUCT_ID);
            //        SmcvData.MoveInfo infoByProd;
            //        if (dic.TryGetValue(key, out infoByProd) == false)
            //            dic.Add(key, infoByProd = new SmcvData.MoveInfo(row.SHOP_ID, targetDate, row.PRODUCT_ID, Consts.NULL_ID, stepInfo));

            //        infoByProd.AddActMoveQty((int)row.OUT_QTY);
            //    }
            //}
        }

        private void SetSimMoveInfo()
        {
            _simMoveChartDic = new Dictionary<string, int>();

            string filter = Globals.CreateFilter(string.Empty, SmcvData.Schema.SHOP_ID, "=", this.SelectedShopID);
            filter = Globals.CreateFilter(filter, SmcvData.Schema.TARGET_DATE, ">=", this.FromTime.ToString(), "AND");
            filter = Globals.CreateFilter(filter, SmcvData.Schema.TARGET_DATE, "<", this.ToTime.ToString(), "AND");
            if (IsSeletectedAllProduct == false)
                filter = Globals.CreateFilter(filter, SmcvData.Schema.PRODUCT_ID, "=", this.SelectedProductList, "OR", "AND", true);

            DataTable dtStepMove = _result.LoadOutput(SimResultData.OutputName.StepMove, filter);
            
            foreach (DataRow row in dtStepMove.Rows)
            {
                SimResultData.StepMoveInfo info = new SimResultData.StepMoveInfo(row);

                //if (this.SelectedShopID != Consts.ALL && info.ShopID != this.SelectedShopID)
                //    continue;
                //if (this.SelectedProductList.Contains(info.ProductID) == false)
                //    continue;
                //if ((this.FromTime <= info.TargetDate && info.TargetDate < this.ToTime) == false)
                //    continue;

                if (_simMoveChartDic.ContainsKey(info.StepID) == false)
                    _simMoveChartDic.Add(info.StepID, 0);

                _simMoveChartDic[info.StepID] += (int)info.OutQty;

                // Product Grid 데이터
                SmcvData.StepInfo stepInfo;
                if (_mainStepInProcessingDic.TryGetValue(info.StepID, out stepInfo))
                {
                    Dictionary<string, SmcvData.MoveInfo> dic;
                    if (_moveGridDDic.TryGetValue(stepInfo.StepDesc, out dic) == false)
                        _moveGridDDic.Add(stepInfo.StepDesc, dic = new Dictionary<string, SmcvData.MoveInfo>());

                    DateTime targetDate = DateTime.MinValue;
                    if (this.IsTimeConditionHourWhenQuery)
                        targetDate = ShopCalendar.StartTimeOfDay(info.TargetDate);
                    else
                        targetDate = ShopCalendar.ShiftStartTimeOfDayT(info.TargetDate);

                    string key = CommonHelper.CreateKey(info.ShopID, targetDate.ToString(), info.ProductID);
                    SmcvData.MoveInfo infoByProd;
                    if (dic.TryGetValue(key, out infoByProd) == false)
                        dic.Add(key, infoByProd = new SmcvData.MoveInfo(info.ShopID, targetDate, info.ProductID, info.ProductVersion, stepInfo));

                    infoByProd.AddSimMoveQty((int)info.OutQty);
                }
            }
        }

        private void SetWip()
        {
            _wipByProdGridDic = new Dictionary<string, SmcvData.Wip>();
            _wipByProdGridDic2 = new Dictionary<string, SmcvData.Wip>();

            if (this.SelectedShopID == Consts.ALL)
            {
                foreach (string key in _wipQtyAllDDic.Keys)
                {
                    if (this.IsSelectedInputWip)
                    {
                        if (this.shopIdComboBoxEdit.Properties.Items.Contains(key) == false)
                            continue;

                        Dictionary<string, SmcvData.Wip> wipQtyDic = _wipQtyAllDDic[key];

                        foreach (KeyValuePair<string, SmcvData.Wip> pair in wipQtyDic)
                            _wipByProdGridDic.Add(pair.Key, pair.Value);
                    }
                    else
                    {
                        string sFromTime = this.FromTime != ShopCalendar.ShiftStartTimeOfDayT(this.FromTime) ?
                            ShopCalendar.ShiftStartTimeOfDayT(this.FromTime).AddHours((double)ShopCalendar.ShiftHours).DbToString()
                            : this.FromTime.DbToString();

                        string sToTime = this.ToTime != ShopCalendar.ShiftStartTimeOfDayT(this.ToTime) ?
                            ShopCalendar.ShiftStartTimeOfDayT(this.ToTime).AddHours((double)ShopCalendar.ShiftHours).DbToString()
                            : this.ToTime.DbToString();

                        if (key.StartsWith(sFromTime) == false || key.StartsWith(sToTime) == false)
                            continue;

                        Dictionary<string, SmcvData.Wip> wipQtyDic = _wipQtyAllDDic[key];

                        foreach (KeyValuePair<string, SmcvData.Wip> pair in wipQtyDic)
                        {
                            if (key.StartsWith(sFromTime) && _wipByProdGridDic.ContainsKey(pair.Key) == false)
                                _wipByProdGridDic.Add(pair.Key, pair.Value);
                            else if (key.StartsWith(sToTime) && _wipByProdGridDic2.ContainsKey(pair.Key) == false)
                                _wipByProdGridDic2.Add(pair.Key, pair.Value);
                        }                            
                    }

                }
            }
            else
            {
                string key = string.Empty;
                if (this.IsSelectedInputWip)
                    key = this.SelectedShopID;
                else
                    key = this.FromTime != ShopCalendar.ShiftStartTimeOfDayT(this.FromTime) ?
                        ShopCalendar.ShiftStartTimeOfDayT(this.FromTime).AddHours((double)ShopCalendar.ShiftHours).DbToString() + this.SelectedShopID
                        : this.FromTime.DbToString() + this.SelectedShopID;

                Dictionary<string, SmcvData.Wip> wipDic;
                _wipQtyAllDDic.TryGetValue(key, out wipDic);

                if (wipDic != null)
                    _wipByProdGridDic = wipDic;

                if (this.IsSelectedInputWip == false)
                {
                    key = this.ToTime != ShopCalendar.ShiftStartTimeOfDayT(this.ToTime) ?
                        ShopCalendar.ShiftStartTimeOfDayT(this.ToTime).AddHours((double)ShopCalendar.ShiftHours).DbToString() + this.SelectedShopID
                        : this.ToTime.DbToString() + this.SelectedShopID;

                    Dictionary<string, SmcvData.Wip> wipDic2;
                    _wipQtyAllDDic.TryGetValue(key, out wipDic2);

                    if (wipDic2 != null)
                        _wipByProdGridDic2 = wipDic2;
                }
            }

            foreach (SmcvData.Wip wipInfo in _wipByProdGridDic.Values)
            {
                string stepDesc = GetMainStepDesc(wipInfo.StepID);
                if (string.IsNullOrEmpty(stepDesc))
                    continue;

                wipInfo.SetStepDesc(stepDesc);
            }

            foreach (SmcvData.Wip wipInfo in _wipByProdGridDic2.Values)
            {
                string stepDesc = GetMainStepDesc(wipInfo.StepID);
                if (string.IsNullOrEmpty(stepDesc))
                    continue;

                wipInfo.SetStepDesc(stepDesc);
            }

            _wipQtyChartDic = new Dictionary<string, int>();
            _wipQtyChartDic2 = new Dictionary<string, int>();

            var wips = _wipByProdGridDic.Where(x => this.SelectedProductList.Contains(x.Value.ProductID))
                .Select(x => x.Value)
                .GroupBy(x => x.StepID)
                //.Select(x => new { STEP_ID = x.Key, WIP_QTY = x.Sum(y => y.GlassQty)})
                .ToDictionary(k => k.Key, v => v.Sum(g => g.GlassQty));

            foreach (var wip in wips)
            {
                if (_wipQtyChartDic.ContainsKey(wip.Key) == false)
                    _wipQtyChartDic.Add(wip.Key, 0);

                _wipQtyChartDic[wip.Key] += wip.Value;
            }

            var wips2 = _wipByProdGridDic2.Where(x => this.SelectedProductList.Contains(x.Value.ProductID))
                .Select(x => x.Value)
                .GroupBy(x => x.StepID)
                //.Select(x => new { STEP_ID = x.Key, WIP_QTY = x.Sum(y => y.GlassQty)})
                .ToDictionary(k => k.Key, v => v.Sum(g => g.GlassQty));

            foreach (var wip in wips2)
            {
                if (_wipQtyChartDic2.ContainsKey(wip.Key) == false)
                    _wipQtyChartDic2.Add(wip.Key, 0);

                _wipQtyChartDic2[wip.Key] += wip.Value;
            }
        }

        private DataTable GetChartSchema()
        {
            DataTable dtChartTable = new DataTable();

            dtChartTable.Columns.Add(SmcvData.Schema.STEP_DESC, typeof(string));
            //dtChartTable.Columns.Add(StepMoveCompareViewData.ChartSchema.PRODUCT_ID, typeof(string));
            dtChartTable.Columns.Add(SmcvData.Schema.SIM_MOVE_QTY, typeof(int));
            dtChartTable.Columns.Add(SmcvData.Schema.ACT_MOVE_QTY, typeof(int));
            dtChartTable.Columns.Add(this.WipQtyColumnName, typeof(int));

            if (this.IsSelectedInputWip == false)
                dtChartTable.Columns.Add(SmcvData.Schema.WIP_EOH, typeof(int));

            DataColumn[] dcPKeys = new DataColumn[1];

            dcPKeys[0] = dtChartTable.Columns[SmcvData.Schema.STEP_DESC];
            dtChartTable.PrimaryKey = dcPKeys;

            return dtChartTable;
        }

        private void dayRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.IsTimeConditionHour)
            {
                this.hourShiftSpinEdit.Value = 24;
                this.hourShiftRangeLabel.Text = "Hours";
            }
            else
            {
                this.hourShiftSpinEdit.Value = 4;
                this.hourShiftRangeLabel.Text = "Shifts";
            }
        }

        private DataTable GetMoveGridDataTable()
        {
            DataTable dtMoveGrid = new DataTable();

            dtMoveGrid.Columns.Add(SmcvData.Schema.SHOP_ID, typeof(string));
            dtMoveGrid.Columns.Add(SmcvData.Schema.TARGET_DATE, typeof(string));
            dtMoveGrid.Columns.Add(SmcvData.Schema.PRODUCT_ID, typeof(string));
            dtMoveGrid.Columns.Add(SmcvData.Schema.PRODUCT_VERSION, typeof(string));
            dtMoveGrid.Columns.Add(SmcvData.Schema.STEP_ID, typeof(string));
            dtMoveGrid.Columns.Add(SmcvData.Schema.STEP_DESC, typeof(string));
            dtMoveGrid.Columns.Add(SmcvData.Schema.STEP_TYPE, typeof(string));
            dtMoveGrid.Columns.Add(SmcvData.Schema.STEP_SEQ, typeof(int));
            dtMoveGrid.Columns.Add(SmcvData.Schema.LAYER, typeof(string));
            dtMoveGrid.Columns.Add(SmcvData.Schema.SIM_MOVE_QTY, typeof(int));
            dtMoveGrid.Columns.Add(SmcvData.Schema.ACT_MOVE_QTY, typeof(int));

            return dtMoveGrid;
        }

        private DataTable GetWipGridDataTable()
        {
            DataTable dtProdGrid = new DataTable();

            dtProdGrid.Columns.Add(SmcvData.Schema.SHOP_ID, typeof(string));
            dtProdGrid.Columns.Add(SmcvData.Schema.PRODUCT_ID, typeof(string));
            dtProdGrid.Columns.Add(SmcvData.Schema.PRODUCT_VERSION, typeof(string));
            dtProdGrid.Columns.Add(SmcvData.Schema.STEP_ID, typeof(string));
            dtProdGrid.Columns.Add(SmcvData.Schema.STEP_DESC, typeof(string));
            dtProdGrid.Columns.Add(this.WipQtyColumnName, typeof(int));
            if (this.IsSelectedInputWipWhenQuery == false)
                dtProdGrid.Columns.Add(SmcvData.Schema.WIP_EOH, typeof(int));

            return dtProdGrid;
        }

        private void SetGridData()
        {
            if (_moveGridDDic == null && _wipByProdGridDic == null)
                return;

            if (_argOfCursorInChart == null)
                return;

            string stepDesc = _argOfCursorInChart;

            _dtMoveGrid = _dtMoveGrid == null ? GetMoveGridDataTable() : _dtMoveGrid;
            _dtMoveGrid.Rows.Clear();

            // Prod Grid
            Dictionary<string, SmcvData.MoveInfo> dic;
            if (_moveGridDDic != null && _moveGridDDic.TryGetValue(stepDesc, out dic))
            {
                var sortedVal = dic.Values.OrderBy(x => x.TargetDate).ThenBy(x => x.ProductID);

                foreach (var info in sortedVal)
                {
                    DataRow dRow = _dtMoveGrid.NewRow();

                    string targetDate = this.IsTimeConditionHourWhenQuery ? info.TargetDate.ToString("yyyy-MM-dd")
                        : info.TargetDate.ToString("yyyy-MM-dd HH:mm:ss");

                    dRow[SmcvData.Schema.SHOP_ID] = info.ShopID;
                    dRow[SmcvData.Schema.TARGET_DATE] = targetDate;
                    dRow[SmcvData.Schema.PRODUCT_ID] = info.ProductID;
                    dRow[SmcvData.Schema.PRODUCT_VERSION] = info.ProductVersion;
                    dRow[SmcvData.Schema.STEP_ID] = info.StepID;
                    dRow[SmcvData.Schema.STEP_DESC] = info.StepDesc;
                    dRow[SmcvData.Schema.STEP_TYPE] = info.StepType;
                    dRow[SmcvData.Schema.STEP_SEQ] = info.StepSeq;
                    dRow[SmcvData.Schema.LAYER] = info.Layer;
                    dRow[SmcvData.Schema.SIM_MOVE_QTY] = info.SimMoveQty;
                    dRow[SmcvData.Schema.ACT_MOVE_QTY] = info.ActMoveQty;

                    _dtMoveGrid.Rows.Add(dRow);
                }
            }

            this.moveGridControl.DataSource = _dtMoveGrid;
            this.moveGridView.BestFitColumns();

            // WipGrid
            _dtWipGrid = _dtWipGrid == null ? GetWipGridDataTable() : _dtWipGrid;
            _dtWipGrid.Rows.Clear();
            
            if (_wipByProdGridDic != null)
            {
                var wips = _wipByProdGridDic
                    .Where(x => this.IsSeletectedAllProductWhenQuery ? true
                        : this.SeletedProductListWhenQuery.Contains(x.Value.ProductID))
                    .Where(x => x.Value.StepDesc == stepDesc).OrderBy(x => x.Value.ProductID);

                var wips2 = _wipByProdGridDic2
                    .Where(x => this.IsSeletectedAllProductWhenQuery ? true
                        : this.SeletedProductListWhenQuery.Contains(x.Value.ProductID))
                    .Where(x => x.Value.StepDesc == stepDesc).OrderBy(x => x.Value.ProductID);

                List<string> wipAddedList = new List<string>();

                DataTable wipDT = new DataTable();
                foreach (var item in wips)
                {
                    string shopID = item.Value.ShopID;
                    string prodID = item.Value.ProductID;
                    string stepID = item.Value.StepID;
                    
                    foreach (KeyValuePair<string, int> pair in item.Value.GlassQtyByVersion)
                    {
                        DataRow dRow = _dtWipGrid.NewRow();

                        dRow[SmcvData.Schema.SHOP_ID] = item.Value.ShopID;
                        dRow[SmcvData.Schema.PRODUCT_ID] = item.Value.ProductID;
                        dRow[SmcvData.Schema.STEP_ID] = item.Value.StepID;
                        dRow[SmcvData.Schema.STEP_DESC] = item.Value.StepDesc;

                        string prodVersion = pair.Key;

                        dRow[SmcvData.Schema.PRODUCT_VERSION] = prodVersion;
                        dRow[this.WipQtyColumnName] = pair.Value;
                        
                        if (this.IsSelectedInputWipWhenQuery == false)
                        {
                            string keyInfo = item.Value.ShopID + item.Value.ProductID + item.Value.StepID;
                            if (wipAddedList.Contains(keyInfo) == false)
                                wipAddedList.Add(keyInfo);

                            var wipEndTmInfo = wips2.Where(x => x.Key == keyInfo).ToList();

                            if (wipEndTmInfo != null && wipEndTmInfo.Count > 0)
                            {
                                int qty = 0;
                                if (wipEndTmInfo[0].Value.GlassQtyByVersion.TryGetValue(prodVersion, out qty))
                                    dRow[SmcvData.Schema.WIP_EOH] = qty;
                            }
                        }

                        _dtWipGrid.Rows.Add(dRow);
                    }
                }

                if (this.IsSelectedInputWipWhenQuery == false)
                {
                    var restWip2 = wips2.Where(x => wipAddedList.Contains(x.Key) == false);

                    foreach (var item in restWip2)
                    {
                        string shopID = item.Value.ShopID;
                        string prodID = item.Value.ProductID;
                        string stepID = item.Value.StepID;
                        int wipQty = item.Value.GlassQty;
                        
                        foreach (KeyValuePair<string, int> pair in item.Value.GlassQtyByVersion)
                        {
                            string prodVersion = pair.Key;

                            DataRow dRow = _dtWipGrid.NewRow();

                            dRow[SmcvData.Schema.SHOP_ID] = item.Value.ShopID;
                            dRow[SmcvData.Schema.PRODUCT_ID] = item.Value.ProductID;
                            dRow[SmcvData.Schema.PRODUCT_VERSION] = ProductVersion;
                            dRow[SmcvData.Schema.STEP_ID] = item.Value.StepID;
                            dRow[SmcvData.Schema.STEP_DESC] = item.Value.StepDesc;
                            dRow[this.WipQtyColumnName] = 0;
                            dRow[SmcvData.Schema.WIP_EOH] = pair.Value;

                            _dtWipGrid.Rows.Add(dRow);
                        }
                    }
                }
            }

            this.wipGridControl.DataSource = _dtWipGrid;
            this.wipGridView.BestFitColumns();
        }

        private void chartControl1_CustomDrawCrosshair(object sender, CustomDrawCrosshairEventArgs e)
        {
            _argOfCursorInChart = Consts.NULL_ID;
            foreach (CrosshairElement element in e.CrosshairElements)
            {
                _argOfCursorInChart = element.SeriesPoint.Argument;
                break;
            }

            if (_fixGridData)
                return;

            SetGridData();
        }

        private void chartControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                _fixGridData = false;
        }

        private void chartControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //ChartHitInfo hitInfo = this.chartControl1.CalcHitInfo(e.Location);

            _fixGridData = true;

            SetGridData();
        }

        private void btnData_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            //this.IsTimeConditionDayWhenQuery = this.IsTimeConditionDay;

            //_fixGridData = false;

            //Query();

            //_selectedProductList = null;

            StepMovePopUp stepMovePopUp = new StepMovePopUp(
                //_wipQtyAllDDic, _stepAllDic, _mainStepDic, _wipByProdDic,
                //_simMoveDic, _actMoveDic, _wipQtyDic, _moveGridDDic,
                _stepInfoDic, _wipCurStepDic, _wipMainStepDic, _maindatoryStepList,
                //this.prodIdCheckedComboBoxEdit,
                this.IsTimeConditionHour, this.IsSelectedInputWip,
                this.shopIdComboBoxEdit, this.hourShiftRangeLabel,
                //this.IsSeletectedAllProduct,
                this.fromTimeEdit, _result,
                this.hourShiftSpinEdit);

            //DataTable _dtMoveGrid;
            //DataTable _dtWipGrid;

            stepMovePopUp.Show();
            stepMovePopUp.Focus();

            this.Cursor = Cursors.Default;
        }
    }
}
