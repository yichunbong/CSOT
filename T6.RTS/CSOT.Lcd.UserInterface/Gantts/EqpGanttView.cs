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
using Mozart.Studio.Application;
using System.Drawing.Drawing2D;
using CSOT.Lcd.UserInterface.Analysis;
using DevExpress.XtraGrid.Views.Grid;

namespace CSOT.Lcd.UserInterface.Gantts
{
    public partial class EqpGanttView : XtraGridControlView
    {
        private const string _pageID = "EqpGanttView";

        private IExperimentResultItem _result;
        private ResultDataContext _resultCtx;

        private EqpMaster EqpMgr { get; set; }
        private EqpGantt Gantt { get; set; }

        private List<EqpGantt.GanttInfo> CurrInfos { get; set; }                
                
        private DateTime PlanStartTime { get; set; }
        private bool IsBeforeFirstQuery { get; set; }

        private DetailBarInfoView DispView { get; set; }
        private ArrangeAnalysisView ArrangeView { get; set; }

        private bool IsNeedSetFocus { get; set; }

        #region PROPERTY
                
        private bool ShowLayerBar
        {
            get { return this.chkShowOtherLayerRun.Checked; }
        }

        private bool IsFilterDownEqp
        {
            get { return this.chkShowDownEqp.Checked == false; }
        }

        private bool IsShowSubEqp
        {
            get { return this.chkShowSubEqp.Checked; }
        }

        private bool IsShowProductColor
        {
            get { return this.chkShowProdColor.Checked; }
        }

        private bool IsProductInBarTitle
        {
            get 
            {
                var item = this.barTitleGroup.Properties.Items[this.barTitleGroup.SelectedIndex];

                return StringHelper.Equals(item.Value as string, "Product");
            }
        }

        private EqpGantt.ViewMode SelectViewMode
        {
            get
            {
                var item = this.firstRowHeaderGroup.Properties.Items[this.firstRowHeaderGroup.SelectedIndex];
                string str = item.Value as string;

                EqpGantt.ViewMode mode;
                if (Enum.TryParse(str, out mode))
                    return mode;

                return EqpGantt.ViewMode.EQPGROUP;
            }
        }

        private bool IsEqpViewMode
        {
            get { return this.SelectViewMode == EqpGantt.ViewMode.EQP; }
        }
        
        private string TargetShopID
        {
            get { return this.ShopIDComboBox.Text; }
        }

        string EqpIdPattern
        {
            get
            {
                return this.eqpIDtextBox.Text.ToUpper();
            }
        }

        private List<string> SelectedEqpGroups
        {
            get
            {
                List<string> eqpGroupList = new List<string>();

                foreach (CheckedListBoxItem item in this.EqpGroupsCheckedBox.Properties.Items)
                {
                    if(item.CheckState == CheckState.Checked)
                        eqpGroupList.Add(item.ToString());
                }
                                                            
                return eqpGroupList;
            }
        }

        private DateTime StartDate
        {
            get
            {
                DateTime dt = dateEdit1.DateTime;

                return dt;
            }
        }

        private DateTime EndDate
        {
            get
            {
                return this.StartDate.AddHours(ShopCalendar.ShiftHours * Convert.ToInt32(fromShiftComboBox.Value));
            }
        }
        
        public int CellWidthSize
        {
            get { return ganttSizeControl1.CellWidth; }
        }

        public int CellHeightSize
        {
            get { return ganttSizeControl1.CellHeight; }
        }
        
        public string PatternOfProductID
        {
            get { return this.tePattern.Text; }
        }

        public MouseSelectType SelectedMouseSelectType
        {
            get
            {
                MouseSelectType type;
                string sele = this.rdoMouseType.Properties.Items[this.rdoMouseType.SelectedIndex].Value.ToString();
                Enum.TryParse(sele, out type);

                return type;
            }
        }

        #endregion

        public EqpGanttView()
        {
            InitializeComponent();
        }

        public EqpGanttView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        #region SET CONTROLS

        protected override void LoadDocument()
        {                        
            if (_result == null)
            {
                var item = (IMenuDocItem)this.Document.ProjectItem;
                _result = (IExperimentResultItem)item.Arguments[0];
            }
                        
            _resultCtx = _result.GetCtx<ResultDataContext>();
            
            Globals.InitFactoryTime(_result.Model);

            this.IsBeforeFirstQuery = true;

            InitControl();

            InitializeData();

            BindEvents();
        }

        private void InitControl()
        {               
            this.fromShiftComboBox.Value = Globals.GetResultPlanPeriod(_result) * ShopCalendar.ShiftCount;
            this.PlanStartTime = _result.StartTime;

            this.dateEdit1.DateTime = PlanStartTime;
            this.dateEdit1.Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            this.dateEdit1.Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;

            ComboHelper.AddDataToComboBox(this.ShopIDComboBox, _result, EqpGanttChartData.STD_STEP_TABLE_NAME,
                EqpGanttChartData.Eqp.Schema.SHOP_ID, false);
            
            this.ShopIDComboBox.SelectedIndex = 0;
                        
            this.barTitleGroup.SelectedIndex = 0;
            this.rdoMouseType.Properties.Items.GetItemByValue(MouseSelectType.PB.ToString()).Enabled = true;
            
            var cellWidth = Extensions.GetLocalSetting(this.ServiceProvider, _pageID + "ganttCellWidth");
            var cellHeight = Extensions.GetLocalSetting(this.ServiceProvider, _pageID + "ganttCellHeight");

            cellWidth = cellWidth == null ? this.CellWidthSize.ToString() : cellWidth;
            cellHeight = cellHeight == null ? this.CellHeightSize.ToString() : cellHeight;

            if (!string.IsNullOrEmpty(cellWidth))
                this.ganttSizeControl1.CellWidth = Convert.ToInt32(cellWidth);

            if (!string.IsNullOrEmpty(cellHeight))
                this.ganttSizeControl1.CellHeight = Convert.ToInt32(cellHeight);
            
            this.firstRowHeaderGroup.SelectedIndex = 0;
            this.barTitleGroup.SelectedIndex = 0;
            this.rdoMouseType.SelectedIndex = 0;

            detailGridView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus;
            detailGridView.SetFocusedRowModified();
            detailGridView.OptionsMenu.ShowGroupSummaryEditorItem = true;

            InitControl_DispDetail();
        }

