using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.UIComponents;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using DevExpress.XtraEditors;
using DevExpress.Spreadsheet;
using DevExpress.XtraEditors.Controls;
using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Utils;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.UserInterface.Gantts;
using CSOT.Lcd.Scheduling.Inputs;

namespace CSOT.Lcd.UserInterface.ToolGantts
{
    public partial class ToolGanttView  : XtraGridControlView
    {
        private const string _pageID = "ToolGanttView";

        private const string _toolGanttCellWidth = "ToolGanttCellWidth";
        private const string _toolGanttCellHeight = "ToolGanttCellHeight";

        private bool _isEndLoadDocument;
        private IExperimentResultItem _result;
        private ResultDataContext _resultCtx;

        private DateTime _planStartTime;

        private EqpMaster _eqpMgr;

        private ToolGantt _gantt;

        HashSet<string> _prodIDList;
        HashSet<string> _stepIDList;
        List<string> _maskIDList;

        private ToolBarDetailView DispView { get; set; }

        private bool IsOnlyToolMode
        {
            get { return this.viewGroupRadioGrp.SelectedIndex == 2; }
        }

        private ToolGantt.ViewMode SelectViewMode
        {
            get
            {
                var item = this.viewGroupRadioGrp.Properties.Items[this.viewGroupRadioGrp.SelectedIndex];
                string str = item.Value as string;

                ToolGantt.ViewMode mode;
                if (Enum.TryParse(str, out mode))
                    return mode;

                return ToolGantt.ViewMode.MASK;
            }
        }

        private string TargetShopID
        {
            get { return this.ShopIDComboBox.Text; }
        }
        
        private string EqpIdPattern
        {
            get
            {
                return this.eqpIDtextBox.Text.ToUpper();
            }
        }

        private HashSet<string> SelectedProdIDs
        {
            get
            {
                if (_prodIDList == null)
                    _prodIDList = new HashSet<string>();
                else
                    _prodIDList.Clear();

                foreach (var item in this.prodCheckedBox.Properties.Items.GetCheckedValues())
                    _prodIDList.Add(item.ToString());

                return _prodIDList;
            }
        }

        private HashSet<string> SelectedStepIDs
        {
            get
            {
                if (_stepIDList == null)
                    _stepIDList = new HashSet<string>();
                else
                    _stepIDList.Clear();

                foreach (var item in this.stepCheckedBox.Properties.Items.GetCheckedValues())
                    _stepIDList.Add(item.ToString());

                return _stepIDList;
            }
        }

        private List<string> SelectedMaskIDs
        {
            get
            {
                // 선택된 장비 그룹 등록
                if (_maskIDList == null)
                    _maskIDList = new List<string>();
                else
                    _maskIDList.Clear();

                foreach (var item in this.masksCheckedBox.Properties.Items.GetCheckedValues())
                    _maskIDList.Add(item.ToString());

                return _maskIDList;
            }
        }

        private bool IsStepView
        {
            get { return false; }
        }

        private DateTime StartDate
        {
            get
            {
                DateTime dt = startDateEdit.DateTime;
                return dt;
            }
        }

        private DateTime EndDate
        {
            get
            {
                return StartDate.AddHours(ShopCalendar.ShiftHours * Convert.ToInt32(fromShiftComboBox.Value));
            }
        }

        public MouseSelectType SelectedMouseSelectType
        {
            get
            {
                MouseSelectType type;
                string sele = this.radioGroup1.Properties.Items[this.radioGroup1.SelectedIndex].Value.ToString();
                Enum.TryParse(sele, out type);

                return type;
            }
        }

        public int CellWidthSize
        {
            get { return ganttSizeControl.CellWidth; }
        }

        public int CellHeightSize
        {
            get { return ganttSizeControl.CellHeight; }
        }

        #region ctor
        public ToolGanttView()
        {
            InitializeComponent();
        }

        public ToolGanttView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        #endregion
        
        protected override void LoadDocument()
        {            
            if (_result == null)
            {
                var item = (IMenuDocItem)this.Document.ProjectItem;
                _result = (IExperimentResultItem)item.Arguments[0];
            }

            _resultCtx = _result.GetCtx<ResultDataContext>();

            InitControl();

            InitializeData();

            this.BindEvents();

            _isEndLoadDocument = true;
        }

        private void InitControl()
        {
            this.fromShiftComboBox.Value = ShopCalendar.ShiftCount;
            _planStartTime = _result.StartTime;

            this.startDateEdit.DateTime = _planStartTime;

            this.startDateEdit.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.startDateEdit.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            this.radioGroup1.SelectedIndex = 0;

            this.viewGroupRadioGrp.SelectedIndex = 0;

            SetShopIDComboBox();

            SetProductIDControl(this.TargetShopID);

            SetStepIDControl(this.TargetShopID);

            SetMaskIDControl(this.TargetShopID);

            SetCellSize();

            InitControl_DispDetail();
        }

        private void InitControl_DispDetail()
        {
            var view = new ToolBarDetailView();

            dispatchingDetailpanel.Controls.Add(view);
            view.Dock = DockStyle.Fill;

            this.DispView = view;
        }

