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

using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class RunTatCompareView : XtraGridControlView
    {
        private IExperimentResultItem _result;
        private DateTime _planStartTime;

        private List<RtcvData.ResultInfo> _resultList;

        string SelectedShopID
        {
            get { return this.shopIdComboBoxEdit.SelectedItem != null ? this.shopIdComboBoxEdit.SelectedItem.ToString() : "Blank"; }
        }

        string SelectedEqpID
        {
            get { return this.eqpIdTextBox.Text == null ? string.Empty : this.eqpIdTextBox.Text.ToUpper(); }
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
                return FromTime.AddHours((double)((int)ShopCalendar.ShiftHours * (int)this.shiftSpinEdit.Value));
            }
        }


        public RunTatCompareView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {
            InitializeBase();

            InitializeControl();
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
            //// ShopID ComboBox
            //ComboHelper.AddDataToComboBox(this.shopIdComboBoxEdit, _result,
            //    SimInputData.InputName.StdStep, SimInputData.StdStepSchema.SHOP_ID, false);

            //if (this.shopIdComboBoxEdit.Properties.Items.Contains("ARRAY"))
            //    this.shopIdComboBoxEdit.SelectedIndex = this.shopIdComboBoxEdit.Properties.Items.IndexOf("ARRAY");
            
            //DateEdit Controls
            this.fromDateEdit.DateTime = ShopCalendar.SplitDate(_planStartTime.AddDays(-2));
            ComboHelper.ShiftName(this.shiftComboBoxEdit, _planStartTime);
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            
            Query();

            this.Cursor = Cursors.Default;
        }

        private void Query()
        {
            ProcessData();

            DataTable dtResultGrid = GetGridSchema();

            dtResultGrid = FillData(dtResultGrid);

            FillGrid(dtResultGrid);
        }

        private void ProcessData()
        {
            //_resultList = new List<RtcvData.ResultInfo>();

            //var modelContext = this._result.GetCtx<ModelDataContext>();

            //var dtLoadHistory = modelContext.LotHistoryAct.Where(x => x.SHOP_ID == this.SelectedShopID &
            //        x.START_TIME >= this.FromTime & x.START_TIME < this.ToTime
            //        & x.RUN_QTY > 0)
            //    .Where(x => this.SelectedEqpID == string.Empty ? true : x.EQP_ID.Contains(this.SelectedEqpID))
            //    .OrderBy(x => x.START_TIME);

            //foreach (var hisRow in dtLoadHistory)
            //{
            //    RtcvData.ResultInfo rsltInfo = new RtcvData.ResultInfo(hisRow.FACTORY_ID, hisRow.SHOP_ID, hisRow.EQP_ID,
            //        hisRow.LOT_ID, hisRow.STEP_ID, hisRow.LAYER_ID, hisRow.START_TIME, hisRow.END_TIME,
            //        hisRow.PROCESS_ID, hisRow.PRODUCT_ID, hisRow.RUN_QTY);

            //    _resultList.Add(rsltInfo);
            //}

            ////var dtStepTime = modelContext.StepTime.Where(x => x.SHOP_ID == this.SelectedShopID)
            ////    .Where(x => this.SelectedEqpID == string.Empty ? true : x.EQP_ID.Contains(this.SelectedEqpID));

            ////Dictionary<string, List<RtcvData.StepTImeInfo>> stepTimeDic = new Dictionary<string, List<RtcvData.StepTImeInfo>>();
            ////foreach (var row in dtStepTime)
            ////{
            ////    string key = row.SHOP_ID + row.EQP_ID + row.PROCESS_ID + row.PRODUCT_ID + row.STEP_ID;

            ////    List<RtcvData.StepTImeInfo> list;
            ////    if (stepTimeDic.TryGetValue(key, out list) == false)
            ////        stepTimeDic.Add(key, list = new List<RtcvData.StepTImeInfo>());

            ////    RtcvData.StepTImeInfo timeInfo = new RtcvData.StepTImeInfo(row.TACT_TIME, row.PROC_TIME);
            ////    list.Add(timeInfo);
            ////}

            //foreach(RtcvData.ResultInfo rsltInfo in _resultList)
            //{
            //    double simRunTime = 0;

            //    List<RtcvData.StepTImeInfo> list;
            //    if (stepTimeDic.TryGetValue(rsltInfo.ShopID + rsltInfo.EqpID + rsltInfo.ProcessID + rsltInfo.ProductID + rsltInfo.StepID, out list))
            //    {
            //        if (list.Count > 1)
            //        {
            //            // 중복 데이터 있으면 -1
            //            simRunTime = -1;
            //        }
            //        else
            //        {
            //            RtcvData.StepTImeInfo info = list[0];

            //            // Array 만 이므로 이렇게 계산
            //            simRunTime = info.TactTime * (rsltInfo.RunQty - 1.0) + info.ProcTime;
            //        }
            //    }

            //    rsltInfo.SetSimRunTime(simRunTime);
            //}
        }

        private DataTable GetGridSchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(RtcvData.Schema.FACTORY_ID, typeof(string));
            dt.Columns.Add(RtcvData.Schema.SHOP_ID, typeof(string));
            dt.Columns.Add(RtcvData.Schema.EQP_ID, typeof(string));
            dt.Columns.Add(RtcvData.Schema.LOT_ID, typeof(string));
            dt.Columns.Add(RtcvData.Schema.STEP_ID, typeof(string));
            dt.Columns.Add(RtcvData.Schema.LAYER_ID, typeof(string));
            dt.Columns.Add(RtcvData.Schema.START_TIME, typeof(DateTime));
            dt.Columns.Add(RtcvData.Schema.END_TIME, typeof(DateTime));
            dt.Columns.Add(RtcvData.Schema.PROCESS_ID, typeof(string));
            dt.Columns.Add(RtcvData.Schema.PRODUCT_ID, typeof(string));
            dt.Columns.Add(RtcvData.Schema.RUN_QTY, typeof(int));
            dt.Columns.Add(RtcvData.Schema.ACT_RUN_TIME, typeof(double));
            dt.Columns.Add(RtcvData.Schema.SIM_RUN_TIME, typeof(double));
            dt.Columns.Add(RtcvData.Schema.DIFFERENCE, typeof(double));
            
            //DataColumn[] dcPKeys = new DataColumn[1];

            //dcPKeys[0] = dtChartTable.Columns[StepMoveCompareViewData.Schema.STEP_DESC];
            //dtChartTable.PrimaryKey = dcPKeys;

            return dt;
        }

        private DataTable FillData(DataTable dt)
        {
            foreach (RtcvData.ResultInfo data in _resultList)
            {
                DataRow dRow = dt.NewRow();

                dRow[RtcvData.Schema.FACTORY_ID] = data.FactoryID;
                dRow[RtcvData.Schema.SHOP_ID] = data.ShopID;
                dRow[RtcvData.Schema.EQP_ID] = data.EqpID;
                dRow[RtcvData.Schema.LOT_ID] = data.LotID;
                dRow[RtcvData.Schema.STEP_ID] = data.StepID;
                dRow[RtcvData.Schema.LAYER_ID] = data.LayerID;
                dRow[RtcvData.Schema.START_TIME] = data.StartTIme;
                dRow[RtcvData.Schema.END_TIME] = data.EndTime;
                dRow[RtcvData.Schema.PROCESS_ID] = data.ProcessID;
                dRow[RtcvData.Schema.PRODUCT_ID] = data.ProductID;
                dRow[RtcvData.Schema.RUN_QTY] = data.RunQty;
                dRow[RtcvData.Schema.ACT_RUN_TIME] = Math.Round(data.ActRunTime, 1);
                dRow[RtcvData.Schema.SIM_RUN_TIME] = Math.Round(data.SimRunTime, 1);
                dRow[RtcvData.Schema.DIFFERENCE] = Math.Round(data.Difference, 1);

                dt.Rows.Add(dRow);
            }

            return dt;
        }

        private void FillGrid(DataTable dt)
        {
            this.gridControl1.DataSource = dt;

            this.gridView1.Columns[RtcvData.Schema.START_TIME].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.gridView1.Columns[RtcvData.Schema.START_TIME].DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            this.gridView1.Columns[RtcvData.Schema.END_TIME].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.gridView1.Columns[RtcvData.Schema.END_TIME].DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            this.gridView1.BestFitColumns();
        }
    }
}