        private void InitControl_DispDetail()
        {
            var presetList = GetPresetList();
            var view = new DetailBarInfoView(this.ServiceProvider, presetList, _result);

            dispatchingDetailpanel.Controls.Add(view);
            view.Dock = DockStyle.Fill;                       

            this.DispView = view;
        }

        private List<EqpGanttChartData.PresetInfo> GetPresetList()
        {
            List<EqpGanttChartData.PresetInfo> list = new List<EqpGanttChartData.PresetInfo>();

            DataTable dt = _result.LoadOutput(EqpGanttChartData.WEIGHT_PRESET_LOG_TABLE_NAME);
            //DataTable dt = _result.LoadInput(EqpGanttChartData.PRESET_INFO_TABLE_NAME);

            dt.DefaultView.Sort = EqpGanttChartData.PresetInfo.Schema.SEQUENCE + " " + "ASC";
            var table = dt.DefaultView.ToTable();

            foreach (DataRow row in table.Rows)
            {
                list.Add(new EqpGanttChartData.PresetInfo(row));
            }

            return list;
        }
        
        private void InitializeData()
        {
            base.SetWaitDialogLoadCaption("Eqp Info. ");

            this.EqpMgr = new EqpMaster();
            this.EqpMgr.LoadEqp(_result);
            
            this.Gantt = new EqpGantt(this.grid1, _result, _resultCtx, this.PlanStartTime, this.EqpMgr);
                        
            var gantt = this.Gantt;

            gantt.DefaultColumnWidth = this.CellWidthSize;
            gantt.DefaultRowHeight = this.CellHeightSize;

            gantt.MouseSelType = MouseSelectType.Product;
            gantt.BindChkListEqpGroup(EqpGroupsCheckedBox.Properties.Items, this.TargetShopID);

            if (this.EqpGroupsCheckedBox.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in EqpGroupsCheckedBox.Properties.Items)
                    item.CheckState = CheckState.Checked;
            }

            this.SetColumnHeaderView();

            base.SetWaitDialogLoadCaption("Eqp Gantt info.");
        }

        private void SetColumnHeaderView()
        {
            var styleBook = this.Gantt.Workbook.Styles;

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
            Gantt.HeaderHourChanged += new GanttColumnHeaderEventHandler(GanttView_HeaderHourChanged);
            Gantt.HeaderShiftChanged += new GanttColumnHeaderEventHandler(GanttView_HeaderShiftChanged);
            Gantt.HeaderDone += new GanttColumnHeaderEventHandler(GanttView_HeaderDone);

            Gantt.BindItemAdded += new GanttItemEventHandler(GanttView_BindItemAdded);
            Gantt.BindRowAdding += new GanttRowEventHandler(GanttView_BindRowAdding);
            Gantt.BindRowAdded += new GanttRowEventHandler(GanttView_BindRowAdded);
            Gantt.BindBarAdded += new GanttCellEventHandler(GanttView_BindBarAdded);
            Gantt.BindDone += new EventHandler(GanttView_BindDone);
            
            Gantt.BarClick += new BarEventHandler(GanttView_BarClick);
            Gantt.BarDoubleClick += new BarEventHandler(GanttView_BarDoubleClick);
            Gantt.BarDraw += new BarDrawEventHandler(GanttView_BarDraw);

            Gantt.CellClick += new CellEventHandler(GanttView_CellClick);
        }

        #region BAR HANDLING
                
        void GanttView_BarDraw(object sender, BarDrawEventArgs args)
        {
            var bar = args.Bar as GanttBar;
            var brushinfo = Gantt.GetBrushInfo(bar, this.PatternOfProductID);
            
            args.Background = brushinfo;
            args.DrawFrame = Gantt.EnableSelect && Gantt.SelectedBar != null && !Gantt.CompareToSelectedBar(bar, this.PatternOfProductID);
            args.FrameColor = Color.White;
            args.DrawFrame = true;

            Color foreColor = Color.Black;
            //if (this.IsShowProductColor)
            //    foreColor = ColorGenHelper.GetComplementaryColor(brushinfo.BackColor);

            if (bar.State == EqpState.DOWN || bar.State == EqpState.PM)
                foreColor = Color.White;
            else if (bar.IsGhostBar)
                foreColor = Color.Gray;

            args.ForeColor = foreColor;
            args.Text = bar.GetTitle(this.Gantt.IsProductInBarTitle);
            args.DrawDefault = true;

            if (bar != null && bar.OwnerType == "OwnerE")
            {
                var p = new Pen(Color.Black, 2);
                args.Graphics.DrawRectangle(p, bar.Bounds.X, bar.Bounds.Y, bar.Bounds.Width + 0.5f, bar.Bounds.Height);
            }

            if (bar != null && bar.EqpRecipe == "Y")
            {
                var p = new Pen(Color.Blue, 3);
                args.Graphics.DrawRectangle(p, bar.Bounds.X, bar.Bounds.Y, bar.Bounds.Width + 0.5f, bar.Bounds.Height);
            }
        }

        private void GanttView_BarDoubleClick(object sender, BarEventArgs e)
        {

        }

        private void GanttView_CellClick(object sender, CellEventArgs e)
        {

        }
                
        void GanttView_BarClick(object sender, BarEventArgs e)
        {
            if (this.Gantt.ColumnHeader == null)
                return;
                        
            this.grid1.BeginUpdate();

            if (e.Mouse.Button == MouseButtons.Right && e.Bar != null)
            {
                if (this.SelectedMouseSelectType != MouseSelectType.Pattern)
                {
                    Gantt.TurnOnSelectMode();

                    Gantt.SelectedBar = e.Bar as GanttBar;
                }
            }
            else
            {
                Gantt.TurnOffSelectMode();
            }

            this.grid1.EndUpdate();
            this.grid1.Refresh();
                        
            var bar = e.Bar as GanttBar;
                        
            ViewDispDetail(bar);            
            ViewEqpProcessDetail(bar);
            ViewArrangeDetail(bar);
            HighLightSelectRow(e.RowIndex);
        }

