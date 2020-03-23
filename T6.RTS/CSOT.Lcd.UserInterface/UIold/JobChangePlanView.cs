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
using CSOT.Lcd.UserInterface.DataMappings;
using DevExpress.XtraGrid.Views.Base;
using Mozart.Studio.TaskModel.UserLibrary;
using DevExpress.XtraPivotGrid;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class JobChangePlanView : XtraPivotGridControlView
    {
        #region Class Variables

        private IExperimentResultItem _result;
        private Dictionary<string, List<string>> _prodGroupDict;
        private Dictionary<string, string> _bomDict;
        private Dictionary<string, ProductInfo> _resultDict;
        private Dictionary<string, DetailInfo> _detailDict;
        private Dictionary<string, string> _layerDict;
        private Dictionary<string, string> _descDict;
        private DataTable _eqpPlanTable;
        private List<DateTime> _shiftList;
        private List<string> _layerOrder;
        private bool IsFirstLoaded = true;

        #endregion

        #region Properties

        string SelectedProdGrp
        { get { return this.prodGrpCombo.SelectedItem.ToString(); } }

        string SelectedProduct
        { get { return this.productCombo.SelectedItem.ToString(); } }

        #endregion

        #region 생성자
        public JobChangePlanView()
        {
            InitializeComponent();

        }

        public JobChangePlanView(IServiceProvider serviceProvider)
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

            SetData();
        }
        #endregion

        #region SetControl
        private void SetControl()
        {

            #region Product Group
            
            _prodGroupDict = new Dictionary<string,List<string>>();

            DataTable prodTable = _result.LoadInput(JobChangePlanData.PRODUCT_DATA_TABLE);

            foreach (DataRow pRow in prodTable.Rows)
            {
                JobChangePlanData.Product prod = new JobChangePlanData.Product(pRow);

                if (prod.ProductGroup == "BARE")
                    continue;

                List<string> prodList;
                if (_prodGroupDict.TryGetValue(prod.ProductGroup, out prodList) == false)
                {
                    prodList = new List<string>();
                    _prodGroupDict.Add(prod.ProductGroup, prodList);
                }

                if(!prodList.Contains(prod.ProductID))
                    prodList.Add(prod.ProductID);
            }

            foreach (string prodGrp in _prodGroupDict.Keys)
            {
                this.prodGrpCombo.Items.Add(prodGrp);
            }
            this.prodGrpCombo.SelectedIndex = 0;

            UpdateProductList();

            #endregion 


        }
        #endregion

        #region SetData

        private void SetData()
        {
            _shiftList = new List<DateTime>();

            DateTime startShift = ShopCalendar.ShiftStartTimeOfDayT(Globals.GetResultStartTime(_result));
            int shiftCount = Globals.GetResultPlanPeriod(_result) * 3;

            for (int i = 0; i < shiftCount; i++)
            {
                _shiftList.Add(startShift.AddHours(ShopCalendar.ShiftHours * i));
            }

            _bomDict = new Dictionary<string, string>();

            DataTable bomTable = _result.LoadInput(JobChangePlanData.INNER_BOM_DATA_TABLE);

            foreach (DataRow bRow in bomTable.Rows)
            {
                JobChangePlanData.InnerBom bom = new JobChangePlanData.InnerBom(bRow);

                if (_bomDict.ContainsKey(bom.FromProductID) == false)
                    _bomDict.Add(bom.FromProductID, bom.ToProductID);
            }

            _layerDict = new Dictionary<string, string>();
            _descDict = new Dictionary<string, string>();
            _layerOrder = new List<string>();
            
            
            DataTable stdStepTable = _result.LoadInput(JobChangePlanData.STD_STEP_DATA_TABLE);
            stdStepTable.DefaultView.Sort = JobChangePlanData.StdStep.Schema.STEP_ID + " " + "ASC";
            stdStepTable = stdStepTable.DefaultView.ToTable();

            foreach (DataRow sRow in stdStepTable.Rows)
            {
                JobChangePlanData.StdStep step = new JobChangePlanData.StdStep(sRow);

                string key = step.ShopID + step.StepID;

                if (_layerDict.ContainsKey(key) == false)
                    _layerDict.Add(key, key.EndsWith("C00000") ? "BANK_" : step.Layer);

                if (_descDict.ContainsKey(key) == false)
                    _descDict.Add(key, step.StepDesc);

                if (step.Layer != "BANK" && step.Layer != "F/T" && _layerOrder.Contains(step.Layer) == false)
                    _layerOrder.Add(step.Layer);
            }
            _layerOrder.Add("F/T");
            _layerOrder.Add("BANK");
            _layerOrder.Add("BANK_");
        }

        #endregion 

        #region LoadData

        private void LoadData()
        {
            _eqpPlanTable = _result.LoadOutput(JobChangePlanData.EQP_PLAN_DATA_TABLE);

            List<string> prodList = _prodGroupDict[SelectedProdGrp];
            _resultDict = new Dictionary<string, ProductInfo>();

            if(SelectedProduct != Consts.ALL)
            {
                prodList = new List<string>();
                prodList.Add(SelectedProduct);
            }

            foreach (DataRow epRow in _eqpPlanTable.Rows)
            {
                JobChangePlanData.EqpPlan ep = new JobChangePlanData.EqpPlan(epRow);

                if (ep.ProductID.StartsWith("CE"))
                    ep.ProductID = GetCellProductID(ep.ProductID);

                if (prodList.Contains(ep.ProductID) == false)
                    continue;

                ProductInfo info;
                if (_resultDict.TryGetValue(ep.ProductID, out info) == false)
                {
                    info = new ProductInfo(ep.ProductID, ep.ShopID);
                    _resultDict.Add(ep.ProductID, info);
                }
                                
                if (IsInStep(ep.StepID))
                {
                    info.AddInfo(ep.OutTargetQty, 0, 0);
                    info.SetInOutTime(ep.StartTime, ep.EndTime, true);
                }
                if (IsOutStep(ep.StepID))
                {
                    info.AddInfo(0, ep.OutTargetQty, 0);
                    info.SetInOutTime(ep.StartTime, ep.EndTime, false);
                }
            }
        }

        private string GetCellProductID(string fromProductID)
        {
            if (_bomDict.ContainsKey(fromProductID))
                return _bomDict[fromProductID];

            return fromProductID;
        }

        private bool IsInStep(string stepID)
        {
            return stepID.EndsWith("01000");
        }

        private bool IsOutStep(string stepID)
        {
            return stepID.EndsWith("09000");
        }

        #endregion

        #region Bind Grid

        private void BindGrid()
        {
            DataTable dt = CreateDataTable();

            foreach (ProductInfo info in _resultDict.Values)
            {
                DataRow row = dt.NewRow();

                row[JobChangePlanData.PRODUCT_ID] = info.ProductID;
                row[JobChangePlanData.SHOP_ID] = info.ShopID;
                row[JobChangePlanData.IN_QTY] = info.InQty;
                row[JobChangePlanData.OUT_QTY] = info.OutQty;
                row[JobChangePlanData.AVG_TAT] = info.AvgTat;
                row[JobChangePlanData.IN_TIME] = info.InTime.ToString("yyyy-MM-dd HH:mm:ss");
                row[JobChangePlanData.OUT_TIME] = info.OutTime.ToString("yyyy-MM-dd HH:mm:ss");
                row[JobChangePlanData.IN_DURATION] = info.InDuration;
                row[JobChangePlanData.OUT_DURATION] = info.OutDuration;


                dt.Rows.Add(row);
            }

            this.gridControl1.DataSource = dt;
            this.gridView1.BestFitColumns();

            this.gridView1.FocusedRowChanged += gridView1_FocusedRowChanged;
        }
                    
        private DataTable CreateDataTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(JobChangePlanData.PRODUCT_ID, typeof(string));
            //dt.Columns[JobChangePlanData.PRODUCT_ID].Caption = "장비";
            dt.Columns.Add(JobChangePlanData.SHOP_ID, typeof(string));
            //dt.Columns[JobChangePlanData.State].Caption = "상태";
            dt.Columns.Add(JobChangePlanData.IN_QTY, typeof(float));
            //dt.Columns[JobChangePlanData.LotID].Caption = "LOT_ID";
            dt.Columns.Add(JobChangePlanData.OUT_QTY, typeof(float));
            //dt.Columns[JobChangePlanData.Layer].Caption = "LAYER";
            dt.Columns.Add(JobChangePlanData.AVG_TAT, typeof(float));
            //dt.Columns[JobChangePlanData.OperName].Caption = "STEP_ID";
            dt.Columns.Add(JobChangePlanData.IN_TIME, typeof(string));
            //dt.Columns[JobChangePlanData.ProductID].Caption = "PRODUCT_ID";
            dt.Columns.Add(JobChangePlanData.OUT_TIME, typeof(string));
           // dt.Columns[JobChangePlanData.StartTime].Caption = "시작시간";
            dt.Columns.Add(JobChangePlanData.IN_DURATION, typeof(float));
           // dt.Columns[JobChangePlanData.EndTime].Caption = "종료시간";
            dt.Columns.Add(JobChangePlanData.OUT_DURATION, typeof(float));
           // dt.Columns[JobChangePlanData.GapTime].Caption = "수행시간";

            return dt;
        }

        #endregion

        #region Bind PivotGrid

        private void BindPivotGrid(string productID)
        {
            _detailDict = new Dictionary<string, DetailInfo>();

            bool isFirstItem = true;

            foreach (DataRow row in _eqpPlanTable.Rows)
            {
                JobChangePlanData.EqpPlan ep = new JobChangePlanData.EqpPlan(row);

                if (ep.ProductID.StartsWith("CE"))
                ep.ProductID = GetCellProductID(ep.ProductID);

                if (ep.ProductID != productID)
                    continue;

                if(ep.StepID == "C00000")
                    Console.WriteLine("a");

                string layer = FindLayer(ep.ShopID, ep.StepID);
                string stepDesc = FindDesc(ep.ShopID, ep.StepID);

                if (layer == string.Empty)
                    continue;

                string key = layer + ep.StepID;
                DetailInfo info;
                if(_detailDict.TryGetValue(key, out info) == false)
                {
                    info = new DetailInfo(ep.ProductID, ep.ShopID, layer, ep.StepID, stepDesc);

                    if (isFirstItem)
                        info.FillDummyDates(_shiftList);

                    _detailDict.Add(key, info);
                }

                DateTime shift = ShopCalendar.ShiftStartTimeOfDayT(ep.StartTime);

                info.AddQty(shift, ep.OutTargetQty);
                info.CalculateTimes(ep.StartTime, ep.EndTime);

                isFirstItem = false;
            }


        }

        private void BindData()
        {
            XtraPivotGridHelper.DataViewTable dt = CreateDataViewTable();

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

            pivotGridControl1.CustomDrawCell += this.pivotGridControl1_CustomDrawCell;

            ShowTotal(this.pivotGridControl1, false);

            pivotGridControl1.CustomFieldSort += this.pivotGridControl1_CustomFieldSort;
            this.pivotGridControl1.Fields[JobChangePlanData.LAYER].SortMode = PivotSortMode.Custom;

            //pivotGridControl1.CustomCellDisplayText += pivotGridControl1_CellDisplayText;
            this.pivotGridControl1.EndUpdate();

            this.pivotGridControl1.BestFitColumnArea();
        }


        private void FillData(XtraPivotGridHelper.DataViewTable dt)
        {
            foreach (DetailInfo info in _detailDict.Values)
            {
                foreach (KeyValuePair<DateTime, float> pair in info.QtyDict)
                {
                    dt.DataTable.Rows.Add(
                       info.Layer,
                       info.StepID,
                       info.StepDesc,
                       info.Span.ToString("0.0"),
                       Convert.ToInt32(info.Total),
                       pair.Key.ToString("yyyyMMdd HH"),
                       Convert.ToInt32(pair.Value)
                     );
                }
            }
        }



        private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        {
            XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();

            dt.AddColumn(JobChangePlanData.LAYER, JobChangePlanData.LAYER, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(JobChangePlanData.STEP_ID, JobChangePlanData.STEP_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(JobChangePlanData.STEP_DESC, JobChangePlanData.STEP_DESC, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(JobChangePlanData.SPAN, JobChangePlanData.SPAN, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(JobChangePlanData.TOTAL, JobChangePlanData.TOTAL, typeof(int), PivotArea.RowArea, null, null);
            dt.AddColumn(JobChangePlanData.SHIFT, JobChangePlanData.SHIFT, typeof(string), PivotArea.ColumnArea, null, null);

            dt.AddColumn(JobChangePlanData.QTY, JobChangePlanData.QTY, typeof(int), PivotArea.DataArea, null, null);

            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns[JobChangePlanData.LAYER],
                        dt.Columns[JobChangePlanData.STEP_ID],
                        dt.Columns[JobChangePlanData.SHIFT]
                    }
                );

            return dt;
        }

        private string FindDesc(string shopID, string stepID)
        {
            if (_descDict.ContainsKey(shopID + stepID))
                return _descDict[shopID + stepID];

            return string.Empty;
        }

        private string FindLayer(string shopID, string stepID)
        {
            if (_layerDict.ContainsKey(shopID + stepID))
                return _layerDict[shopID + stepID];

            return string.Empty;
        }

        #endregion

        #region Event handler

        private void pivotGridControl1_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        {
            if (e.Field.FieldName == JobChangePlanData.LAYER)
            {
                if (e.Value1 == null || e.Value2 == null) return;
                e.Handled = true;
                string d1 = "" + e.Value1.ToString();
                string d2 = "" + e.Value2.ToString();

                int c1 = _layerOrder.IndexOf(d1);
                int c2 = _layerOrder.IndexOf(d2);

                e.Result = c1.CompareTo(c2);
            }
        }


        private void pivotGridControl1_CustomDrawCell(object sender, PivotCustomDrawCellEventArgs e)
        {
            // 색상 변경 : 특정 Data Cell : 색상 처리
            if ((e.DataField.FieldName == JobChangePlanData.QTY) && e.Value != null && (decimal)e.GetFieldValue(e.DataField) > 0)
            {
                e.Appearance.Options.UseBackColor = true;
                e.Appearance.DrawBackground(e.GraphicsCache, e.Bounds);
                e.Appearance.BackColor = Color.Yellow;
                e.Appearance.BackColor2 = Color.Yellow;
                e.Appearance.DrawString(e.GraphicsCache, e.DisplayText, e.Bounds, e.Appearance.Font, Brushes.Black, e.Appearance.GetStringFormat());
                e.Handled = true;
            }

        }

        private void gridView1_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            int rowIndex = e.FocusedRowHandle;

            DataRow row = (gridControl1.DataSource as DataTable).Rows[rowIndex];

            string productID = row[JobChangePlanData.PRODUCT_ID].ToString();

            BindPivotGrid(productID);

            BindData();

            if (IsFirstLoaded)
            {
                BindPivotGrid(productID);

                BindData();

                IsFirstLoaded = false;
            }

            
        }

        private void prodGrpCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProductList();
        }

        private void UpdateProductList()
        {
            this.productCombo.Items.Clear();

            string prodGrp = this.prodGrpCombo.SelectedItem.ToString();

            if (_prodGroupDict.ContainsKey(prodGrp) == false)
                return;

            this.productCombo.Items.Add(Consts.ALL);

            foreach (string prod in _prodGroupDict[prodGrp])
            {
                this.productCombo.Items.Add(prod);
            }

            this.productCombo.SelectedIndex = 0;

        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            Query();
        }

        private void Query()
        {
            LoadData();
            
            BindGrid();
        }

        #endregion 

        #region Inner Class

        internal class ProductInfo
        {
            public string ProductID;
            public string ShopID;
            public float InQty;
            public float OutQty;
            public float AvgTat;
            public DateTime InTime;
            public DateTime OutTime;

            public float InDuration;
            public float OutDuration;

            public ProductInfo(string prodID, string shopID)
            {
                this.ProductID = prodID;
                this.ShopID = shopID;
                this.InQty = 0;
                this.OutQty = 0;
                this.AvgTat = 0;
                this.InTime = DateTime.MaxValue;
                this.OutTime = DateTime.MinValue;

                this.InDuration = 0.0f;
                this.OutDuration = 0.0f;
            }

            public void AddInfo(float inQty, float outQty, float tat)
            {
                this.InQty += inQty;
                this.OutQty += outQty;
                this.AvgTat = CalculateAvgTat(tat);
            }

            public void SetInOutTime(DateTime inTime, DateTime outTime, bool IsInStep)
            {
                if (IsInStep && this.InTime > inTime)
                    this.InTime = inTime;

                if (!IsInStep && this.OutTime < outTime)
                    this.OutTime = outTime;

                if (IsInStep)
                    this.InDuration += (outTime - inTime).Hours;

                if (!IsInStep)
                    this.OutDuration += (outTime - inTime).Hours;
            }

            private float CalculateAvgTat(float tat)
            {
                return tat;
            }
        }

        internal class DetailInfo
        {
            public string ProductID;
            public string ShopID;
            public string Layer;
            public string StepID;
            public string StepDesc;
            public float Total;
            public float Span;

            public DateTime FirstTime;
            public DateTime LastTime;

            public Dictionary<DateTime, float> QtyDict;

            public DetailInfo(string prodID, string shopID, string layer, string stepID, string stepDesc)
            {
                this.ProductID = prodID;
                this.ShopID = shopID;
                this.StepID = stepID;
                this.Layer = layer;
                this.StepDesc = stepDesc;

                this.FirstTime = DateTime.MaxValue;
                this.LastTime = DateTime.MinValue;

                this.QtyDict = new Dictionary<DateTime, float>();
            }

            public void FillDummyDates(List<DateTime> list)
            {
                foreach (DateTime date in list)
                {
                    QtyDict.Add(date, 0.0f);
                }
            }

            public void AddQty(DateTime shift, float qty)
            {
                float prev;
                if (this.QtyDict.TryGetValue(shift, out prev) == false)
                {
                    prev = 0.0f;
                    this.QtyDict.Add(shift, prev);
                }
                this.QtyDict[shift] = prev + qty;

                this.Total += qty;
            }

            public void CalculateTimes(DateTime startTime, DateTime endTime)
            {
                if (endTime != DateTime.MinValue)
                {
                    int hours = (endTime - startTime).Hours;

                    this.Span += (float)((float)hours / 24);
                }
                else
                    this.Span = 0;
            }
           
        }

        #endregion 

        #region Helper function

        private void ShowTotal(PivotGridControl pivot, bool isCheck)
        {
            pivot.OptionsView.ShowRowTotals = false;
            pivot.OptionsView.ShowRowGrandTotals = false;
            pivot.OptionsView.ShowColumnTotals = isCheck;
            pivot.OptionsView.ShowColumnGrandTotals = isCheck;
        }

        #endregion 
    }
}
