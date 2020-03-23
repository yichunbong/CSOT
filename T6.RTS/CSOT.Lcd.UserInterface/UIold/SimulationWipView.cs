using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Mozart.Collections;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserInterface;


using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class SimulationWipView : XtraPivotGridControlView
    {
        private IExperimentResultItem _result;
        private DateTime _planStartTime;

        private List<string> _selectedProductList;

        private Dictionary<string, SwvData.StepInfo> _stepInfoDic;

        private DoubleDictionary<DateTime, string, List<SwvData.StepWipChartInf>> _stepWipChartAllDDic;
        private Dictionary<string, SwvData.StepWipInf> _stepWipAllDic;

        private List<SwvData.StepWipChartInf> _stepWipChartRsltList;


        string SelectedShopID
        {
            get { return this.shopIdComboBoxEdit.SelectedItem != null ? this.shopIdComboBoxEdit.SelectedItem.ToString() : "Blank"; }
        }

        bool IsAllProductSeletected
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

        public SimulationWipView(IServiceProvider serviceProvider)
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
            this.fromDateEdit.DateTime = ShopCalendar.SplitDate(_planStartTime);
            ComboHelper.ShiftName(this.shiftComboBoxEdit, _planStartTime);
        }

        private void InitializeData()
        {
            var modelContext = this._result.GetCtx<ModelDataContext>();
            
            // ALL Step정보
            _stepInfoDic = new Dictionary<string, SwvData.StepInfo>();

            var stdStep = modelContext.StdStep.Where(x => x.STEP_SEQ > 0)
                .OrderBy(x => x.SHOP_ID).OrderByDescending(x => x.STEP_SEQ);

            foreach (var step in stdStep)
            {
                if (_stepInfoDic.ContainsKey(step.SHOP_ID + step.STEP_ID) == false)
                    _stepInfoDic.Add(step.SHOP_ID + step.STEP_ID, new SwvData.StepInfo(step.SHOP_ID, step.STEP_ID,
                        step.STEP_DESC, step.STEP_TYPE, (int)step.STEP_SEQ, step.LAYER_ID));
            }

            // StepWip 정보
            _stepWipAllDic = new Dictionary<string, SwvData.StepWipInf>();
            _stepWipChartAllDDic = new DoubleDictionary<DateTime, string, List<SwvData.StepWipChartInf>>();

            // Result / StepWip 재공정보
            DataTable dtStepWip = _result.LoadOutput(SimResultData.OutputName.StepWip);

            foreach (DataRow drow in dtStepWip.Rows)
            {
                SimResultData.StepWip row = new SimResultData.StepWip(drow);
                
                SwvData.StepInfo stepInfo;
                _stepInfoDic.TryGetValue(row.ShopID + row.StepID, out stepInfo);
                string stepDesc = stepInfo == null ? Consts.NULL_ID : stepInfo.StepDesc;
                int stepSeq = stepInfo == null ? int.MaxValue : stepInfo.StepSeq;
                string layer = stepInfo == null ? Consts.NULL_ID : stepInfo.Layer;
                
                Dictionary<string, List<SwvData.StepWipChartInf>> dic;
                if (_stepWipChartAllDDic.TryGetValue(row.TargetDate, out dic) == false)
                    _stepWipChartAllDDic.Add(row.TargetDate, dic = new Dictionary<string, List<SwvData.StepWipChartInf>>());

                List<SwvData.StepWipChartInf> list;
                if (dic.TryGetValue(row.ShopID, out list) == false)
                    dic.Add(row.ShopID, list = new List<SwvData.StepWipChartInf>());

                SwvData.StepWipChartInf inf = new SwvData.StepWipChartInf(row.TargetDate, row.ShopID, row.StepID, stepDesc,
                    stepSeq, layer);

                list.Add(inf);
            }
        }

        private void Query()
        {
            ProcessData();

            //DataTable dtStepResultGrid = GetStepRsltGridSchema();

            //dtStepResultGrid = FillData(dtStepResultGrid);

        }

        private void ProcessData()
        {
            _stepWipChartRsltList = new List<SwvData.StepWipChartInf>();

            //DoubleDictionary<DateTime, string, List<SwvData.StepWipChartInf>> _stepWipChartAllDDic;

            //var rslt = _stepWipChartAllDDic

        }


        #region Event Collection

        private void queryBtn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            Query();

            _selectedProductList = null;

            this.Cursor = Cursors.Default;
        }



        #endregion
    }
}
