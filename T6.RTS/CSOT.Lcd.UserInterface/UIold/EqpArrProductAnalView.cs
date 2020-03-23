using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraPivotGrid;

using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;

using CSOT.Lcd.Scheduling;

using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class EqpArrProductAnalView : XtraPivotGridControlView
    {
        private IExperimentResultItem _result;

        DataTable _dtEqpArrange;
        DataTable _dtStdStep;

        DataTable _eqpInfoDT;

        Dictionary<string, string> _prodTypeDic;

        List<string> _selectedEqpGroupList;

        Dictionary<string, EqpArrProductAnalViewData.EqpArrAnalResult> _resultDic;
                
        public bool IsShowReworkStep
        { get { return this.ShowReworkStep.Checked; } }

        List<string> SelectedEqpGroupsInArea
        {
            get
            {
                if (_selectedEqpGroupList != null)
                    return _selectedEqpGroupList;

                _selectedEqpGroupList = new List<string>();

                var modelContext = this._result.GetCtx<ModelDataContext>();

                var totalEqpGrpIDs = (from a in modelContext.Eqp
                               where a.SHOP_ID ==  this.shopIdComboBox.SelectedItem.ToString()
                               select new { EQP_GROUP_ID = a.DSP_EQP_GROUP_ID })
                               .Distinct().OrderBy(x => x.EQP_GROUP_ID);

                foreach (var checkedAreaID in this.checkedComboBoxEdit1.Properties.Items.GetCheckedValues())
                {
                    if (checkedAreaID.ToString() == "OTHERS")
                    {
                        //var eqpGroupInArea = modelContext.Const.Where(x => x.CATEGORY == "AREA_INFO" & x.CODE != "OTHERS")
                        //    .Select(x => x.DESCRIPTION);

                        //List<string> noOthersEqpGrpList = new List<string>();
                        //foreach (var item in eqpGroupInArea)
                        //{
                        //    string[] eqpGroups = item.ToString().Split('@');
                        //    foreach (string eqpGroup in eqpGroups)
                        //    {
                        //        if (noOthersEqpGrpList.Contains(eqpGroup) == false)
                        //            noOthersEqpGrpList.Add(eqpGroup);
                        //    }
                        //}

                        foreach (var item in totalEqpGrpIDs)
                        {
                            string eqpGroupID = item.EQP_GROUP_ID;

                            //if (noOthersEqpGrpList.Contains(eqpGroupID) == false && _selectedEqpGroupList.Contains(eqpGroupID) == false)
                            //    _selectedEqpGroupList.Add(eqpGroupID);
                        }
                    }
                    else
                    {
                        //var eqpGroupInArea = modelContext.Const.Where(x => x.CATEGORY == "AREA_INFO" & x.CODE == checkedAreaID.ToString())
                        //    .Select(x => x.DESCRIPTION);


                        string filter = string.Format("{0} = 'AREA_INFO' AND {1} = '{2}'", EqpGanttChartData.Const.Schema.CATEGORY
                            , EqpGanttChartData.Const.Schema.CODE, checkedAreaID);

                        DataRow[] drs = this._eqpInfoDT.Select(filter);

                        foreach (DataRow drow in drs)
                        {
                            string eqpGroupInArea = drow.GetString(EqpGanttChartData.Const.Schema.DESCRIPTION);

                            string[] eqpGroups = eqpGroupInArea.ToString().Split('@');

                            foreach (string eqpGroup in eqpGroups)
                            {
                                if (_selectedEqpGroupList.Contains(eqpGroup) == false)
                                    _selectedEqpGroupList.Add(eqpGroup);
                            }

                        }
                    }
                }

                return _selectedEqpGroupList;
            }
        }

        bool IsShowOnlyProduction
        {
            get
            {
                return this.showOnlyProductionChckBox.Checked;
            }
        }


        public EqpArrProductAnalView()
        {
            InitializeComponent();
        }

        public EqpArrProductAnalView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {
            // 진입점

            var item = (IMenuDocItem)this.Document.ProjectItem;
            _result = (IExperimentResultItem)item.Arguments[0];

            if (_result == null)
                return;

            Globals.InitFactoryTime(_result.Model);

            SetControl();
            
            InitializeData();
        }
        
        private void SetControl()
        {
            this.EqpIdBox.Text = "";
            this.ShowReworkStep.Checked = false;
            
            var modelContext = _result.GetCtx<ModelDataContext>();

            // ShopID ComboBox
            this.shopIdComboBox.Items.Clear();

            if (this.shopIdComboBox.Items.Count > 0)
                this.shopIdComboBox.SelectedIndex = 0;
            
            var shopIDs = (from a in modelContext.EqpArrange
                           select new { SHOP_ID = a.SHOP_ID })
                           .Distinct().OrderBy(x => x.SHOP_ID);

            foreach (var item in shopIDs)
                this.shopIdComboBox.Items.Add(item.SHOP_ID.ToString());

            if (this.shopIdComboBox.Items.Count > 0)
                this.shopIdComboBox.SelectedIndex = 0;

            this._eqpInfoDT = Globals.GetConsInfo(modelContext);
            
            // AREA CheckedBox
            //var areaInfo = modelContext.Const.Where(x => x.CATEGORY == "AREA_INFO")
            //    .Select(x => x.CODE);



            //areaInfo.ToList().ForEach(x => this.checkedComboBoxEdit1.Properties.Items.Add(x));

            //if (checkedComboBoxEdit1.Properties.Items.Count > 0
            //    && this.checkedComboBoxEdit1.Properties.Items.Contains("OTHERS") == false)
            //    this.checkedComboBoxEdit1.Properties.Items.Add("OTHERS");

            if (this._eqpInfoDT != null)
            {
                foreach (DataRow drow in this._eqpInfoDT.Rows)
                {
                    EqpGanttChartData.Const configConst = new EqpGanttChartData.Const(drow);

                    this.checkedComboBoxEdit1.Properties.Items.Add(configConst.Code);
                }

                //if (areaCheckdBox.Properties.Items.Contains("OTHERS") == false)
                //    areaCheckdBox.Properties.Items.Add("OTHERS");
            }
        }

        
        private void InitializeData()
        {
            //var modelContext = _result.GetCtx<ModelDataContext>();
            
            base.SetWaitDialogCaption(EqpArrProductAnalViewData.EQP_ARRANGE_DT);
            _dtEqpArrange = _result.LoadInput(EqpArrProductAnalViewData.EQP_ARRANGE_DT);

            base.SetWaitDialogCaption(EqpArrProductAnalViewData.STD_STEP_DT);
            _dtStdStep = _result.LoadInput(EqpArrProductAnalViewData.STD_STEP_DT);

            var modelContext = this._result.GetCtx<ModelDataContext>();

            _prodTypeDic = new Dictionary<string, string>();
            foreach (var row in modelContext.Product)
            {
                if (_prodTypeDic.ContainsKey(row.SHOP_ID + row.PRODUCT_ID) == false)
                    _prodTypeDic.Add(row.SHOP_ID + row.PRODUCT_ID /*+ row.PRODUCT_VERSION*/, row.PRODUCT_TYPE);
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            _resultDic = new Dictionary<string, EqpArrProductAnalViewData.EqpArrAnalResult>();

            Query();

            _selectedEqpGroupList = null;
        }
        
        private void Query()
        {
            DataProcessing();

            XtraPivotGridHelper.DataViewTable dt = CreateDataViewTable();

            FillPivotGrid(dt);

            DrawGrid(dt);
        }

        private void DataProcessing()
        {
            var modelContext = _result.GetCtx<ModelDataContext>();

            string selectedShopID = this.shopIdComboBox.SelectedItem.ToString();

            string eqpBoxItem = string.IsNullOrEmpty(this.EqpIdBox.Text) ? string.Empty : this.EqpIdBox.Text.Trim();

            var filteredEqp =  modelContext.Eqp.Where(x => x.SHOP_ID == selectedShopID
                                        & (this.SelectedEqpGroupsInArea.Count <= 0 ? true :
                                            this.SelectedEqpGroupsInArea.Contains(x.DSP_EQP_GROUP_ID)));

            List<string> eqpList = new List<string>();
            filteredEqp.ToList().ForEach(x => eqpList.Add(x.EQP_ID.ToString()));

            var filteredStdStep = modelContext.StdStep.Where(x => x.SHOP_ID == selectedShopID & (double)x.STEP_SEQ > 0)
                    .Where(x => string.IsNullOrEmpty(x.STEP_DESC) == false)
                    .Where(x => this.IsShowReworkStep ? true : (x.STEP_DESC.Contains("Rework") == false
                                                        & x.STEP_DESC.Contains("- R") == false)
                                                        );

            var filteredEqpArr = modelContext.EqpArrange.Where(x => x.SHOP_ID == selectedShopID
                & x.EQP_ID.Contains(eqpBoxItem.ToUpper())
                & eqpList.Contains(x.EQP_ID));

            var analData = (from a in filteredEqpArr
                            join b in filteredStdStep
                            on a.STEP_ID equals b.STEP_ID
                            select new
                            {
                                SHOP_ID = a.SHOP_ID,
                                EQP_ID = a.EQP_ID,
                                PRODUCT_ID = a.PRODUCT_ID,
                                //PRODUCT_VERSION = a.PRODUCT_VERSION,
                                STEP_ID = a.STEP_ID,
                                STEP_DESC = b.STEP_DESC,
                                STEP_SEQ = (int)b.STEP_SEQ
                            }).Distinct().OrderBy(x => x.STEP_SEQ);                       

            foreach (var item in analData)
            {
                if (this.IsShowOnlyProduction)
                {
                    string prodType;
                    if (_prodTypeDic.TryGetValue(item.SHOP_ID + item.PRODUCT_ID /*+ item.PRODUCT_VERSION*/, out prodType) == false)
                        continue;

                    if (prodType != EqpArrProductAnalViewData.Consts.Production)
                        continue;
                }

                string key = CommonHelper.CreateKey(item.EQP_ID, item.STEP_DESC);

                EqpArrProductAnalViewData.EqpArrAnalResult resultInfo;
                if (_resultDic.TryGetValue(key, out resultInfo) == false)
                {
                    resultInfo = new EqpArrProductAnalViewData.EqpArrAnalResult(item.SHOP_ID, item.EQP_ID, item.STEP_ID, item.STEP_DESC);
                    _resultDic.Add(key, resultInfo);
                }

                resultInfo.AddProduct(item.PRODUCT_ID, "-"/*item.PRODUCT_VERSION*/);
            }
        }

        
        #region CreateDataViewTable
        private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        {
            XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();

            dt.AddColumn(EqpArrProductAnalViewData.SHOP_ID, EqpArrProductAnalViewData.SHOP_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(EqpArrProductAnalViewData.EQP_ID, EqpArrProductAnalViewData.EQP_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(EqpArrProductAnalViewData.STEP_ID, EqpArrProductAnalViewData.STEP_ID, typeof(string), PivotArea.ColumnArea, null, null);
            dt.AddColumn(EqpArrProductAnalViewData.STEP_DESC, EqpArrProductAnalViewData.STEP_DESC, typeof(string), PivotArea.ColumnArea, null, null);
            dt.AddColumn(EqpArrProductAnalViewData.PROD_QTY, EqpArrProductAnalViewData.PROD_QTY, typeof(int), PivotArea.DataArea, null, null);

            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns[EqpArrProductAnalViewData.SHOP_ID],
                        dt.Columns[EqpArrProductAnalViewData.EQP_ID],
                        dt.Columns[EqpArrProductAnalViewData.STEP_ID],
                        dt.Columns[EqpArrProductAnalViewData.STEP_DESC],
                        dt.Columns[EqpArrProductAnalViewData.PROD_QTY]
                    }
                );

            return dt;
        }
        #endregion

        private void FillPivotGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            foreach (EqpArrProductAnalViewData.EqpArrAnalResult resultInfo in _resultDic.Values)
            {
                DataRow dRow = dt.DataTable.NewRow();

                dRow[EqpArrProductAnalViewData.SHOP_ID] = resultInfo.SHOP_ID;
                dRow[EqpArrProductAnalViewData.EQP_ID] = resultInfo.EQP_ID;
                dRow[EqpArrProductAnalViewData.STEP_ID] = resultInfo.STEP_ID;
                dRow[EqpArrProductAnalViewData.STEP_DESC] = resultInfo.STEP_DESC;
                dRow[EqpArrProductAnalViewData.PROD_QTY] = resultInfo.PROD_QTY;

                dt.DataTable.Rows.Add(dRow);
            }
        }

        private void DrawGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            this.pivotGridControl1.BeginUpdate();

            this.pivotGridControl1.ClearPivotGridFields();
            this.pivotGridControl1.CreatePivotGridFields(dt);

            this.pivotGridControl1.DataSource = dt.DataTable;

            //this.pivotGridControl1.OptionsView.ShowRowTotals = false;
            //this.pivotGridControl1.OptionsView.ShowRowGrandTotals = false;
            //this.pivotGridControl1.OptionsView.ShowColumnTotals = false;
            //this.pivotGridControl1.OptionsView.ShowColumnGrandTotals = false;
            
            this.pivotGridControl1.EndUpdate();

            this.pivotGridControl1.BestFitColumnArea();
        }

        private void pivotGridControl1_CellDoubleClick(object sender, PivotCellEventArgs e)
        {
            //string eqpId = e.;// [e.RowIndex, e.ColumnIndex].CellValue.ToString();
            //string stepSeq = this.pivotGridControl1[e.RowIndex, e.ColIndex + 1].CellValue.ToString();

            string shopID = this.shopIdComboBox.SelectedItem.ToString();
            string eqpID = string.Empty;
            string stepDesc = string.Empty;

            string key = string.Empty;
            if (e.ColumnField.FieldName == EqpArrProductAnalViewData.EQP_ID
                || e.RowField.FieldName == EqpArrProductAnalViewData.EQP_ID)
            {
                eqpID = e.ColumnField.FieldName == EqpArrProductAnalViewData.EQP_ID ?
                    this.pivotGridControl1.GetFieldValue(e.ColumnField, e.ColumnIndex).ToString()
                    : this.pivotGridControl1.GetFieldValue(e.RowField, e.RowFieldIndex).ToString();
            }

            if (e.ColumnField.FieldName == EqpArrProductAnalViewData.STEP_DESC
                || e.RowField.FieldName == EqpArrProductAnalViewData.STEP_DESC)
            {
                stepDesc = e.RowField.FieldName == EqpArrProductAnalViewData.STEP_DESC ?
                    this.pivotGridControl1.GetFieldValue(e.RowField, e.RowFieldIndex).ToString()
                    : this.pivotGridControl1.GetFieldValue(e.ColumnField, e.ColumnIndex).ToString();
            }


            key = CommonHelper.CreateKey(eqpID, stepDesc);

            EqpArrProductAnalViewData.EqpArrAnalResult resultInfo;
            if (_resultDic.TryGetValue(key, out resultInfo) == false)
                return;

            DataTable dt = new DataTable();
            dt.Columns.Add(EqpArrProductAnalViewData.SHOP_ID);
            dt.Columns.Add(EqpArrProductAnalViewData.EQP_ID);
            dt.Columns.Add(EqpArrProductAnalViewData.STEP_DESC);
            dt.Columns.Add(EqpArrProductAnalViewData.PRODUCT_ID);
            dt.Columns.Add(EqpArrProductAnalViewData.PRODUCT_VERSION);
            
            var prodDic = resultInfo.PROD_DIC.OrderBy(x => x.Key);

            foreach (var dic in prodDic)
            {
                dic.Value.Sort();

                foreach (string prodVer in dic.Value)
                {
                    DataRow dRow = dt.NewRow();
                    dRow[EqpArrProductAnalViewData.SHOP_ID] = shopID;
                    dRow[EqpArrProductAnalViewData.EQP_ID] = eqpID;
                    dRow[EqpArrProductAnalViewData.STEP_DESC] = stepDesc;
                    dRow[EqpArrProductAnalViewData.PRODUCT_ID] = dic.Key;
                    dRow[EqpArrProductAnalViewData.PRODUCT_VERSION] = prodVer;

                    dt.Rows.Add(dRow);
                }
            }

            SimpleGridPopUp simpleGrid = new SimpleGridPopUp("EqpArrange/Product", dt);
            simpleGrid.SetFormSize(700, 530);
            simpleGrid.SetFooter(true);

            simpleGrid.Show();
            simpleGrid.Focus();
        }
    }
}
