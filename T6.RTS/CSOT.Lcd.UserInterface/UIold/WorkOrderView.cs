using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraPivotGrid;
using DevExpress.Spreadsheet;
using DevExpress.XtraSpreadsheet;

using Mozart.Studio.UIComponents;
using Mozart.Studio.TaskModel.Utility;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserInterface;

using CSOT.Lcd.Scheduling;

using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class WorkOrderView : XtraPivotGridControlView
    {
        private IExperimentResultItem _result;
        private DateTime _planStartTime;

        private Dictionary<string, string> _eqpGrpInEqpDic;
        private Dictionary<string, List<string>> _eqpListbyEqpGrpDic;

        private Dictionary<string, List<string>> _eqpGrpsInAreaDic;

        private List<string> _selectedEqpGrpList;

        private Dictionary<string, List<WoData.WorkOrder>> _totalWoListDic;

        private DataTable _workOrderDt;

        private bool _isEndLoadDocument = false;
        
        public WorkOrderView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }
        
        private string SelectedShopID
        {
            get { return this.shopIdComboBoxEdit.SelectedItem != null ? this.shopIdComboBoxEdit.SelectedItem.ToString() : "Blank"; }
        }

        private bool FilterBySelectedAreaOrEqpGrp
        {
            get
            {
                bool useArea = GetUseAreaControl();

                bool useEqpGroup = GetUseEqpGroupControl();

                return useArea || useEqpGroup;
            }
        }

        List<string> SelectedEqpGroups
        {
            get
            {
                if (_selectedEqpGrpList == null)
                    _selectedEqpGrpList = new List<string>();
                else
                    return _selectedEqpGrpList;

                bool useArea = GetUseAreaControl();

                bool useEqpGroup = GetUseEqpGroupControl();

                List<string> seleInEqpGrpBoxList = new List<string>();
                foreach (var item in eqpGroupsCheckedBox.CheckedItems)
                    seleInEqpGrpBoxList.Add(item.ToString());

                if (useArea)
                {
                    foreach (string area in this.areaChkBoxEdit.Properties.Items.GetCheckedValues())
                    {
                        List<string> eqpGrps;
                        if (_eqpGrpsInAreaDic.TryGetValue(area, out eqpGrps) == false)
                            continue;

                        if (useEqpGroup)
                            eqpGrps = eqpGrps.Where(x => seleInEqpGrpBoxList.Contains(x.ToString())).ToList();

                        _selectedEqpGrpList.AddRange(eqpGrps);
                    }
                }
                else if (useEqpGroup)
                {
                    _selectedEqpGrpList = seleInEqpGrpBoxList;
                }
                else
                {
                    foreach (var eqpGrp in eqpGroupsCheckedBox.Items)
                        _selectedEqpGrpList.Add(eqpGrp.ToString());
                }

                return _selectedEqpGrpList;
            }
        }

        private bool GetUseAreaControl()
        {
            bool useArea = false;

            int totalCnt = this.areaChkBoxEdit.Properties.Items.Count;

            //if (totalCnt > 0)
            //    useArea = true;

            int selectedCount = this.areaChkBoxEdit.Properties.Items.GetCheckedValues().Count;
            if (selectedCount > 0 && selectedCount < totalCnt)
                useArea = true;

            return useArea;
        }

        private bool GetUseEqpGroupControl()
        {
            bool useEqpGroup = false;

            int totalCnt2 = this.eqpGroupsCheckedBox.Items.Count;

            //if (totalCnt2 > 0)
            //    useEqpGroup = true;

            int selectedCount = this.eqpGroupsCheckedBox.CheckedItems.Count;
            if (selectedCount > 0 && selectedCount < totalCnt2)
                useEqpGroup = true;

            return useEqpGroup;
        }

        private bool IsAllEqpGrpSelected
        {
            get
            {
                int totalCnt = this.eqpGroupsCheckedBox.Items.Count;
                if (totalCnt <= 0)
                    return true;

                if (totalCnt == this.eqpGroupsCheckedBox.CheckedItems.Count)
                    return true;

                return false;
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

        protected override void LoadDocument()
        {
            InitializeBase();

            InitializeControl();

            //InitializeData();

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
            var modelContext = this._result.GetCtx<ModelDataContext>();

            // ShopID ComboBox
            ComboHelper.AddDataToComboBox(this.shopIdComboBoxEdit, _result,
                SimInputData.InputName.StdStep, SimInputData.StdStepSchema.SHOP_ID, false);

            if (this.shopIdComboBoxEdit.Properties.Items.Contains("ARRAY"))
                this.shopIdComboBoxEdit.SelectedIndex = this.shopIdComboBoxEdit.Properties.Items.IndexOf("ARRAY");

            // Area CheckComboBox
            _eqpGrpsInAreaDic = new Dictionary<string, List<string>>();

            string filter = string.Format("{0} = '{1}'", SimInputData.ConstSchema.CATEGORY, "AREA_INFO");
            DataTable dtConst = _result.LoadInput(SimInputData.InputName.Const, filter);
            if (dtConst != null)
            {
                List<string> eqpGrpsAllInAreaList = new List<string>();

                foreach (DataRow drow in dtConst.Rows)
                {
                    SimInputData.Const configConst = new SimInputData.Const(drow);

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
                    }
                }

                if (this.areaChkBoxEdit.Properties.Items.Contains("OTHERS") == false)
                    this.areaChkBoxEdit.Properties.Items.Add("OTHERS");

                var eqpGrpInEqpList = modelContext.Eqp.Select(x => x.DSP_EQP_GROUP_ID).Distinct();
                foreach (var eqpGrp in eqpGrpInEqpList)
                {
                    if (eqpGrpsAllInAreaList.Contains(eqpGrp) == false)
                    {
                        List<string> eqpGrpList;
                        if (_eqpGrpsInAreaDic.TryGetValue("OTHERS", out eqpGrpList) == false)
                            _eqpGrpsInAreaDic.Add("OTHERS", eqpGrpList = new List<string>());

                        if (eqpGrpList.Contains(eqpGrp) == false)
                            eqpGrpList.Add(eqpGrp);
                    }
                }
            }

            if (this.areaChkBoxEdit.Properties.Items.Count > 0)
                this.areaChkBoxEdit.CheckAll();

            _eqpGrpInEqpDic = new Dictionary<string, string>();
            _eqpListbyEqpGrpDic = new Dictionary<string, List<string>>();

            foreach (var row in modelContext.Eqp)
            {
                if (_eqpGrpInEqpDic.ContainsKey(row.SHOP_ID + row.EQP_ID) == false)
                    _eqpGrpInEqpDic.Add(row.SHOP_ID + row.EQP_ID, row.DSP_EQP_GROUP_ID);

                List<string> eqpList;
                if (_eqpListbyEqpGrpDic.TryGetValue(row.SHOP_ID + row.DSP_EQP_GROUP_ID, out eqpList) == false)
                    _eqpListbyEqpGrpDic.Add(row.SHOP_ID + row.DSP_EQP_GROUP_ID, eqpList = new List<string>());

                if (eqpList.Contains(row.EQP_ID) == false)
                    eqpList.Add(row.EQP_ID);
            }

            // EqpGroup CheckComboBox
            SetEqpGroupCheckBox();

            //DateEdit Controls
            this.fromDateEdit.DateTime = ShopCalendar.SplitDate(_planStartTime);
            ComboHelper.ShiftName(this.shiftComboBoxEdit, _planStartTime);

            dayShiftSpinEdit.Value = 1; // _result.GetPlanPeriod(1);
        }

        private void SetEqpGroupCheckBox()
        {
            // EqpGroup CheckComboBox

            bool useArea = GetUseAreaControl();

            List<string> eqpGroupInAreaList = new List<string>();
            if (useArea)
            {
                foreach (string area in this.areaChkBoxEdit.Properties.Items.GetCheckedValues())
                {
                    List<string> eqpGrps;
                    if (_eqpGrpsInAreaDic.TryGetValue(area, out eqpGrps) == false)
                        continue;

                    eqpGroupInAreaList.AddRange(eqpGrps);
                }
            }

            this.eqpGroupsCheckedBox.Items.Clear();
            //ICollection<string> eqpGroupList = ComboHelper.Distinct(dtEqpDispatchLog, SimResultData.EqpDispatchLog.Schema.EQP_GROUP, filter);

            string filter = Globals.CreateFilter(string.Empty, SimResultData.EqpDispatchLog.Schema.SHOP_ID, "=", this.SelectedShopID);
            DataTable dtEqpDispatchLog = _result.LoadOutput(SimResultData.OutputName.EqpDispatchLog, filter);

            List<string> eqpGroupList = new List<string>();

            foreach (DataRow row in dtEqpDispatchLog.Rows)
            {
                SimResultData.EqpDispatchLog info = new SimResultData.EqpDispatchLog(row);

                string eqpGroupInEqp;
                if (_eqpGrpInEqpDic.TryGetValue(info.ShopID + info.EqpID, out eqpGroupInEqp) == false)
                    continue;

                if (useArea)
                {
                    if (eqpGroupInAreaList.Contains(eqpGroupInEqp) == false)
                        continue;
                }

                // if (this.eqpGroupsCheckedBox.Items.Contains(eqpGroupInEqp) == false) 이거 이거 안먹음
                if (eqpGroupList.Contains(eqpGroupInEqp) == false)
                {
                    eqpGroupList.Add(eqpGroupInEqp);
                    this.eqpGroupsCheckedBox.Items.Add(eqpGroupInEqp);
                }
            }
        }

        private void InitializeData()
        {
            var modelContext = this._result.GetCtx<ModelDataContext>();

            _totalWoListDic = new Dictionary<string, List<WoData.WorkOrder>>();

            string filter = Globals.CreateFilter(string.Empty, SimResultData.EqpPlan.Schema.SHOP_ID, "=", this.SelectedShopID);
            filter = Globals.CreateFilter(filter, SimResultData.EqpPlan.Schema.EQP_ID, "<>", Consts.NULL_ID, "AND");
            filter = Globals.CreateFilter(filter, SimResultData.EqpPlan.Schema.START_TIME, ">=", this.FromTime.ToString(), "AND");
            filter = Globals.CreateFilter(filter, SimResultData.EqpPlan.Schema.START_TIME, "<", this.ToTime.ToString(), "AND");

            DataTable dtEqpPlan = _result.LoadOutput(SimResultData.OutputName.EqpPlan, filter);
            
            string sorter = SimResultData.EqpPlan.Schema.SHOP_ID + "," + SimResultData.EqpPlan.Schema.EQP_ID + ","
                + SimResultData.EqpPlan.Schema.START_TIME + "," + SimResultData.EqpPlan.Schema.PRODUCT_ID + ","
                + SimResultData.EqpPlan.Schema.LAYER_ID + "," + SimResultData.EqpPlan.Schema.STEP_ID;

            DataView eqpPlanView = new DataView(dtEqpPlan, "", sorter, DataViewRowState.CurrentRows);
            dtEqpPlan = eqpPlanView.ToTable();

            int totalRowCnt = dtEqpPlan.Rows.Count;
            bool isLast = false;

            WoData.WorkOrder workOrder = new WoData.WorkOrder();

            int rowCnt = 0;
            string preShopEqp = string.Empty;
            string preShopEqpProd = string.Empty;
            string preStepInfo = string.Empty;
            DateTime preEndTime = DateTime.MaxValue;
            foreach (DataRow row in dtEqpPlan.Rows)
            {
                rowCnt++;
                SimResultData.EqpPlan eqpPlan = new SimResultData.EqpPlan(row);
                
                if (eqpPlan.EqpID == "8APPH13")
                {
                }

                string shift = ShopCalendar.ClassifyShift(eqpPlan.StartTime) == 1 ? WoData.Const.DAY : WoData.Const.NIGHT;

                if (rowCnt == 1)
                {
                    string eqpGrpIdInEqp;
                    if (_eqpGrpInEqpDic.TryGetValue(eqpPlan.ShopID + eqpPlan.EqpID, out eqpGrpIdInEqp) == false)
                        eqpGrpIdInEqp = string.Empty;

                    workOrder.SetBaseInfo(eqpPlan.ShopID, eqpPlan.EqpID, eqpPlan.EqpGroupID, eqpGrpIdInEqp, eqpPlan.ProductID, shift, eqpPlan.StartTime,
                        eqpPlan.LayerID, eqpPlan.StepID);
                }

                string shopEqp = eqpPlan.ShopID + eqpPlan.EqpID;
                string shopEqpProd = eqpPlan.ShopID + eqpPlan.EqpID + eqpPlan.ProductID;
                string stepInfo = eqpPlan.LayerID + eqpPlan.StepID;
                
                bool isNew = false;
                if (shopEqp != preShopEqp)
                    isNew = true;
                else if (shopEqpProd != preShopEqpProd)
                    isNew = true;
                else if (eqpPlan.StartTime > preEndTime)
                    isNew = true;
                else if (preStepInfo != stepInfo)
                    isNew = true;

                if (rowCnt == totalRowCnt)
                    isLast = true;

                bool savePreVal = false;
                bool newWorkOrder = false;
                
                if ((isNew == false && isLast == false) || rowCnt == 1)
                {
                    workOrder.AddQty(eqpPlan.Qty);
                }
                else if (isNew && isLast == false)
                {
                    savePreVal = true;  // 여태까지꺼 rlst에 기록
                    newWorkOrder = true;    // workOrder 새로 만들기 + Qty추가
                }
                else if (isNew == false && isLast)
                {
                    savePreVal = true;
                    workOrder.AddQty(eqpPlan.Qty);
                }
                else if (isNew && isLast)
                {
                    savePreVal = true;  // 여태까지꺼 rlst에 기록
                    newWorkOrder = true;    // workOrder 새로 만들고 + Qty추가하고 + rlst에 기록
                }

                if (savePreVal)
                {
                    List<WoData.WorkOrder> woList;
                    if (_totalWoListDic.TryGetValue(preShopEqp, out woList) == false)
                        _totalWoListDic.Add(preShopEqp, woList = new List<WoData.WorkOrder>());

                    woList.Add(workOrder);
                }

                if (newWorkOrder)
                {
                    string eqpGrpIdInEqp;
                    if (_eqpGrpInEqpDic.TryGetValue(eqpPlan.ShopID + eqpPlan.EqpID, out eqpGrpIdInEqp) == false)
                        eqpGrpIdInEqp = string.Empty;

                    workOrder = new WoData.WorkOrder();
                    workOrder.SetBaseInfo(eqpPlan.ShopID, eqpPlan.EqpID, eqpPlan.EqpGroupID, eqpGrpIdInEqp, eqpPlan.ProductID, shift, eqpPlan.StartTime,
                        eqpPlan.LayerID, eqpPlan.StepID);
                    workOrder.AddQty(eqpPlan.Qty);

                    if (isLast)
                    {
                        List<WoData.WorkOrder> woList;
                        if (_totalWoListDic.TryGetValue(shopEqp, out woList) == false)
                            _totalWoListDic.Add(shopEqp, woList = new List<WoData.WorkOrder>());

                        woList.Add(workOrder);
                    }
                }

                preShopEqp = shopEqp;
                preShopEqpProd = shopEqpProd;
                preEndTime = eqpPlan.EndTime;
                preStepInfo = stepInfo;
            }            
        }

        private void Query()
        {
            ProcessData();
            
            //XtraPivotGridHelper.DataViewTable dt = CreateDataViewTable();

            FillSpreadSheet();

            //FillPivotGrid(dt);

            //DrawPivotGrid(dt);
        }

        private void ProcessData()
        {
            InitializeData();

            CreateWorkOrderTable();
            
            foreach (string seleEqpGrpID in this.SelectedEqpGroups)
            {
                List<string> eqpList;
                if (_eqpListbyEqpGrpDic.TryGetValue(this.SelectedShopID + seleEqpGrpID, out eqpList) == false)
                    continue;

                eqpList.Sort();

                foreach (string eqpID in eqpList)
                {
                    List<WoData.WorkOrder> woList;
                    if (_totalWoListDic.TryGetValue(this.SelectedShopID + eqpID, out woList) == false)
                        continue;

                    foreach (WoData.WorkOrder wo in woList)
                    {
                        if (wo.Qty <= 0)
                            continue;
                                                                        
                        DataRow row = _workOrderDt.NewRow();

                        if (eqpID == "8APPH13")
                        {
                        }

                        row[WoData.Schema.SHOP_ID] = wo.ShopID;
                        row[WoData.Schema.EQP_GROUP_ID] = wo.EqpGroupIdInEqp;
                        row[WoData.Schema.EQP_ID] = wo.EqpID;
                        row[WoData.Schema.PRODUCT_ID] = wo.ProductID;
                        row[WoData.Schema.START_TIME] = wo.Shift == WoData.Const.DAY ? wo.StartTimeString : string.Empty;
                        row[WoData.Schema.LAYER] = wo.Shift == WoData.Const.DAY ? wo.Layer : string.Empty;
                        row[WoData.Schema.STEP_ID] = wo.Shift == WoData.Const.DAY ? wo.StepID : string.Empty;
                        row[WoData.Schema.QTY] = wo.Shift == WoData.Const.DAY ? wo.Qty.ToString() : string.Empty;
                        row[WoData.Schema.START_TIME2] = wo.Shift == WoData.Const.NIGHT ? wo.StartTimeString : string.Empty;
                        row[WoData.Schema.LAYER2] = wo.Shift == WoData.Const.NIGHT ? wo.Layer : string.Empty;
                        row[WoData.Schema.STEP_ID2] = wo.Shift == WoData.Const.NIGHT ? wo.StepID : string.Empty;
                        row[WoData.Schema.QTY2] = wo.Shift == WoData.Const.NIGHT ? wo.Qty.ToString() : string.Empty;

                        _workOrderDt.Rows.Add(row);
                    }
                }
            }
        }

        private void CreateWorkOrderTable()
        {
            _workOrderDt = null;
            _workOrderDt = new DataTable();

            _workOrderDt.Columns.Add(WoData.Schema.SHOP_ID, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.EQP_GROUP_ID, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.EQP_ID, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.PRODUCT_ID, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.START_TIME, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.LAYER, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.STEP_ID, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.QTY, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.START_TIME2, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.LAYER2, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.STEP_ID2, typeof(string));
            _workOrderDt.Columns.Add(WoData.Schema.QTY2, typeof(string));
        }

        private void FillSpreadSheet()
        {
            this.spreadsheetControl1.BeginUpdate();

            //Worksheet worksheet = this.spreadsheetControl1.ActiveWorksheet;


            Worksheet worksheet = spreadsheetControl1.Document.Worksheets[0];

            worksheet.Clear(worksheet["A:L"]);
                                                
            worksheet.Rows[0][0].SetValue(WoData.Schema.SHOP_ID);
            worksheet.Rows[0][1].SetValue(WoData.Schema.EQP_GROUP_ID);
            worksheet.Rows[0][2].SetValue(WoData.Schema.EQP_ID);
            worksheet.Rows[0][3].SetValue(WoData.Schema.PRODUCT_ID);
            worksheet.Rows[0][4].SetValue(WoData.Schema.DAY);
            worksheet.Rows[0][8].SetValue(WoData.Schema.NIGHT);
            
            worksheet.MergeRows(0, 1, 0);   // SHOP
            worksheet.MergeRows(0, 1, 1);   // EQP_GROUP_ID
            worksheet.MergeRows(0, 1, 2);   // EQP_ID
            worksheet.MergeRows(0, 1, 3);   // PRODUCT_ID
            worksheet.MergeCells(worksheet["E1:H1"]);
            worksheet.MergeCells(worksheet["I1:L1"]);

            worksheet.Rows[0][0].SetValue(WoData.Schema.SHOP_ID);
            worksheet.Rows[0][1].SetValue(WoData.Schema.EQP_GROUP_ID);
            worksheet.Rows[1][2].SetValue(WoData.Schema.EQP_ID);
            worksheet.Rows[1][3].SetValue(WoData.Schema.PRODUCT_ID);
            worksheet.Rows[1][4].SetValue(WoData.Schema.START_TIME);
            worksheet.Rows[1][5].SetValue(WoData.Schema.LAYER);
            worksheet.Rows[1][6].SetValue(WoData.Schema.STEP_ID);
            worksheet.Rows[1][7].SetValue(WoData.Schema.QTY);
            worksheet.Rows[1][8].SetValue(WoData.Schema.START_TIME);
            worksheet.Rows[1][9].SetValue(WoData.Schema.LAYER);
            worksheet.Rows[1][10].SetValue(WoData.Schema.STEP_ID);
            worksheet.Rows[1][11].SetValue(WoData.Schema.QTY);

            worksheet.Range["A1:D2"].FillColor = Color.LightGreen;
            worksheet.Range["E1:H2"].FillColor = Color.MistyRose;
            worksheet.Range["I1:L2"].FillColor = Color.SteelBlue;

            worksheet.FreezeRows(1);

            //worksheet.Import(_workOrderDt, false, 2, 0);
            
            string preShopID = string.Empty;
            string preEqpGrpID = string.Empty;
            string preEqpID = string.Empty;
            string preProductID = string.Empty;

            Color colorByShop = Color.MintCream;
            Color colorByEqpGrp = Color.AliceBlue;
            Color colorByEqp = Color.AliceBlue;
            Color colorByProd = Color.MintCream;
            
            int totalCnt = _workOrderDt.Rows.Count;

            #region FillColor, SetBorders 너무 느려
            //var usingCells = worksheet.Cells.Where(x => x.ColumnIndex <= 11 && x.RowIndex <= 2 + totalCnt - 1);

            //bool isEqpChanged = false;

            //foreach (Cell cell in usingCells)
            //{
            //    if (cell.RowIndex < 2)
            //    {
            //        if (cell.Value.TextValue == WoData.Schema.SHOP_ID || cell.Value.TextValue == WoData.Schema.EQP_GROUP_ID
            //            || cell.Value.TextValue == WoData.Schema.EQP_ID || cell.Value.TextValue == WoData.Schema.PRODUCT_ID)
            //            cell.FillColor = Color.CadetBlue;
            //        else if (cell.Value.TextValue == WoData.Schema.DAY
            //            || cell.Value.TextValue == WoData.Schema.START_TIME || cell.Value.TextValue == WoData.Schema.LAYER
            //            || cell.Value.TextValue == WoData.Schema.STEP_ID || cell.Value.TextValue == WoData.Schema.QTY)
            //            cell.FillColor = Color.MistyRose;

            //        else if (cell.Value.TextValue == WoData.Schema.NIGHT
            //            || cell.Value.TextValue == WoData.Schema.START_TIME2 || cell.Value.TextValue == WoData.Schema.LAYER2
            //            || cell.Value.TextValue == WoData.Schema.STEP_ID2 || cell.Value.TextValue == WoData.Schema.QTY2)
            //            cell.FillColor = Color.SteelBlue;

            //        cell.Borders.SetAllBorders(Color.Black, BorderLineStyle.Thick);

            //        continue;
            //    }

            //    if (cell.ColumnIndex > 11)
            //        continue;

            //    if (cell.RowIndex > 2 + totalCnt - 1)
            //        break;

            //    if (cell.RowIndex == 2 + totalCnt - 1)
            //    {
            //        cell.Borders.BottomBorder.Color = Color.Black;
            //        cell.Borders.BottomBorder.LineStyle = BorderLineStyle.Thick;
            //    }

            //    string cellValue = cell.Value.TextValue;

            //    if (cell.ColumnIndex == 0)  // Shop
            //    {
            //        cell.Borders.SetAllBorders(Color.Black, BorderLineStyle.Thick);

            //        if (cell.RowIndex != 2 && preShopID != cellValue)
            //            colorByShop = colorByShop == Color.MintCream ? Color.White : Color.MintCream;

            //        cell.FillColor = colorByShop;
            //    }
            //    else if (cell.ColumnIndex == 1)  // EqpGrp
            //    {
            //        cell.Borders.SetAllBorders(Color.Black, BorderLineStyle.Thick);

            //        if (cell.RowIndex != 2 && preEqpGrpID != cellValue)
            //            colorByEqpGrp = colorByEqpGrp == Color.MintCream ? Color.White : Color.MintCream;

            //        cell.FillColor = colorByEqpGrp;
            //    }
            //    else if (cell.ColumnIndex == 2)  // Eqp
            //    {
            //        cell.Borders.LeftBorder.Color = Color.Black;
            //        cell.Borders.LeftBorder.LineStyle = BorderLineStyle.Thick;
            //        cell.Borders.LeftBorder.Color = Color.Black;
            //        cell.Borders.LeftBorder.LineStyle = BorderLineStyle.Thick;
            //        cell.Borders.LeftBorder.Color = Color.Black;
            //        cell.Borders.LeftBorder.LineStyle = BorderLineStyle.Thick;

            //        if (cell.RowIndex != 2 && preEqpID != cellValue)
            //        {
            //            isEqpChanged = true;

            //            cell.Borders.TopBorder.Color = Color.Black;
            //            cell.Borders.TopBorder.LineStyle = BorderLineStyle.Thick;

            //            colorByEqp = colorByEqp == Color.MintCream ? Color.White : Color.MintCream;
            //        }
            //        else
            //        {
            //            isEqpChanged = false;
            //        }

            //        cell.FillColor = colorByEqp;
            //    }
            //    else if (cell.ColumnIndex == 3)  // ProdID
            //    {
            //        cell.Borders.RightBorder.Color = Color.Black;
            //        cell.Borders.RightBorder.LineStyle = BorderLineStyle.Thick;
            //        if (isEqpChanged)
            //        {
            //            cell.Borders.TopBorder.Color = Color.Black;
            //            cell.Borders.TopBorder.LineStyle = BorderLineStyle.Thick;
            //        }

            //        //if (cell.RowIndex != 2 && pre != cellValue)
            //        //    colorByShop = colorByShop == Color.MintCream ? Color.White : Color.MintCream;

            //        cell.FillColor = colorByEqp;
            //    }
            //    else
            //    {
            //        if (isEqpChanged)
            //        {
            //            cell.Borders.TopBorder.Color = Color.Black;
            //            cell.Borders.TopBorder.LineStyle = BorderLineStyle.Thick;
            //        }

            //        if (cell.ColumnIndex == 7)
            //        {
            //            cell.Borders.RightBorder.Color = Color.Black;
            //            cell.Borders.RightBorder.LineStyle = BorderLineStyle.Thick;
            //        }
            //        else if (cell.ColumnIndex == 11)
            //        {
            //            cell.Borders.RightBorder.Color = Color.Black;
            //            cell.Borders.RightBorder.LineStyle = BorderLineStyle.Thick;
            //        }

            //    }
            //}
            #endregion

            int curRow = 2; // 3번째 줄

            for (int i = 0; i < totalCnt; i++)
            {
                string shopID = worksheet.GetCellValue(0, curRow).TextValue;
                string eqpGrpID = worksheet.GetCellValue(1, curRow).TextValue;
                string eqpID = worksheet.GetCellValue(2, curRow).TextValue;
                string prodID = worksheet.GetCellValue(3, curRow).TextValue;
                
                if (colorByEqp != Color.White)
                    worksheet.PaintRowCells(curRow - 1, 3, colorByEqp);

                if (curRow != 2)
                {                    
                    if (preEqpID != eqpID)
                        colorByEqp = colorByEqp == Color.White ? Color.AliceBlue : Color.White;
                }

                preShopID = shopID;
                preEqpGrpID = eqpGrpID;
                preEqpID = eqpID;
                preProductID = prodID;
                curRow++;
            }

            curRow = 2; // 3번째 줄
            for (int i = 0; i < totalCnt; i++)
            {
                string eqpGrpID = worksheet.GetCellValue(1, curRow).TextValue;
                               
                worksheet.PaintRowCells(curRow - 1, 1, colorByEqpGrp);

                if (curRow != 2)
                {
                    if (preEqpGrpID != eqpGrpID)
                        colorByEqpGrp = colorByEqpGrp == Color.White ? Color.AliceBlue : Color.White;
                }

                preEqpGrpID = eqpGrpID;
                curRow++;
            }
            
            Console.Write("A");

            preShopID = string.Empty;
            int mergeStartRowByShop = 2;
            preEqpGrpID = string.Empty;
            int mergeStartRowByEqpGrp = 2;
            preEqpID = string.Empty;
            int mergeStartRowByEqp = 2;
            preProductID = string.Empty;
            int mergeStartRowByProd = 2;

            curRow = 2; // 3번째 줄
            for (int i = 0; i < totalCnt + 1; i++)
            {
                string shopID = worksheet.GetCellValue(0, curRow).TextValue;
                string eqpGrpID = worksheet.GetCellValue(1, curRow).TextValue;
                string eqpID = worksheet.GetCellValue(2, curRow).TextValue;
                string prodID = worksheet.GetCellValue(3, curRow).TextValue;

                bool isShopMerge = false;
                bool isEqpGrpMerge = false;
                bool isEqpMerge = false;
                bool isProdMerge = false;

                if (i != 0 && preShopID != shopID)
                {
                    isShopMerge = true;
                    isEqpGrpMerge = true;
                    isEqpMerge = true;
                    isProdMerge = true;
                }
                if (i != 0 && preEqpGrpID != eqpGrpID)
                {
                    isEqpGrpMerge = true;
                    isEqpMerge = true;
                    isProdMerge = true;
                }
                if (i != 0 && preEqpID != eqpID)
                {
                    isEqpMerge = true;
                    isProdMerge = true;
                }
                if (i != 0 && preProductID != prodID)
                {
                    isProdMerge = true;
                }

                if (isShopMerge)
                {
                    if (mergeStartRowByShop != curRow - 1)
                        worksheet.MergeRowsOneColumn(mergeStartRowByShop, curRow - 1, 0);

                    mergeStartRowByShop = curRow;
                }
                if (isEqpGrpMerge)
                {
                    if (mergeStartRowByEqpGrp != curRow - 1)
                        worksheet.MergeRowsOneColumn(mergeStartRowByEqpGrp, curRow - 1, 1);

                    mergeStartRowByEqpGrp = curRow;
                }
                if (isEqpMerge)
                {
                    if (mergeStartRowByEqp != curRow - 1)
                        worksheet.MergeRowsOneColumn(mergeStartRowByEqp, curRow - 1, 2);

                    mergeStartRowByEqp = curRow;
                }
                if (isProdMerge)
                {
                    if (mergeStartRowByProd != curRow - 1)
                        worksheet.MergeRowsOneColumn(mergeStartRowByProd, curRow - 1, 3);

                    mergeStartRowByProd = curRow;
                }

                preShopID = shopID;
                preEqpGrpID = eqpGrpID;
                preEqpID = eqpID;
                preProductID = prodID;
                curRow++;
            }
                        
            for (int i = 0; i < 12; i++)
            {
                if (i == 4 || i == 7 || i == 8 || i == 11)
                    worksheet.Columns[i].Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                else
                    worksheet.Columns[i].Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            }

            worksheet.Cells.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            worksheet.Rows[0].Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            worksheet.Rows[1].Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            worksheet.Rows[0].Font.Bold = true;
            worksheet.Rows[1].Font.Bold = true;
            
            curRow = 2; // 3번째 줄
            for (int i = 0; i < totalCnt + 2; i++)
            {
                for (int j = 0; j < 12; j++)
                    worksheet.Cells[i, j].Borders.SetAllBorders(Color.Black, BorderLineStyle.Thin);
            }

            worksheet.ResizeColumnToFit(0, 6);
            worksheet.ResizeColumnToFit(8, 10);

            Console.Write("A");

            this.spreadsheetControl1.EndUpdate();
        }

        //private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        //{
        //    XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();

        //    dt.AddColumn(WoData.Schema.SHOP_ID, WoData.Schema.SHOP_ID, typeof(string), PivotArea.RowArea, null, null);
        //    dt.AddColumn(WoData.Schema.EQP_GROUP_ID, WoData.Schema.EQP_GROUP_ID, typeof(string), PivotArea.RowArea, null, null);
        //    dt.AddColumn(WoData.Schema.EQP_ID, WoData.Schema.EQP_ID, typeof(string), PivotArea.RowArea, null, null);
        //    dt.AddColumn(WoData.Schema.PRODUCT_ID, WoData.Schema.PRODUCT_ID, typeof(string), PivotArea.RowArea, null, null);
        //    dt.AddColumn(WoData.Schema.SHIFT, WoData.Schema.SHIFT, typeof(string), PivotArea.ColumnArea, null, null);
        //    dt.AddColumn(WoData.Schema.WORK_ORDER, WoData.Schema.WORK_ORDER, typeof(string), PivotArea.ColumnArea, null, null);
        //    dt.AddColumn(WoData.Schema.VALUE, WoData.Schema.VALUE, typeof(string), PivotArea.DataArea, null, null);

        //    //dt.AddDataTablePrimaryKey(
        //    //        new DataColumn[]
        //    //        {
        //    //            dt.Columns[EqpArrProductAnalViewData.SHOP_ID],
        //    //            dt.Columns[EqpArrProductAnalViewData.EQP_ID],
        //    //            dt.Columns[EqpArrProductAnalViewData.STEP_ID],
        //    //            dt.Columns[EqpArrProductAnalViewData.STEP_DESC],
        //    //            dt.Columns[EqpArrProductAnalViewData.PROD_QTY]
        //    //        }
        //    //    );

        //    return dt;
        //}
        
        //private void FillPivotGrid(XtraPivotGridHelper.DataViewTable dt)
        //{
        //    foreach (WoData.WorkOrderRslt result in _workOrderRsltList2)
        //    {
        //        DataRow dRow = dt.DataTable.NewRow();

        //        dRow[WoData.Schema.SHOP_ID] = result.ShopID;
        //        dRow[WoData.Schema.EQP_GROUP_ID] = result.EqpGroupID;
        //        dRow[WoData.Schema.EQP_ID] = result.EqpID;
        //        dRow[WoData.Schema.PRODUCT_ID] = result.ProductID;
        //        dRow[WoData.Schema.SHIFT] = result.Shift;
        //        dRow[WoData.Schema.WORK_ORDER] = result.Category;
        //        dRow[WoData.Schema.VALUE] = result.Value;

        //        dt.DataTable.Rows.Add(dRow);
        //    }
        //}

        //private void DrawPivotGrid(XtraPivotGridHelper.DataViewTable dt)
        //{
        //    this.pivotGridControl1.BeginUpdate();

        //    this.pivotGridControl1.ClearPivotGridFields();
        //    this.pivotGridControl1.CreatePivotGridFields(dt);

        //    this.pivotGridControl1.DataSource = dt.DataTable;

        //    this.pivotGridControl1.OptionsView.ShowRowTotals = false;
        //    this.pivotGridControl1.OptionsView.ShowRowGrandTotals = false;
        //    this.pivotGridControl1.OptionsView.ShowColumnTotals = false;
        //    this.pivotGridControl1.OptionsView.ShowColumnGrandTotals = false;

        //    //this.pivotGridControl1.Fields[EuData.UTILIZATION].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
        //    //this.pivotGridControl1.Fields[EuData.UTILIZATION].CellFormat.FormatString = "###,##0.0";

        //    this.pivotGridControl1.EndUpdate();
        //}

        #region Event

        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            _selectedEqpGrpList = null;

            Query();

            _selectedEqpGrpList = null;

            this.Cursor = Cursors.Default;
        }

        private void btnExcelExport_Click(object sender, EventArgs e)
        {
            string fileName = _planStartTime.ToString("yyyyMMddHHmmss") + "_" + _result.Name + "_" + "WorkOrder";

            Mozart.Studio.TaskModel.UserLibrary.XtraSheetHelper.ExportToXls(this.spreadsheetControl1.ActiveWorksheet, fileName);
        }

        private void shopIdComboBoxEdit_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_isEndLoadDocument == false)
                return;

            SetEqpGroupCheckBox();
        }

        private void areaChkBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (_isEndLoadDocument == false)
                return;

            SetEqpGroupCheckBox();
        }

        #endregion Event

        //protected override bool UpdateCommand(Command command)
        //{
        //    bool handled = false;
        //    if (command.CommandGroup == typeof(TaskCommands))
        //    {
        //        switch (command.CommandID)
        //        {
        //            case TaskCommands:
        //                command.Enabled = true;
        //                handled = true;
        //                break;
        //        }
        //    }

        //    if (handled) return true;
        //    return base.UpdateCommand(command);
        //}

        //protected override bool HandleCommand(Command command)
        //{
        //    bool handled = false;

        //    if (command.CommandGroup == typeof(TaskCommands))
        //    {
        //        switch (command.CommandID)
        //        {
        //            //case TaskCommands.DataExportToExcel:
        //            //    {
        //            //        string filename = SfGridHelper.GetXlsFileName(_result.Path, _pageID);
        //            //        SfGridHelper.ExportToXls(this.grid, filename);
        //            //        handled = true;
        //            //        break;
        //            //    }
        //        }
        //    }
        //    if (handled) return true;
        //    return base.HandleCommand(command);
        //}
    }
}