        private void SetCellSize()
        {
            var cellWidth = Extensions.GetLocalSetting(this.ServiceProvider, _pageID + _toolGanttCellWidth);
            var cellHeight = Extensions.GetLocalSetting(this.ServiceProvider, _pageID + _toolGanttCellHeight);

            cellWidth = cellWidth == null ? this.CellWidthSize.ToString() : cellWidth;
            cellHeight = cellHeight == null ? this.CellHeightSize.ToString() : cellHeight;

            if (!string.IsNullOrEmpty(cellWidth))
                this.ganttSizeControl.CellWidth = Convert.ToInt32(cellWidth);

            if (!string.IsNullOrEmpty(cellHeight))
                this.ganttSizeControl.CellHeight = Convert.ToInt32(cellHeight);
        }

        private void SetShopIDComboBox()
        {
            ComboHelper.AddDataToComboBox(this.ShopIDComboBox, _result, EqpGanttChartData.EQP_TABLE_NAME
                , EqpGanttChartData.Eqp.Schema.SHOP_ID, false);

            this.ShopIDComboBox.SelectedIndex = 0;
        }

        private void SetProductIDControl(string targetShopID)
        {
            this.prodCheckedBox.Properties.Items.Clear();

            var modelContext = _result.GetCtx<ModelDataContext>();
            var dtToolArrange = modelContext.ToolArrange.Where(x => x.SHOP_ID == targetShopID).OrderBy(x => x.PRODUCT_ID);

            HashSet<string> prodIDList = new HashSet<string>();
            foreach (var row in dtToolArrange)
            {
                if (string.IsNullOrEmpty(row.PRODUCT_ID))
                    continue;

                if (prodIDList.Contains(row.PRODUCT_ID) == false)
                {
                    prodIDList.Add(row.PRODUCT_ID);
                    this.prodCheckedBox.Properties.Items.Add(row.PRODUCT_ID);
                }
            }

            if (this.prodCheckedBox.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in prodCheckedBox.Properties.Items)
                    item.CheckState = CheckState.Checked;
            }
        }

