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
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Inputs;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class ArrangeAnalysisView : XtraGridControlView
    {
        enum LimitType
        {
            NONE,
            L,
            P,
            O,
            B,
            M,
        }

        private IExperimentResultItem _result;
        private ModelDataContext _modelContext;
        private ResultDataContext _resultCtx;

        private Dictionary<string, ResultData> _datas;

        //Dictionary<eqpID, List<EAItem>>
        private Dictionary<string, List<EAItem>> _eaInfos;

        //EqpID+StepID+ProductID+ProductVersion+ToolID (LPOBM)
        private Dictionary<string, List<EAItem>> _eaMatchInfos;
        private Dictionary<string, List<MAItem>> _maMatchInfos;

        //EqpArrange(LimitType = M), Key = MaskID
        private Dictionary<string, List<EAItem>> _eamMatchInfos;

        private Dictionary<string, StdStep> _stdStepInfos;

        public string TargetEqpID { get; private set; }
        public string TargetSubEqpID { get; private set; }
        public string TargetWipInitRun { get; private set; }

        private bool OnlyIssue
        {
            get
            {
                if (this.isShowIssue.Checked == false)
                    return false;

                return true;
            }
        }

        public ArrangeAnalysisView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();

            //dockPanel1.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
        }

        public ArrangeAnalysisView(IServiceProvider serviceProvider, IExperimentResultItem result)
            : base(serviceProvider)
        {
            InitializeComponent();

            _result = result;

            LoadDocument();

            expandablePanel1.IsExpanded = false;
        }

        protected override void LoadDocument()
        {
            Initialize();
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

            _modelContext = this._result.GetCtx<ModelDataContext>();
            _resultCtx = _result.GetCtx<ResultDataContext>();

            _stdStepInfos = new Dictionary<string, StdStep>();

            _datas = new Dictionary<string, ResultData>();
            _eaInfos = new Dictionary<string, List<EAItem>>();
            _maMatchInfos = new Dictionary<string, List<MAItem>>();
            _eaMatchInfos = new Dictionary<string, List<EAItem>>();
            _eamMatchInfos = new Dictionary<string, List<EAItem>>();

            ImportData_StdStep();
            ImportData();

            //this.gridView1.OptionsBehavior.Editable = false;
        }

        private void Query()
        {
            BindGrid();
        }

        private void Reset()
        {
            this.TargetEqpID = null;
            this.TargetSubEqpID = null;
            this.TargetWipInitRun = null;
        }

        public void Query(string eqpID, string subEqpID, string wipInitRun, string matchedKey = null)
        {
            if (this.TargetEqpID != eqpID || this.TargetSubEqpID != subEqpID || this.TargetWipInitRun != wipInitRun)
            {
                this.TargetEqpID = eqpID;
                this.TargetSubEqpID = subEqpID;
                this.TargetWipInitRun = wipInitRun;

                BindGrid();
            }

            SetFilter(eqpID, subEqpID, wipInitRun, matchedKey);

            DataRow row = this.gridView1.GetDataRow(0);
            BindGrid_Detail(row);
        }


        private void SetFilter(string eqpID, string subEqpID, string wipInitRun, string matchedKey)
        {
            var gv = this.gridView1;

            if (gv.ActiveFilter != null)
                gv.ActiveFilter.Clear();

            var condition = DevExpress.XtraGrid.Columns.AutoFilterCondition.Equals;

            if (string.IsNullOrEmpty(eqpID) == false)
                gv.SetAutoFilterValue(gv.Columns[Schema.EQP_ID], eqpID, condition);

            if (string.IsNullOrEmpty(subEqpID) == false)
                gv.SetAutoFilterValue(gv.Columns[Schema.SUB_EQP_ID], subEqpID, condition);

            if (string.IsNullOrEmpty(wipInitRun) == false)
                gv.SetAutoFilterValue(gv.Columns[Schema.WIP_INIT_RUN], wipInitRun, condition);

            if (string.IsNullOrEmpty(matchedKey) == false)
                gv.SetAutoFilterValue(gv.Columns[Schema.MATCH_KEY], matchedKey, condition);
        }

        private void ImportData()
        {
            _datas.Clear();
            _eaInfos.Clear();
            _maMatchInfos.Clear();
            _eamMatchInfos.Clear();

            ImportData_StdStep();

            ImportData_EA();
            ImportData_MA();
            ImportData_EAM();
            ImportData_EqpPlan();

            CheckIssue();
        }

        private void ImportData_StdStep()
        {
            var table = _modelContext.StdStep;

            foreach (var it in table)
            {
                string shopID = it.SHOP_ID;
                string stdStepID = it.STEP_ID;

                string key = CommonHelper.CreateKey(shopID, stdStepID);
                _stdStepInfos[key] = it;
            }
        }

        private void ImportData_EA()
        {
            var infos = _eaInfos;

            var eqpArranges = _modelContext.EqpArrange;

            foreach (var it in eqpArranges)
            {
                string eqpID = it.EQP_ID;
                if (string.IsNullOrEmpty(eqpID))
                    continue;

                List<EAItem> list;
                if (infos.TryGetValue(eqpID, out list) == false)
                    infos.Add(eqpID, list = new List<EAItem>());

                EAItem item = new EAItem(it);
                list.Add(item);
            }
        }

        private void ImportData_MA()
        {
            var infos = _maMatchInfos;

            var toolArranges = _modelContext.ToolArrange;

            foreach (var it in toolArranges)
            {
                string key = CreateKey_Match(it.EQP_ID,
                                             it.STEP_ID,
                                             it.PRODUCT_ID,
                                             it.PRODUCT_VERSION,
                                             it.TOOL_ID);

                if (string.IsNullOrEmpty(key))
                    continue;

                List<MAItem> list;
                if (infos.TryGetValue(key, out list) == false)
                    infos.Add(key, list = new List<MAItem>());

                MAItem item = new MAItem(it);
                list.Add(item);
            }
        }

        private void ImportData_EAM()
        {
            var infos = _eamMatchInfos;

            var eqpArranges = _modelContext.EqpArrange;

            foreach (var it in eqpArranges)
            {
                if (CommonHelper.Equals(it.LIMIT_TYPE, "M") == false)
                    continue;

                string maskID = it.MASK_ID;
                if (string.IsNullOrEmpty(maskID))
                    continue;

                List<EAItem> list;
                if (infos.TryGetValue(maskID, out list) == false)
                    infos.Add(maskID, list = new List<EAItem>());

                EAItem item = new EAItem(it);
                list.Add(item);
            }
        }

        private void ImportData_EqpPlan()
        {
            var eqpPlans = _resultCtx.EqpPlan;
            var datas = _datas;

            foreach (var item in eqpPlans)
            {
                var eqpState = CommonHelper.ToEnum(item.EQP_STATUS, EqpState.NONE);
                if (eqpState != EqpState.BUSY)
                    continue;

                string eqpID = item.EQP_ID;

                if (string.IsNullOrEmpty(eqpID))
                    continue;

                string shopID = item.SHOP_ID;
                string stepID = item.STEP_ID;

                string stdStepKey = CommonHelper.CreateKey(shopID, stepID);

                StdStep stdStep;
                if (_stdStepInfos.TryGetValue(stdStepKey, out stdStep) == false)
                    continue;

                string productVer = item.PRODUCT_VERSION;
                if (CommonHelper.Equals(shopID, "ARRAY") == false)
                    productVer = "00001";

                string key = CommonHelper.CreateKey(item.SHOP_ID, item.EQP_GROUP_ID, item.EQP_ID, item.PRODUCT_ID, item.PRODUCT_VERSION,
                                                    item.STEP_ID, item.TOOL_ID, item.WIP_INIT_RUN);
                if (datas.ContainsKey(key))
                    continue;

                ResultData info = new ResultData(shopID,
                                                 item.EQP_GROUP_ID,
                                                 eqpID,
                                                 item.SUB_EQP_ID,
                                                 stdStep,
                                                 item.PRODUCT_ID,
                                                 productVer,
                                                 item.TOOL_ID,
                                                 item.WIP_INIT_RUN);

                ImportData_MatchEA(info);

                datas.Add(key, info);
            }
        }

        private bool IsMatched(ResultData item)
        {
            if (string.IsNullOrEmpty(this.TargetEqpID) == false)
            {
                if (item.EqpID != this.TargetEqpID)
                    return false;
            }

            if (string.IsNullOrEmpty(this.TargetSubEqpID) == false)
            {
                if (item.SubEqpID != this.TargetSubEqpID)
                    return false;
            }

            return true;
        }

        private void ImportData_MatchEA(ResultData info)
        {
            string eqpID = info.EqpID;
            if (string.IsNullOrEmpty(eqpID))
                return;

            var eaInfos = _eaInfos;
            if (eaInfos == null || eaInfos.Count == 0)
                return;

            string key = CreateKey_Match(info.EqpID,
                                         info.StepID,
                                         info.ProductID,
                                         info.ProductVersion,
                                         info.MaskID);


            var map = _eaMatchInfos;

            //이미 등록된 정보 존재시
            if (map.ContainsKey(key))
                return;

            List<EAItem> list;
            if (eaInfos.TryGetValue(eqpID, out list) == false)
                return;

            if (list == null || list.Count == 0)
                return;

            var finds = list.FindAll(t => t.IsMatched(info));
            if (finds == null && finds.Count == 0)
                return;

            map.Add(key, finds);
        }

        private void CheckIssue()
        {
            var datas = _datas;
            if (datas == null || datas.Count == 0)
                return;

            foreach (var info in datas.Values)
            {
                //EA
                CheckIssue_EA(info);

                //MA
                CheckIssue_MA(info);

                //EAM
                CheckIssue_EAM(info);
            }
        }

        private void CheckIssue_EA(ResultData info)
        {
            string matchKey = info.MatchKey;

            var list = GetMatchedList_EA(matchKey);

            //ParallelChamber Chamber
            if (string.IsNullOrEmpty(info.SubMatchKey) == false)
            {
                var sublist = GetMatchedList_EA(info.SubMatchKey);
                if (sublist != null && sublist.Count > 0)
                    list.AddRange(sublist);
            }

            info.Count_EA = list == null ? 0 : list.Count;

            info.Issue_EA = info.Count_EA == 0;
            if (info.Issue_EA == false)
            {
                var find = list.FirstOrDefault(t => t.IsLoadable() == false);
                if (find != null)
                    info.Issue_EA = true;
            }
        }

        private void CheckIssue_MA(ResultData info)
        {
            string maskID = info.MaskID;
            if (string.IsNullOrEmpty(maskID) || maskID == Consts.NULL_ID)
                return;

            string matchKey = info.MatchKey;

            var list = GetMatchedList_MA(matchKey);

            int count = list == null ? 0 : list.Count;

            info.Issue_MA = count == 0;
            if (info.Issue_MA == false)
            {
                var find = list.FirstOrDefault(t => t.IsLoadable() == false);
                if (find != null)
                    info.Issue_MA = true;
            }
        }

        private void CheckIssue_EAM(ResultData info)
        {
            CheckIssue_EAM_LPOBM(info);
            CheckIssue_EAM_OnlyM(info);
        }

        private void CheckIssue_EAM_LPOBM(ResultData info)
        {
            if (info.HasLimit_M == false)
                return;

            string maskID = info.MaskID;
            if (string.IsNullOrEmpty(maskID) || maskID == Consts.NULL_ID)
                return;

            if (info.Issue_EA)
            {
                info.Issue_EAM = true;
                return;
            }

            string matchKey = info.MatchKey;

            var list = GetMatchedList_EA(matchKey);

            int count = list == null ? 0 : list.Count;
            if (count == 0)
            {
                info.Issue_EAM = true;
                return;
            }

            var notloadables = list.FirstOrDefault(t => t.Target.MASK_ID == info.MaskID && t.IsLoadable() == false);
            if (notloadables != null)
            {
                info.Issue_EAM = true;
                return;
            }

            var loadables = list.FirstOrDefault(t => t.Target.MASK_ID == info.MaskID && t.IsLoadable());
            if (loadables == null)
            {
                info.Issue_EAM = true;
                return;
            }
        }

        private void CheckIssue_EAM_OnlyM(ResultData info)
        {
            string maskID = info.MaskID;
            if (string.IsNullOrEmpty(maskID) || maskID == Consts.NULL_ID)
                return;

            if (info.Issue_EA)
            {
                info.Issue_EAM = true;
                return;
            }

            string matchKey = maskID;
            var list = GetMatchedList_EAM(matchKey);
            if (list == null || list.Count == 0)
                return;

            //check limit
            var notloadables = list.FirstOrDefault(t => t.IsLoadable() == false);
            if (notloadables != null)
            {
                info.Issue_EAM = true;
                return;
            }
        }

        private List<EAItem> GetMatchedList_EA(string key)
        {
            var infos = _eaMatchInfos;

            if (infos == null || infos.Count == 0)
                return null;

            List<EAItem> list;
            if (infos.TryGetValue(key, out list))
                return list;

            return null;
        }

        private List<MAItem> GetMatchedList_MA(string key)
        {
            var infos = _maMatchInfos;

            if (infos == null || infos.Count == 0)
                return null;

            List<MAItem> list;
            if (infos.TryGetValue(key, out list))
                return list;

            return null;
        }

        private List<EAItem> GetMatchedList_EAM(string key)
        {
            var infos = _eamMatchInfos;

            if (infos == null || infos.Count == 0)
                return null;

            List<EAItem> list;
            if (infos.TryGetValue(key, out list))
                return list;

            return null;
        }

        private DataTable CreateSchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(Schema.SHOP_ID, typeof(string));
            dt.Columns.Add(Schema.EQP_GROUP, typeof(string));
            dt.Columns.Add(Schema.EQP_ID, typeof(string));
            dt.Columns.Add(Schema.SUB_EQP_ID, typeof(string));

            dt.Columns.Add(Schema.PRODUCT_ID, typeof(string));
            dt.Columns.Add(Schema.STEP_ID, typeof(string));
            dt.Columns.Add(Schema.PRODUCT_VERSION, typeof(string));
            dt.Columns.Add(Schema.MASK_ID, typeof(string));

            dt.Columns.Add(Schema.WIP_INIT_RUN, typeof(string));
            dt.Columns.Add(Schema.DEFAULT_ARRAGE, typeof(string));

            dt.Columns.Add(Schema.EA_COUNT, typeof(int));

            dt.Columns.Add(Schema.EA_ISSUE, typeof(string));
            dt.Columns.Add(Schema.MA_ISSUE, typeof(string));
            dt.Columns.Add(Schema.EAM_ISSUE, typeof(string));
            dt.Columns.Add(Schema.MATCH_KEY, typeof(string));

            return dt;
        }

        private DataTable CreateEASchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(Schema.SHOP_ID, typeof(string));
            dt.Columns.Add(Schema.LIMIT_TYPE, typeof(string));
            dt.Columns.Add(Schema.EQP_ID, typeof(string));
            dt.Columns.Add(Schema.SUB_EQP_ID, typeof(string));
            dt.Columns.Add(Schema.PRODUCT_ID, typeof(string));
            dt.Columns.Add(Schema.PRODUCT_VERSION, typeof(string));
            dt.Columns.Add(Schema.STEP_ID, typeof(string));
            dt.Columns.Add(Schema.MASK_ID, typeof(string));

            dt.Columns.Add(Schema.ACTIVATE_TYPE, typeof(string));
            dt.Columns.Add(Schema.LIMIT_QTY, typeof(int));
            dt.Columns.Add(Schema.ACTUAL_QTY, typeof(int));
            dt.Columns.Add(Schema.DAILY_MODE, typeof(string));

            return dt;
        }

        private DataTable CreateMASchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(Schema.SHOP_ID, typeof(string));
            dt.Columns.Add(Schema.TOOL_ID, typeof(string));
            dt.Columns.Add(Schema.TOOL_TYPE, typeof(string));
            dt.Columns.Add(Schema.EQP_ID, typeof(string));
            dt.Columns.Add(Schema.PRODUCT_ID, typeof(string));
            dt.Columns.Add(Schema.STEP_ID, typeof(string));
            dt.Columns.Add(Schema.PRODUCT_VERSION, typeof(string));
            dt.Columns.Add(Schema.ACTIVATE_TYPE, typeof(string));
            dt.Columns.Add(Schema.PRIORITY, typeof(string));

            return dt;
        }

        private void BindGrid()
        {
            DataTable dt = CreateSchema();

            var datas = _datas;
            if (datas != null)
            {
                foreach (var info in datas.Values)
                {
                    if (IsMatched(info) == false)
                        continue;

                    DataRow row = dt.NewRow();

                    bool isIssue = false;
                    if (info.Issue_EA || info.Issue_MA || info.Issue_EAM)
                        isIssue = true;

                    if (this.OnlyIssue && isIssue == false)
                        continue;

                    row[Schema.SHOP_ID] = info.ShopID;
                    row[Schema.EQP_GROUP] = info.EqpGroup;
                    row[Schema.EQP_ID] = info.EqpID;
                    row[Schema.SUB_EQP_ID] = info.SubEqpID == null ? string.Empty : info.SubEqpID;

                    row[Schema.PRODUCT_ID] = info.ProductID;
                    row[Schema.STEP_ID] = info.StepID;
                    row[Schema.PRODUCT_VERSION] = info.ProductVersion;
                    row[Schema.MASK_ID] = info.MaskID;
                    row[Schema.WIP_INIT_RUN] = info.WipInitRun;
                    row[Schema.DEFAULT_ARRAGE] = info.DefaultArrage;

                    row[Schema.EA_COUNT] = info.Count_EA;

                    row[Schema.EA_ISSUE] = CommonHelper.ToStringYN(info.Issue_EA);
                    row[Schema.MA_ISSUE] = CommonHelper.ToStringYN(info.Issue_MA);
                    row[Schema.EAM_ISSUE] = CommonHelper.ToStringYN(info.Issue_EAM);

                    row[Schema.MATCH_KEY] = info.MatchKey;

                    dt.Rows.Add(row);
                }
            }

            this.gridControl1.DataSource = dt;

            DesignGrid();
        }

        private void BindEAGrid(List<EAItem> ealist, List<EAItem> eamlist, string subEqpID)
        {
            DataTable dt = CreateEASchema();

            List<EAItem> list = new List<EAItem>();

            if (ealist != null && ealist.Count > 0)
                list.AddRange(ealist);

            if (eamlist != null && eamlist.Count > 0)
                list.AddRange(eamlist);

            foreach (var item in list)
            {
                var info = item.Target;
                DataRow dRow = dt.NewRow();

                dRow[Schema.SHOP_ID] = info.SHOP_ID;
                dRow[Schema.LIMIT_TYPE] = info.LIMIT_TYPE;
                dRow[Schema.EQP_ID] = info.EQP_ID;
                dRow[Schema.SUB_EQP_ID] = subEqpID == null ? string.Empty : subEqpID;
                dRow[Schema.PRODUCT_ID] = info.PRODUCT_ID;
                dRow[Schema.PRODUCT_VERSION] = info.PRODUCT_VERSION;
                dRow[Schema.STEP_ID] = info.STEP_ID;
                dRow[Schema.MASK_ID] = info.MASK_ID;
                dRow[Schema.ACTIVATE_TYPE] = info.ACTIVATE_TYPE;
                dRow[Schema.LIMIT_QTY] = info.LIMIT_QTY;
                dRow[Schema.ACTUAL_QTY] = info.ACTUAL_QTY;
                dRow[Schema.DAILY_MODE] = info.DAILY_MODE;

                dt.Rows.Add(dRow);

            }

            this.gridControl2.DataSource = dt;
            this.gridView2.BestFitColumns();
        }

        private void BindMAGrid(List<MAItem> list)
        {
            DataTable dt = CreateMASchema();

            if (list != null)
            {
                foreach (var item in list)
                {
                    var info = item.Target;
                    DataRow dRow = dt.NewRow();

                    dRow[Schema.SHOP_ID] = info.SHOP_ID;
                    dRow[Schema.TOOL_ID] = info.TOOL_ID;
                    dRow[Schema.TOOL_TYPE] = info.TOOL_TYPE;
                    dRow[Schema.EQP_ID] = info.EQP_ID;
                    dRow[Schema.PRODUCT_ID] = info.PRODUCT_ID;
                    dRow[Schema.STEP_ID] = info.STEP_ID;
                    dRow[Schema.PRODUCT_VERSION] = info.PRODUCT_VERSION;
                    dRow[Schema.ACTIVATE_TYPE] = info.ACTIVATE_TYPE;
                    dRow[Schema.PRIORITY] = info.PRIORITY;

                    dt.Rows.Add(dRow);
                }
            }

            this.gridControl3.DataSource = dt;
            this.gridView3.BestFitColumns();
        }

        private void DesignGrid()
        {
            var grid = this.gridView1;

            //Color custPurple = Color.FromArgb(242, 243, 255);
            ////Color custYellow = Color.FromArgb(252, 254, 203);
            ////Color custSky = Color.FromArgb(233, 251, 254);


            //int sindex = grid.Columns[Schema.EQP_ID].AbsoluteIndex;
            //int eindex = grid.Columns[Schema.MASK_ID].AbsoluteIndex;

            //int count = grid.Columns.Count;
            //for (int i = 0; i < count; i++)
            //{
            //    var col = grid.Columns[i];

            //    if (i >= sindex && i <= eindex)
            //    {
            //        col.OptionsColumn.AllowEdit = true;
            //    }
            //    else
            //    {
            //        col.OptionsColumn.AllowEdit = false;
            //        col.AppearanceCell.BackColor = custPurple;
            //    }
            //}
            
            grid.Columns[Schema.MATCH_KEY].Visible = false;
            grid.BestFitColumns();

        }

        public class Schema
        {            
            public const string SHOP_ID = "SHOP_ID";
            public const string EQP_GROUP = "EQP_GROUP";
            public const string EQP_ID = "EQP_ID";
            public const string SUB_EQP_ID = "SUB_EQP_ID";
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string STEP_ID = "STEP_ID";
            public const string PRODUCT_VERSION = "PRODUCT_VERSION";
            public const string MASK_ID = "MASK_ID";

            public const string MATCH_KEY = "MATCH_KEY";

            public const string WIP_INIT_RUN = "WIP_INIT_RUN";
            public const string DEFAULT_ARRAGE = "DEFAULT_ARRAGE";

            public const string EA_COUNT = "EA_COUNT";

            public const string EA_ISSUE = "EA_ISSUE";            
            public const string MA_ISSUE = "MA_ISSUE";
            public const string EAM_ISSUE = "EAM_ISSUE";

            public const string LIMIT_TYPE = "LIMIT_TYPE";
            public const string ACTIVATE_TYPE = "ACTIVATE_TYPE";
            public const string LIMIT_QTY = "LIMIT_QTY";
            public const string ACTUAL_QTY = "ACTUAL_QTY";
            public const string DAILY_MODE = "DAILY_MODE";

            public const string TOOL_ID = "TOOL_ID";
            public const string TOOL_TYPE = "TOOL_TYPE";
            public const string PRIORITY = "PRIORITY";            
        }

        public class ResultData
        {
            public string MatchKey { get; set; }
            public string SubMatchKey { get; set; }
            public string ShopID { get; set; }
            public string EqpGroup { get; set; }
            public string EqpID { get; set; }
            public string SubEqpID { get; set; }
            public StdStep StdStep { get; set; }
            public string ProductID { get; set; }            
            public string ProductVersion { get; set; }
            public string MaskID { get; set; }            
            public string WipInitRun { get; set; }

            public int Count_EA { get; set; }
            public bool Issue_EA { get; set; }            
            public bool Issue_MA { get; set; }
            public bool Issue_EAM { get; set; }
                        
            public string StepID 
            {
                get { return this.StdStep.STEP_ID; }
            }

            public string DefaultArrage
            {
                get { return this.StdStep.DEFAULT_ARRANGE; }
            }
                        
            public bool HasLimit_M { get; set; }

            public ResultData(string shopID, string eqpGroup, string eqpID, string subEqpID,
                StdStep stdStep, string productID, string productVer, string maskID, string wipInitRun)
            {                
                this.ShopID = shopID;
                this.EqpGroup = eqpGroup;
                this.EqpID = eqpID;
                this.SubEqpID = subEqpID;
                this.StdStep = stdStep;
                this.ProductID = productID;
                this.ProductVersion = productVer;
                this.MaskID = maskID;
                this.WipInitRun = wipInitRun;

                this.MatchKey = CreateKey_Match(this.EqpID,
                                                this.StepID,
                                                this.ProductID,
                                                this.ProductVersion,
                                                this.MaskID);

                if (string.IsNullOrEmpty(subEqpID) == false)
                {
                    this.SubMatchKey = CreateKey_Match(this.SubEqpID,
                                                       this.StepID,
                                                       this.ProductID,
                                                       this.ProductVersion,
                                                       this.MaskID);
                }

                string defaultArrange = stdStep.DEFAULT_ARRANGE;

                var limitTypeList = ParseLimitType(defaultArrange);
                this.HasLimit_M = HasLimitType(limitTypeList, LimitType.M);
            }

            public static string CreateKey(string eqpID, string stepID, string productID, string productVer, string maskID)
            {
                string key = CommonHelper.CreateKey(eqpID,
                                                    stepID,
                                                    productID,
                                                    productVer,
                                                    maskID);

                return key;
            }
        }

        private class EAItem
        {
            public string Key { get; set; }
            public EqpArrange Target { get; set; }
                                    
            public List<LimitType> LimitTypeList { get; set; }            
            
            public EAItem(EqpArrange item)
            {
                this.Target = item;

                this.Key = item.EQP_ID;
                this.LimitTypeList = ParseLimitType(item.LIMIT_TYPE);
            }

            public bool IsMatched(ResultData info)
            {
                var item = this.Target;
                var limitTypeList = this.LimitTypeList;

                if (HasLimitType(limitTypeList, LimitType.L))
                {
                    if (IsMatched(item.EQP_ID, info.EqpID) == false)
                        return false;
                }

                if (HasLimitType(limitTypeList, LimitType.O))
                {
                    if (IsMatched(item.STEP_ID, info.StepID) == false)
                        return false;
                }

                if (HasLimitType(limitTypeList, LimitType.P))
                {
                    if (IsMatched(item.PRODUCT_ID, info.ProductID) == false)
                        return false;
                }

                if (HasLimitType(limitTypeList, LimitType.B))
                {
                    if (IsMatched(item.PRODUCT_VERSION, info.ProductVersion) == false)
                        return false;
                }

                return true;
            }

            public bool IsLoadable()
            {
                var item = this.Target;

                if (CommonHelper.Equals(item.ACTIVATE_TYPE, "Y") || CommonHelper.Equals(item.ACTIVATE_TYPE, "M"))
                    return true;

                return false;
            }

            private bool IsMatched(string arrangeStr, string targetStr)
            {
                if (CommonHelper.Equals(arrangeStr, "ALL"))
                    return true;

                return arrangeStr == targetStr;
            }
        }

        private class MAItem
        {
            public string Key { get; set; }
            public ToolArrange Target { get; set; }
            
            public MAItem(ToolArrange item)
            {                
                this.Target = item;

                this.Key = CreateKey_Match(item.EQP_ID, 
                                           item.STEP_ID,
                                           item.PRODUCT_ID,
                                           item.PRODUCT_VERSION,
                                           item.TOOL_ID);
            }

            public bool IsLoadable()
            {
                var item = this.Target;

                if (CommonHelper.Equals(item.ACTIVATE_TYPE, "Y"))
                    return true;

                return false;
            }
        }

        public static string CreateKey_Match(string eqpID, string stepID, string productID, string productVer, string maskID)
        {
            string key = CommonHelper.CreateKey(eqpID,
                                                stepID,
                                                productID,
                                                productVer,
                                                maskID);

            return key;
        }

        private static bool HasLimitType(List<LimitType> list, LimitType limitType)
        {
            if (list == null || list.Count == 0)
                return false;

            if (list.Contains(limitType))
                return true;

            return false;
        }

        private static List<LimitType> ParseLimitType(string limitTypeStr)
        {
            List<LimitType> list = new List<LimitType>();

            if (string.IsNullOrEmpty(limitTypeStr))
                return list;

            var arr = limitTypeStr.ToCharArray();
            foreach (var c in arr)
            {
                var limitType = CommonHelper.ToEnum(c.ToString(), LimitType.NONE);
                if (limitType == LimitType.NONE)
                    continue;

                list.Add(limitType);
            }

            return list;
        }
        
        #region Event

        private void btnQuery_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            Reset();
            Query();

            this.Cursor = Cursors.Default;
        }

        private void isShowIssue_CheckedChanged(object sender, EventArgs e)
        {
            Query();
        }

        #endregion

        private void BindGrid_Detail(DataRow row)
        {
            if (row == null)
                return;

            string eqpID = row[Schema.EQP_ID].ToString();
            string subEqpID = row[Schema.SUB_EQP_ID].ToString();
            string stepID = row[Schema.STEP_ID].ToString();
            string productID = row[Schema.PRODUCT_ID].ToString();
            string productVer = row[Schema.PRODUCT_VERSION].ToString();
            string maskID = row[Schema.MASK_ID].ToString();

            string key = CreateKey_Match(eqpID, stepID, productID, productVer, maskID);

            var ealist = GetMatchedList_EA(key);
            if (string.IsNullOrEmpty(subEqpID) == false)
            {
                string subKey = CreateKey_Match(subEqpID, stepID, productID, productVer, maskID);

                var sublist = GetMatchedList_EA(subKey);
                if (sublist != null && sublist.Count > 0)
                    ealist.AddRange(sublist);
            }

            var eamlist = GetMatchedList_EAM(maskID);
            
            BindEAGrid(ealist, eamlist, subEqpID);

            var malist = GetMatchedList_MA(key);
            BindMAGrid(malist);
        }

        private void GridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DataRow row = gridView1.GetFocusedDataRow();
            if (row == null)
                return;

            BindGrid_Detail(row);
        }
    }
}