        private void HighLightSelectRow(int rowIndex)
        {
            var worksheet = this.Gantt.Worksheet;

            worksheet.SelectRowUsed(rowIndex, false, 0);
        }

        private void ViewDispDetail(GanttBar bar = null)
        {
            if (this.DispView == null)
                return;

            this.DispView.SetBarInfo(bar);
        }

        private void ViewArrangeDetail(GanttBar bar)
        {
            if (bar == null)
                return;

            var control = this.ArrangeView;
            if (control == null)
            {
                control = this.ArrangeView = new ArrangeAnalysisView(this.ServiceProvider, _result);

                arrangePanel.Controls.Add(control);
                control.Dock = DockStyle.Fill;
                control.Show();
            }

            string eqpID = bar.EqpID;
            string subEqpID = bar.SubEqpID;
            string wipInitRun = bar.WipInitRun;

            string matchKey = ArrangeAnalysisView.CreateKey_Match(eqpID, bar.StepID, bar.ProductID,
                string.IsNullOrEmpty(bar.ProductVersion) ? Consts.NULL_ID : bar.ProductVersion, 
                string.IsNullOrEmpty(bar.ToolID) ? Consts.NULL_ID : bar.ToolID);

            control.Query(eqpID, subEqpID, wipInitRun, matchKey);           
        }

        private void ViewEqpProcessDetail(GanttBar bar = null)
        {
            this.IsNeedSetFocus = true;

            DataTable dt = CrateProcessDataTable();

            string eqpID = bar == null ? null : bar.EqpID;
            string subEqpID = bar == null ? null : bar.SubEqpID;

            List<EqpGantt.GanttInfo> list = GetGanttInfo(eqpID, subEqpID);
            if (list != null)
            {
                foreach (EqpGantt.GanttInfo info in list)
                {
                    foreach (var item in info.Items)
                    {
                        foreach (GanttBar b in item.Value)
                        {
                            if (b.State == EqpState.IDLE || b.State == EqpState.IDLERUN)
                                continue;

                            if (b.State != EqpState.DOWN)
                            {
                                if (b.BarKey != item.Key)
                                    continue;
                            }

                            DataRow drow = dt.NewRow();

                            drow[ColName.EqpId] = b.EqpID;
                            drow[ColName.SubEqpId] = b.SubEqpID;
                            drow[ColName.State] = b.State.ToString();

                            if (b.State == EqpState.BUSY || b.State == EqpState.SETUP)
                            {
                                drow[ColName.LotID] = b.OrigLotID;
                                drow[ColName.OwnerType] = b.OwnerType;
                                drow[ColName.ProductID] = b.ProductID;
                                drow[ColName.ProductVersion] = b.ProductVersion;
                                drow[ColName.Layer] = b.Layer;
                                drow[ColName.OperName] = b.StepID;
                                drow[ColName.ToolID] = b.ToolID;

                                drow[ColName.LotPriority] = b.LotPriority;
                                drow[ColName.EqpRecipe] = b.EqpRecipe;
                                drow[ColName.WipInitRun] = b.WipInitRun;

                                if (b.State == EqpState.BUSY)
                                    drow[ColName.TIQty] = b.TIQty;
                            }

                            drow[ColName.StartTime] = DateHelper.Format(b.TkinTime);
                            drow[ColName.EndTime] = DateHelper.Format(b.TkoutTime);
                            drow[ColName.GapTime] = b.TkoutTime - b.TkinTime;

                            dt.Rows.Add(drow);
                        }
                    }
                }
            }

            detailGridControl.BeginUpdate();
            detailGridControl.DataSource = new DataView(dt, "", ColName.StartTime, DataViewRowState.CurrentRows);
            detailGridControl.EndUpdate();

            detailGridView.BestFitColumns();

            Globals.SetGridViewColumnWidth(detailGridView);
        }


        private DataTable CrateProcessDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(ColName.EqpId, typeof(string));            
            dt.Columns.Add(ColName.SubEqpId, typeof(string));            
            dt.Columns.Add(ColName.State, typeof(string));            
            dt.Columns.Add(ColName.LotID, typeof(string));            
            dt.Columns.Add(ColName.Layer, typeof(string));            
            dt.Columns.Add(ColName.OwnerType, typeof(string));
            
            dt.Columns.Add(ColName.OperName, typeof(string));
            dt.Columns[ColName.OperName].Caption = "STEP_ID";

            dt.Columns.Add(ColName.ToolID, typeof(string));
            dt.Columns[ColName.ToolID].Caption = "TOOL_ID";

            dt.Columns.Add(ColName.ProductID, typeof(string));
            dt.Columns[ColName.ProductID].Caption = "PRODUCT_ID";
            dt.Columns.Add(ColName.ProductVersion, typeof(string));
            dt.Columns[ColName.ProductVersion].Caption = "PRODUCT_VERSION";
            dt.Columns.Add(ColName.StartTime, typeof(string));
            dt.Columns[ColName.StartTime].Caption = "START_TIME";
            dt.Columns.Add(ColName.EndTime, typeof(string));
            dt.Columns[ColName.EndTime].Caption = "END_TIME";
            dt.Columns.Add(ColName.TIQty, typeof(int));
            dt.Columns[ColName.TIQty].Caption = "IN_QTY";
            dt.Columns.Add(ColName.GapTime, typeof(string));
            dt.Columns[ColName.GapTime].Caption = "PROCESSED_TIME";

            dt.Columns.Add(ColName.LotPriority, typeof(int));            
            dt.Columns.Add(ColName.EqpRecipe, typeof(string));            
            dt.Columns.Add(ColName.WipInitRun, typeof(string));            

