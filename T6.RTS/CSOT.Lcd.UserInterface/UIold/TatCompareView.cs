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
using Mozart.Studio.TaskModel.UserLibrary;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class TatCompareView : XtraGridControlView
    {
        private IExperimentResultItem _result;
        private DateTime _planStartTime;

        private bool _isEndLoadDocument;
        private bool _isOnQuerying;

        private List<string> _selectedProductList;

        double _defaultRunTat;
        double _defaultWaitTat;

        private Dictionary<string, List<ProcStep>> _procStepDic;
        private Dictionary<string, TcData.StepInfo> _stdStepDic;
        private Dictionary<string, TcData.ProcStepTatInfo> _tatDic;

        private List<string> _allStepRunProdInSimList;

        private Dictionary<string, TcData.SimTatInfo> _simTatInfoDic;

        private Dictionary<string, TcData.StepTatRsltInf> _stepTatRsltInf;
        private Dictionary<string, TcData.TotalTatRsltInf> _totalTatRsltInf;

        string _shopIdGridView1Clicked;
        string _prodIdGridView1Clicked;
        string _mainProcIdGridView1Clicked;

        string _shopIdGridView2Clicked;
        string _prodIdGridView2Clicked;
        string _mainProcIdGridView2Clicked;
        
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
                return this.FromTime.AddDays((int)this.daySpinEdit.Value);
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

        public bool IsAllStepRunChecked
        {
            get { return this.onlyAllStepRunCheckBox.Checked; }
        }

        public bool IsUseDefaultTatChecked
        {
            get { return this.useDefaultTatCheckBox.Checked; }
        }

        public TatCompareView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {
            InitializeBase();

            InitializeControl();

            InitializeData();

            _isEndLoadDocument = true;
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
            _isEndLoadDocument = false;

            //DateEdit Controls
            this.fromDateEdit.DateTime = ShopCalendar.SplitDate(_planStartTime);
            ComboHelper.ShiftName(this.shiftComboBoxEdit, _planStartTime);

            this.daySpinEdit.Value = _result.GetPlanPeriod();

            // ShopID ComboBox
            ComboHelper.AddDataToComboBox(this.shopIdComboBoxEdit, _result,
                SimInputData.InputName.StdStep, SimInputData.StdStepSchema.SHOP_ID, false);
            
            // ProductIDs
            SetProdIdCheckBox();
            
            // Step TAT Unit
            this.stepTatUnitComboBox.Properties.Items.Add("Sec");
            this.stepTatUnitComboBox.Properties.Items.Add("Min");
            this.stepTatUnitComboBox.Properties.Items.Add("Hour");
            this.stepTatUnitComboBox.Properties.Items.Add("Day");
            this.stepTatUnitComboBox.SelectedIndex = 1;
            
            // Total TAT Unit
            this.totalTatUnitComboBox.Properties.Items.Add("Sec");
            this.totalTatUnitComboBox.Properties.Items.Add("Min");
            this.totalTatUnitComboBox.Properties.Items.Add("Hour");
            this.totalTatUnitComboBox.Properties.Items.Add("Day");
            this.totalTatUnitComboBox.SelectedIndex = 3;
        }
        
        private void InitializeData()
        {
            var modelContext = this._result.GetCtx<ModelDataContext>();

            _stdStepDic = new Dictionary<string, TcData.StepInfo>();
            var stdStep = modelContext.StdStep.Where(x => x.STEP_SEQ > 0)
                .OrderBy(x => x.SHOP_ID);//.OrderByDescending(x => x.STEP_SEQ);

            foreach (var step in stdStep)
            {
                if (_stdStepDic.ContainsKey(step.SHOP_ID + step.STEP_ID) == false)
                    _stdStepDic.Add(step.SHOP_ID + step.STEP_ID, new TcData.StepInfo(step.SHOP_ID, step.STEP_ID,
                        step.STEP_DESC, step.STEP_TYPE, (int)step.STEP_SEQ, step.LAYER_ID));
            }

            try
            {
                _defaultRunTat = this.IsUseDefaultTatChecked ?
                    Convert.ToDouble(_result.Experiment.GetArgument("DefaultRunTAT")) : 0.0;

                _defaultWaitTat = this.IsUseDefaultTatChecked ?
                    Convert.ToDouble(_result.Experiment.GetArgument("DefaultWaitTAT")) : 0.0;
            }
            catch
            {
                _defaultWaitTat = 0;
                _defaultRunTat = 0;
            }


            var dtProduct = modelContext.Product;

            Dictionary<string, string> mainProcByProdDic = new Dictionary<string, string>();
            foreach (var prod in dtProduct)
            {
                if (mainProcByProdDic.ContainsKey(prod.SHOP_ID + prod.PRODUCT_ID) == false)
                    mainProcByProdDic.Add(prod.SHOP_ID + prod.PRODUCT_ID, prod.PROCESS_ID);
            }

            var dtTat = modelContext.Tat;//modelContext.Tat.Where(x => x.PRODUCT_TYPE == Consts.Production);

            Dictionary<string, TcData.TatInfo> tatAllDic = new Dictionary<string, TcData.TatInfo>();
            foreach (var row in dtTat)
            {
                TcData.TatInfo tatInfo = new TcData.TatInfo(row.WAIT_TAT, row.RUN_TAT);
                if (tatAllDic.ContainsKey(row.SHOP_ID + row.PRODUCT_ID + row.PROCESS_ID + row.STEP_ID) == false)
                    tatAllDic.Add(row.SHOP_ID + row.PRODUCT_ID + row.PROCESS_ID + row.STEP_ID, tatInfo);
            }

            var dtProcStepMand = modelContext.ProcStep.OrderBy(x => x.SHOP_ID + x.PROCESS_ID).OrderBy(x => x.STEP_SEQ);

            _procStepDic = new Dictionary<string, List<ProcStep>>();
            foreach (var row in dtProcStepMand)
            {
                string key = CommonHelper.CreateKey(row.SHOP_ID, row.PROCESS_ID);

                List<ProcStep> list;
                if (_procStepDic.TryGetValue(key, out list) == false)
                    _procStepDic.Add(key, list = new List<ProcStep>());

                list.Add(row);
            }

            List<string> mainProcInEqpPlanList = new List<string>();
            DataTable dtEqpPlan = _result.LoadOutput(SimResultData.OutputName.EqpPlan);
            foreach (DataRow drow in dtEqpPlan.Rows)
            {
                SimResultData.EqpPlan eqpPlan = new SimResultData.EqpPlan(drow);

                string mainProcID = string.Empty;
                mainProcByProdDic.TryGetValue(eqpPlan.ShopID + eqpPlan.ProductID, out mainProcID);
                if (string.IsNullOrEmpty(mainProcID))
                    continue;

                string item = CommonHelper.CreateKey(eqpPlan.ShopID, eqpPlan.ProductID, mainProcID);
                if (mainProcInEqpPlanList.Contains(item) == false)
                    mainProcInEqpPlanList.Add(item);
            }

            // 기준정보 Tat 집계
            _tatDic = new Dictionary<string, TcData.ProcStepTatInfo>();
            foreach (string item in mainProcInEqpPlanList)
            {
                string shopID = item.Split('@')[0];
                string prodID = item.Split('@')[1];
                string mainProcID = item.Split('@')[2];

                string key = CommonHelper.CreateKey(shopID, mainProcID);
                List<ProcStep> procStepList;
                if (_procStepDic.TryGetValue(key, out procStepList) == false)
                    continue;

                double cumTat = 0;
                List<string> keyList = new List<string>();
                foreach (ProcStep procStep in procStepList)
                {
                    double waitTat = _defaultWaitTat;
                    double runTat = _defaultWaitTat;
                    double tat = waitTat + runTat;

                    string key2 = procStep.SHOP_ID + prodID + procStep.PROCESS_ID + procStep.STEP_ID;
                    TcData.TatInfo tatInfo;
                    if (tatAllDic.TryGetValue(key2, out tatInfo))
                    {
                        waitTat = tatInfo.WaitTat;
                        runTat = tatInfo.RunTat;
                        tat = tatInfo.Tat;
                    }

                    cumTat += tat;

                    TcData.ProcStepTatInfo psTatInfo;
                    if (_tatDic.TryGetValue(key2, out psTatInfo) == false)
                    {
                        _tatDic.Add(key2, psTatInfo = new TcData.ProcStepTatInfo(procStep.SHOP_ID, prodID, procStep.PROCESS_ID,
                            procStep.STEP_ID, procStep.STEP_SEQ, waitTat, runTat, cumTat));
                    }

                    if (keyList.Contains(key2) == false)
                        keyList.Add(key2);
                }

                double totalTat = cumTat;

                foreach (string k in keyList)
                {
                    TcData.ProcStepTatInfo psTatInfo;
                    if (_tatDic.TryGetValue(k, out psTatInfo) == false)
                        continue; // 있을 수 없음

                    psTatInfo.SetTotalTat(totalTat);
                }
            }

            // Simuation Run, Wait TAT 산출 && 시뮬레이션에서 FabIn부터 FabOut까지 흐른 Product 집계
            _simTatInfoDic = new Dictionary<string, TcData.SimTatInfo>();
            _allStepRunProdInSimList = new List<string>();

            var eqpPlanDic = dtEqpPlan.AsEnumerable()
                .GroupBy(x => x.GetString(SimResultData.EqpPlanSchema.LOT_ID))
                .ToDictionary(x => x.Key);

            foreach (string lotID in eqpPlanDic.Keys)
            {
                var sortedList = eqpPlanDic[lotID].OrderBy(x => x.GetDateTime(SimResultData.EqpPlanSchema.START_TIME));
                if (sortedList.Count() >= 2)
                {
                    var firstRow = sortedList.ToList()[0];
                    foreach (var row in sortedList)
                    {
                        string rowShopID = row.GetString(SimResultData.EqpPlanSchema.SHOP_ID);
                        string rowStepID = row.GetString(SimResultData.EqpPlanSchema.STEP_ID);

                        if (_stdStepDic.ContainsKey(rowShopID + rowStepID))
                        {
                            firstRow = row;
                            break;
                        }
                    }

                    var lastRow = sortedList.ToList()[sortedList.Count() - 1];

                    string shopID = firstRow.GetString(SimResultData.EqpPlanSchema.SHOP_ID);
                    string prodID = firstRow.GetString(SimResultData.EqpPlanSchema.PRODUCT_ID);

                    string mainProcID = string.Empty;
                    mainProcByProdDic.TryGetValue(shopID + prodID, out mainProcID);
                    if (string.IsNullOrEmpty(mainProcID) == false)
                    {
                        string key = CommonHelper.CreateKey(shopID, mainProcID);
                        List<ProcStep> procStepList;
                        if (_procStepDic.TryGetValue(key, out procStepList) && procStepList.Count() >= 2)
                        {
                            string firstStepReal = procStepList[0].STEP_ID;
                            string lastStepReal = procStepList[procStepList.Count() - 1].STEP_ID;

                            string firstStepLot = firstRow.GetString(SimResultData.EqpPlanSchema.STEP_ID);
                            string lastStepLot = lastRow.GetString(SimResultData.EqpPlanSchema.STEP_ID);

                            if (firstStepReal == firstStepLot && lastStepReal == lastStepLot)
                            {
                                if (_allStepRunProdInSimList.Contains(shopID + prodID) == false)
                                    _allStepRunProdInSimList.Add(shopID + prodID);
                            }
                        }
                    }
                }

                DateTime preRowEndTime = DateTime.MinValue;
                foreach (var row in sortedList)
                {
                    string shopID = row.GetString(SimResultData.EqpPlanSchema.SHOP_ID);
                    string prodID = row.GetString(SimResultData.EqpPlanSchema.PRODUCT_ID);
                    string procID = row.GetString(SimResultData.EqpPlanSchema.PROCESS_ID);
                    string stepID = row.GetString(SimResultData.EqpPlanSchema.STEP_ID);
                    DateTime startTime = row.GetDateTime(SimResultData.EqpPlanSchema.START_TIME);
                    DateTime endTime = row.GetDateTime(SimResultData.EqpPlanSchema.END_TIME);

                    // 시뮬레이션 투입 Lot은 EqpPlan에 ARRAY/ARRAY00000 이런 식으로 정보가 한 줄 더 찍힘 -> 이에 대한 예외처리
                    if (_stdStepDic.ContainsKey(shopID + stepID) == false)
                        continue;

                    string mainChecker = CommonHelper.CreateKey(shopID, prodID, procID);
                    if (mainProcInEqpPlanList.Contains(mainChecker) == false)
                    {
                        preRowEndTime = endTime;
                        continue;
                    }

                    double runTAT = endTime > startTime ? (endTime - startTime).TotalSeconds : 0;
                    double waitTAT = preRowEndTime == DateTime.MinValue ? 0
                        : startTime > preRowEndTime ? (startTime - preRowEndTime).TotalSeconds : 0;

                    if (runTAT <= 0 && preRowEndTime == DateTime.MinValue)
                    {
                        preRowEndTime = endTime;
                        continue;
                    }
                    
                    DateTime targetShift = ShopCalendar.ShiftStartTimeOfDayT(startTime);

                    TcData.SimTatInfo simTatInfo;
                    if (_simTatInfoDic.TryGetValue(shopID + targetShift + prodID + procID + stepID, out simTatInfo) == false)
                    {
                        _simTatInfoDic.Add(shopID + targetShift + prodID + procID + stepID,
                            simTatInfo = new TcData.SimTatInfo(shopID, targetShift, prodID, procID, stepID));
                    }

                    if (runTAT > 0)
                        simTatInfo.AddRunTatToList(runTAT);

                    if (preRowEndTime != DateTime.MinValue)
                        simTatInfo.AddWaitTatToList(waitTAT);

                    preRowEndTime = endTime;
                }
            }
        }

        private void SetProdIdCheckBox()
        {
            this.prodIdCheckedComboBoxEdit.Properties.Items.Clear();

            var modelContext = this._result.GetCtx<ModelDataContext>();

            // ProductID CheckComboBox
            this.prodIdCheckedComboBoxEdit.Properties.Items.Clear();

            var prodIDs = modelContext.Product.Where(x => this.SelectedShopID == x.SHOP_ID).Select(x => x.PRODUCT_ID)
                .Distinct();
            
            string filter = Globals.CreateFilter(string.Empty, SimResultData.StepWipSchema.SHOP_ID, "=",
                this.SelectedShopID);
            filter = Globals.CreateFilter(filter, SimResultData.EqpPlanSchema.START_TIME, ">=",
                this.FromTime.ToString(), "AND");
            filter = Globals.CreateFilter(filter, SimResultData.EqpPlanSchema.START_TIME, "<",
                this.ToTime.ToString(), "AND");

            DataTable dtEqpPlan = _result.LoadOutput(SimResultData.OutputName.EqpPlan, filter);
            var prodsInEqpPlan = ComboHelper.Distinct(dtEqpPlan, SimResultData.EqpPlanSchema.PRODUCT_ID, string.Empty);

            var seleProds = prodIDs.Where(x => prodsInEqpPlan.Contains(x));

            foreach (var item in seleProds)
                this.prodIdCheckedComboBoxEdit.Properties.Items.Add(item.ToString());

            this.prodIdCheckedComboBoxEdit.CheckAll();
        }

        private void Query()
        {
            ProcessData();

            DataTable dtStepResultGrid = GetStepRsltGridSchema();

            dtStepResultGrid = FillData(dtStepResultGrid);

            DataTable dtTotalResultGrid = GetTotalRsltGridSchema();

            dtTotalResultGrid = FillData2(dtTotalResultGrid);

            FillGrid(dtStepResultGrid, dtTotalResultGrid);
        }

        private void ProcessData()
        {
            _stepTatRsltInf = new Dictionary<string, TcData.StepTatRsltInf>();
            _totalTatRsltInf = new Dictionary<string, TcData.TotalTatRsltInf>();
            
            var seleSimTatInfoDic = _simTatInfoDic.Values.Where(x => x.ShopID == this.SelectedShopID)
                .Where(x => this.IsSeletectedAllProduct ? true : this.SelectedProductList.Contains(x.ProductID))
                .Where(x => this.FromTime <= x.TargetShift & x.TargetShift < this.ToTime);

            Dictionary<string, List<string>> simStepListDic = new Dictionary<string, List<string>>();
            Dictionary<string, TcData.SimTatInfo> simTatInfoDic = new Dictionary<string, TcData.SimTatInfo>();
            foreach (var info in seleSimTatInfoDic)
            {
                string key = info.ShopID + info.ProductID + info.MainProcessID + info.StepID;

                TcData.SimTatInfo simTatInfo;
                if (simTatInfoDic.TryGetValue(key, out simTatInfo) == false)
                    simTatInfoDic.Add(key, simTatInfo = new TcData.SimTatInfo(info.ShopID, info.TargetShift,
                        info.ProductID, info.MainProcessID, info.StepID));

                simTatInfo.AddRunTatToList(info.RunTatList);
                simTatInfo.AddWaitTatToList(info.WaitTatList);

                string key2 = CommonHelper.CreateKey(info.ShopID, info.ProductID, info.MainProcessID);
                List<string> stepList;
                if (simStepListDic.TryGetValue(key2, out stepList) == false)
                    simStepListDic.Add(key2, stepList = new List<string>());

                if (stepList.Contains(info.StepID) == false)
                    stepList.Add(info.StepID);
            }

            foreach (var info in simTatInfoDic.Values)
            {
                string key = info.ShopID + info.ProductID + info.MainProcessID + info.StepID;

                TcData.StepInfo stdStepInfo;
                _stdStepDic.TryGetValue(info.ShopID + info.StepID, out stdStepInfo);

                TcData.StepTatRsltInf stepTatRsltInf;
                if (_stepTatRsltInf.TryGetValue(key, out stepTatRsltInf) == false)
                    _stepTatRsltInf.Add(key, stepTatRsltInf = new TcData.StepTatRsltInf(info.ShopID, info.ProductID,
                        info.MainProcessID, info.StepID, stdStepInfo));

                stepTatRsltInf.SetSimulationTat(info.WaitTatAvg, info.RunTatAvg);
            }

            foreach (var info in _tatDic.Values)
            {
                string key = info.ShopID + info.ProdID + info.ProcID + info.StepID;
                
                TcData.StepTatRsltInf stepTatRsltInf;
                if (_stepTatRsltInf.TryGetValue(key, out stepTatRsltInf) == false)
                    continue;

                stepTatRsltInf.SetActualTat(info.WaitTat, info.RunTat);
                
                bool isAddTotalTat = this.IsAllStepRunChecked == false ||
                    (this.IsAllStepRunChecked && _allStepRunProdInSimList.Contains(info.ShopID + info.ProdID));

                string key2 = CommonHelper.CreateKey(info.ShopID, info.ProdID, info.ProcID);
                TcData.TotalTatRsltInf totalTatRsltInf;
                if (_totalTatRsltInf.TryGetValue(key2, out totalTatRsltInf) == false && isAddTotalTat)
                {
                    _totalTatRsltInf.Add(key2, totalTatRsltInf = new TcData.TotalTatRsltInf(info.ShopID, info.ProdID, info.ProcID));

                    totalTatRsltInf.SetActTat(info.TotalTat);
                }
            }

            foreach (KeyValuePair<string, TcData.TotalTatRsltInf> pair in _totalTatRsltInf)
            {
                string shopID = pair.Key.Split('@')[0];
                string prodID = pair.Key.Split('@')[1];
                string procID = pair.Key.Split('@')[2];
                
                string key = CommonHelper.CreateKey(shopID, procID);
                List<ProcStep> procStepList;
                if (_procStepDic.TryGetValue(key, out procStepList) == false)
                    continue;

                foreach (ProcStep procStep in procStepList)
                {
                    double stepTat = _defaultWaitTat + _defaultRunTat;

                    TcData.StepTatRsltInf stepTatRsltInf;
                    if (_stepTatRsltInf.TryGetValue(shopID + prodID + procID + procStep.STEP_ID, out stepTatRsltInf))
                        stepTat = stepTatRsltInf.TatSim;

                    pair.Value.AddSimTat(stepTat);
                }
            }
        }

        private DataTable GetStepRsltGridSchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(TcData.Schema.SHOP_ID, typeof(string));
            dt.Columns.Add(TcData.Schema.PRODUCT_ID, typeof(string));
            dt.Columns.Add(TcData.Schema.MAIN_PROC_ID, typeof(string));
            dt.Columns.Add(TcData.Schema.STEP_ID, typeof(string));
            dt.Columns.Add(TcData.Schema.STEP_DESC, typeof(string));
            dt.Columns.Add(TcData.Schema.STD_STEP_SEQ, typeof(int));
            dt.Columns.Add(TcData.Schema.LAYER_ID, typeof(string));
            dt.Columns.Add(TcData.Schema.ACT_WAIT_TAT, typeof(double));
            dt.Columns.Add(TcData.Schema.ACT_RUN_TAT, typeof(double));
            dt.Columns.Add(TcData.Schema.ACT_TAT, typeof(double));
            dt.Columns.Add(TcData.Schema.SIM_WAIT_TAT, typeof(double));
            dt.Columns.Add(TcData.Schema.SIM_RUN_TAT, typeof(double));
            dt.Columns.Add(TcData.Schema.SIM_TAT, typeof(double));
            dt.Columns.Add(TcData.Schema.TAT_DIFF, typeof(double));

            //DataColumn[] dcPKeys = new DataColumn[1];

            //dcPKeys[0] = dtChartTable.Columns[StepMoveCompareViewData.Schema.STEP_DESC];
            //dtChartTable.PrimaryKey = dcPKeys;

            return dt;
        }

        private DataTable GetTotalRsltGridSchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(TcData.Schema.SHOP_ID, typeof(string));
            dt.Columns.Add(TcData.Schema.PRODUCT_ID, typeof(string));
            dt.Columns.Add(TcData.Schema.MAIN_PROC_ID, typeof(string));
            dt.Columns.Add(TcData.Schema.ACT_TAT, typeof(double));
            dt.Columns.Add(TcData.Schema.SIM_TAT, typeof(double));
            dt.Columns.Add(TcData.Schema.DIFF, typeof(double));

            //DataColumn[] dcPKeys = new DataColumn[1];

            //dcPKeys[0] = dtChartTable.Columns[StepMoveCompareViewData.Schema.STEP_DESC];
            //dtChartTable.PrimaryKey = dcPKeys;

            return dt;
        }

        private DataTable FillData(DataTable dt)
        {
            foreach (TcData.StepTatRsltInf data in _stepTatRsltInf.Values)
            {
                DataRow dRow = dt.NewRow();

                double unit = this.stepTatUnitComboBox.SelectedIndex == 1 ? 60.0
                    : this.stepTatUnitComboBox.SelectedIndex == 2 ? 60.0 * 60.0
                    : this.stepTatUnitComboBox.SelectedIndex == 3 ? 60.0 * 60.0 * 24.0
                    : 1.0;

                dRow[TcData.Schema.SHOP_ID] = data.ShopID;
                dRow[TcData.Schema.PRODUCT_ID] = data.ProdID;
                dRow[TcData.Schema.MAIN_PROC_ID] = data.MainProcID;
                dRow[TcData.Schema.STEP_ID] = data.StepID;
                dRow[TcData.Schema.STEP_DESC] = data.StepDesc;
                dRow[TcData.Schema.STD_STEP_SEQ] = data.StdStepSeq;
                dRow[TcData.Schema.LAYER_ID] = data.Layer;
                dRow[TcData.Schema.ACT_WAIT_TAT] = Math.Round(data.WaitTatAct / unit, 2);
                dRow[TcData.Schema.ACT_RUN_TAT] = Math.Round(data.RunTatAct / unit, 2);
                dRow[TcData.Schema.ACT_TAT] = Math.Round(data.TatAct / unit, 2);
                dRow[TcData.Schema.SIM_WAIT_TAT] = Math.Round(data.WaitTatSim / unit, 2);
                dRow[TcData.Schema.SIM_RUN_TAT] = Math.Round(data.RunTatSim / unit, 2);
                dRow[TcData.Schema.SIM_TAT] = Math.Round(data.TatSim / unit, 2);
                dRow[TcData.Schema.TAT_DIFF] = Math.Round(data.TatAct - data.TatSim / unit, 2);

                dt.Rows.Add(dRow);
            }

            return dt;
        }

        private DataTable FillData2(DataTable dt)
        {
            foreach (TcData.TotalTatRsltInf data in _totalTatRsltInf.Values)
            {
                DataRow dRow = dt.NewRow();

                double unit = this.totalTatUnitComboBox.SelectedIndex == 1 ? 60.0
                    : this.totalTatUnitComboBox.SelectedIndex == 2 ? 60.0 * 60.0
                    : this.totalTatUnitComboBox.SelectedIndex == 3 ? 60.0 * 60.0 * 24.0
                    : 1.0;
                
                dRow[TcData.Schema.SHOP_ID] = data.ShopID;
                dRow[TcData.Schema.PRODUCT_ID] = data.ProdID;
                dRow[TcData.Schema.MAIN_PROC_ID] = data.MainProcID;
                dRow[TcData.Schema.ACT_TAT] = Math.Round(data.TotalTatAct / unit, 2);
                dRow[TcData.Schema.SIM_TAT] = Math.Round(data.TotalTatSim / unit, 2);
                dRow[TcData.Schema.DIFF] = Math.Round((data.TotalTatAct - data.TotalTatSim) / unit, 2);

                dt.Rows.Add(dRow);
            }

            return dt;
        }

        private void FillGrid(DataTable dt1, DataTable dt2)
        {
            // GridControl1
            this.gridControl1.DataSource = dt1;

            this.gridView1.Columns[TcData.Schema.STD_STEP_SEQ].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[TcData.Schema.STD_STEP_SEQ].DisplayFormat.FormatString = "###,###";
            this.gridView1.Columns[TcData.Schema.ACT_RUN_TAT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[TcData.Schema.ACT_RUN_TAT].DisplayFormat.FormatString = "###,###.00";
            this.gridView1.Columns[TcData.Schema.ACT_WAIT_TAT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[TcData.Schema.ACT_WAIT_TAT].DisplayFormat.FormatString = "###,###.00";
            this.gridView1.Columns[TcData.Schema.ACT_TAT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[TcData.Schema.ACT_TAT].DisplayFormat.FormatString = "###,###.00";
            this.gridView1.Columns[TcData.Schema.SIM_RUN_TAT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[TcData.Schema.SIM_RUN_TAT].DisplayFormat.FormatString = "###,###.00";
            this.gridView1.Columns[TcData.Schema.SIM_WAIT_TAT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[TcData.Schema.SIM_WAIT_TAT].DisplayFormat.FormatString = "###,###.00";
            this.gridView1.Columns[TcData.Schema.SIM_TAT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[TcData.Schema.SIM_TAT].DisplayFormat.FormatString = "###,###.00";
            this.gridView1.Columns[TcData.Schema.TAT_DIFF].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[TcData.Schema.TAT_DIFF].DisplayFormat.FormatString = "###,###.00";
            //this.gridView1.Columns[TcData.Schema.START_TIME].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            //this.gridView1.Columns[TcData.Schema.START_TIME].DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            
            this.gridView1.BestFitColumns();

            // GridControl2
            this.gridControl2.DataSource = dt2;

            this.gridView2.Columns[TcData.Schema.ACT_TAT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView2.Columns[TcData.Schema.ACT_TAT].DisplayFormat.FormatString = "###,###.00";
            this.gridView2.Columns[TcData.Schema.SIM_TAT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView2.Columns[TcData.Schema.SIM_TAT].DisplayFormat.FormatString = "###,###.00";
            this.gridView2.Columns[TcData.Schema.DIFF].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView2.Columns[TcData.Schema.DIFF].DisplayFormat.FormatString = "###,###.00";
            
            this.gridView2.BestFitColumns();

            this.gridView1.Columns[TcData.Schema.PRODUCT_ID].BestFit();
            this.gridView2.Columns[TcData.Schema.PRODUCT_ID].BestFit();
        }


        # region Event
        private void queryBtn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            _isOnQuerying = true;

            Query();

            _selectedProductList = null;

            _shopIdGridView1Clicked = _prodIdGridView1Clicked = _mainProcIdGridView1Clicked = null;
            _shopIdGridView2Clicked = _prodIdGridView2Clicked = _mainProcIdGridView2Clicked = null;
            _isOnQuerying = false;

            this.Cursor = Cursors.Default;
        }
        
        private void shopIdComboBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_isEndLoadDocument == false)
                return;

            SetProdIdCheckBox();
        }

        private void fromDateEdit_DateTimeChanged(object sender, EventArgs e)
        {
            if (_isEndLoadDocument == false)
                return;

            SetProdIdCheckBox();
        }

        private void shiftComboBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_isEndLoadDocument == false)
                return;

            SetProdIdCheckBox();
        }

        private void daySpinEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_isEndLoadDocument == false)
                return;

            SetProdIdCheckBox();
        }
        
        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            _shopIdGridView2Clicked = _prodIdGridView2Clicked = _mainProcIdGridView2Clicked = null;

            DataRow clickedRow = gridView1.GetFocusedDataRow();

            if (clickedRow == null)
                return;

            _shopIdGridView1Clicked = clickedRow[TcData.Schema.SHOP_ID].ToString();
            _prodIdGridView1Clicked = clickedRow[TcData.Schema.PRODUCT_ID].ToString();
            _mainProcIdGridView1Clicked = clickedRow[TcData.Schema.MAIN_PROC_ID].ToString();

            this.gridView1.RefreshData();
            this.gridView2.RefreshData();
        }

        private void gridView2_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            _shopIdGridView1Clicked = _prodIdGridView1Clicked = _mainProcIdGridView1Clicked = null;

            DataRow clickedRow = gridView2.GetFocusedDataRow();

            if (clickedRow == null)
                return;

            _shopIdGridView2Clicked = clickedRow[TcData.Schema.SHOP_ID].ToString();
            _prodIdGridView2Clicked = clickedRow[TcData.Schema.PRODUCT_ID].ToString();
            _mainProcIdGridView2Clicked = clickedRow[TcData.Schema.MAIN_PROC_ID].ToString();

            this.gridView1.RefreshData();
            this.gridView2.RefreshData();
        }
        
        private void gridView1_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (_isOnQuerying)
                return;

            if (string.IsNullOrEmpty(_shopIdGridView2Clicked) || string.IsNullOrEmpty(_prodIdGridView2Clicked)
                || string.IsNullOrEmpty(_mainProcIdGridView2Clicked))
            {
                return;
            }

            string shopID = (string)this.gridView1.GetRowCellValue(e.RowHandle, TcData.Schema.SHOP_ID);
            string prodID = (string)this.gridView1.GetRowCellValue(e.RowHandle, TcData.Schema.PRODUCT_ID);
            string procID = (string)this.gridView1.GetRowCellValue(e.RowHandle, TcData.Schema.MAIN_PROC_ID);

            if (shopID == _shopIdGridView2Clicked && prodID == _prodIdGridView2Clicked
                && procID == _mainProcIdGridView2Clicked)
            {
                e.Appearance.BackColor = Color.PaleGreen;
                e.HighPriority = true;
            }
            else
            {
                e.Appearance.BackColor = Color.White;
                e.HighPriority = true;
            }
        }

        private void gridView2_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (_isOnQuerying)
                return;

            if (string.IsNullOrEmpty(_shopIdGridView1Clicked) || string.IsNullOrEmpty(_prodIdGridView1Clicked)
                || string.IsNullOrEmpty(_mainProcIdGridView1Clicked))
            {
                return;
            }

            string shopID = (string)this.gridView2.GetRowCellValue(e.RowHandle, TcData.Schema.SHOP_ID);
            string prodID = (string)this.gridView2.GetRowCellValue(e.RowHandle, TcData.Schema.PRODUCT_ID);
            string procID = (string)this.gridView2.GetRowCellValue(e.RowHandle, TcData.Schema.MAIN_PROC_ID);

            if (shopID == _shopIdGridView1Clicked && prodID == _prodIdGridView1Clicked
                && procID == _mainProcIdGridView1Clicked)
            {
                e.Appearance.BackColor = Color.PaleGreen;
                e.HighPriority = true;
            }
            else
            {
                e.Appearance.BackColor = Color.White;
                e.HighPriority = true;
            }
        }

        # endregion
    }
}
