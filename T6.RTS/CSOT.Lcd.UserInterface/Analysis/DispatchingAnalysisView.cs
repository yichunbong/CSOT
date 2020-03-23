using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;

using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.UserInterface.Gantts;
using DevExpress.XtraEditors.Controls;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class DispatchingAnalysisView : XtraGridControlView
    {
        private IExperimentResultItem _result;
        private DateTime _planStartTime;
        
        private List<EqpGanttChartData.PresetInfo> _presetList;

        private List<DaData.DispatchingInfo> _infos;

        private EqpMaster EqpMgr { get; set; }

        private string TargetShopID
        {
            get
            {
                return this.shopIdComboBoxEdit.Text;
            }
        }

        private List<string> SelectedEqpGroups
        {
            get
            {
                List<string> eqpGroupList = new List<string>();

                foreach (CheckedListBoxItem item in this.EqpGroupsCheckedBox.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                        eqpGroupList.Add(item.ToString());
                }

                return eqpGroupList;
            }
        }

        private DateTime FromTime
        {
            get
            {
                return this.fromDateEdit.DateTime;
            }
        }

        private DateTime ToTime
        {
            get
            {
                return this.FromTime.AddDays((double)this.dayShiftSpinEdit.Value);
            }
        }

        public DispatchingAnalysisView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        public DispatchingAnalysisView(IServiceProvider serviceProvider, IExperimentResultItem result)
            : base(serviceProvider)
        {
            InitializeComponent();

            _result = result;

            LoadDocument();
        }

        protected override void LoadDocument()
        {
            Initialize();

            SetControl();
        }

        private void Initialize()
        {
            if (_result == null)
            {
                var item = (IMenuDocItem)this.Document.ProjectItem;
                _result = (IExperimentResultItem)item.Arguments[0];
            }

            if (_result == null)
                return;

            Globals.InitFactoryTime(_result.Model);

            _planStartTime = _result.StartTime;

            GetPreprocessData();
        }

        private void GetPreprocessData()
        {
            this.EqpMgr = new EqpMaster();
            this.EqpMgr.LoadEqp(_result);

            _presetList = new List<EqpGanttChartData.PresetInfo>();
            
            DataTable pdt = _result.LoadOutput(EqpGanttChartData.WEIGHT_PRESET_LOG_TABLE_NAME);
            //DataTable pdt = _result.LoadInput(EqpGanttChartData.PRESET_INFO_TABLE_NAME);

            foreach (DataRow row in pdt.Rows)
            {
                _presetList.Add(new EqpGanttChartData.PresetInfo(row));
            }
        }

        private void SetControl()
        {
            this.fromDateEdit.DateTime = _planStartTime;

            this.fromDateEdit.Properties.EditMask = "yyyy-MM-dd HH:mm:ss";
            this.fromDateEdit.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.fromDateEdit.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            dayShiftSpinEdit.Value = _result.GetPlanPeriod(1);

            // ShopID ComboBox
            ComboHelper.AddDataToComboBox(this.shopIdComboBoxEdit, _result,
                SimInputData.InputName.StdStep, SimInputData.StdStepSchema.SHOP_ID, false);

            if (this.shopIdComboBoxEdit.Properties.Items.Count > 0)
                this.shopIdComboBoxEdit.SelectedIndex = 0;

            // EqpGroup CheckComboBox
            SetControl_EqpGroup(this.TargetShopID);
        }

        private void SetControl_EqpGroup(string targetShopID)
        {
            var control = this.EqpGroupsCheckedBox;
            control.Properties.Items.Clear();

            if (this.EqpMgr == null)
                return;

            bool isAll = CommonHelper.Equals(targetShopID, "ALL");
            foreach (var eqp in this.EqpMgr.EqpAll.Values)
            {
                if (isAll == false)
                {
                    if (eqp.ShopID != targetShopID)
                        continue;
                }

                string eqpGroupID = eqp.EqpGroup;
                if (control.Properties.Items.Contains(eqpGroupID))
                    continue;

                control.Properties.Items.Add(eqpGroupID);
            }

            if (control.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in control.Properties.Items)
                    item.CheckState = CheckState.Checked;
            }
        }

        private List<DaData.DispatchingInfo> GetDispatchingInfo()
        {
            if (_infos != null)
                return _infos;

            var list = _infos = new List<DaData.DispatchingInfo>();

            DataTable dt = _result.LoadOutput(SimResultData.OutputName.EqpDispatchLog);
            if (dt == null || dt.Rows.Count == 0)
                return list;

            foreach (DataRow row in dt.Rows)
            {
                var item = new DaData.DispatchingInfo(row);
                string key = item.ShopID + item.EqpGroupID;

                list.Add(item);
            }

            return list;
        }

        private void Query()
        {
            this.Cursor = Cursors.WaitCursor;
            
            BindGrid();

            DesignGrid();

            this.Cursor = Cursors.Default;
        }

        public void Query(string shopID, string eqpGroupID, string eqpID, string subEqpID)
        {
            this.Cursor = Cursors.WaitCursor;

            SetControlValue(shopID, eqpGroupID);

            BindGrid();
            DesignGrid();

            SetFilter(eqpGroupID, eqpID, subEqpID);

            this.Cursor = Cursors.Default;
        }

        private void SetControlValue(string shopID, string eqpGroupID)
        {
            var shopControl = this.shopIdComboBoxEdit;
            if (shopControl.Properties.Items.Contains(shopID))
                shopControl.SelectedIndex = shopControl.Properties.Items.IndexOf(shopID);

            var egControl = this.EqpGroupsCheckedBox;
            if (egControl.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in egControl.Properties.Items)
                {
                    if((item.Value as string) == eqpGroupID)
                        item.CheckState = CheckState.Checked;
                    else
                        item.CheckState = CheckState.Unchecked;
                }
            }
        }

        private void SetFilter(string eqpGroupID, string eqpID, string subEqpID)
        {
            var gv = this.gridView1;

            if (gv.ActiveFilter != null)
                gv.ActiveFilter.Clear();

            var condition = DevExpress.XtraGrid.Columns.AutoFilterCondition.Equals;

            if (string.IsNullOrEmpty(eqpGroupID) == false)
                gv.SetAutoFilterValue(gv.Columns[DaData.Schema.EQP_GROUP], eqpGroupID, condition);

            if (string.IsNullOrEmpty(eqpID) == false)
                gv.SetAutoFilterValue(gv.Columns[DaData.Schema.EQP_ID], eqpID, condition);;

            if (string.IsNullOrEmpty(subEqpID) == false)
                gv.SetAutoFilterValue(gv.Columns[DaData.Schema.SUB_EQP_ID], subEqpID, condition); ;
        }

        private DataTable CreateSchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(DaData.Schema.SHOP_ID, typeof(string));
            dt.Columns.Add(DaData.Schema.EQP_GROUP, typeof(string));
            dt.Columns.Add(DaData.Schema.EQP_ID, typeof(string));
            dt.Columns.Add(DaData.Schema.SUB_EQP_ID, typeof(string));
            dt.Columns.Add(DaData.Schema.DISPATCHING_TIME, typeof(string));
            dt.Columns.Add(DaData.Schema.SELECTED_LOT, typeof(string));
            dt.Columns.Add(DaData.Schema.SELECTED_PRODDUCT, typeof(string));
            dt.Columns.Add(DaData.Schema.SELECTED_STEP, typeof(string));

            dt.Columns.Add(DaData.Schema.INIT_WIP_CNT, typeof(int));
            dt.Columns.Add(DaData.Schema.FILTERED_WIP_CNT, typeof(int));
            dt.Columns.Add(DaData.Schema.SELECTED_WIP_CNT, typeof(int));

            dt.Columns.Add(DaData.Schema.FILTERED_REASON, typeof(string));
            dt.Columns.Add(DaData.Schema.FILTERED_PRODUCT, typeof(string));
            dt.Columns.Add(DaData.Schema.DISPATCH_PRODUCT, typeof(string));
            dt.Columns.Add(DaData.Schema.PRESET_ID, typeof(string));

            return dt;
        }

        private List<DaData.DispatchingInfo> GetData()
        {
            var infos = GetDispatchingInfo();

            if (infos == null || infos.Count == 0)
                return infos;

            DateTime fromTime = this.FromTime;
            DateTime toTime = this.ToTime;

            string targetShopID = this.TargetShopID;
            var eqpGroupList = this.SelectedEqpGroups;

            var list = infos.FindAll(t => IsMatched(t, fromTime, toTime, targetShopID, eqpGroupList));

            return list;
        }

        private bool IsMatched(DaData.DispatchingInfo info, DateTime fromTime, DateTime toTime, string targetShopID, List<string> eqpGroupList)
        {
            DateTime t = info.DispatchingTime;
            if (t < fromTime || t >= toTime)
                return false;

            bool isAll = CommonHelper.Equals(targetShopID, "ALL");
            if (isAll == false)
            {
                if (info.ShopID != targetShopID)
                    return false;
            }

            if (eqpGroupList == null || eqpGroupList.Count == 0)
                return false;

            if (eqpGroupList.Contains(info.EqpGroupID) == false)
                return false;

            return true;
        }


        private void BindGrid()
        {
            var dt = CreateSchema();

            var list = GetData();

            foreach (DaData.DispatchingInfo item in list)
            {
                DataRow row = dt.NewRow();

                row[DaData.Schema.SHOP_ID] = item.ShopID;
                row[DaData.Schema.EQP_GROUP] = item.EqpGroupID;
                row[DaData.Schema.EQP_ID] = item.EqpID;
                row[DaData.Schema.SUB_EQP_ID] = item.SubEqpID;

                row[DaData.Schema.DISPATCHING_TIME] = item.DispatchingTime.ToString("yyyyMMdd HHmmss");
                row[DaData.Schema.SELECTED_LOT] = item.SeleLotID;
                row[DaData.Schema.SELECTED_PRODDUCT] = item.SeleProdID;
                row[DaData.Schema.SELECTED_STEP] = item.SeleStepID;

                row[DaData.Schema.INIT_WIP_CNT] = item.InitWipCnt;
                row[DaData.Schema.FILTERED_WIP_CNT] = item.FilteredWipCnt;
                row[DaData.Schema.SELECTED_WIP_CNT] = item.SelectedWipCnt;

                row[DaData.Schema.FILTERED_REASON] = item.FilteredReason;
                row[DaData.Schema.FILTERED_PRODUCT] = item.FilteredProdID;
                row[DaData.Schema.DISPATCH_PRODUCT] = item.DispatchProdID;
                row[DaData.Schema.PRESET_ID] = item.PresetID;

                dt.Rows.Add(row);
            }

            // GridControl1
            this.gridControl1.DataSource = dt;
        }

        private void DesignGrid()
        {
            Color custPurple = Color.FromArgb(242, 243, 255);

            var grid = this.gridView1;

            //var dtimeCol = grid.Columns[DaData.Schema.DISPATCHING_TIME];
            //dtimeCol.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            //dtimeCol.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            //dtimeCol.OptionsColumn.AllowEdit = false;
            //dtimeCol.AppearanceCell.BackColor = custPurple;

            grid.Columns[DaData.Schema.INIT_WIP_CNT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            grid.Columns[DaData.Schema.INIT_WIP_CNT].DisplayFormat.FormatString = "###,###";
            grid.Columns[DaData.Schema.FILTERED_WIP_CNT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            grid.Columns[DaData.Schema.FILTERED_WIP_CNT].DisplayFormat.FormatString = "###,###";
            grid.Columns[DaData.Schema.SELECTED_WIP_CNT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            grid.Columns[DaData.Schema.SELECTED_WIP_CNT].DisplayFormat.FormatString = "###,###";

            grid.Columns[DaData.Schema.DISPATCHING_TIME].Resize(135);

            //Color custYellow = Color.FromArgb(252, 254, 203);
            //Color custSky = Color.FromArgb(233, 251, 254);
            
            int index = grid.Columns[DaData.Schema.INIT_WIP_CNT].AbsoluteIndex;

            int count = grid.Columns.Count;
            for (int i = 0; i < count; i++)
            {
                if (i < index)
                    continue;

                var col = grid.Columns[i];
                col.OptionsColumn.AllowEdit = false;
                col.AppearanceCell.BackColor = custPurple;
            }

            //grid.BestFitColumns();
            Globals.SetGridViewColumnWidth(grid);
        }
        
        #region Event
       
        private void ShopIdComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetControl_EqpGroup(this.TargetShopID);
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            Query();
        }
        
        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            GridView view = (GridView)sender;
            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            GridHitInfo info = view.CalcHitInfo(pt);
            if (info.InRowCell == false)
                return;

            DataRow selectRow = gridView1.GetFocusedDataRow();

            if (selectRow == null)
                return;

            string shopID = selectRow.GetString(DaData.Schema.SHOP_ID);
            string eqpID = selectRow.GetString(DaData.Schema.EQP_ID);
            string subEqpID = selectRow.GetString(DaData.Schema.SUB_EQP_ID);

            string dispatchTimeStr = selectRow.GetString(DaData.Schema.DISPATCHING_TIME);

            DateTime dispatchTime = DateHelper.StringToDateTime(dispatchTimeStr);

            DaData.DispatchingInfo resultInfo = _infos.Where(t => t.ShopID == shopID && t.EqpID == eqpID
                && t.SubEqpID == subEqpID && t.DispatchingTime == dispatchTime).FirstOrDefault();

            if (resultInfo == null)
                return;

            EqpMaster.Eqp eqp = this.EqpMgr.FindEqp(eqpID);

            DispatchingInfoViewPopup dialog = new DispatchingInfoViewPopup(this.ServiceProvider, _result,
                resultInfo.DispatchInfoRow, eqp, _presetList); 

            dialog.Show();
        }

        #endregion
    }
}
