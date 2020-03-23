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

using Mozart.Collections;
using DevExpress.XtraPivotGrid;
using DevExpress.Data.PivotGrid;
using Mozart.Studio.UIComponents;
using Mozart.Studio.TaskModel.Utility;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using Mozart.Text;
using DevExpress.XtraCharts;
using CSOT.UserInterface.Utils;
using Mozart.Studio.Application;
using DevExpress.XtraEditors.Controls;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class TargetPlanCompareView : XtraPivotGridControlView
    {
        public enum QtyType
        {
            TARGET,
            PLAN,
            DIFF
        }

        #region Variables

        private const string _pageID = "TargetPlanCompareView";
        private IVsApplication _application;
        private string _dirPath;
        private string _fileName;

        private string viewName = "TARGETPLANCOMPARE";
        DateTime _planStartTime;
        private IExperimentResultItem _result;
        private Dictionary<string, int> _stepIndexs;

        private Dictionary<string, string> _ownerTypeDict = new Dictionary<string,string>();
        private Dictionary<string, string> _productDict = new Dictionary<string, string>();
        private List<string> _dateList;
        bool _isFirstLoad = false;

        /* dictionary */
        private Dictionary<string, CompareItem> _resultDict;

        private string TargetAreaID { get { return this.areaComboBox.Text; } }

        private string TargetShopID { get { return this.shopIdComboBox.Text; } }

        private string TargetStdStep { get { return this.StepComboBox.Text; } }

        private string OwnerType { get { return ownerTypeCombo.Text; } }

        private List<string> Products
        {
            get
            {
                List<string> productList = new List<string>();

                foreach (CheckedListBoxItem item in this.productCombo.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                        productList.Add(item.ToString());
                }

                return productList;
            }
        }
        private bool IsOnlyMainStep
        {
            get
            {
                if (this.chkOnlyMain.Checked == false)
                    return false;

                return true;
            }
        }

        private bool ShowAccumulativeQty
        {
            get
            {
                if (this.isShowAccQty.Checked == true)
                    return true;
                return false;
            }
        }

        DateTime StartTime
        { get { return ShopCalendar.ShiftStartTimeOfDayT(startDatePicker.DateTime); } }

        DateTime EndTime
        { get { return ShopCalendar.ShiftStartTimeOfDayT(endDatePicker.DateTime); } }

        #endregion

        #region Constructor
        /* constructor */
        public TargetPlanCompareView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();

            _dirPath = string.Format("{0}\\DefaultLayOut", Application.StartupPath);
            _fileName = string.Format("{0}.xml", _pageID);
        }
        #endregion

        #region Load Document

        protected override void LoadDocument()
        {
            var item = (IMenuDocItem)this.Document.ProjectItem;
            _result = (IExperimentResultItem)item.Arguments[0];
            _application = (IVsApplication)GetService(typeof(IVsApplication));
            
            Globals.InitFactoryTime(_result);

            _planStartTime = _result.StartTime;                        
            _resultDict = new Dictionary<string, CompareItem>();

            SetControls();
       }

        private void SetDateRange()
        {
            _dateList = new List<string>();
            for (DateTime dt = ShopCalendar.ShiftStartTimeOfDayT(_planStartTime); dt <= EndTime; dt = dt.AddDays(1))
            {
                _dateList.Add(dt.ToString("yyyyMMdd"));
            }
        }

        #endregion

        #region Set Controls
        private void SetControls()
        {

            this.startDatePicker.DateTime = _planStartTime;

            this.startDatePicker.Properties.EditMask = "yyyy-MM-dd";
            this.startDatePicker.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.startDatePicker.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            DateTime defaultEndTime = _result.StartTime.AddDays(Globals.GetResultPlanPeriod(_result));
            this.endDatePicker.DateTime = ShopCalendar.ShiftStartTimeOfDayT(defaultEndTime);
            
            this.endDatePicker.Properties.EditMask = "yyyy-MM-dd";
            this.endDatePicker.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.endDatePicker.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            SetAreaIDComboBox();

            SetShopCombo();

            SetStepIDCombo(TargetShopID);

            SetOwnerTypeCombo();

            SetProductCombo();

            _isFirstLoad = true;
        }

        private void SetAreaIDComboBox()
        {
            this.shopIdComboBox.Items.Clear();

            SortedSet<string> list = new SortedSet<string>();

            var stepMove =  _result.GetCtx<ResultDataContext>().StepMove;

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

        private void SetShopCombo()
        {
            this.shopIdComboBox.Items.Clear();

            List<string> list = new List<string>{"ALL", "ARRAY", "CF", "CELL"};

            foreach (string shopID in list)
            {
                if (this.shopIdComboBox.Items.Contains(shopID))
                    continue;

                this.shopIdComboBox.Items.Add(shopID);
            }

            if (this.shopIdComboBox.Items.Count > 0)
                this.shopIdComboBox.SelectedIndex = 0;

        }

        private void SetStepIDCombo(string shopID)
        {
            this.StepComboBox.Items.Clear();

            this.StepComboBox.Items.Add(Consts.ALL);

            SortedSet<string> stepSet = new SortedSet<string>();

            var stepMove = _result.GetCtx<ResultDataContext>().StepMove;

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

        private void SetProductCombo()
        {
            this.productCombo.Properties.Items.Clear();

            DataTable productTable = _result.LoadInput(TargetPlanCompareData.DATA_TABLE_0);

            List<string> products = new List<string>();

            foreach (DataRow prow in productTable.Rows)
            {
                TargetPlanCompareData.Product prod = new TargetPlanCompareData.Product(prow);

                if (this.TargetShopID != "ALL")
                {
                    if (StringHelper.Equals(this.TargetShopID, prod.ShopID) == false)
                        continue;
                }
                
                if (!products.Contains(prod.ProductID))
                    products.Add(prod.ProductID);

                string key = prod.ShopID + prod.ProductID;
                if (!_productDict.Keys.Contains(key))
                    _productDict.Add(key, prod.ProductID);
            }

            products.Sort();

            foreach (string product in products)
            {
                this.productCombo.Properties.Items.Add(product);
            }

            if (this.productCombo.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in productCombo.Properties.Items)
                    item.CheckState = CheckState.Checked;
            }
        }

        private void SetOwnerTypeCombo()
        {
            #region OwnerTypes Combo

            DataTable dt = _result.LoadOutput(TargetPlanCompareData.DATA_TABLE_3);

            List<string> ownerTypes = new List<string>();

            foreach (DataRow row in dt.Rows)
            {
                TargetPlanCompareData.EqpPlan ep = new TargetPlanCompareData.EqpPlan(row);

                if (string.IsNullOrWhiteSpace(ep.OwnerType))
                    continue;

                if (!ownerTypes.Contains(ep.OwnerType))
                    ownerTypes.Add(ep.OwnerType);


                string key = ep.ShopID + ep.ProductID + ep.StepID;

                if (!this._ownerTypeDict.Keys.Contains(key))
                    _ownerTypeDict.Add(key, ep.OwnerType);
            }


            ownerTypes.Sort();


            foreach (string ownerType in ownerTypes)
            {
                this.ownerTypeCombo.Items.Add(ownerType);
            }

            this.ownerTypeCombo.SelectedIndex = 1;
            #endregion
        }

        private List<string> GetMainStdStepList(string areaID)
        {
            var stdStep = _result.GetCtx<ModelDataContext>().StdStep;
            var mainStdStep = stdStep.Where(t => t.AREA_ID == areaID && t.STEP_TYPE == "MAIN").OrderBy(t => t.STEP_SEQ);

            List<string> mainStdSteps = new List<string>();
            foreach (var item in mainStdStep)
            {
                mainStdSteps.Add(item.STEP_ID);
            }

            return mainStdSteps;
        }

            #endregion



            #region Load Data

            private void LoadData()
        {
            _resultDict.Clear();

            LoadData_Target(_resultDict);
            LoadData_Plan(_resultDict);
            LoadData_Diff(_resultDict);
        }

        private void LoadData_Target(Dictionary<string, CompareItem> results)
        {            
            string filter = string.Empty;

            if (this.shopIdComboBox.Text != Consts.ALL)
                filter = string.Format("SHOP_ID = '{0}'", this.shopIdComboBox.Text);

            DataTable stepTargets = _result.LoadOutput(TargetPlanCompareData.DATA_TABLE_2, filter);

            string shopID = this.shopIdComboBox.Text == ComboHelper.ALL ? "" : this.shopIdComboBox.Text.ToUpper();

            var mainStdStepList = GetMainStdStepList(this.TargetAreaID);

            foreach (DataRow row in stepTargets.Rows)
            {
                TargetPlanCompareData.StepTarget st = new TargetPlanCompareData.StepTarget(row);

                if (st.ProductID == "TH645A1AB100" && st.StepID == "9900")
                    Console.WriteLine();

                if (st.StepType != "MAIN")
                    continue;

                if (this.TargetAreaID != st.AreaID)
                    continue;


                if (st.CampareTargetDate < StartTime)
                    continue;

                if (st.CampareTargetDate > EndTime)
                    continue;

                if (this.TargetShopID != "ALL")
                {
                    if (!st.ShopID.ToUpper().Contains(this.TargetShopID))
                        continue;
                }

                if (OwnerType != "OwnerP")
                    continue;

                if (Products.Contains(st.ProductID) == false)
                    continue;

                if (IsOnlyMainStep)
                {
                    if (mainStdStepList.Contains(st.StepID) == false)
                        continue;
                }

                if (this.TargetStdStep != Consts.ALL && st.StepID != this.TargetStdStep)
                    continue;

                string key = st.ShopID + st.ProductID + st.StepID;

                CompareItem compItem;
                if (_resultDict.TryGetValue(key, out compItem) == false)
                {

                    compItem = new CompareItem(st.ShopID, st.ProductID, st.StepID);
                    results.Add(key, compItem);

                    compItem.SHOP_ID = st.ShopID;
                    compItem.PRODUCT_ID = st.ProductID;
                    compItem.STEP_ID = st.StepID;
                    compItem.OWNER_TYPE = "OwnerP";
                }

                compItem.AddQty(st.TargetDate, st.OutTargetQty, QtyType.TARGET);
            }

        }

        private void LoadData_Plan(Dictionary<string, CompareItem> results)
        {
            string filter2 = string.Empty;

            if (this.shopIdComboBox.Text != Consts.ALL)
                filter2 = string.Format("SHOP_ID = '{0}'", this.shopIdComboBox.Text);

            DataTable dt = _result.LoadOutput(TargetPlanCompareData.DATA_TABLE_4, filter2);

            var mainStdStepList = GetMainStdStepList(this.TargetAreaID);

            _stepIndexs = new Dictionary<string, int>();

            foreach (DataRow row in dt.Rows)
            {
                TargetPlanCompareData.StepMove sm = new TargetPlanCompareData.StepMove(row);

                if (this.TargetAreaID != sm.AreaID)
                    continue;

                if (sm.CompareTargetDate < StartTime)
                    continue;

                if (sm.CompareTargetDate > EndTime)
                    continue;

                if (this.TargetShopID != "ALL")
                {
                    if (!sm.ShopID.ToUpper().Contains(this.TargetShopID))
                        continue;
                }

                if (sm.OwnerType != OwnerType)
                    continue;

                if (Products.Contains(sm.ProductID) == false)
                    continue;

                if (IsOnlyMainStep)
                {
                    if (mainStdStepList.Contains(sm.StepID) == false)
                        continue;
                }

                if (this.TargetStdStep != Consts.ALL && sm.StepID != this.TargetStdStep)
                    continue;

                string key = sm.ShopID + sm.ProductID + sm.StepID;

                string stepKey = sm.StepID;
                _stepIndexs[stepKey] = sm.StdStepSeq;

                CompareItem compItem;
                if (!results.TryGetValue(key, out compItem))
                {
                    compItem = new CompareItem(sm.ShopID, sm.ProductID, sm.StepID);
                    _resultDict.Add(key, compItem);

                    compItem.PRODUCT_ID = sm.ProductID;
                    compItem.STEP_ID = sm.StepID;
                    compItem.SHOP_ID = sm.ShopID;
                    compItem.OWNER_TYPE = sm.OwnerType;
                }

                if (sm.StepID == "9900")
                    Console.WriteLine();

                compItem.AddQty(sm.TargetDate, sm.OutQty, QtyType.PLAN);
            }
        }

        private void LoadData_Diff(Dictionary<string, CompareItem> results)
        {
            if (results == null || results.Count == 0)
                return;

            foreach (var item in results.Values)
            {
                item.SetDiff();
            }
        }

        #endregion

        #region Bind Data
        private XtraPivotGridHelper.DataViewTable BindData()
        {
            XtraPivotGridHelper.DataViewTable dataViewTable = new XtraPivotGridHelper.DataViewTable(viewName);
            dataViewTable.AddColumn(TargetPlanCompareData.SHOP_ID, TargetPlanCompareData.SHOP_ID, typeof(string), PivotArea.RowArea, null, null);

            dataViewTable.AddColumn(TargetPlanCompareData.STEP_ID, TargetPlanCompareData.STEP_ID, typeof(string), PivotArea.RowArea, null, null);
            dataViewTable.AddColumn(TargetPlanCompareData.PRODUCT_ID, TargetPlanCompareData.PRODUCT_ID, typeof(string), PivotArea.RowArea, null, null);
            dataViewTable.AddColumn(TargetPlanCompareData.OWNER_TYPE, TargetPlanCompareData.OWNER_TYPE, typeof(string), PivotArea.RowArea, null, null);
            dataViewTable.AddColumn(TargetPlanCompareData.CATEGORY, TargetPlanCompareData.CATEGORY, typeof(string), PivotArea.RowArea, null, null);
            dataViewTable.AddColumn(TargetPlanCompareData.TOTAL, TargetPlanCompareData.TOTAL, typeof(string), PivotArea.RowArea, null, null);
            dataViewTable.AddColumn(TargetPlanCompareData.TARGET_DATE, TargetPlanCompareData.TARGET_DATE, typeof(string), PivotArea.ColumnArea, null, null);

            dataViewTable.AddColumn(ColName.QTY, ColName.QTY, typeof(float), PivotArea.DataArea, null, null);

            dataViewTable.AddDataTablePrimaryKey(
                new DataColumn[]
                    {
                        dataViewTable.Columns[TargetPlanCompareData.SHOP_ID],
                        dataViewTable.Columns[TargetPlanCompareData.STEP_ID],
                        dataViewTable.Columns[TargetPlanCompareData.PRODUCT_ID],
                        dataViewTable.Columns[TargetPlanCompareData.CATEGORY],
                        dataViewTable.Columns[TargetPlanCompareData.TARGET_DATE]
                    });

            return dataViewTable;
        }
        #endregion

        #region Fill Data
        private void FillData(XtraPivotGridHelper.DataViewTable dataViewTable)
        {
            CompareItem total = new CompareItem("TOTAL", "", "");

            foreach (CompareItem item in _resultDict.Values)
            {
                item.SetAccQty();

                foreach (var targetDate in item.Dates)
                {
                    FillData(dataViewTable, item, targetDate, total, QtyType.TARGET);

                    FillData(dataViewTable, item, targetDate, total, QtyType.PLAN);

                    FillData(dataViewTable, item, targetDate, total, QtyType.DIFF);
                }               
            }

            //Total
            _resultDict.Add("TOTAL", total);

            List<float> totalTargetList = new List<float>();
            List<float> totalPlanList = new List<float>();
            foreach (var item in _resultDict.Values)
            {
                totalTargetList.Add(item.TARGET_TOTAL);
                totalPlanList.Add(item.PLAN_TOTAL);
            }

            float totalTarge = totalTargetList.Sum();
            float totalPlan = totalPlanList.Sum();

            foreach (KeyValuePair<string, float> pair in total.Targets)
            {
                dataViewTable.DataTable.Rows.Add(
                            total.SHOP_ID,
                            total.STEP_ID,
                            total.PRODUCT_ID,
                            total.OWNER_TYPE,
                            QtyType.TARGET.ToString(),
                            string.Format("{0:#,##0}", totalTarge),
                            pair.Key,
                            pair.Value
                            );
            }

            foreach (KeyValuePair<string, float> pair in total.Plans)
            {
                dataViewTable.DataTable.Rows.Add(
                            total.SHOP_ID,
                            total.STEP_ID,
                            total.PRODUCT_ID,
                            total.OWNER_TYPE,
                            QtyType.PLAN.ToString(),
                            string.Format("{0:#,##0}", totalPlan),
                            pair.Key,
                            pair.Value
                            );
            }
        }

        private void FillData(XtraPivotGridHelper.DataViewTable dataViewTable, CompareItem item, string targetDate, CompareItem total, QtyType qtyType)
        {
            string category = qtyType.ToString();
            float qty = 0.0f;
            if (ShowAccumulativeQty)
                qty = item.GetAccQty(targetDate, qtyType);
            else
                qty = item.GetQty(targetDate, qtyType);

            dataViewTable.DataTable.Rows.Add(
            item.SHOP_ID,
            item.STEP_ID,
            item.PRODUCT_ID,
            item.OWNER_TYPE,
            category,
            string.Format("{0:#,##0}", item.GetTotalQty(qtyType)),
            targetDate,
            qty);

            //Total
            var infos = qtyType == QtyType.TARGET ? total.Targets : total.Plans;
            if(qtyType == QtyType.DIFF)
                infos = total.Diffs;

            float prevQty = 0;
            if (infos.TryGetValue(targetDate, out prevQty))
                infos[targetDate] += qty;
            else
                infos.Add(targetDate, 0);
        }

        #endregion

        private void pivotGridControl_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        {
            if (e.Field.FieldName != TargetPlanCompareData.STEP_ID)
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

        #region Draw Grid
        private void DrawGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            pivotGridControl1.BeginUpdate();

            XtraPivotGridHelper.ClearPivotGridFields(pivotGridControl1);
            XtraPivotGridHelper.CreatePivotGridFields(pivotGridControl1, dt);
            pivotGridControl1.DataSource = dt.DataTable;

            formatCells(pivotGridControl1);

            pivotGridControl1.EndUpdate();

            pivotGridControl1.OptionsView.HideAllTotals();

            foreach (PivotGridField field in pivotGridControl1.Fields)
            {
                if (field.FieldName == ColName.TOTAL)
                {
                    field.Appearance.Value.Options.UseTextOptions = true;
                    field.Appearance.Value.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                }
            }

            pivotGridControl1.BestFit();
        }

        private void formatCells(PivotGridControl pivotGridControl1)
        {
            
            pivotGridControl1.Fields[TargetPlanCompareData.STEP_ID].SortMode = PivotSortMode.Custom;

            pivotGridControl1.Fields[TargetPlanCompareData.QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;

            pivotGridControl1.Fields[TargetPlanCompareData.QTY].CellFormat.FormatString = "#,##0";
            
            pivotGridControl1.CustomDrawCell += this.pivotGridControl1_CustomDrawCell;
            pivotGridControl1.CustomCellDisplayText += this.pivotGridControl1_CellDisplayText;

            pivotGridControl1.FocusedCellChanged += this.pivotGridControl1_FocusedCellChanged;
            pivotGridControl1.CustomFieldSort += this.pivotGridControl_CustomFieldSort;

        }

        private void pivotGridControl1_CustomDrawCell(object sender, PivotCustomDrawCellEventArgs e)
        {
            int index = 0;
            PivotGridField[] fields = e.GetRowFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Caption == TargetPlanCompareData.CATEGORY)
                {
                    index = i; break;
                }
            }

            if (e.Value != null && (decimal)e.GetFieldValue(e.DataField) < 0)
            {
                e.Appearance.Options.UseBackColor = true;
                //e.Appearance.BackColor = Color.Yellow;
                //e.Appearance.BackColor2 = Color.Yellow;
                e.Appearance.ForeColor = Color.Red;
            }

            index = 0;

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Caption == "FACTORY")
                {
                    index = i; break;
                }
            }

            if (index >= 0 && fields.Length > index && e.GetFieldValue(fields[index]).ToString().StartsWith(TargetPlanCompareData.TOTAL))
            {
                e.Appearance.BackColor = Color.LightGreen;
                e.Appearance.BackColor2 = Color.LightGreen;
                e.Appearance.Options.UseBackColor = true;
            }

            //if (index >= 0 && fields.Length > index && e.GetFieldValue(fields[index]).ToString().StartsWith("Plan"))
            //{
            //    e.Appearance.BackColor = Color.Gainsboro;
            //    e.Appearance.BackColor2 = Color.Gainsboro;
            //    e.Appearance.Options.UseBackColor = true;
            //}
        }

        #endregion

        #region Chart

        private void DrawChart(string shopID, string stepID, string prodID)
        {
            string key = shopID + prodID + stepID;

            CompareItem item;
            if (_resultDict.TryGetValue(key, out item) == false)
                return;

            this.chartControl1.Series.Clear();

            DataTable dt = CreateChartTable();

            FillChartTable(item, dt);

            GenerateLineSeries(dt, QtyType.TARGET.ToString());
            GenerateLineSeries(dt, QtyType.PLAN.ToString());
            GenerateLineSeries(dt, QtyType.DIFF.ToString());

        }

        private void FillChartTable(CompareItem item, DataTable dt)
        {
            List<string> dates = item.Targets.Keys.ToList<string>();

            foreach (string d in dates)
            {
                if (_dateList.Contains(d) == false)
                    _dateList.Add(d);
            }
            _dateList.Sort();

            float tarQty = 0.0f;
            float planQty = 0.0f;
            float diffQty = 0.0f;
                  
            foreach (string date in _dateList)
            {
                // TARGET
                float qty1 = 0.0f;
                item.Targets.TryGetValue(date, out qty1);
                tarQty += qty1;

                DataRow chartRow = dt.NewRow();

                chartRow[TargetPlanCompareData.CATEGORY] = QtyType.TARGET.ToString();
                chartRow[TargetPlanCompareData.TARGET_DATE] = date;
                chartRow[TargetPlanCompareData.QTY] = tarQty;

                dt.Rows.Add(chartRow);


                // Plan
                float qty2 = 0.0f;
                item.Plans.TryGetValue(date, out qty2);
                planQty += qty2;

                DataRow chartRow2 = dt.NewRow();

                chartRow2[TargetPlanCompareData.CATEGORY] = QtyType.PLAN.ToString();
                chartRow2[TargetPlanCompareData.TARGET_DATE] = date;
                chartRow2[TargetPlanCompareData.QTY] = planQty;

                dt.Rows.Add(chartRow2);

                // DIFF
                float qty3 = 0.0f;
                item.Diffs.TryGetValue(date, out qty3);
                diffQty += qty3;

                DataRow chartRow3 = dt.NewRow();

                chartRow3[TargetPlanCompareData.CATEGORY] = QtyType.DIFF.ToString();
                chartRow3[TargetPlanCompareData.TARGET_DATE] = date;
                chartRow3[TargetPlanCompareData.QTY] = diffQty;

                dt.Rows.Add(chartRow3);
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

        private void GenerateLineSeries(DataTable dt, string category)
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

            Color color;
            if (category == QtyType.TARGET.ToString())
                color = Color.CornflowerBlue;
            else if (category == QtyType.PLAN.ToString())
                color = Color.MediumVioletRed;
            else
                color = Color.ForestGreen;

            series.View.Color = color;
        }


        #endregion 


        #region Helper Functions

        private string NextDateToString(string str)
        {
            DateTime dt = DateUtility.DbToDate(str);

            dt = dt.AddDays(1);

            return dt.ToString("yyyyMMdd");
        }

        #endregion

        #region Inner Class : CompareItem

        internal class CompareItem
        {
            public string SHOP_ID { get; set; }
            
            public string PRODUCT_ID { get; set; }
            public string STEP_ID { get; set; }
            public string OWNER_TYPE { get; set; }

            public float TARGET_TOTAL { get; set; }
            public float PLAN_TOTAL { get; set; }

            public float DIFF_TOTAL
            {
                get { return this.PLAN_TOTAL - this.TARGET_TOTAL; }
            }

            public List<string> Dates { get; set; }

            // <TARGET_DATE, CHIP_QTY>
            public Dictionary<string, float> Targets;
            public Dictionary<string, float> AccTargets;
            public Dictionary<string, float> Plans;
            public Dictionary<string, float> AccPlans;
            public Dictionary<string, float> Diffs;
            public Dictionary<string, float> AccDiffs;

            public CompareItem(string factory, string productID, string stepID)
            {
                this.SHOP_ID = factory;
                this.PRODUCT_ID = productID;
                this.STEP_ID = stepID;

                this.Dates = new List<string>();
                this.Targets = new Dictionary<string, float>();
                this.AccTargets = new Dictionary<string, float>();
                this.Plans = new Dictionary<string, float>();
                this.AccPlans = new Dictionary<string, float>();
                this.Diffs = new Dictionary<string, float>();
                this.AccDiffs = new Dictionary<string, float>();

                this.TARGET_TOTAL = 0;
                this.PLAN_TOTAL = 0;
            }

            public void AddQty(string targetDate, float qty, QtyType qtyType)
            {
                if (qtyType == QtyType.DIFF)
                    return;

                if (this.Dates.Contains(targetDate) == false)
                    this.Dates.Add(targetDate);

                bool isPlan = qtyType == QtyType.PLAN;
                var infos = qtyType == QtyType.PLAN ? this.Plans : this.Targets;

                float prevQty;
                if (infos.TryGetValue(targetDate, out prevQty))
                    infos[targetDate] += qty;
                else
                    infos.Add(targetDate, qty);

                if (isPlan)
                    this.PLAN_TOTAL += qty;
                else
                    this.TARGET_TOTAL += qty;
            }

            public float GetQty(string targetDate, QtyType qtyType)
            {
                var infos = qtyType == QtyType.TARGET ? this.Targets : this.Plans;

                if (qtyType == QtyType.DIFF)
                    infos = this.Diffs;

                float qty;
                if (infos.TryGetValue(targetDate, out qty))
                    return qty;

                return 0;
            }

            public float GetTotalQty(QtyType qtyType)
            {
                if (qtyType == QtyType.TARGET)
                    return this.TARGET_TOTAL;

                if (qtyType == QtyType.PLAN)
                    return this.PLAN_TOTAL;

                if (qtyType == QtyType.DIFF)
                    return this.DIFF_TOTAL;

                return 0;
            }

            public void SetDiff()
            {
                foreach (string targetDate in this.Dates)
                {
                    float targetQty = GetQty(targetDate, QtyType.TARGET);
                    float planQty = GetQty(targetDate, QtyType.PLAN);
                    float diffQty = planQty - targetQty;

                    float prevQty;
                    if (this.Diffs.TryGetValue(targetDate, out prevQty))
                        this.Diffs[targetDate] += diffQty;
                    else
                        this.Diffs.Add(targetDate, diffQty);
                }
            }

            public void SetAccQty()
            {
                float accTargetQty = 0.0f;
                this.Dates.Sort();
                foreach (string targetDate in this.Dates)
                {
                    float targetQty = GetQty(targetDate, QtyType.TARGET);
                    accTargetQty = accTargetQty + targetQty;

                    float prevAccQty;
                    if (this.AccTargets.TryGetValue(targetDate, out prevAccQty))
                        this.AccTargets[targetDate] += accTargetQty;
                    else
                        this.AccTargets.Add(targetDate, accTargetQty);
                }

                float accPlanQty = 0.0f;
                this.Dates.Sort();
                foreach (string targetDate in this.Dates)
                {
                    float planQty = GetQty(targetDate, QtyType.PLAN);
                    accPlanQty = accPlanQty + planQty;

                    float prevAccQty;
                    if (this.AccPlans.TryGetValue(targetDate, out prevAccQty))
                        this.AccPlans[targetDate] += accPlanQty;
                    else
                        this.AccPlans.Add(targetDate, accPlanQty);
                }

                float accDiffQty = 0.0f;
                this.Dates.Sort();
                foreach (string targetDate in this.Dates)
                {
                    float diffQty = GetQty(targetDate, QtyType.DIFF);
                    accDiffQty = accDiffQty + diffQty;

                    float prevAccQty;
                    if (this.AccDiffs.TryGetValue(targetDate, out prevAccQty))
                        this.AccDiffs[targetDate] += accDiffQty;
                    else
                        this.AccDiffs.Add(targetDate, accDiffQty);
                }
            }

            public float GetAccQty(string targetDate, QtyType qtyType)
            {
                var infos = qtyType == QtyType.TARGET ? this.AccTargets : this.AccPlans;

                if (qtyType == QtyType.DIFF)
                    infos = this.AccDiffs;

                float qty;
                if (infos.TryGetValue(targetDate, out qty))
                    return qty;

                return 0;
            }
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

        #region Event Handlers

        private void pivotGridControl1_FocusedCellChanged(object sender, EventArgs e)
        {
            PivotCellEventArgs c = this.pivotGridControl1.Cells.GetFocusedCellInfo();

            string shopID = string.Empty;
            string stepID = string.Empty;
            string ownerTpye = string.Empty;
            string prodID = string.Empty;

            foreach (PivotGridField r in c.GetRowFields())
            {
                string strFieldName = r.FieldName;
                string strFieldValue = r.GetValueText(c.GetFieldValue(r));

                if (strFieldName == TargetPlanCompareData.SHOP_ID)
                    shopID = strFieldValue;
                else if (strFieldName == TargetPlanCompareData.STEP_ID)
                    stepID = strFieldValue;
                else if (strFieldName == TargetPlanCompareData.PRODUCT_ID)
                    prodID = strFieldValue;
                else if (strFieldName == TargetPlanCompareData.OWNER_TYPE)
                    ownerTpye = strFieldName;                   
            }

            DrawChart(shopID, stepID, prodID);
        }

        //private void pivotGridControl_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        //{
        //    if (e.Field.FieldName == TargetPlanCompareData.CATEGORY)
        //    {
        //        if (e.Value1 == null || e.Value2 == null) return;
        //        e.Handled = true;
        //        string d1 = "" + e.Value1.ToString();
        //        string d2 = "" + e.Value2.ToString();

        //        if (d1.CompareTo(d2) > 0)
        //            e.Result = -1;
        //        else if (d1 == d2)
        //            e.Result = 0;
        //        else
        //            e.Result = 1;
        //    }
        //}

        private void pivotGridControl1_CellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
        {
            if (e.GetFieldValue(e.DataField) != null && e.GetFieldValue(e.DataField).ToString() == "0")
            {
                e.DisplayText = string.Empty;
            }
        }

        private void Query()
        {
            SetDateRange();

            LoadData();

            XtraPivotGridHelper.DataViewTable dataViewTable = BindData();

            FillData(dataViewTable);

            DrawGrid(dataViewTable);

            LoadDefaultLayOutFromXml();

            this.expandablePanel1.Text = "StepTarget VS EqpPlan : ";
        }

        private void queryButton_Click(object sender, EventArgs e)
        {
            Query();
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

        private void LoadDefaultLayOutFromXml()
        {
            string dirPath = string.Format("{0}\\DefaultLayOut", _application.ApplicationPath);
            string fileName = string.Format("{0}.xml", _pageID);

            PivotGridLayoutHelper.LoadXml(this.pivotGridControl1, dirPath, fileName);
        }

        private void shopIdComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isFirstLoad == false)
                return;

            SetProductCombo();
            SetStepIDCombo(TargetShopID);
        }

        private void isShowAccQty_CheckedChanged(object sender, EventArgs e)
        {
            Query();
        }

        private void StepComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