            return dt;
        }

        private List<EqpGantt.GanttInfo> GetGanttInfo(string eqpID, string subEqpID)
        {
            List<EqpGantt.GanttInfo> result = new List<EqpGantt.GanttInfo>();

            if (string.IsNullOrEmpty(eqpID))
                return result;

            var list = this.CurrInfos;
            foreach (EqpGantt.GanttInfo info in list)
            {
                if (info.EqpID != eqpID)
                    continue;

                if(subEqpID != null)
                {
                    if (info.SubEqpID != subEqpID)
                        continue;
                }

                result.Add(info);
            }

            return result;
        }

        private void radioMainDataTable_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            this.barTitleGroup.Properties.Items[1].Enabled = true;

            if (dateEdit1.DateTime < PlanStartTime.AddDays(-3))
                dateEdit1.DateTime = PlanStartTime.AddDays(-3);
        }

        //private void highLightOptionChkEdit_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (this.IsCheckedHighLightOption)
        //    {
        //        this.rdoMouseType.Enabled = true;
        //        this.tePattern.Enabled = true;
        //        this.prodPatternApplyBtn.Enabled = true;

        //        _gantt.TurnOnSelectMode();
        //        rdoMouseType.Enabled = true;
        //    }
        //    else
        //    {
        //        _gantt.TurnOffSelectMode();
        //        this.rdoMouseType.Enabled = false;
        //        this.tePattern.Enabled = false;
        //        this.prodPatternApplyBtn.Enabled = false;

        //        this.grid1.BeginUpdate();
        //        this.grid1.EndUpdate();
        //        this.grid1.Refresh();
        //        // 기능 안먹도록
        //    }
        //}

        private void prodPatternApplyBtn_Click(object sender, EventArgs e)
        {
            if (this.SelectedMouseSelectType != MouseSelectType.Pattern)
                return;

            this.grid1.BeginUpdate();

            Gantt.TurnOnSelectMode();

            Gantt.SelectedBar = new Bar(PlanStartTime, PlanStartTime.AddHours(1), 20, 20, EqpState.BUSY);
            
            this.grid1.EndUpdate();
            this.grid1.Refresh();
        }
        
        private void rdoMouseType_EditValueChanged(object sender, EventArgs e)
        {
            if (Gantt == null)
                return;

            Gantt.MouseSelType = this.SelectedMouseSelectType;
        }
        
        private void shopIDcomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.barTitleGroup.Properties.Items[1].Enabled = true;
                        
            if (Gantt != null)
                Gantt.BindChkListEqpGroup(EqpGroupsCheckedBox.Properties.Items, this.TargetShopID);

            if (this.EqpGroupsCheckedBox.Properties.Items.Count > 0)
            {
                foreach (CheckedListBoxItem item in EqpGroupsCheckedBox.Properties.Items)
                    item.CheckState = CheckState.Checked;
            }


        }

        private void grid1_PopupMenuShowing(object sender, DevExpress.XtraSpreadsheet.PopupMenuShowingEventArgs e)
        {
            e.Menu = null;
        }

        private void barTitleGroup_EditValueChanged(object sender, EventArgs e)
        {
            if (this.IsBeforeFirstQuery == false)
                return;

            SetBarTitleGroupProperties();
        }

        private void SetBarTitleGroupProperties()
        {
            if (this.barTitleGroup.Properties.Items[this.barTitleGroup.SelectedIndex].Value.ToString() == "Product")
            {
                if (rdoMouseType.SelectedIndex == 1)
                    rdoMouseType.SelectedIndex = 0;

                rdoMouseType.Properties.Items.GetItemByValue(MouseSelectType.PB.ToString()).Enabled = true;
            }
            else
                rdoMouseType.Properties.Items.GetItemByValue(MouseSelectType.PB.ToString()).Enabled = true;
        }

        private void tePattern_EditValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.tePattern.Text))
                return;

            this.rdoMouseType.SelectedIndex = 4;
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
            Globals.SetLocalSetting(this.ServiceProvider, _pageID + "ganttCellWidth", this.CellWidthSize.ToString());
            Globals.SetLocalSetting(this.ServiceProvider, _pageID + "ganttCellHeight", this.CellHeightSize.ToString());
        }               

        #endregion

        #region BAR BUILD

        void GanttView_BindDone(object sender, EventArgs e)
        {
            var colHeader = Gantt.ColumnHeader;

            // 마지막 Row 값 세팅
            if (this.IsEqpViewMode)
            {
                string sLoadRate = string.Empty;
                double rate = _rowsumLoadTimeFrBar / (this.EndDate - this.StartDate).TotalSeconds * 100.0;
                sLoadRate = Math.Round(rate, 1).ToString() + "%";
                
                XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(_curRowIdx, ColName.LoadRate), sLoadRate);
                colHeader.GetCellInfo(_curRowIdx, ColName.LoadRate).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

                string subJobChangeCnt = string.Format("{0:n0}", _subJobChg);
                XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(_curRowIdx, ColName.SetupCount), subJobChangeCnt);
                colHeader.GetCellInfo(_curRowIdx, ColName.SetupCount).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            }

            string subTotalRun = string.Format("{0:n0}", _subTotal);
            XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(_curRowIdx, ColName.TotalRun), subTotalRun);
            colHeader.GetCellInfo(_curRowIdx, ColName.TotalRun).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

            string jobChgColName = string.Format("{0} \n({1})", ColName.SetupCount, _totalJobChg.ToString("#.#"));
            XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(0, ColName.SetupCount), jobChgColName);
            colHeader.GetCellInfo(0, ColName.SetupCount).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;

            string loadRateColName = string.Empty;
            double avgLoadTIme = _totalLoadTImeFrBarDic.Count <= 0 ? 0 : _totalLoadTImeFrBarDic.Values.Average();
            double totalRateForSim = _totalLoadTImeFrBarDic.Count <= 0 ? 0
                    : avgLoadTIme / (this.EndDate - this.StartDate).TotalSeconds * 100.0;
            loadRateColName = string.Format("{0} \n({1}%)", ColName.LoadRate, totalRateForSim.ToString("#.#"));
            XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(0, ColName.LoadRate), loadRateColName);

            string totalColName = string.Empty;
            string totalString = string.Empty;
            totalString = string.Format("{0:n0}", _total);
            totalColName = string.Format("{0} \n({1})", ColName.TotalRun, totalString);
            XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(0, ColName.TotalRun), totalColName);
            colHeader.GetCellInfo(0, ColName.TotalRun).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;

            int fromRowIdx = this.IsEqpViewMode ? _startSameEqpRowIdx : _startSameRowKeyIdx;
            MergeRows(fromRowIdx, Gantt.LastRowIndex);

            var eqpGroupCol = colHeader.GetCellInfo(0, ColName.EqpGroup);
            if (eqpGroupCol != null)
            {
                eqpGroupCol.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                eqpGroupCol.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            }

            var layerCol = colHeader.GetCellInfo(0, ColName.Layer);
            if (layerCol != null)
            {
                layerCol.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                layerCol.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            }
                       
            colHeader.GetCellInfo(0, ColName.EqpId).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.EqpId).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;

            colHeader.GetCellInfo(0, ColName.LoadRate).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.LoadRate).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

            colHeader.GetCellInfo(0, ColName.RunQtySum).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.RunQtySum).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

            colHeader.GetCellInfo(0, ColName.TotalRun).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.TotalRun).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            
            PaintTotColumnCell();
        }

        void GanttView_BindBarAdded(object sender, GanttCellEventArgs args)
        {
            args.Bar.CumulateQty(ref _rowsum, ref _rowsum);
            
            _rowsumJobChg += args.Bar.BarList.Where(x => x.State == EqpState.SETUP).Count();

            double loadTime = args.Bar.BarList.Where(x => x.State == EqpState.BUSY || x.State == EqpState.IDLERUN || x.State == EqpState.SETUP)
                .Where(x => x.TkinTime < this.EndDate)
                .Sum(x => (x.TkoutTime - x.TkinTime).TotalSeconds);

            _rowsumLoadTimeFrBar += loadTime;
        }

        void GanttView_BindRowAdding(object sender, GanttRowEventArgs args)
        {
            var worksheet = this.Gantt.Worksheet;

            var info = args.Item as EqpGantt.GanttInfo;
            var colHeader = Gantt.ColumnHeader;

            _curRowIdx = args.RowIndex;

            string eqpGroup = info.EqpGroup;
            string eqpID = info.EqpID;
            string layer = info.Layer;
            string subEqpID = info.SubEqpID;
            int? subEqpCount = info.SubEqpCount;
                        
            if (this.IsShowSubEqp)
            {
                if (string.IsNullOrEmpty(subEqpID) == false)
                    eqpID = string.Format("{0}-{1}({2})", eqpID, subEqpID, subEqpCount);
            }

            SetRowHeaderValue(args.RowIndex, eqpGroup, eqpID, layer, subEqpID, subEqpCount);
                                    
            this._rowsum = 0;            
            this._rowsumJobChg = 0;
            this._rowsumLoadTimeFrBar = 0;

            if (args.Node == null)
                return;

            var rows = args.Node.LinkedBarList;

            if (rows.Count > 1 && args.Index > 0 && args.Index < rows.Count - 1)
            {
                var eqpGroupCol = colHeader.GetCellInfo(args.RowIndex, ColName.EqpGroup);
                if(eqpGroupCol != null)
                    XtraSheetHelper.SetCellText(eqpGroupCol, eqpGroup);

                var layerCol = colHeader.GetCellInfo(args.RowIndex, ColName.Layer);
                if (layerCol != null)
                    XtraSheetHelper.SetCellText(eqpGroupCol, layer);

                XtraSheetHelper.SetCellText(colHeader.GetCellInfo(args.RowIndex, ColName.EqpId), eqpID);

                PaintRowKeyedCell(args.RowIndex, _currColor);
            }
        }

        void GanttView_BindRowAdded(object sender, GanttRowEventArgs args)
        {
            var info = args.Item as EqpGantt.GanttInfo;
            var colHeader = Gantt.ColumnHeader;

            if (_totalLoadTImeFrBarDic.ContainsKey(info.EqpID) == false)
                _totalLoadTImeFrBarDic.Add(info.EqpID, _rowsumLoadTimeFrBar);

            if (this.IsEqpViewMode == false)
            {                
                double rate = _rowsumLoadTimeFrBar / (this.EndDate - this.StartDate).TotalSeconds * 100.0;
                string loadRate = Math.Round(rate, 1).ToString() + "%";

                XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(args.RowIndex, ColName.LoadRate), loadRate);              
                XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(args.RowIndex, ColName.SetupCount), _rowsumJobChg);
                

                colHeader.GetCellInfo(args.RowIndex, ColName.LoadRate).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            }
            
            XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(args.RowIndex, ColName.RunQtySum), _rowsum);            
            
            _subTotal += _rowsum;
            _total += _rowsum;
            _subJobChg += _rowsumJobChg;
            _totalJobChg += _rowsumJobChg;
        }

        void GanttView_BindItemAdded(object sender, GanttItemEventArgs args)
        {
        }

        #endregion

        #region HEADER BUILD

        void GanttView_HeaderDone(object sender, GanttColumnHeaderEventArgs e)
        {
            var colHeader = e.ColumnHeader;
                        
            colHeader.AddColumn(new XtraSheetHelper.SfColumn(ColName.SetupCount, 40));                
            colHeader.AddColumn(new XtraSheetHelper.SfColumn(ColName.LoadRate, 50));
            colHeader.AddColumn(new XtraSheetHelper.SfColumn(ColName.RunQtySum, 50));            
            colHeader.AddColumn(new XtraSheetHelper.SfColumn(ColName.TotalRun, 50));
            
            this.ganttSizeControl1.LeftExceptCount = this.Gantt.FixedColCount;
            this.ganttSizeControl1.TopExceptCount = this.Gantt.FixedRowCount;

            this.ganttSizeControl1.RightExceptCount = 4;
        }

        void GanttView_HeaderShiftChanged(object sender, GanttColumnHeaderEventArgs args)
        {
            try
            {
                MergeHeader_Shift(args);
            }
            catch { }
        }

        private void MergeHeader_Shift(GanttColumnHeaderEventArgs args)
        {
            string parttern = this.Gantt.DateKeyPattern;

            DateTime shiftStart = DateHelper.Trim(args.Time, "HH");

            DateTime hstart = DateHelper.Trim(this.StartDate, "HH");
            DateTime hend = DateHelper.Trim(this.EndDate, "HH");
            if (hend == this.EndDate)
                hend = hend.AddHours(-1);

            DateTime ss = shiftStart;
            DateTime ee = ss.AddHours(ShopCalendar.ShiftHours - 1);

            if (ss < hstart)
                ss = hstart;

            if (ee > hend)
                ee = hend;

            var sColName = ss.ToString(parttern);
            var eColName = ee.ToString(parttern);
            string headText = Gantt.GetJobChgShiftCntFormat(ss);

            var gcol = new XtraSheetHelper.SfGroupColumn(headText, sColName, eColName);
            args.ColumnHeader.AddGroupColumn(gcol);
        }

        void GanttView_HeaderHourChanged(object sender, GanttColumnHeaderEventArgs args)
        {
            string key = args.Time.ToString(Gantt.DateKeyPattern);
            string caption = Gantt.GetJobChgHourCntFormat(args.Time);

            args.ColumnHeader.AddColumn(new XtraSheetHelper.SfColumn(key, caption, Gantt.DefaultColumnWidth, true, false));            
        }

        #endregion
        
        #endregion

        #region BIND DATA

        protected void Query()
        {
            Gantt.GanttType = GanttType.SimGantt;

            this.CurrInfos = GenerateGantt_Sim();

            BindGrid(this.CurrInfos);
        }

        private List<EqpGantt.GanttInfo> GenerateGantt_Sim()
        {
            var gantt = Gantt;

            gantt.PrepareData(this.IsProductInBarTitle, this.IsShowProductColor);            

            gantt.BuildData_Sim(this.TargetShopID,
                                this.SelectedEqpGroups,
                                this.EqpIdPattern,
                                this.StartDate,
                                this.EndDate,
                                this.SelectViewMode,
                                this.IsFilterDownEqp);
            
            return gantt.Expand(this.ShowLayerBar, this.SelectViewMode);
        }
                
        #endregion

        #region BIND GRID

        bool _isFirst = true;
        string _preLayer = string.Empty;
        string _preEqpID = string.Empty;
        string _preEqpGroup = string.Empty;
        string _preRowKey = string.Empty;

        Color _preColor = XtraSheetHelper.AltColor;
        Color _currColor = XtraSheetHelper.AltColor2;

        int _startSameEqpRowIdx = 0;
        int _startSameRowKeyIdx = 0;
        int _curRowIdx = 0;
        
        double _subTotal = 0;
        double _total = 0;
        double _subJobChg = 0;
        double _totalJobChg = 0;

        Dictionary<string, double> _totalLoadTimeFrStatDic;
        Dictionary<string, double> _totalLoadTImeFrBarDic;

        double _rowsum = 0;        
        double _rowsumJobChg = 0;
        double _rowsumLoadTimeFrBar = 0;

        public Dictionary<string, int> _moveByLayer;

        private void Clear()
        {
            _isFirst = true;
            _preRowKey = string.Empty;

            _preColor = XtraSheetHelper.AltColor;
            _currColor = XtraSheetHelper.AltColor2;

            _startSameEqpRowIdx = 0;
            _startSameRowKeyIdx = 0;

            _subTotal = 0;
            _total = 0;
            _subJobChg = 0;
            _totalJobChg = 0;

            _totalLoadTimeFrStatDic = new Dictionary<string, double>();
            _totalLoadTImeFrBarDic = new Dictionary<string, double>();

            _rowsum = 0;
            _rowsumJobChg = 0;
            _rowsumLoadTimeFrBar = 0;
        }

        private void BindGrid(List<EqpGantt.GanttInfo> currInfos)
        {                                   
            Clear();
                           
            Gantt.Workbook.BeginUpdate();
            Gantt.ResetWorksheet();

            this.Gantt.TurnOffSelectMode();

            SetColumnHeaders();
                        
            this.Gantt.SchedBarComparer = new GanttMaster.CompareMBarList();
   
            if(currInfos != null && currInfos.Count > 1)
            {                
                
                EqpGantt.SortOptions[] options = null;
                if (this.SelectViewMode == EqpGantt.ViewMode.EQPGROUP)
                {
                    options = new EqpGantt.SortOptions[]{EqpGantt.SortOptions.EQP_GROUP, 
                                                         EqpGantt.SortOptions.EQP};                    
                }
                else if (this.SelectViewMode == EqpGantt.ViewMode.EQP)
                {
                    options = new EqpGantt.SortOptions[] { EqpGantt.SortOptions.EQP, 
                                                           EqpGantt.SortOptions.LAYER};                    
                }
                else if (this.SelectViewMode == EqpGantt.ViewMode.LAYER)
                {
                    options = new EqpGantt.SortOptions[] { EqpGantt.SortOptions.LAYER, 
                                                           EqpGantt.SortOptions.EQP };
                }

                currInfos.Sort(new EqpGantt.CompareGanttInfo(Gantt.GanttType,
                                                             this.EqpMgr, 
                                                             this.TargetShopID, 
                                                             options));
            }
                     
            this.Gantt.Bind(currInfos);

            this.Gantt.Workbook.EndUpdate();
        }
                
        private void SetRowHeaderValue(int rowIndex, string eqpGroup, string eqpID, string layer, string subEqpID, int? subEqpCount)
        {
            if (this.IsEqpViewMode == false)
            {
                var colHeader = Gantt.ColumnHeader;

                if (_isFirst)
                {
                    _preLayer = layer;
                    _preEqpID = eqpID;
                    _preEqpGroup = eqpGroup;
                    _startSameEqpRowIdx = rowIndex;
                    _startSameRowKeyIdx = rowIndex;
                    
                    _isFirst = false;
                }

                if (eqpID.Equals(_preEqpID) == false)
                {
                    _preEqpID = eqpID;
                    _startSameEqpRowIdx = rowIndex;
                }

                if (eqpGroup.Equals(_preEqpGroup) == false && this.SelectViewMode == EqpGantt.ViewMode.EQPGROUP)
                {
                    MergeRows(_startSameRowKeyIdx, rowIndex - 1);

                    Color tmp = _preColor;
                    _preColor = _currColor;
                    _currColor = tmp;
                    _startSameRowKeyIdx = rowIndex;

                    _preEqpID = eqpID;
                    _preEqpGroup = eqpGroup;
                    _startSameEqpRowIdx = rowIndex;

                    if (_startSameEqpRowIdx > 1)
                    {
                        XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(rowIndex - 1, ColName.TotalRun), _subTotal);
                    }

                    _preEqpGroup = eqpGroup;
                    _preEqpID = eqpID;
                    _startSameEqpRowIdx = rowIndex;
                    _subTotal = 0;
                    _subJobChg = 0;

                }

                if (layer.Equals(_preLayer) == false && this.SelectViewMode == EqpGantt.ViewMode.LAYER)
                {
                    MergeRows(_startSameRowKeyIdx, rowIndex - 1);

                    Color tmp = _preColor;
                    _preColor = _currColor;
                    _currColor = tmp;
                    _startSameRowKeyIdx = rowIndex;

                    if (_startSameEqpRowIdx > 1)
                    {
                        XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(rowIndex - 1, ColName.TotalRun), _subTotal);
                    }

                    _preLayer = layer;
                    _preEqpID = eqpID;
                    _startSameEqpRowIdx = rowIndex;
                    _subTotal = 0;
                    _subJobChg = 0;
                }
                
                PaintRowKeyedCell(rowIndex, _currColor);

                var eqpGroupCol = colHeader.GetCellInfo(rowIndex, ColName.EqpGroup);
                if (eqpGroupCol != null)
                {
                    Gantt.Worksheet[rowIndex, eqpGroupCol.ColumnIndex].SetCellText(eqpGroup);

                    XtraSheetHelper.SetCellText(eqpGroupCol, eqpGroup);
                    eqpGroupCol.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    eqpGroupCol.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                }

                var layerCol = colHeader.GetCellInfo(rowIndex, ColName.Layer);
                if (layerCol != null)
                {
                    Gantt.Worksheet[rowIndex, layerCol.ColumnIndex].SetCellText(layer);

                    XtraSheetHelper.SetCellText(layerCol, layer);
                    layerCol.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    layerCol.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                }

                XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.EqpId), eqpID);
                colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            }
            else
            {
                var colHeader = Gantt.ColumnHeader;

                if (_isFirst)
                {
                    _preLayer = layer;
                    _preEqpID = eqpID;
                    _startSameEqpRowIdx = rowIndex;
                    _startSameRowKeyIdx = rowIndex;

                    _isFirst = false;
                }

                if (layer.Equals(_preLayer) == false)
                {
                    _preLayer = layer;
                    _startSameRowKeyIdx = rowIndex;
                }
                
                if (eqpID.Equals(_preEqpID) == false)
                {
                    MergeRows(_startSameEqpRowIdx, rowIndex - 1);

                    if (_startSameEqpRowIdx > 1)
                    {
                        string sLoadRate = string.Empty;

                        double rate = _rowsumLoadTimeFrBar / (this.EndDate - this.StartDate).TotalSeconds * 100.0;
                        sLoadRate = Math.Round(rate, 1).ToString() + "%";
                     
                        XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(rowIndex - 1, ColName.LoadRate), sLoadRate);
                        colHeader.GetCellInfo(rowIndex - 1, ColName.LoadRate).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

                        XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(rowIndex - 1, ColName.TotalRun), _subTotal);
                        XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(rowIndex - 1, ColName.SetupCount), _subJobChg);
                    }

                    Color tmp = _preColor;
                    _preColor = _currColor;
                    _currColor = tmp;
                    _preEqpID = eqpID;

                    _preLayer = layer;
                    _startSameEqpRowIdx = rowIndex;
                    _subTotal = 0;
                    _subJobChg = 0;
                }

                PaintRowKeyedCell(rowIndex, _currColor);

                var eqpGroupCol = colHeader.GetCellInfo(rowIndex, ColName.EqpGroup);
                if (eqpGroupCol != null)
                {
                    Gantt.Worksheet[rowIndex, eqpGroupCol.ColumnIndex].SetCellText(eqpGroup);

                    XtraSheetHelper.SetCellText(eqpGroupCol, eqpGroup);
                    eqpGroupCol.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    eqpGroupCol.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                } 
               
                var layerCol = colHeader.GetCellInfo(rowIndex, ColName.Layer);
                if(layerCol != null)
                {
                    Gantt.Worksheet[rowIndex, layerCol.ColumnIndex].SetCellText(layer);

                    XtraSheetHelper.SetCellText(layerCol, layer);
                    layerCol.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    layerCol.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                }

                XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.EqpId), eqpID);
                colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                colHeader.GetCellInfo(rowIndex, ColName.EqpId).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            }
        }

        private void MergeRows(int fromRowIdx, int toRowIdx, int colIdx = 0)
        {            
            var worksheet = this.Gantt.Worksheet;

            worksheet.MergeRowsOneColumn(fromRowIdx, toRowIdx, colIdx);

            if (colIdx > 1)
                SetBorder(fromRowIdx, toRowIdx);
        }

        private void SetBorder(int fromRowIdx, int toRowIdx)
        {
            var worksheet = this.Gantt.Worksheet;
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
            var worksheet = this.Gantt.Worksheet;

            for (int colindex = 0; colindex < 2; colindex++)
            {
                worksheet[rowIndex, colindex].FillColor = color;
            }
        }

        private void PaintTotColumnCell()
        {
            var worksheet = Gantt.Worksheet;
            var colHeader = Gantt.ColumnHeader;

            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(ColName.SetupCount), Color.FromArgb(248, 223, 224));
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(ColName.LoadRate), Color.FromArgb(254, 240, 222));
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(ColName.RunQtySum), Color.FromArgb(219, 236, 216));
            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(ColName.TotalRun), Color.FromArgb(204, 255, 195));
        }

        #endregion

        #region SET COLUMN

        struct ColName
        {
            public static string EqpGroup = "EQP_GROUP";
            public static string EqpId = "EQP_ID";
            public static string SubEqpId = "SUB_EQP_ID";
            public static string State = "STATE";
            public static string OperName = "OPERATION_NAME";
            public static string LotID = "LOT_ID";
            public static string ProductID = "PRODUCT_ID";
            public static string ProductVersion = "PRODUCT_VERSION";
            public static string StartTime = "START_TIME";
            public static string EndTime = "END_TIME";
            public static string GapTime = "GAP_TIME";
            public static string NextTkinTime = "NEXT_TKIN_TIME";
            public static string Layer = "LAYER";
            public static string ProductKind = "PRODUCT_KIND";
            public static string OwnerType = "OWNER_TYPE";
            public static string ToolID = "TOOL_ID";
            
            public static string EqpRecipe = "EQP_RECIPE";
            public static string LotPriority = "LOT_PRIORITY";
            public static string WipInitRun = "WIP_INIT_RUN";

            public static string TIQty = "IN_QTY";
            public static string TOQty = "OUT_QTY";

            public static string LoadRate = "LOAD";
            public static string ChangRage = "CHANGE";
            public static string SetupCount = "SETUP";

            public static string TIQtySum = "T/I\nQTY";
            public static string RunQtySum = "RUN QTY";
            public static string TITotal = "T/I\nTOTAL";
            public static string TotalRun = "TOTAL";
        }
                
        protected void SetColumnHeaders()
        {
            var gantt = this.Gantt;

            gantt.FixedColCount = 2;
            gantt.FixedRowCount = 2;

            int colCount = Convert.ToInt32(fromShiftComboBox.Value) * (int)ShopCalendar.ShiftHours + 5 + 2;

            var eqpGroupCol = new XtraSheetHelper.SfColumn(ColName.EqpGroup, ColName.EqpGroup, 105);                
            var eqpCol = new XtraSheetHelper.SfColumn(ColName.EqpId, ColName.EqpId, 110);
            var layerCol = new XtraSheetHelper.SfColumn(ColName.Layer, ColName.Layer, 105);

            if (this.SelectViewMode == EqpGantt.ViewMode.EQPGROUP)
                gantt.SetColumnHeaders(colCount, eqpGroupCol, eqpCol);
            else if (this.SelectViewMode == EqpGantt.ViewMode.EQP)
                gantt.SetColumnHeaders(colCount, eqpCol, layerCol);
            else if (this.SelectViewMode == EqpGantt.ViewMode.LAYER)
                gantt.SetColumnHeaders(colCount, layerCol, eqpCol);   

            gantt.Worksheet.Rows[0].Style = Gantt.Workbook.Styles["CustomHeader"];
            gantt.Worksheet.Rows[1].Style = Gantt.Workbook.Styles["CustomHeader"];

            gantt.Worksheet.SelectedCell[0, 0].Style = Gantt.Workbook.Styles["CustomHeaderCenter"];
            gantt.Worksheet.SelectedCell[0, 1].Style = Gantt.Workbook.Styles["CustomHeaderCenter"]; 
        }

        #endregion

        #region EXCEL EXPORT

        //상위메뉴 Data>ExportToExcel --> Enable
        protected override bool UpdateCommand(Command command)
        {
            bool handled = false;
            if (command.CommandGroup == typeof(TaskCommands))
            {
                //switch (command.CommandID)
                //{
                //    case TaskCommands.DataExportToExcel:
                //        command.Enabled = true;
                //        handled = true;
                //        break;
                //}
            }

            if (handled)
                return true;

            return base.UpdateCommand(command);
        }

        protected override bool HandleCommand(Command command)
        {
            bool handled = false;

            if (command.CommandGroup == typeof(TaskCommands))
            {
                //switch (command.CommandID)
                //{
                //    case TaskCommands.DataExportToExcel:
                //        {
                //            //string filename = SfGridHelper.GetXlsFileName(_result.Path, _pageID);
                //            //SfGridHelper.ExportToXls(this.grid, filename);
                //            //handled = true;
                //            break;
                //        }
                //}
            }

            if (handled)
                return true;

            return base.HandleCommand(command);
        }

        #endregion        

        #region EVENT

        private void Search()
        {            
            Gantt.DefaultColumnWidth = this.CellWidthSize;
            Gantt.DefaultRowHeight = this.CellHeightSize;

            Cursor.Current = Cursors.WaitCursor;
            queryButton.Enabled = false;

            Query();

            queryButton.Enabled = true;

            Cursor.Current = Cursors.Default;
        }
        
        private void btnQuery_Click(object sender, EventArgs e)
        {
            detailGridControl.DataSource = null;

            Search();
            
            SetBarTitleGroupProperties();
            ViewDispDetail();

            this.IsBeforeFirstQuery = false;
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            BindGrid(this.CurrInfos);

            Cursor.Current = Cursors.Default;
        }

        private void detailGridView_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (this.Gantt == null)
                return;

            GanttBar bar = this.Gantt.SelectedBar as GanttBar;
            if (bar == null)
                return;

            if (bar.IsGhostBar)
                return;

            var text = detailGridView.GetRowCellDisplayText(e.RowHandle, ColName.StartTime);
            if (string.IsNullOrEmpty(text))
                return;

            if (text == DateHelper.Format(bar.StartTime))
            {
                e.Appearance.BackColor = Color.LightCoral;

                if (this.IsNeedSetFocus)
                {
                    detailGridView.FocusedRowHandle = e.RowHandle;
                    this.IsNeedSetFocus = false;
                }
            }            
        }

        #endregion               
    }
}
