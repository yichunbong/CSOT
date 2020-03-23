using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraCharts;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using Mozart.Collections;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.UserInterface.Gantts;
using DevExpress.XtraEditors.Controls;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class DispatchingLogView : XtraGridControlView
    {
        private IExperimentResultItem _result;
        private DateTime _planStartTime;

        private List<DaData.DispatchingInfo> _infos;

        private EqpMaster EqpMgr { get; set; }

        public DispatchingLogView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

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

        protected override void LoadDocument()
        {
            Initialize();

            SetControl();
        }

        private void Initialize()
        {
            var item = (IMenuDocItem)this.Document.ProjectItem;
            _result = (IExperimentResultItem)item.Arguments[0];

            if (_result == null)
                return;

            Globals.InitFactoryTime(_result.Model);

            _planStartTime = _result.StartTime;

            GetPreprocessData();
        }

        private void SetControl()
        {
            this.fromDateEdit.DateTime = _planStartTime;

            this.fromDateEdit.Properties.EditMask = "yyyy-MM-dd HH:mm:ss";
            this.fromDateEdit.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.fromDateEdit.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            dayShiftSpinEdit.Value = 1;
            //dayShiftSpinEdit.Value = _result.GetPlanPeriod(1);

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

        private void GetPreprocessData()
        {
            this.EqpMgr = new EqpMaster();
            this.EqpMgr.LoadEqp(_result);
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
                
                list.Add(item);
            }

            return list;
        }


        private void Query()
        {
            BindGrid();

            DesignGrid();
        }

        private DataTable CreateSchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(Schema.SHOP_ID, typeof(string));
            dt.Columns.Add(Schema.EQP_GROUP, typeof(string));
            dt.Columns.Add(Schema.EQP_ID, typeof(string));
            dt.Columns.Add(Schema.SUB_EQP_ID, typeof(string));

            dt.Columns.Add(Schema.PRESET_ID, typeof(string));
            dt.Columns.Add(Schema.DISPATCHING_TIME, typeof(string));

            dt.Columns.Add(Schema.SELECTED_LOT, typeof(string));
            dt.Columns.Add(Schema.SELECTED_PRODDUCT, typeof(string));
            dt.Columns.Add(Schema.SELECTED_STEP, typeof(string));

            dt.Columns.Add(Schema.INIT_WIP_CNT, typeof(int));
            dt.Columns.Add(Schema.FILTERED_WIP_CNT, typeof(int));
            dt.Columns.Add(Schema.SELECTED_WIP_CNT, typeof(int));

            dt.Columns.Add(Schema.INFO_TYPE, typeof(string));
            dt.Columns.Add(Schema.LOT_ID, typeof(string));
            dt.Columns.Add(Schema.PRODUCT_ID, typeof(string));
            dt.Columns.Add(Schema.PRODUCT_VER, typeof(string));
            dt.Columns.Add(Schema.STEP_ID, typeof(string));
            dt.Columns.Add(Schema.LOT_QTY, typeof(int));
            dt.Columns.Add(Schema.OWNER_TYPE, typeof(string));
            dt.Columns.Add(Schema.OWNER_ID, typeof(string));

            dt.Columns.Add(Schema.F_REASON, typeof(string));

            dt.Columns.Add(Schema.W_SEQ, typeof(int));
            dt.Columns.Add(Schema.W_SCORE_SUM, typeof(float));

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
            if(isAll == false)
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
                bool existF = AddRow_Filter(item, dt);
                bool existW = AddRow_WeightFactor(item, dt);

                if (existF == false && existW == false)
                    AddRow_Base(item, dt);
            }

            this.gridControl1.DataSource = dt;
        }

        private DataRow AddRow_Base(DaData.DispatchingInfo item, DataTable dt)
        {
            DataRow row = dt.NewRow();

            row[Schema.SHOP_ID] = item.ShopID;
            row[Schema.EQP_GROUP] = item.EqpGroupID;
            row[Schema.EQP_ID] = item.EqpID;
            row[Schema.SUB_EQP_ID] = item.SubEqpID;
            row[Schema.PRESET_ID] = item.PresetID;

            row[Schema.DISPATCHING_TIME] = item.DispatchingTime.ToString("yyyyMMdd HHmmss");
            row[Schema.SELECTED_LOT] = item.SeleLotID;
            row[Schema.SELECTED_PRODDUCT] = item.SeleProdID;
            row[Schema.SELECTED_STEP] = item.SeleStepID;

            row[Schema.INIT_WIP_CNT] = item.InitWipCnt;
            row[Schema.FILTERED_WIP_CNT] = item.FilteredWipCnt;
            row[Schema.SELECTED_WIP_CNT] = item.SelectedWipCnt;

            dt.Rows.Add(row);

            return row;
        }

        private bool AddRow_Filter(DaData.DispatchingInfo item, DataTable dt)
        {
            var list = DaData.FilteredInfo.Parse(item.FilteredWipLog);
            if (list == null || list.Count == 0)
                return false;

            foreach (var it in list)
            {
                DataRow row = AddRow_Base(item, dt);
                
                row[Schema.LOT_ID] = it.LotID;
                row[Schema.STEP_ID] = it.StepID;
                row[Schema.PRODUCT_ID] = it.ProductID;
                row[Schema.PRODUCT_VER] = it.ProductVersion;
                row[Schema.LOT_QTY] = it.LotQty;
                row[Schema.OWNER_TYPE] = it.OwnerType;
                row[Schema.OWNER_ID] = it.OwnerID;

                row[Schema.INFO_TYPE] = "FILTERED_WIP";
                row[Schema.F_REASON] = it.Reason;
            }           

            return true;
        }
        
        private bool AddRow_WeightFactor(DaData.DispatchingInfo item, DataTable dt)
        {
            var list = DaData.WeightFactorInfo.Parse(item.DispatchWipLog);
            if (list == null || list.Count == 0)
                return false;

            foreach (var it in list)
            {
                DataRow row = AddRow_Base(item, dt);
                
                row[Schema.LOT_ID] = it.LotID;
                row[Schema.STEP_ID] = it.StepID;
                row[Schema.PRODUCT_ID] = it.ProductID;
                row[Schema.PRODUCT_VER] = it.ProductVersion;
                row[Schema.LOT_QTY] = it.LotQty;
                row[Schema.OWNER_TYPE] = it.OwnerType;
                row[Schema.OWNER_ID] = it.OwnerID;

                row[Schema.INFO_TYPE] = "WEIGHT_FACTOR";
                row[Schema.W_SEQ] = it.Seq;
                row[Schema.W_SCORE_SUM] = it.Sum;
            }

            return true;
        }

        private void DesignGrid()
        {
            //this.gridView1.Columns[Schema.DISPATCHING_TIME].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            //this.gridView1.Columns[Schema.DISPATCHING_TIME].DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            //this.gridView1.Columns[Schema.DISPATCHING_TIME].Resize(135);

            this.gridView1.Columns[Schema.INIT_WIP_CNT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[Schema.INIT_WIP_CNT].DisplayFormat.FormatString = "###,###";
            this.gridView1.Columns[Schema.FILTERED_WIP_CNT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[Schema.FILTERED_WIP_CNT].DisplayFormat.FormatString = "###,###";
            this.gridView1.Columns[Schema.SELECTED_WIP_CNT].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.gridView1.Columns[Schema.SELECTED_WIP_CNT].DisplayFormat.FormatString = "###,###";

            this.gridView1.BestFitColumns();
        }
        
        #region Event
                
        private void ShopIdComboBoxEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetControl_EqpGroup(this.TargetShopID);
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
                        
            Query();

            this.Cursor = Cursors.Default;
        }

        #endregion

        private class Schema
        {
            public const string SHOP_ID = "SHOP_ID";
            public const string EQP_GROUP = "EQP_GROUP";
            public const string EQP_ID = "EQP_ID";
            public const string SUB_EQP_ID = "SUB_EQP_ID";

            public const string PRESET_ID = "PRESET_ID";
            public const string DISPATCHING_TIME = "DISPATCHING_TIME";

            public const string SELECTED_LOT = "SELECTED_LOT";
            public const string SELECTED_PRODDUCT = "SELECTED_PRODDUCT";
            public const string SELECTED_STEP = "SELECTED_STEP";

            public const string INIT_WIP_CNT = "INIT_WIP_CNT";
            public const string FILTERED_WIP_CNT = "FILTERED_WIP_CNT";
            public const string SELECTED_WIP_CNT = "SELECTED_WIP_CNT";

            public const string INFO_TYPE = "INFO_TYPE";
            public const string LOT_ID = "LOT_ID";
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string PRODUCT_VER = "PRODUCT_VER";
            public const string STEP_ID = "STEP_ID";
            public const string LOT_QTY = "LOT_QTY";
            public const string OWNER_TYPE = "OWNER_TYPE";
            public const string OWNER_ID = "OWNER_ID";

            public const string F_REASON = "F_REASON";

            public const string W_SEQ = "W_SEQ";
            public const string W_SCORE_SUM = "W_SCORE_SUM";
        }
    }
}