        private void SetStepIDControl(string targetShopID)
        {
            this.stepCheckedBox.Properties.Items.Clear();

            var modelContext = _result.GetCtx<ModelDataContext>();
            var dtToolArrange = modelContext.ToolArrange.Where(x => x.SHOP_ID == targetShopID).OrderBy(x => x.STEP_ID);

            HashSet<string> stepIDList = new HashSet<string>();
            foreach (var row in dtToolArrange)
            {
                if (string.IsNullOrEmpty(row.STEP_ID))
                    continue;

                if (stepIDList.Contains(row.STEP_ID) == false)
                {
                    stepIDList.Add(row.STEP_ID);
                    this.stepCheckedBox.Properties.Items.Add(row.STEP_ID);
                }
            }

            if (this.stepCheckedBox.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in stepCheckedBox.Properties.Items)
                    item.CheckState = CheckState.Checked;
            }
        }

        private void SetMaskIDControl(string targetShopID)
        {
            this.masksCheckedBox.Properties.Items.Clear();

            var modelContext = _result.GetCtx<ModelDataContext>();
            var dtTool = modelContext.Tool.Where(x => x.SHOP_ID == targetShopID).OrderBy(x => x.TOOL_ID);
            
            List<string> masksList = new List<string>();
            foreach (var row in dtTool)
            {
                if (string.IsNullOrEmpty(row.TOOL_ID))
                    continue;

                if (CommonHelper.Equals(row.TOOL_TYPE, "MASK") == false)
                    continue;

                if (masksList.Contains(row.TOOL_ID) == false)
                {
                    masksList.Add(row.TOOL_ID);
                    this.masksCheckedBox.Properties.Items.Add(row.TOOL_ID);
                }
            }

            if (this.masksCheckedBox.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in masksCheckedBox.Properties.Items)
                    item.CheckState = CheckState.Checked;
            }
        }

        private Tuple<int, int> LoadCellSize()
        {
            var strCellWidth = Globals.GetLocalSetting(this.ServiceProvider, _pageID + _toolGanttCellWidth);
            var strCellHeight = Globals.GetLocalSetting(this.ServiceProvider, _pageID + _toolGanttCellHeight);

            return new Tuple<int, int>(Convert.ToInt32(strCellWidth), Convert.ToInt32(strCellHeight));
        }

        private void InitializeData()
        {
            base.SetWaitDialogLoadCaption("Tool Info. ");

            _eqpMgr = new EqpMaster();
            _eqpMgr.LoadEqp(_result);

            base.SetWaitDialogLoadCaption("Tool Gantt info.");

            _gantt = new ToolGantt(this.grid1, _result, _resultCtx, this.TargetShopID, _planStartTime, _eqpMgr);

            _gantt.DefaultColumnWidth = CellWidthSize;
            _gantt.DefaultRowHeight = CellHeightSize;

            _gantt.MouseSelType = MouseSelectType.Product;
                        
            this.SetColumnHeaderView();
        }

        private void SetColumnHeaderView()
        {
            var styleBook = this._gantt.Workbook.Styles;

            var customHeader = styleBook.Add("CustomHeader");
            var customHeaderCenter = styleBook.Add("CustomHeaderCenter");
            var customHeaderRight = styleBook.Add("CustomHeaderRight");
            var customDefaultCenter = styleBook.Add("CustomDefaultCenter");

            customHeader.Fill.BackgroundColor = Color.MintCream;

            customHeaderCenter.Fill.BackgroundColor = Color.MintCream;
            customHeaderCenter.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            customHeaderCenter.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

            customHeaderRight.Fill.BackgroundColor = Color.MintCream;
            customHeaderRight.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            customHeaderRight.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;

            customDefaultCenter.Fill.BackgroundColor = Color.WhiteSmoke;
            customDefaultCenter.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            customDefaultCenter.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
        }

        private void BindEvents()
        {
            _gantt.HeaderHourChanged += new GanttColumnHeaderEventHandler(GanttView_HeaderHourChanged);
            _gantt.HeaderShiftChanged += new GanttColumnHeaderEventHandler(GanttView_HeaderShiftChanged);
            _gantt.HeaderDone += new GanttColumnHeaderEventHandler(GanttView_HeaderDone);

            _gantt.BindItemAdded += new GanttItemEventHandler(GanttView_BindItemAdded);
            _gantt.BindRowAdding += new GanttRowEventHandler(GanttView_BindRowAdding);
            _gantt.BindRowAdded += new GanttRowEventHandler(GanttView_BindRowAdded);
            _gantt.BindBarAdded += new GanttCellEventHandler(GanttView_BindBarAdded);
            _gantt.BindDone += new EventHandler(GanttView_BindDone);

            _gantt.BarClick += new BarEventHandler(GanttView_BarClick);
            //_gantt.BarDoubleClick += new BarEventHandler(GanttView_BarDoubleClick);
            _gantt.BarDraw += new BarDrawEventHandler(GanttView_BarDraw);

            _gantt.CellClick += new CellEventHandler(GanttView_CellClick);
        }
        
        #region bar handling
        void GanttView_BarDraw(object sender, BarDrawEventArgs args)
        {
            var bar = args.Bar as ToolBar;

            args.Background = _gantt.GetBrushInfo(bar, string.Empty);
            args.DrawFrame = _gantt.EnableSelect && _gantt.SelectedBar != null && !_gantt.CompareToSelectedBar(bar, string.Empty);
            args.FrameColor = Color.White;
            args.DrawFrame = true;

            args.ForeColor = bar.IsGhostBar && bar.State != EqpState.PM ? Color.Gray
                : bar.State == EqpState.DOWN ? Color.White : Color.Black;

            if (bar.State == EqpState.PM)
                args.ForeColor = Color.White;

            if (_gantt.IsOnlyToolMode)
                args.Text = bar.GetTitle(true);
            else
                args.Text = bar.GetTitle(false);

            args.DrawDefault = true;
        }

        //public void GanttView_BarDoubleClick(object sender, BarEventArgs e)
        //{
        //    if (e.Bar != null)
        //    {
        //        var dialog = new ToolBarDetailView();

        //        dialog.SetBarInfo(e.Bar as ToolBar);

        //        //dialog.TopMost = true;
        //        //dialog.StartPosition = FormStartPosition.Manual;
        //        dialog.Text = "Bar Information";

        //        dialog.Focus();
        //        dialog.Show();
        //    }
        //}

        void GanttView_BarClick(object sender, BarEventArgs e)
        {
            if (_gantt.ColumnHeader == null)
                return;

            this.grid1.BeginUpdate();

            if (e.Mouse.Button == MouseButtons.Right && e.Bar != null)
            {
                _gantt.TurnOnSelectMode();

                _gantt.SelectedBar = e.Bar as ToolBar;
            }
            else
            {
                _gantt.TurnOffSelectMode();
            }

            this.grid1.EndUpdate();
            this.grid1.Refresh();

            var bar = e.Bar as ToolBar;

            ViewDispDetail(bar); 
            ViewEqpProcessDetail(bar.ToolID);
        }

        private void GanttView_CellClick(object sender, CellEventArgs e)
        {
            
        }

        private void grid1_PopupMenuShowing(object sender, DevExpress.XtraSpreadsheet.PopupMenuShowingEventArgs e)
        {
            e.Menu = null;
        }
  
        private void ganttSizeControl1_CellWidthChanged(object sender, EventArgs e)
        {
            SaveCellSize();
        }

        private void ganttSizeControl1_CellHeightChanged(object sender, EventArgs e)
        {
            SaveCellSize();
        }

        private void SaveCellSize()
        {
            Globals.SetLocalSetting(this.ServiceProvider, _pageID + _toolGanttCellWidth, this.CellWidthSize.ToString());
            Globals.SetLocalSetting(this.ServiceProvider, _pageID + _toolGanttCellHeight, this.CellHeightSize.ToString());
        }

        private void teEqpId_EditValueChanged(object sender, EventArgs e)
        {

        }

        #endregion

        #region build bars

        void GanttView_BindDone(object sender, EventArgs e)
        {
            var colHeader = _gantt.ColumnHeader;

            // 마지막 Row 값 세팅
            if (this.IsOnlyToolMode == false)
            {
                XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(_startSameEqpRowIdx, ColName.MaskChangeCnt), _subJobChg);
            }

            XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(_startSameEqpRowIdx, ColName.TotalRun), _subTotalTO);

            #region // mask count
            //totChgRate = totChgRate / calCount;
            //loadRateColName = string.Format("{0} \n({1}%)", ColName.LoadRate, totalRateForAct.ToString("#.#"));

            //var jobChgColName = string.Format("{0} \n({1})", ColName.MaskChangeCnt, _totalJobChg.ToString("#.#"));

            //XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(0, ColName.MaskChangeCnt), jobChgColName);

            //colHeader.GetCellInfo(0, ColName.MaskChangeCnt).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            #endregion

            string totalToColName = string.Format("{0} \n({1})", "TOTAL", _totalTO.ToString("#.#"));
            XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(0, ColName.TotalRun), totalToColName);

            for (int i=0; i < _gantt.LastRowIndex; i++)
            {
                colHeader.GetCellInfo(i, ColName.MaskID).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            }

            colHeader.GetCellInfo(0, ColName.MaskID).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.RunQtySum).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.TotalRun).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;

            colHeader.GetCellInfo(0, ColName.MaskID).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.RunQtySum).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.TotalRun).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

            if (this.IsOnlyToolMode == false)
            {
                colHeader.GetCellInfo(0, ColName.EqpId).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                colHeader.GetCellInfo(0, ColName.EqpId).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            }

            if (SelectViewMode == ToolGantt.ViewMode.SHOP)
            {
                int fromRowIdx = this.IsOnlyToolMode == false ? _startSameEqpRowIdx : _startSameRowKeyIdx;
                MergeRows(fromRowIdx, _gantt.LastRowIndex);
            }
            
            PaintTotColumnCell();
        }

        void GanttView_BindBarAdded(object sender, GanttCellEventArgs args)
        {
            args.Bar.CumulateQty(ref _rowsumti, ref _rowsumto);

            _rowsumJobChg += args.Bar.BarList.Where(x => x.State == EqpState.SETUP).Count();

            double loadTime = args.Bar.BarList.Where(x => x.State == EqpState.BUSY || x.State == EqpState.IDLERUN || x.State == EqpState.SETUP)
                .Sum(x => (x.TkoutTime - x.TkinTime).TotalSeconds);

            _rowsumLoadTimeFrBar += loadTime;
        }

        void GanttView_BindRowAdding(object sender, GanttRowEventArgs args)
        {
            var worksheet = this._gantt.Worksheet;

            var info = args.Item as ToolGantt.GanttInfo;
            var colHeader = _gantt.ColumnHeader;

            string shopID = info.ShopID;
            string eqpID = info.EqpID;
            string toolID = info.ToolID;

            SetRowHeaderValue(args.RowIndex, shopID, eqpID, string.IsNullOrEmpty(args.Key) ? "-" : args.Key, toolID);

            this._rowsumti = 0; 
            this._rowsumto = 0; 
            this._rowsumJobChg = 0;
            this._rowsumLoadTimeFrBar = 0;

            if (args.Node == null)
                return;

            var rows = args.Node.LinkedBarList;

            if (rows.Count > 1 && args.Index > 0 && args.Index < rows.Count - 1)
            {
                XtraSheetHelper.SetCellText(colHeader.GetCellInfo(args.RowIndex, ColName.ShopID), shopID);
                XtraSheetHelper.SetCellText(colHeader.GetCellInfo(args.RowIndex, ColName.MaskID), toolID);
                if (this.IsOnlyToolMode == false)
                    XtraSheetHelper.SetCellText(colHeader.GetCellInfo(args.RowIndex, ColName.EqpId), eqpID);

                PaintRowKeyedCell(args.RowIndex, _currColor);
            }
        }

        void GanttView_BindRowAdded(object sender, GanttRowEventArgs args)
        {
            var info = args.Item as ToolGantt.GanttInfo;
            var colHeader = _gantt.ColumnHeader;

            if (_totalLoadTImeFrBarDic.ContainsKey(info.EqpID) == false)
                _totalLoadTImeFrBarDic.Add(info.EqpID, _rowsumLoadTimeFrBar);

            if (this.IsOnlyToolMode)
            {
                string sLoadRate = string.Empty;
                double loadRate = 0;
                sLoadRate = Math.Round(loadRate, 1).ToString() + "%";
                //XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(args.RowIndex, ColName.MaskChangeCnt), _rowsumJobChg);
            }

            XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(args.RowIndex, ColName.RunQtySum), _rowsumto);

            _subTotalTI += _rowsumti;
            _subTotalTO += _rowsumto;
            _totalTO += _rowsumto;
            _subJobChg += _rowsumJobChg;
            _totalJobChg += _rowsumJobChg;
        }

        void GanttView_BindItemAdded(object sender, GanttItemEventArgs args)
        {
            var info = args.Item as ToolGantt.GanttInfo;
            var colHeader = _gantt.ColumnHeader;
        }

        #endregion

        #region build Header
        void GanttView_HeaderDone(object sender, GanttColumnHeaderEventArgs e)
        {
            var colHeader = e.ColumnHeader;

            //colHeader.AddColumn(new XtraSheetHelper.SfColumn(ColName.MaskChangeCnt, 70));

            colHeader.AddColumn(new XtraSheetHelper.SfColumn(ColName.RunQtySum, 70));
            
            colHeader.AddColumn(new XtraSheetHelper.SfColumn(ColName.TotalRun, 82));

            this.ganttSizeControl.LeftExceptCount = this._gantt.FixedColCount;
            this.ganttSizeControl.TopExceptCount = this._gantt.FixedRowCount;

            this.ganttSizeControl.RightExceptCount = 3;
        }

        void GanttView_HeaderShiftChanged(object sender, GanttColumnHeaderEventArgs args)
        {
            var startColName = args.Time.ToString(_gantt.DateKeyPattern);
            var endColName = args.Time.AddHours(ShopCalendar.ShiftHours - 1).ToString(_gantt.DateKeyPattern);

            if (this.StartDate.ShiftStartTimeOfDayT() == args.Time)
                startColName = this.StartDate.ToString(_gantt.DateKeyPattern);
            else if (this.EndDate.ShiftStartTimeOfDayT() == args.Time)
                endColName = this.EndDate.ToString(_gantt.DateKeyPattern);

            args.ColumnHeader.AddGroupColumn(
                new XtraSheetHelper.SfGroupColumn(_gantt.GetJobChgShiftCntFormat(args.Time), startColName, endColName)
                );   
        }

        void GanttView_HeaderHourChanged(object sender, GanttColumnHeaderEventArgs args)
        {
            string key = args.Time.ToString(_gantt.DateKeyPattern);
            string caption = _gantt.GetJobChgHourCntFormat(args.Time);

            args.ColumnHeader.AddColumn(new XtraSheetHelper.SfColumn(key, caption, _gantt.DefaultColumnWidth, true, false));    
        }
        
        #endregion
        
        #region // BindData

        protected void BindData()
        {
            GenerateToolGantt();

            BindGrid();
        }


        List<ToolGantt.GanttInfo> _list;
        private void GenerateToolGantt()
        {
            _gantt.BuildGantt(
                this.IsOnlyToolMode,
                this.TargetShopID,
                this.SelectedProdIDs,
                this.SelectedStepIDs,
                this.SelectedMaskIDs,
                this.StartDate,
                this.EndDate,
                this._planStartTime,
                this.EqpIdPattern
            );

            Dictionary<string, ToolGantt.GanttInfo> collectData = _gantt.Table;
            _list = new List<ToolGantt.GanttInfo>(collectData.Values);

            _gantt.Expand(this.IsStepView);
        }

        #endregion

        #region //BindGrid
        bool _isFirst = true;
        string _prePrcGroup = string.Empty;
        string _preShopID = string.Empty;
        string _preToolID = string.Empty;
        string _preEqpID = string.Empty;
        string _preRowKey = string.Empty;

        Color _preColor = XtraSheetHelper.AltColor;
        Color _currColor = XtraSheetHelper.AltColor2;

        int _startSameEqpRowIdx = 0;
        int _startSameRowKeyIdx = 0;
        double _subTotalTI = 0;
        double _subTotalTO = 0;
        double _totalTO = 0;
        double _subJobChg = 0;
        double _totalJobChg = 0;
        Dictionary<string, double> _totalLoadTimeFrStatDic;
        Dictionary<string, double> _totalLoadTImeFrBarDic;

        double _rowsumti = 0;
        double _rowsumto = 0;
        double _rowsumJobChg = 0;
        double _rowsumLoadTimeFrBar = 0;

        public Dictionary<string, int> _moveByLayer;


        private void BindGrid()
        {
            if (IsOnlyToolMode || SelectViewMode == ToolGantt.ViewMode.MASK)
                _list.Sort(new ToolGantt.CompareGanttInfo(ToolGantt.SortOptions.TOOL_ID, ToolGantt.SortOptions.EQP_ID));
            else
                _list.Sort(new ToolGantt.CompareGanttInfo(ToolGantt.SortOptions.EQP_ID, ToolGantt.SortOptions.TOOL_ID));

            _gantt.Workbook.BeginUpdate();
            _gantt.ResetWorksheet();

            _gantt.TurnOffSelectMode();

            SetColumnHeaders();

            var colHeader = _gantt.ColumnHeader;
            _gantt.SchedBarComparer = new CompareMBarList();

            _isFirst = true;
            _preRowKey = string.Empty;

            _preColor = XtraSheetHelper.AltColor;
            _currColor = XtraSheetHelper.AltColor2;

            _startSameEqpRowIdx = 0;
            _startSameRowKeyIdx = 0;
            _subTotalTI = 0;
            _subTotalTO = 0;
            _subJobChg = 0;

            _totalTO = 0;
            _totalJobChg = 0;
            _totalLoadTimeFrStatDic = new Dictionary<string, double>();
            _totalLoadTImeFrBarDic = new Dictionary<string, double>();

            _gantt.Bind(_list);
            _gantt.Workbook.EndUpdate();
        }

        private void SetRowHeaderValue(int rowIndex, string shopID, string eqpId, string stepSeq, string toolID)
        {   
            if (IsOnlyToolMode)
            {
                string curKey = toolID;
                var colHeader = _gantt.ColumnHeader;

                if (_isFirst)
                {
                    _preShopID = shopID;
                    _preToolID = toolID;
                    _preEqpID = eqpId;
                    _preRowKey = curKey;
                    _startSameEqpRowIdx = rowIndex;
                    _startSameRowKeyIdx = rowIndex;

                    _isFirst = false;
                }

                if (_isFirst == false && toolID.Equals(_preToolID) == false)
                {
                    MergeRows(_startSameEqpRowIdx, rowIndex - 1);
                    _startSameEqpRowIdx = rowIndex;
                }

                if (_isFirst == false && shopID.Equals(_preShopID) == false)
                {
                    MergeRows(_startSameEqpRowIdx, rowIndex - 1);
                    _startSameEqpRowIdx = rowIndex;
                }

                if (_isFirst == false && toolID.Equals(_preToolID) == false)
                {
                    if (_startSameEqpRowIdx > 1)
                    {
                        XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(_startSameEqpRowIdx - 1, ColName.TotalRun), _subTotalTO);

                        if (this.IsOnlyToolMode == false)
                        {
                            //XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(_startSameEqpRowIdx - 1, ColName.MaskChangeCnt), _subJobChg);
                        }
                    }

                    _preShopID = shopID;
                    _preToolID = toolID;
                    _preEqpID = eqpId;
                    _startSameEqpRowIdx = rowIndex;
                    _subTotalTI = 0;
                    _subTotalTO = 0;
                    _subJobChg = 0;
                }

                if (_isFirst == false && curKey.Equals(_preRowKey) == false)
                {
                    MergeRows(_startSameRowKeyIdx, rowIndex - 1);

                    Color tmp = _preColor;
                    _preColor = _currColor;
                    _currColor = tmp;
                    _preRowKey = curKey;
                    //_startSameRowKeyIdx = rowIndex;
                }

                PaintRowKeyedCell(rowIndex, _currColor);

                XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.ShopID), shopID);
                XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.MaskID), toolID);

                if (this.IsOnlyToolMode == false)
                    XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.EqpId), eqpId);

                _gantt.Worksheet[rowIndex, colHeader.TryGetColumnIndex(ColName.ShopID)].SetCellText(shopID);
                colHeader.GetCellInfo(rowIndex, ColName.ShopID).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                colHeader.GetCellInfo(rowIndex, ColName.ShopID).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

                _gantt.Worksheet[rowIndex, colHeader.TryGetColumnIndex(ColName.MaskID)].SetCellText(toolID);

                colHeader.GetCellInfo(rowIndex, ColName.MaskID).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                colHeader.GetCellInfo(rowIndex, ColName.MaskID).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

                if (this.IsOnlyToolMode == false)
                {
                    colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                }
            }
            else
            {
                string curKey = eqpId;
                string curToolKey = toolID;
                var colHeader = _gantt.ColumnHeader;

                if (_isFirst)
                {
                    _preToolID = toolID;
                    _preEqpID = eqpId;
                    _preRowKey = curKey;
                    _startSameEqpRowIdx = rowIndex;
                    _startSameRowKeyIdx = rowIndex;

                    _isFirst = false;
                }

                if (SelectViewMode == ToolGantt.ViewMode.EQP)
                {
                    if (_isFirst == false && (toolID.Equals(_preToolID) == false || eqpId.Equals(_preEqpID) == false))
                    {
                        MergeRows(_startSameEqpRowIdx, rowIndex - 1);
                        _preToolID = toolID;
                        _startSameEqpRowIdx = rowIndex;
                    }

                    if (_isFirst == false && eqpId.Equals(_preEqpID) == false)
                    {
                        if (_startSameEqpRowIdx > 1)
                        {
                            string sLoadRate = string.Empty;
                            XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(_startSameEqpRowIdx - 1, ColName.TotalRun), _subTotalTO);
                        }

                        _preToolID = toolID;
                        _preEqpID = eqpId;
                        _startSameEqpRowIdx = rowIndex;
                        _subTotalTI = 0;
                        _subTotalTO = 0;
                        _subJobChg = 0;
                    }

                    if (_isFirst == false && curKey.Equals(_preRowKey) == false)
                    {
                        MergeRows(_startSameRowKeyIdx, rowIndex - 1);

                        Color tmp = _preColor;
                        _preColor = _currColor;
                        _currColor = tmp;
                        _preRowKey = curKey;
                        _startSameRowKeyIdx = rowIndex;

                    }

                    PaintRowKeyedCell(rowIndex, _currColor);
                                       
                    XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.MaskID), toolID);
                    _gantt.Worksheet[rowIndex, colHeader.TryGetColumnIndex(ColName.MaskID)].SetCellText(toolID);

                    XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.EqpId), eqpId);
                    colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                    colHeader.GetCellInfo(rowIndex, ColName.MaskID).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    colHeader.GetCellInfo(rowIndex, ColName.MaskID).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                }

                if (SelectViewMode == ToolGantt.ViewMode.MASK)
                {

                    if (_isFirst == false && (toolID.Equals(_preToolID) == false || eqpId.Equals(_preEqpID) == false))
                    {
                        MergeRows(_startSameEqpRowIdx, rowIndex - 1, 1);
                        _preToolID = toolID;
                        _preEqpID = eqpId;
                        _startSameEqpRowIdx = rowIndex;
                    }

                    if (_isFirst == false && eqpId.Equals(_preToolID) == false)
                    {
                        if (_startSameEqpRowIdx > 1)
                        {
                            string sLoadRate = string.Empty;
                            XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(_startSameEqpRowIdx - 1, ColName.TotalRun), _subTotalTO);
                        }

                        _preToolID = toolID;
                        _preEqpID = eqpId;
                        _startSameEqpRowIdx = rowIndex;
                        _subTotalTI = 0;
                        _subTotalTO = 0;
                        _subJobChg = 0;
                    }

                    if (_isFirst == false && curKey.Equals(_preRowKey) == false)
                    {
                        MergeRows(_startSameRowKeyIdx, rowIndex - 1, 1);

                        Color tmp = _preColor;
                        _preColor = _currColor;
                        _currColor = tmp;
                        _preRowKey = curKey;
                        _startSameRowKeyIdx = rowIndex;

                    }

                    PaintRowKeyedCell(rowIndex, _currColor);

                    XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.MaskID), toolID);
                    _gantt.Worksheet[rowIndex, colHeader.TryGetColumnIndex(ColName.MaskID)].SetCellText(toolID);

                    XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.EqpId), eqpId);
                    colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                    colHeader.GetCellInfo(rowIndex, ColName.MaskID).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    colHeader.GetCellInfo(rowIndex, ColName.MaskID).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

                }
            }
        }

        private void MergeRows(int fromRowIdx, int toRowIdx, int colIdx = 0)
        {
            var worksheet = this._gantt.Worksheet;

            worksheet.MergeRowsOneColumn(fromRowIdx, toRowIdx, colIdx);

            if (colIdx > 1)
                SetBorder(fromRowIdx, toRowIdx);
        }

        private void SetBorder(int fromRowIdx, int toRowIdx)
        {
            var worksheet = this._gantt.Worksheet;
            var color = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));

            for (int i = fromRowIdx; i <= toRowIdx; i++)
            {
                if (i == fromRowIdx)
                {
                    XtraSheetHelper.SetRowBorderTopLine(worksheet, i, color, BorderLineStyle.Thin);
                }
                else
                {
                    XtraSheetHelper.SetRowBorderTopLine(worksheet, i, Color.Transparent, BorderLineStyle.Thin);
                }

                XtraSheetHelper.SetRowBorderBottomLine(worksheet, i, Color.Transparent);
            }
        }

        private void PaintRowKeyedCell(int rowIndex, Color color)
        {
            var worksheet = this._gantt.Worksheet;

            int toColIndex = 2;
            //if (SelectViewMode == ToolGantt.ViewMode.EQP)
            //    toColIndex = 2;
            //else if (SelectViewMode == ToolGantt.ViewMode.SHOP)
            //    toColIndex = 2;
            //else
            //    toColIndex = 2;

            for (int colindex = 0; colindex < toColIndex; colindex++)
            {
                worksheet[rowIndex, colindex].FillColor = color;
            }
        }

        private void PaintTotColumnCell()
        {
            var worksheet = this._gantt.Worksheet;

            var colHeader = this._gantt.ColumnHeader;

            //worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(ColName.MaskChangeCnt), Color.FromArgb(248, 223, 224));

            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(ColName.RunQtySum), Color.FromArgb(219, 236, 216));

            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(ColName.TotalRun), Color.FromArgb(204, 255, 195));
        }

        #endregion

        #region ColName

        struct ColName
        {
            public static string ShopID = "SHOP_ID";
            public static string ToolID = "TOOL_ID";
            public static string EqpId = "EQP_ID";
            public static string State = "State";
            public static string OperName = "OPERATIONNAME";
            public static string LotID = "LOTID";
            public static string ProductID = "ProductID";
            public static string StartTime = "START_TIME";
            public static string EndTime = "END_TIME";
            public static string TrackInTime = "TRACK_IN_TIME";
            public static string TrackOutTime = "TRACK_OUT_TIME";
            public static string GapTime = "GAP_TIME";
            public static string NextTkinTime = "NEXT_TKIN_TIME";
            public static string Layer = "LAYER";
            public static string MaskID = "MASK_ID";
            public static string ProductKind = "PRODUCT_KIND";

            public static string TIQty = "T/I QTY";
            public static string TOQty = "T/O QTY";

            public static string MaskChangeCnt = "MASK CHG";

            public static string TIQtySum = "T/I\nQTY";
            public static string RunQtySum = "RUN QTY";
            public static string TITotal = "T/I\nTOTAL";
            public static string TotalRun = "TOTAL RUN";
        }

        #endregion

        #region // SetColumnHeader

        protected void SetColumnHeaders()
        {
            int colCount = Convert.ToInt32(fromShiftComboBox.Value) * (int)ShopCalendar.ShiftHours + 4 + 2;
            
            if (this.IsOnlyToolMode)
            {
                _gantt.FixedColCount = 2;
            }
            else
            {
                colCount += 1;
                _gantt.FixedColCount = 2;
            }

            if (this.IsStepView)
                _gantt.FixedRowCount = 3;
            else
                _gantt.FixedRowCount = 2;


            if (this.IsOnlyToolMode)
            {                    
                _gantt.SetColumnHeaders(colCount,
                    new XtraSheetHelper.SfColumn(ColName.ShopID, ColName.ShopID, 80),
                    new XtraSheetHelper.SfColumn(ColName.MaskID, ColName.MaskID, 150));
            }
            else
            {
                if (SelectViewMode == ToolGantt.ViewMode.EQP)
                {
                    _gantt.SetColumnHeaders(colCount,
                        new XtraSheetHelper.SfColumn(ColName.EqpId, ColName.EqpId, 93),
                        new XtraSheetHelper.SfColumn(ColName.MaskID, ColName.MaskID, 150));
                }

                if (SelectViewMode == ToolGantt.ViewMode.MASK)
                {
                    _gantt.SetColumnHeaders(colCount,
                        new XtraSheetHelper.SfColumn(ColName.MaskID, ColName.MaskID, 150),
                        new XtraSheetHelper.SfColumn(ColName.EqpId, ColName.EqpId, 93));
                }
            }

            _gantt.Worksheet.Rows[0].Style = _gantt.Workbook.Styles["CustomHeader"];
            _gantt.Worksheet.Rows[1].Style = _gantt.Workbook.Styles["CustomHeader"];

            _gantt.Worksheet.SelectedCell[0, 0].Style = _gantt.Workbook.Styles["CustomHeaderCenter"];
            _gantt.Worksheet.SelectedCell[0, 1].Style = _gantt.Workbook.Styles["CustomHeaderCenter"];
        }


        #endregion

        #region //Event Handlers

        private void queryButton_Click(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;

            Search();
        }

        private void Search()
        {
            _gantt.DefaultColumnWidth = this.CellWidthSize;
            _gantt.DefaultRowHeight = this.CellHeightSize;

            Cursor.Current = Cursors.WaitCursor;
            queryButton.Enabled = false;
            BindData();
            queryButton.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (_list != null) BindGrid();

            Cursor.Current = Cursors.Default;

        }
        
        private void shopIDComboBox_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (_isEndLoadDocument == false)
                return;

            SetProductIDControl(this.TargetShopID);
            SetStepIDControl(this.TargetShopID);
            SetMaskIDControl(this.TargetShopID);
        }

        private void ViewDispDetail(ToolBar bar = null)
        {
            if (this.DispView == null)
                return;

            this.DispView.SetBarInfo(bar);
        }

        #region // GetGridDetail
        private void ViewEqpProcessDetail(string toolID)
        {
            List<ToolGantt.GanttInfo> list = GetGanttInfo(toolID);

            if (list == null)
                return;

            DataTable dt = CrateDataTable();

            BindProcessData(dt, list);
        }

        private List<ToolGantt.GanttInfo> GetGanttInfo(string toolID)
        {
            List<ToolGantt.GanttInfo> result = new List<ToolGantt.GanttInfo>();

            foreach (ToolGantt.GanttInfo info in _list)
            {
                if (info.ToolID == toolID)
                    result.Add(info);
            }
            if (result.Count > 0)
                return result;
            else
                return null;
        }
        #endregion

        private void radioGroup1_EditValueChanged(object sender, EventArgs e)
        {
            if (_gantt == null)
                return;

            _gantt.MouseSelType = this.SelectedMouseSelectType;
        }


        private void BindProcessData(DataTable dt, List<ToolGantt.GanttInfo> list)
        {
            foreach (ToolGantt.GanttInfo info in list)
            {
                foreach (var item in info.Items)
                {
                    string step = item.Key;

                    foreach (ToolBar b in item.Value)
                    {
                        if (b.BarKey != step || b.State == EqpState.PM)
                            continue;

                        DataRow drow = dt.NewRow();
                        drow[ColName.ShopID] = b.ShopID;
                        drow[ColName.EqpId] = b.EqpId;
                        drow[ColName.MaskID] = b.ToolID;

                        drow[ColName.State] = b.State.ToString();

                        if (b.State == EqpState.BUSY || b.State == EqpState.IDLERUN)
                        {
                            drow[ColName.ProductID] = b.ProductId;
                            drow[ColName.Layer] = b.Layer;
                            drow[ColName.OperName] = b.StepId;

                        }
                        drow[ColName.StartTime] = b.TkinTime.ToString("yyyy-MM-dd HH:mm:ss");
                        drow[ColName.EndTime] = b.TkoutTime.ToString("yyyy-MM-dd HH:mm:ss");


                        if (b.State == EqpState.BUSY)
                        {
                            drow[ColName.TIQty] = b.TIQty;
                        }

                        dt.Rows.Add(drow);
                    }
                }
            }

            detailGridControl.BeginUpdate();
            detailGridControl.DataSource = new DataView(dt, "", ColName.StartTime, DataViewRowState.CurrentRows);
            detailGridControl.EndUpdate();

            detailGridView.BestFitColumns();

            //Globals.GetColumnWeith(detailGridView);
        }

        private DataTable CrateDataTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(ColName.ShopID, typeof(string));
            dt.Columns[ColName.ShopID].Caption = ColName.ShopID;

            dt.Columns.Add(ColName.MaskID, typeof(string));
            dt.Columns[ColName.MaskID].Caption = ColName.MaskID;
            dt.Columns.Add(ColName.EqpId, typeof(string));
            dt.Columns[ColName.EqpId].Caption = ColName.EqpId;
            dt.Columns.Add(ColName.State, typeof(string));
            dt.Columns[ColName.State].Caption = "STATUS";

            dt.Columns.Add(ColName.Layer, typeof(string));
            dt.Columns[ColName.Layer].Caption = ColName.Layer;

            dt.Columns.Add(ColName.OperName, typeof(string));
            dt.Columns[ColName.OperName].Caption = "STEP_ID";
            dt.Columns.Add(ColName.ProductID, typeof(string));
            dt.Columns[ColName.ProductID].Caption = "PRODUCT_ID";
            dt.Columns.Add(ColName.StartTime, typeof(string));
            dt.Columns[ColName.StartTime].Caption = "START_TIME";
            dt.Columns.Add(ColName.EndTime, typeof(string));
            dt.Columns[ColName.EndTime].Caption = "END_TIME";

            dt.Columns.Add(ColName.TIQty, typeof(int));
            dt.Columns[ColName.TIQty].Caption = "QTY";

            return dt;
        }

        #endregion
    }
}
