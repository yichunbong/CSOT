using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraPivotGrid;

using Mozart.Collections;
//using Mozart.Studio.UIComponents;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
//using Mozart.Studio.TaskModel.UserInterface;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;


namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class StepMovePopUp : Form 
    {
        IExperimentResultItem _result;

        //private DoubleDictionary<string, string, SmcvData.Wip> _wipQtyAllDDic;

        //private Dictionary<string, SmcvData.StepInfo> _mainStepDic;
        //private Dictionary<string, SmcvData.Wip> _wipByProdDic;
        //private Dictionary<string, int> _simMoveDic;
        //private Dictionary<string, int> _actMoveDic;
        //private Dictionary<string, int> _wipQtyDic;
        //private DoubleDictionary<string, string, SmcvData.MoveInfo> _moveGridDDic;
        
        //private List<string> _selectedProductList;

        private Dictionary<string, SmcvData.StepInfo> _stepInfoDic;
        private List<string> _maindatoryStepList;

        DataTable _eqpInfoDT;

        private Dictionary<string, SmcvData.WipDetail> _wipCurStepDic;
        private Dictionary<string, SmcvData.WipDetail> _wipMainStepDic;

        private Dictionary<string, string> _eqpGrpDic;

        private Dictionary<string, List<string>> _eqpGrpsInAreaDic;
        private List<string> _selectedEqpGrpInAreaList;

        // 추가
        private DataTable _dtStepMove;
        private DataTable _dtStepMoveAct;
        
        private List<DateTime> _dateRangeList;

        private Dictionary<string, SmcvData.ResultItem> _dict;
        
        string SelectedShopID
        {
            get { return this.shopIdComboBoxEdit.SelectedItem != null ? this.shopIdComboBoxEdit.SelectedItem.ToString() : "Blank"; }
        }

        private DateTime FromTime
        {
            get
            {
                DateTime dt = this.fromTimeEdit.DateTime;
                
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
                
        bool IsTimeConditionHour
        {
            get { return this.hourRadioButton.Checked; }
        }

        bool IsMainOnly
        {
            get { return this.mainOnlyCheckBox.Checked; }
        }

        bool IsMoveOver0
        {
            get { return this.moveOver0CheckBox.Checked; }
        }

        List<string> SelectedEqpGrpInAreaList
        {
            get
            {
                if (_selectedEqpGrpInAreaList != null)
                    return _selectedEqpGrpInAreaList;

                _selectedEqpGrpInAreaList = new List<string>();

                foreach (string area in this.areaChkBoxEdit.Properties.Items.GetCheckedValues())
                {
                    List<string> eqpGrpList;
                    if (_eqpGrpsInAreaDic.TryGetValue(area, out eqpGrpList) == false)
                        continue;

                    _selectedEqpGrpInAreaList.AddRange(eqpGrpList);
                }

                return _selectedEqpGrpInAreaList;
            }
        }

        bool IsAllAreaSelected
        {
            get
            {
                int totalCnt = this.areaChkBoxEdit.Properties.Items.Count;
                if (totalCnt <= 0)
                    return true;

                if (totalCnt == this.areaChkBoxEdit.Properties.Items.GetCheckedValues().Count)
                    return true;

                return false;
            }
        }

        private bool IsSelectedInputWip
        {
            get
            {
                return this.wipCondInputBtn.Checked;
            }
        }

        public StepMovePopUp(
            Dictionary<string, SmcvData.StepInfo> stepInfoDic,
            Dictionary<string, SmcvData.WipDetail> wipCurStepDic, Dictionary<string, SmcvData.WipDetail> wipMainStepDic,
            List<string> maindatoryStepList,
            bool isTimeConditionDay, bool isSelectedInputWip,
            DevExpress.XtraEditors.ComboBoxEdit paramShopIdComboBox,
            System.Windows.Forms.Label dayShiftLabel, DevExpress.XtraEditors.DateEdit paramFromDateEdit,
            IExperimentResultItem result,
            DevExpress.XtraEditors.SpinEdit paramDayShiftSpinEdit)
        {
            InitializeComponent();
            
            _stepInfoDic = stepInfoDic;
            _maindatoryStepList = maindatoryStepList;
            _wipCurStepDic = wipCurStepDic;
            _wipMainStepDic = wipMainStepDic;

            _result = result;

            this.fromTimeEdit.DateTime = paramFromDateEdit.DateTime;
            this.fromTimeEdit.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.fromTimeEdit.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            this.hourRadioButton.Checked = isTimeConditionDay;
            this.shiftRadioButton.Checked = isTimeConditionDay ? false : true;
            this.hourShiftRangeLabel.Text = dayShiftLabel.Text;

            this.wipCondInputBtn.Checked = isSelectedInputWip;
            this.wipCondSimTimeBtn.Checked = isSelectedInputWip ? false : true;

            foreach (string shopID in paramShopIdComboBox.Properties.Items)
                this.shopIdComboBoxEdit.Properties.Items.Add(shopID);
            this.shopIdComboBoxEdit.SelectedIndex = paramShopIdComboBox.SelectedIndex;
            
            this.hourShiftSpinEdit.Value = paramDayShiftSpinEdit.Value;

            this.mainOnlyCheckBox.Checked = true;

            var modelContext = this._result.GetCtx<ModelDataContext>();
            //this.GetConsInfo();
            this._eqpInfoDT = Globals.GetConsInfo(modelContext);
            
            _selectedEqpGrpInAreaList = null;
            _eqpGrpsInAreaDic = new Dictionary<string, List<string>>();

            //string filter = string.Format("{0} = '{1}'", SimInputData.ConstSchema.CATEGORY, "AREA_INFO");
            //DataRow[] drs = this._eqpInfoDT.Select(filter);
            
            //DataTable dtConst = _result.LoadInput(SimInputData.InputName.Const, filter);

            List<string> eqpGrpsAllInAreaList = new List<string>();

            foreach (DataRow drow in _eqpInfoDT.Rows)
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

            var eqpGrpInEqpList = modelContext.Eqp.Select(x => x.EQP_GROUP_ID).Distinct();
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


            if (this.areaChkBoxEdit.Properties.Items.Count > 0)
                this.areaChkBoxEdit.CheckAll();
            
            InitializeData();

            Query();
        }


        private void GetConsInfo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(EqpGanttChartData.Const.Schema.CATEGORY, typeof(string));
            dt.Columns.Add(EqpGanttChartData.Const.Schema.CODE, typeof(string));
            dt.Columns.Add(EqpGanttChartData.Const.Schema.DESCRIPTION, typeof(string));


            var modelContext = this._result.GetCtx<ModelDataContext>();
            var x = (from a in modelContext.Eqp
                     select new { SHOP_ID = a.SHOP_ID, LOCATION = a.LOCATION, DSP_EQP_GROUP_ID = a.DSP_EQP_GROUP_ID }).Distinct();

            foreach (var eqpinfo in x)
            {
                DataRow dRow = dt.NewRow();
                dRow[EqpGanttChartData.Const.Schema.CATEGORY] = "AREA_INFO";
                dRow[EqpGanttChartData.Const.Schema.CODE] = eqpinfo.LOCATION;
                dRow[EqpGanttChartData.Const.Schema.DESCRIPTION] = eqpinfo.DSP_EQP_GROUP_ID;
                dt.Rows.Add(dRow);
            }

            this._eqpInfoDT = dt;
        }
        
        private bool IsMainStep(string stepID)
        {
            if (_maindatoryStepList.Contains(stepID) == false)
                return false;

            if (stepID.Contains("00"))
                return true;

            return false;
        }
        
        private void InitializeData()
        {
            _dtStepMove = _result.LoadOutput(SimResultData.OutputName.StepMove);

            //_dtStepMoveAct = _result.LoadInput(SimInputData.InputName.StepMoveAct);

            _eqpGrpDic = new Dictionary<string, string>();

            DataTable dtEqp = _result.LoadInput(SimInputData.InputName.Eqp);

            foreach (DataRow dRow in dtEqp.Rows)
            {
                SimInputData.Eqp eqp = new SimInputData.Eqp(dRow);

                if (_eqpGrpDic.ContainsKey(eqp.ShopID + eqp.EqpID) == false)
                    _eqpGrpDic.Add(eqp.ShopID + eqp.EqpID, eqp.EqpGroupID);
            }
        }

        private void Query()
        {
            SetData();

            BindData();

            _selectedEqpGrpInAreaList = null;
        }

        public void SetData()
        {
            SetDateRanges();

            _dict = new Dictionary<string, SmcvData.ResultItem>();
            
            LoadSimData();

            //LoadActData();

            LoadWipData();
        }

        private void SetDateRanges()
        {
            _dateRangeList = new List<DateTime>();

            float hours = this.IsTimeConditionHour ? 24 : (int)ShopCalendar.ShiftHours;
            
            for (DateTime start = ShopCalendar.ShiftStartTimeOfDayT(this.FromTime);// this.FromTime;
                start < ShopCalendar.ShiftStartTimeOfDayT(this.ToTime);   //this.ToTime;
                start = start.AddHours(hours))
            {
                _dateRangeList.Add(start);
            }

            //if (/*this.shiftComboBoxEdit.SelectedIndex > 0 &&*/ this.ToTime != ShopCalendar.StartTimeOfDay(this.ToTime)
            //    && _dateRangeList.Contains(this.ToTime) == false)
            //{
            //    _dateRangeList.Add(this.ToTime);
            //}
        }

        private void LoadSimData()
        {            
            string shopID = this.SelectedShopID;

            if (shopID == Consts.ALL)
                shopID = string.Empty;

            foreach (DataRow row in _dtStepMove.Rows)
            {
                SimResultData.StepMoveInfo item = new SimResultData.StepMoveInfo(row);
                
                if (shopID != "" && item.ShopID != shopID)
                    continue;

                if (item.TargetDate < this.FromTime || item.TargetDate >= this.ToTime)
                    continue;

                string eqpGrpID = string.Empty;
                _eqpGrpDic.TryGetValue(item.ShopID + item.EqpID, out eqpGrpID);

                if (this.IsAllAreaSelected == false)
                {
                    if (this.SelectedEqpGrpInAreaList.Count <= 0)
                        continue;

                    if (string.IsNullOrEmpty(eqpGrpID))
                        continue;
                    
                    if (this.SelectedEqpGrpInAreaList.Contains(eqpGrpID) == false)
                        continue;
                }
                
                foreach (DateTime date in _dateRangeList)
                {
                    SmcvData.ResultItem padding;

                    string dateString = this.IsTimeConditionHour ? ShopCalendar.StartTimeOfDayT(date).ToString("yyyyMMdd") : date.DbToTimeString();

                    string k = item.ShopID + item.ProductID + item.ProductVersion + item.OwnerType + item.StepID + item.EqpID + dateString;
                    string k2 = item.ShopID + item.ProductID + item.ProductVersion + item.OwnerType + item.StepID;

                    int wipCurStep = 0;
                    int wipMainStep = 0;
                    if (_dict.TryGetValue(k, out padding) == false)
                    {
                        SmcvData.WipDetail wipDetail;
                        _wipCurStepDic.TryGetValue(k2, out wipDetail);
                        wipCurStep = wipDetail == null ? 0 : wipDetail.GlassQty;
                        wipDetail = null;
                        _wipMainStepDic.TryGetValue(k2, out wipDetail);
                        wipMainStep = wipDetail == null ? 0 : wipDetail.GlassQty;

                        if (k2 == "ARRAYB8A550QU5V501AASMP041500-00")
                        { }

                        SmcvData.StepInfo stepInfo;
                        _stepInfoDic.TryGetValue(item.ShopID + item.StepID, out stepInfo);

                        padding = new SmcvData.ResultItem(item.ShopID, item.ProductID, item.ProductVersion, item.OwnerType, item.StepID, item.EqpID, date);
                        padding.SetStepInfo(stepInfo);
                        
                        _dict.Add(k, padding);
                    }
                }

                SmcvData.ResultItem ri;

                DateTime modTargetDate = this.IsTimeConditionHour ? ShopCalendar.StartTimeOfDayT(item.TargetDate)
                    : ShopCalendar.ShiftStartTimeOfDayT(item.TargetDate);
                string curShopID = item.ShopID;
                string productID = item.ProductID;
                //string productVersion = item.ProductVersion;
                string productVersion = "-";
                string ownerType = item.OwnerType;
                string stepID = item.StepID;
                string eqpID = item.EqpID;
                
                string dateString2 = this.IsTimeConditionHour ? modTargetDate.ToString("yyyyMMdd") : modTargetDate.DbToTimeString();

                string key = curShopID + productID + productVersion + ownerType + stepID + eqpID + dateString2;

                if (_dict.TryGetValue(key, out ri) == false)
                {
                    continue;   // 있을 수 없음

                    //ri = new SmcvData.ResultItem(curShopID, productID, productVersion, ownerType, stepID, eqpID, modTargetDate);

                    //_dict.Add(key, ri);
                }

                ri.UpdateSimQty((int)item.InQty, (int)item.OutQty);
            }
        }

        private void LoadActData()
        {
            string shopID = this.SelectedShopID;

            if (shopID == Consts.ALL)
                shopID = string.Empty;

            foreach (DataRow row in _dtStepMoveAct.Rows)
            {
                SimInputData.StepMoveAct item = new SimInputData.StepMoveAct(row);

                if (shopID != "" && item.ShopID != shopID)
                    continue;

                if (item.TargetDate < this.FromTime || item.TargetDate >= this.ToTime)
                    continue;

                string eqpGrpID = string.Empty;
                _eqpGrpDic.TryGetValue(item.ShopID + item.EqpID, out eqpGrpID);

                if (this.IsAllAreaSelected == false)
                {
                    if (this.SelectedEqpGrpInAreaList.Count <= 0)
                        continue;

                    if (string.IsNullOrEmpty(eqpGrpID))
                        continue;

                    if (this.SelectedEqpGrpInAreaList.Contains(eqpGrpID) == false)
                        continue;
                }

                foreach (DateTime date in _dateRangeList)
                {
                    SmcvData.ResultItem padding;

                    string dateString = this.IsTimeConditionHour ? ShopCalendar.StartTimeOfDayT(date).ToString("yyyyMMdd") : date.DbToTimeString();

                    string k = item.ShopID + item.ProductID + Consts.NULL_ID + item.OwnerType + item.StepID + item.EqpID + dateString;
                    string k2 = item.ShopID + item.ProductID + Consts.NULL_ID + item.OwnerType + item.StepID;

                    int wipCurStep = 0;
                    int wipMainStep = 0;
                    if (_dict.TryGetValue(k, out padding) == false)
                    {
                        if (k2 == "ARRAYB8A550QU5V501AASMP041500-00")
                        { }

                        SmcvData.WipDetail wipDetail;
                        _wipCurStepDic.TryGetValue(k2, out wipDetail);
                        wipCurStep = wipDetail == null ? 0 : wipDetail.GlassQty;
                        wipDetail = null;
                        _wipMainStepDic.TryGetValue(k2, out wipDetail);
                        wipMainStep = wipDetail == null ? 0 : wipDetail.GlassQty;

                        SmcvData.StepInfo stepInfo;
                        _stepInfoDic.TryGetValue(item.ShopID + item.StepID, out stepInfo);

                        padding = new SmcvData.ResultItem(item.ShopID, item.ProductID, Consts.NULL_ID, item.OwnerType, item.StepID, item.EqpID, date);//, 0, 0);
                        padding.SetStepInfo(stepInfo);

                        _dict.Add(k, padding);
                    }
                }

                SmcvData.ResultItem ri;

                DateTime modTargetDate = this.IsTimeConditionHour ? ShopCalendar.StartTimeOfDayT(item.TargetDate)
                    : ShopCalendar.ShiftStartTimeOfDayT(item.TargetDate);
                string curShopID = item.ShopID;
                string productID = item.ProductID;
                string productVersion = Consts.NULL_ID;
                string ownerType = item.OwnerType;
                string stepID = item.StepID;
                string eqpID = item.EqpID;

                //DateTime shift = ShopCalendar.ShiftStartTimeOfDayT(modTargetDate);

                string dateString2 = this.IsTimeConditionHour ? modTargetDate.ToString("yyyyMMdd") : modTargetDate.DbToTimeString();

                string key = curShopID + productID + ownerType + stepID + eqpID + dateString2;

                if (_dict.TryGetValue(key, out ri) == false)
                {
                    continue;   // 있을 수 없음

                    //ri = new SmcvData.ResultItem(curShopID, productID, productVersion, ownerType, stepID, eqpID, modTargetDate);

                    //_dict.Add(key, ri);
                }

                ri.UpdateActQty((int)item.InQty, (int)item.OutQty);
            }
        }

        private void LoadWipData()
        {
            //Dictionary<string, SmcvData.WipDetail>
            var wipCurStepDic = _wipCurStepDic.ToDictionary(x => x.Key, y => y.Value);

            if (this.SelectedShopID == Consts.ALL && this.IsSelectedInputWip)
            {
                foreach (string shopID in this.shopIdComboBoxEdit.Properties.Items)
                {
                    if (shopID == Consts.ALL)
                        continue;

                    wipCurStepDic.Clear();

                    foreach (string key in _wipCurStepDic.Keys)
                    {
                        if (key.StartsWith(shopID) == false)
                            continue;

                        if (wipCurStepDic.ContainsKey(key) == false)
                            wipCurStepDic.Add(key, _wipCurStepDic[key]);
                    }
                }
            }
            else if (this.SelectedShopID == Consts.ALL && this.IsSelectedInputWip == false)
            {
                wipCurStepDic.Clear();

                string sFromTime = this.FromTime != ShopCalendar.ShiftStartTimeOfDayT(this.FromTime) ?
                            ShopCalendar.ShiftStartTimeOfDayT(this.FromTime).AddHours((double)ShopCalendar.ShiftHours).DbToString()
                            : this.FromTime.DbToString();

                wipCurStepDic = _wipCurStepDic.Where(x => x.Key.StartsWith(sFromTime))
                    .ToDictionary(x => x.Key, y => y.Value);
            }
            else if (this.SelectedShopID != Consts.ALL && this.IsSelectedInputWip)
            {
                wipCurStepDic.Clear();

                wipCurStepDic = _wipCurStepDic.Where(x => x.Key.StartsWith(this.SelectedShopID))
                    .ToDictionary(x => x.Key, y => y.Value);
            }
            else
            {
                wipCurStepDic.Clear();

                string sFromTime = this.FromTime != ShopCalendar.ShiftStartTimeOfDayT(this.FromTime) ?
                            ShopCalendar.ShiftStartTimeOfDayT(this.FromTime).AddHours((double)ShopCalendar.ShiftHours).DbToString()
                            : this.FromTime.DbToString();

                wipCurStepDic = _wipCurStepDic
                    .Where(x => x.Key.StartsWith(sFromTime + this.SelectedShopID))
                    .ToDictionary(x => x.Key, y => y.Value);
            }

           
            foreach (var wipCur in wipCurStepDic)
            {
                foreach (DateTime date in _dateRangeList)
                {
                    SmcvData.ResultItem padding;

                    string dateString = this.IsTimeConditionHour ? ShopCalendar.StartTimeOfDayT(date).ToString("yyyyMMdd") : date.DbToTimeString();

                    string k = wipCur.Value.ShopID + wipCur.Value.ProductID + wipCur.Value.ProductVersion + wipCur.Value.OwnerType + wipCur.Value.StepID
                        + Consts.NULL_ID + dateString;
                    string k2 = wipCur.Value.ShopID + wipCur.Value.ProductID + wipCur.Value.ProductVersion + wipCur.Value.OwnerType + wipCur.Value.StepID;
                    if (this.IsSelectedInputWip == false)
                        k2 = this.FromTime.DbToString() + k2;

                    int wipCurStep = 0;
                    SmcvData.WipDetail wipDetail;
                    _wipCurStepDic.TryGetValue(k2, out wipDetail);
                    wipCurStep = wipDetail == null ? 0 : wipDetail.GlassQty;

                    if (_dict.TryGetValue(k, out padding) == false)
                    {
                        SmcvData.StepInfo stepInfo;
                        _stepInfoDic.TryGetValue(wipCur.Value.ShopID + wipCur.Value.StepID, out stepInfo);

                        padding = new SmcvData.ResultItem(wipCur.Value.ShopID, wipCur.Value.ProductID, wipCur.Value.ProductVersion, wipCur.Value.OwnerType,
                            wipCur.Value.StepID, Consts.NULL_ID, date);//, 0, 0);
                        padding.SetStepInfo(stepInfo);
                        
                        _dict.Add(k, padding);
                    }

                    padding.UpdateWipQty(wipCurStep, 0);
                }
            }

            var wipMainStepDic = _wipMainStepDic.ToDictionary(x => x.Key, y => y.Value);

            if (this.SelectedShopID == Consts.ALL && this.IsSelectedInputWip)
            {
                foreach (string shopID in this.shopIdComboBoxEdit.Properties.Items)
                {
                    if (shopID == Consts.ALL)
                        continue;

                    wipMainStepDic.Clear();

                    foreach (string key in _wipMainStepDic.Keys)
                    {
                        if (key.StartsWith(shopID) == false)
                            continue;

                        if (wipMainStepDic.ContainsKey(key) == false)
                            wipMainStepDic.Add(key, _wipMainStepDic[key]);
                    }
                }
            }
            else if (this.SelectedShopID == Consts.ALL && this.IsSelectedInputWip == false)
            {
                wipMainStepDic.Clear();

                wipMainStepDic = _wipMainStepDic.Where(x => x.Key.StartsWith(this.FromTime.DbToString()))
                    .ToDictionary(x => x.Key, y => y.Value);
            }
            else if (this.SelectedShopID != Consts.ALL && this.IsSelectedInputWip)
            {
                wipMainStepDic.Clear();

                wipMainStepDic = _wipMainStepDic.Where(x => x.Key.StartsWith(this.SelectedShopID))
                    .ToDictionary(x => x.Key, y => y.Value);
            }
            else
            {
                wipMainStepDic.Clear();

                wipMainStepDic = _wipMainStepDic
                    .Where(x => x.Key.StartsWith(this.FromTime.DbToString() + this.SelectedShopID))
                    .ToDictionary(x => x.Key, y => y.Value);
            }

            foreach (var wipMain in wipMainStepDic)
            {
                foreach (DateTime date in _dateRangeList)
                {
                    SmcvData.ResultItem padding;

                    string dateString = this.IsTimeConditionHour ? ShopCalendar.StartTimeOfDayT(date).ToString("yyyyMMdd") : date.DbToTimeString();

                    string k = wipMain.Value.ShopID + wipMain.Value.ProductID + wipMain.Value.ProductVersion + wipMain.Value.OwnerType + wipMain.Value.StepID
                        + Consts.NULL_ID + dateString;
                    string k2 = wipMain.Value.ShopID + wipMain.Value.ProductID + wipMain.Value.ProductVersion + wipMain.Value.OwnerType + wipMain.Value.StepID;
                    if (this.IsSelectedInputWip == false)
                        k2 = this.FromTime.DbToString() + k2;

                    if (k2 == "ARRAYB8A550QU5V501AASMP041500-00")
                    { }

                    int wipMainStep = 0;
                    SmcvData.WipDetail wipDetail;
                    _wipMainStepDic.TryGetValue(k2, out wipDetail);
                    wipMainStep = wipDetail == null ? 0 : wipDetail.GlassQty;

                    if (_dict.TryGetValue(k, out padding) == false)
                    {
                        SmcvData.StepInfo stepInfo;
                        _stepInfoDic.TryGetValue(wipMain.Value.ShopID + wipMain.Value.StepID, out stepInfo);

                        padding = new SmcvData.ResultItem(wipMain.Value.ShopID, wipMain.Value.ProductID, wipMain.Value.ProductVersion, wipMain.Value.OwnerType,
                            wipMain.Value.StepID, Consts.NULL_ID, date);//, 0, 0);
                        padding.SetStepInfo(stepInfo);
                        
                        _dict.Add(k, padding);
                    }

                    padding.UpdateWipQty(0, wipMainStep);
                }
            }
        }

        #region 2) BindData
        private void BindData()
        {
            XtraPivotGridHelper.DataViewTable dt = CreateDataViewTable();

            FillData(dt);

            DrawGrid(dt);
        }

        private XtraPivotGridHelper.DataViewTable CreateDataViewTable()
        {
            XtraPivotGridHelper.DataViewTable dt = new XtraPivotGridHelper.DataViewTable();

            dt.AddColumn(StepMoveData.SHOP_ID, StepMoveData.SHOP_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(StepMoveData.PRODUCT_ID, StepMoveData.PRODUCT_ID, typeof(string), PivotArea.FilterArea, null, null);
            dt.AddColumn(StepMoveData.PRODUCT_VERSION, StepMoveData.PRODUCT_VERSION, typeof(string), PivotArea.FilterArea, null, null);
            dt.AddColumn(StepMoveData.OWNER_TYPE, StepMoveData.OWNER_TYPE, typeof(string), PivotArea.FilterArea, null, null);

            dt.AddColumn(SmcvData.Schema.STEP_SEQ, SmcvData.Schema.STEP_SEQ, typeof(int), PivotArea.RowArea, null, null);
            dt.AddColumn(SmcvData.Schema.STEP_ID, SmcvData.Schema.STEP_ID, typeof(string), PivotArea.RowArea, null, null);
            dt.AddColumn(SmcvData.Schema.STEP_DESC, SmcvData.Schema.STEP_DESC, typeof(string), PivotArea.FilterArea, null, null);
            dt.AddColumn(SmcvData.Schema.LAYER, SmcvData.Schema.LAYER, typeof(string), PivotArea.FilterArea, null, null);
            dt.AddColumn(SmcvData.Schema.EQP_ID, SmcvData.Schema.EQP_ID, typeof(string), PivotArea.FilterArea, null, null);
            dt.AddColumn(SmcvData.Schema.TARGET_DATE, SmcvData.Schema.TARGET_DATE, typeof(string), PivotArea.ColumnArea, null, null);
            //dt.AddColumn(StepMoveData.IN_QTY, StepMoveData.IN_QTY, typeof(float), PivotArea.DataArea, null, null);
            dt.AddColumn(SmcvData.Schema.ACT_MOVE_QTY, SmcvData.Schema.ACT_MOVE_QTY, typeof(float), PivotArea.DataArea, null, null);
            dt.AddColumn(SmcvData.Schema.SIM_MOVE_QTY, SmcvData.Schema.SIM_MOVE_QTY, typeof(float), PivotArea.DataArea, null, null);
            dt.AddColumn(SmcvData.Schema.WIP_CUR, SmcvData.Schema.WIP_CUR, typeof(float), PivotArea.FilterArea, null, null);
            dt.AddColumn(SmcvData.Schema.WIP_MAIN, SmcvData.Schema.WIP_MAIN, typeof(float), PivotArea.FilterArea, null, null);

            dt.AddDataTablePrimaryKey(
                    new DataColumn[]
                    {
                        dt.Columns[StepMoveData.SHOP_ID],
                        dt.Columns[StepMoveData.PRODUCT_ID],
                        dt.Columns[StepMoveData.PRODUCT_VERSION],
                        dt.Columns[StepMoveData.OWNER_TYPE],
                        dt.Columns[StepMoveData.STD_STEP],
                        dt.Columns[StepMoveData.EQP_ID],
                        dt.Columns[StepMoveData.TARGET_DATE]
                    }
                );

            return dt;
        }
        #endregion

        #region 3) FillData
        private void FillData(XtraPivotGridHelper.DataViewTable dt)
        {
            foreach (SmcvData.ResultItem item in _dict.Values)
            {
                if (this.IsMainOnly && IsMainStep(item.StepID) == false)
                    continue;
                
                if (this.IsMoveOver0 && item.ActOutQty + item.OutQty <= 0)
                    continue;

                if (item.WipCurStepQty > 0 || item.WipMainStepQty > 0)
                    Console.WriteLine();
                
                dt.DataTable.Rows.Add(
                    item.ShopID,
                    item.ProductID,
                    item.ProductVersion,
                    item.OwnerType,
                    item.StepSeq,
                    item.StepID,
                    item.StepDesc,
                    item.Layer,
                    item.EqpID,
                    this.IsTimeConditionHour ? item.TargetDate.ToString("yyyyMMdd") : item.TargetDate.DbToString(),
                    item.ActOutQty,
                    item.OutQty,
                    item.WipCurStepQty,
                    item.WipMainStepQty
                );
            }
        }
        #endregion

        #region 4) DrawGrid
        private void DrawGrid(XtraPivotGridHelper.DataViewTable dt)
        {
            this.pivotGridControl1.BeginUpdate();

            this.pivotGridControl1.ClearPivotGridFields();
            this.pivotGridControl1.CreatePivotGridFields(dt);

            this.pivotGridControl1.DataSource = dt.DataTable;
            
            ShowTotal(this.pivotGridControl1);
            this.pivotGridControl1.Fields[SmcvData.Schema.STEP_ID].SortMode = PivotSortMode.Custom;

            //for (int i = 0; i < this.pivotGridControl1.Fields.Count; i++)
            //{
            //    if (this.pivotGridControl1.Fields[i].FieldName == "IN_QTY")
            //        this.pivotGridControl1.Fields[i].Area = PivotArea.FilterArea;
            //}

            //pivotGridControl1.CustomCellDisplayText += pivotGridControl1_CellDisplayText;
            this.pivotGridControl1.EndUpdate();

            this.pivotGridControl1.BestFitColumnArea();

            pivotGridControl1.Fields[SmcvData.Schema.ACT_MOVE_QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[SmcvData.Schema.ACT_MOVE_QTY].CellFormat.FormatString = "#,##0";

            pivotGridControl1.Fields[SmcvData.Schema.SIM_MOVE_QTY].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[SmcvData.Schema.SIM_MOVE_QTY].CellFormat.FormatString = "#,##0";

            pivotGridControl1.Fields[SmcvData.Schema.WIP_CUR].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[SmcvData.Schema.WIP_CUR].CellFormat.FormatString = "#,##0";

            pivotGridControl1.Fields[SmcvData.Schema.WIP_MAIN].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pivotGridControl1.Fields[SmcvData.Schema.WIP_MAIN].CellFormat.FormatString = "#,##0";
        }

        private void ShowTotal(PivotGridControl pivot)//, bool isCheck)
        {
            pivot.OptionsView.ShowRowTotals = true;
            pivot.OptionsView.ShowRowGrandTotals = true;
            pivot.OptionsView.ShowColumnTotals = true;
            pivot.OptionsView.ShowColumnGrandTotals = true;
        }
        #endregion

        # region Event

        private void btnQuery_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            Query();

            this.Cursor = Cursors.Default;
        }

        private void btnExportExel_Click(object sender, EventArgs e)
        {
            Mozart.Studio.TaskModel.Utility.PivotGridExporter.ExportToExcel(this.pivotGridControl1);
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
                this.hourShiftSpinEdit.Value = 2;
                this.hourShiftRangeLabel.Text = "Shifts";
            }
        }

        # endregion

    }
}
