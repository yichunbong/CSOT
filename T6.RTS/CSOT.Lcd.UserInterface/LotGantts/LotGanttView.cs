using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

//using Mzui.Windows.Forms.Grid;
using DevExpress.Spreadsheet;       // 추가

using Mozart.Studio.UIComponents;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;

using CSOT.Lcd.Scheduling;
using CSOT.Lcd.UserInterface.Gantts;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.LotGantts
{
    public partial class LotGanttView : XtraGridControlView
    {
        private const string _pageID = "LotGanttView";

        #region Veriables
        IExperimentResultItem _result;

        DateTime _planStartTime;

        LotGantt _gantt;
        #endregion

        #region Property

        bool ShowLayerBar
        {
            get { return this.showLayerBar.Checked; }
        }

        bool BarTitleGroup
        {
            get { return this.barTitleGroup.Properties.Items[this.barTitleGroup.SelectedIndex].Value.ToString() == "KOP"; }
        }

        bool IsLayerFirst
        {
            get { return this.queryFirstGroup.Properties.Items[this.queryFirstGroup.SelectedIndex].Value.ToString() == "LAYER"; }
        }

        string SelectedShopID
        {
            get { return shopIDcomboBox.SelectedItem != null ? shopIDcomboBox.SelectedItem.ToString() : "Blank"; }
        }

        bool InTime
        {
            get { return true; }
        }

        string LotIdPattern
        {
            get { return lotIDtextBox.Text.ToUpper() + "%"; }
        }
        
        private DateTime StartDate
        {
            get
            {
                int iShift = shiftNameComboBox.SelectedIndex + 1;
                DateTime dt = ShopCalendar.GetShiftStartTime(dateEdit1.DateTime.Date, iShift);

                dt = dt.AddMinutes(-dt.Minute).AddSeconds(-dt.Second);

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

        public int CellWidthSize
        {
            get { return ganttSizeControl1.CellWidth; }
        }

        public int CellHeightSize
        {
            get { return ganttSizeControl1.CellHeight; }
        }

        #endregion

        #region ctor
        public LotGanttView()
        {
            InitializeComponent();
        }

        public LotGanttView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();


        }

        #endregion

        #region Initialize Controls

        protected override void LoadDocument()
        {
            if (this.Document != null)
            {
                var item = (IMenuDocItem)this.Document.ProjectItem;

                _result = (IExperimentResultItem)item.Arguments[0];

                if (_result == null)
                    return;
                Globals.InitFactoryTime(_result.Model);

            }
            else
            {
            }

            InitControl();

            InitializeData();
        }

        protected void InitControl()
        {
            layoutControlItem8.Text = LotGanttChartData.LOT_ID;

            fromShiftComboBox.Value = Globals.GetResultPlanPeriod(_result) * ShopCalendar.ShiftCount;//ShopCalendar.ShiftCount * 7;
            _planStartTime = _result.StartTime;
            dateEdit1.DateTime = ShopCalendar.SplitDate(_planStartTime);

            ComboHelper.AddDataToComboBox(shopIDcomboBox, _result, LotGanttChartData.LOT_TABLE_NAME,
                LotGanttChartData.Lot.Schema.SHOP_ID, false);
            if (this.shopIDcomboBox.Properties.Items.Contains("ARRAY"))
                this.shopIDcomboBox.SelectedIndex = this.shopIDcomboBox.Properties.Items.IndexOf("ARRAY");
            else
                this.shopIDcomboBox.SelectedIndex = 0;
            
            var cellWidth = Extensions.GetLocalSetting(this.ServiceProvider, _pageID + "ganttCellWidth");
            var cellHeight = Extensions.GetLocalSetting(this.ServiceProvider, _pageID + "ganttCellHeight");

            cellWidth = cellWidth == null ? this.CellWidthSize.ToString() : cellWidth;
            cellHeight = cellHeight == null ? this.CellHeightSize.ToString() : cellHeight;

            if (!string.IsNullOrEmpty(cellWidth))
                this.ganttSizeControl1.CellWidth = Convert.ToInt32(cellWidth);

            if (!string.IsNullOrEmpty(cellHeight))
                this.ganttSizeControl1.CellHeight = Convert.ToInt32(cellHeight);

            ComboHelper.ShiftName(shiftNameComboBox, _planStartTime);
            
            //lindIDcomboBox.SelectedIndexChanged += new EventHandler(cbeLine_SelectedIndexChanged);
        }

        #endregion

        #region initializeData
        private void InitializeData()
        {
            base.SetWaitDialogLoadCaption("Lot Gantt info.");

            _gantt = new LotGantt(this.grid1, _result, _planStartTime);
            _gantt.DefaultColumnWidth = CellWidthSize;
            _gantt.DefaultRowHeight = CellHeightSize;

            _gantt.MouseSelType = MouseSelectType.LotId;

            SetColumnHeaderView();

            this.BindEvents();
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
            _gantt.BarDoubleClick += new BarEventHandler(GanttView_BarDoubleClick);
            _gantt.BarDraw += new BarDrawEventHandler(GanttView_BarDraw);

            //grid1.CellClick += new GridCellClickEventHandler(grid1_CellClick);
        }

        #region bar handling
        void GanttView_BarDraw(object sender, BarDrawEventArgs args)
        {
            var bar = args.Bar as GanttBar;

            args.Background = _gantt.GetBrushInfo(bar);
            args.DrawFrame = _gantt.EnableSelect && _gantt.SelectedBar != null && !_gantt.CompareToSelectedBar(bar);
            args.FrameColor = Color.Gray; //Color.White;//Color.Gray;
            args.DrawFrame = true;

            args.ForeColor = Color.Black;
            args.Text = bar.GetTitle();

            args.DrawDefault = true;
        }

        public void GanttView_BarClick(object sender, BarEventArgs e)
        {
            if (_gantt.ColumnHeader == null)
                return;

            if (e.Bar != null)
            {
                var bar = e.Bar as GanttBar;

                var modelDataContext = this._result.GetCtx<ModelDataContext>();

                /////////////////////////////////////////////////////////////////////////////
                // Current Arrange 정보 조회
                var currentArrangInfo = from a in modelDataContext.EqpArrange
                                        where a.STEP_ID == bar.StepSeq && a.PRODUCT_ID == bar.ProductID
                                        select new
                                        {
                                            FACTORY_ID = a.FACTORY_ID,
                                            SHOP_ID = a.SHOP_ID,
                                            EQP_ID = a.EQP_ID,
                                            //EQP_TYPE = a.EQP_TYPE,
                                            //PROCESS_ID = a.PROCESS_ID,
                                            PRODUCT_ID = a.PRODUCT_ID,
                                            STEP_ID = a.STEP_ID,
                                            MASK_ID = a.MASK_ID,
                                            ACTIVE_TYPE = a.ACTIVATE_TYPE
                                        };

                gridControlCurrentInfo.BeginUpdate();
                gridControlCurrentInfo.DataSource = currentArrangInfo;
                gridViewCurrentInfo.BestFitColumns();
                gridControlCurrentInfo.EndUpdate();
                /////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////////


                /////////////////////////////////////////////////////////////////////////////
                // Next Arrange 정보 조회
                var index = from a in modelDataContext.ProcStep
                            where a.PROCESS_ID == bar.ProcessID
                                //&& a.IS_MANDATORY == 'Y' 
                                && a.STEP_ID == bar.StepSeq
                            select a.STEP_SEQ;

                if (index.Single() > 0)
                {
                    var result = modelDataContext.ProcStep.Where(x => x.PROCESS_ID == bar.ProcessID && x.STEP_SEQ > index.Single()).FirstOrDefault();
                    if (result == null)
                    { 
                        gridControlNextInfo.DataSource = null;
                        return;
                    }

                    var nextArrangeInfo = from a in modelDataContext.EqpArrange
                                          where a.STEP_ID == result.STEP_ID && a.PRODUCT_ID == bar.ProductID
                                          select new
                                          {
                                              FACTORY_ID = a.FACTORY_ID,
                                              SHOP_ID = a.SHOP_ID,
                                              EQP_ID = a.EQP_ID,
                                              //EQP_TYPE = a.EQP_TYPE,
                                              //PROCESS_ID = a.PROCESS_ID,
                                              PRODUCT_ID = a.PRODUCT_ID,
                                              STEP_ID = a.STEP_ID,
                                              MASK_ID = a.MASK_ID,
                                              ACTIVE_TYPE = a.ACTIVATE_TYPE
                                          };
                    gridControlNextInfo.BeginUpdate();
                    gridControlNextInfo.DataSource = nextArrangeInfo;
                    gridViewNextInfo.BestFitColumns();
                    gridControlNextInfo.EndUpdate();
                }
                else
                    gridControlNextInfo.DataSource = null;
                /////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////////
            }
        }

        public void GanttView_BarDoubleClick(object sender, BarEventArgs e)
        {
            if (e.Bar == null)
                return;

            var dialog = new DetailBarInfoDialog();
            dialog.SetBarInfo(e.Bar as GanttBar);

            dialog.TopMost = true;
            dialog.StartPosition = FormStartPosition.Manual;

            var myScreen = Screen.FromControl(this);
            dialog.Location = new Point(myScreen.WorkingArea.Left + 24, myScreen.WorkingArea.Top + 24);
            dialog.Text = "Bar Information";

            dialog.Show(this);
        }
        #endregion

        #region build bars

        void GanttView_BindDone(object sender, EventArgs e)
        {
            var colHeader = _gantt.ColumnHeader;

            _totLoadRate = _totLoadRate / _calCount;

            double totalLoadRate = _queryPeriod * _totalRowCnt > 0 ? _totalRunHours / (_queryPeriod * (double)_totalRowCnt) * 100 : 0;
            string sTotalLoadRate = totalLoadRate <= 0 ? string.Empty : Math.Round(totalLoadRate, 1).ToString();

            XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(0, ColName.LoadRate), 
                string.Format("{0}\n({1}%)", ColName.LoadRate, sTotalLoadRate));

            XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(0, ColName.RunQty),
                string.Format("{0}\n({1})", ColName.RunQty, _totalTO.ToString("#.#")));

            _totalRunHours = 0;
            _totalRowCnt = 0;

            colHeader.GetCellInfo(0, ColName.LotID).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.LotID).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            colHeader.GetCellInfo(0, ColName.LoadRate).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Left;
            colHeader.GetCellInfo(0, ColName.RunQty).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Left;
            //colHeader.GetCellInfo(1, ColName.LoadRate).HorizontalAlignment = GridHorizontalAlignment.Left;

            MergeRows(_startSameRowKeyIdx, _gantt.LastRowIndex);
            //MergeRows(_startSameEqpRowIdx, _gantt.LastRowIndex, 1);
            
            // Sample에 없음
            //grid1.Model.Options.MergeCellsMode = GridMergeCellsMode.OnDemandCalculation | GridMergeCellsMode.MergeRowsInColumn;

            PaintTotColumnCell();
        }


        void GanttView_BindBarAdded(object sender, GanttCellEventArgs args)
        {
            if (args.Bar.BarList == null)
                return;

            //foreach (Bar bar in args.Bar.BarList)
            //{
            //    this._rowsumti += bar.TOQty;
            //    this._rowsumto += bar.TOQty;

            //    this._rowRunHours += (bar.TkoutTime - bar.TkinTime).TotalHours;
            //    this._totalRunHours += _rowRunHours;
            //}
            
            _rowsumti += args.Bar.BarList.Sum(x => x.TOQty);
            _rowsumto += args.Bar.BarList.Sum(x => x.TOQty);

            double loadTime = args.Bar.BarList.Sum(x => (x.TkoutTime - x.TkinTime).TotalHours);
            this._rowRunHours += loadTime;
            this._totalRunHours += loadTime;
            //args.Bar.CumulateQty(ref _rowsumti, ref _rowsumto);
        }

        void GanttView_BindRowAdding(object sender, GanttRowEventArgs args)
        {
            var info = args.Item as LotGantt.GanttInfo;
            var colHeader = _gantt.ColumnHeader;

            SetRowHeaderValue(args.RowIndex, info.LotID);

            this._rowsumti = 0; //2010.09.13 by ldw
            this._rowsumto = 0; //2010.09.13 by ldw
            this._rowRunHours = 0;

            if (args.Node == null)
                return;

            var rows = args.Node.LinkedBarList;

            if (rows.Count > 1 && args.Index > 0 && args.Index < rows.Count - 1)
            {
                XtraSheetHelper.SetCellText(colHeader.GetCellInfo(args.RowIndex, ColName.LotID), info.LotID);
                //SfGridHelper.SetCellText(colHeader.GetCellInfo(args.RowIndex, ColName.LotID), info.LotID);

                PaintRowKeyedCell(args.RowIndex, _currColor);
            }
        }

        void GanttView_BindRowAdded(object sender, GanttRowEventArgs args)
        {
            var info = args.Item as LotGantt.GanttInfo;
            var colHeader = _gantt.ColumnHeader;

            _calCount++;
            XtraSheetHelper.SetCellFloatValue(colHeader.GetCellInfo(args.RowIndex, ColName.LoadRate), 0);
            //SfGridHelper.SetCellFloatValue(colHeader.GetCellInfo(args.RowIndex, ColName.LoadRate), 0);
            
            //SfGridHelper.SetTotCellValue(colHeader.GetCellInfo(args.RowIndex, ColName.TIQtySum), _rowsumti);

            XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(args.RowIndex, ColName.RunQty), _rowsumto);
            //SfGridHelper.SetTotCellValue(colHeader.GetCellInfo(args.RowIndex, ColName.TOQtySum), _rowsumto);

            double rowLoadRate = _queryPeriod > 0 ? _rowRunHours / _queryPeriod * 100.0 : 0;
            string sRowLoadRate = rowLoadRate <= 0 ? string.Empty : Math.Round(rowLoadRate, 1).ToString() + "%";

            XtraSheetHelper.SetTotCellValue(colHeader.GetCellInfo(args.RowIndex, ColName.LoadRate), sRowLoadRate);
            //SfGridHelper.SetTotCellValue(colHeader.GetCellInfo(args.RowIndex, ColName.LoadRate), sRowLoadRate);

            _totalRowCnt++;

            colHeader.GetCellInfo(args.RowIndex, ColName.LoadRate).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Left;
            colHeader.GetCellInfo(args.RowIndex, ColName.LoadRate).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            colHeader.GetCellInfo(args.RowIndex, ColName.RunQty).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Left;
            colHeader.GetCellInfo(args.RowIndex, ColName.RunQty).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;

            _subTotalTI += _rowsumti;
            _subTotalTO += _rowsumto;
            _totLoadRate += rowLoadRate;
            _totalTO += _rowsumto;
        }

        void GanttView_BindItemAdded(object sender, GanttItemEventArgs args)
        {
            //var colHeader = _gantt.ColumnHeader;
            //SfGridHelper.SetTotCellValue(colHeader.GetCellInfo(_startSameEqpRowIdx, ColName.TITotal), _subTotalTI);
            //SfGridHelper.SetTotCellValue(colHeader.GetCellInfo(_startSameEqpRowIdx, ColName.TOTotal), _subTotalTO);
        }

        #endregion

        #region build Header
        void GanttView_HeaderDone(object sender, GanttColumnHeaderEventArgs e)
        {
            var colHeader = e.ColumnHeader;

            colHeader.AddColumn(new XtraSheetHelper.SfColumn(ColName.LoadRate, 60));
            //colHeader.AddColumn(new SfGridHelper.SfColumn(ColName.LoadRate, loadRateString, 60));

            //colHeader.AddColumn(new SfGridHelper.SfColumn(ColName.ChangRage, chgRateString, 60));

            //colHeader.AddColumn(new SfGridHelper.SfColumn(ColName.TIQtySum, 60));

            colHeader.AddColumn(new XtraSheetHelper.SfColumn(ColName.RunQty, 60));
            //colHeader.AddColumn(new SfGridHelper.SfColumn(ColName.TOQtySum, 60));

            //colHeader.AddColumn(new SfGridHelper.SfColumn(ColName.TITotal, 80));
            //colHeader.AddColumn(new SfGridHelper.SfColumn(ColName.TOTotal, 80));

            this.ganttSizeControl1.LeftExceptCount = this._gantt.FixedColCount;
            this.ganttSizeControl1.RightExceptCount = 2;
            this.ganttSizeControl1.TopExceptCount = this._gantt.FixedRowCount;
        }

        void GanttView_HeaderShiftChanged(object sender, GanttColumnHeaderEventArgs args)
        {            
            var startColName = args.Time.ToString(_gantt.DateKeyPattern);
            var endColName = args.Time.AddHours(ShopCalendar.ShiftHours - 1).ToString(_gantt.DateKeyPattern);

            if (this.StartDate.ShiftStartTimeOfDayT() == args.Time)
                startColName = this.StartDate.ToString(_gantt.DateKeyPattern);
            else if (this.EndDate.ShiftStartTimeOfDayT() == args.Time)
                endColName = this.EndDate.AddHours(-1).ToString(_gantt.DateKeyPattern);

            args.ColumnHeader.AddGroupColumn(
                new XtraSheetHelper.SfGroupColumn(_gantt.GetJobChgShiftCntFormat(args.Time), startColName, endColName));

            //args.ColumnHeader.AddGroupColumn(
            //    new SfGridHelper.SfGroupColumn(_gantt.GetJobChgShiftCntFormat(args.Time),
            //    args.Time.ToString(_gantt.DateKeyPattern), args.Time.AddHours(ShopCalendar.ShiftHours - 1).ToString(_gantt.DateKeyPattern)));
        }

        void GanttView_HeaderHourChanged(object sender, GanttColumnHeaderEventArgs args)
        {
            string key = args.Time.ToString(_gantt.DateKeyPattern);
            string caption = _gantt.GetJobChgHourCntFormat(args.Time);
            args.ColumnHeader.AddColumn(new XtraSheetHelper.SfColumn(key, caption, _gantt.DefaultColumnWidth, true, false));
            //args.ColumnHeader.AddColumn(new SfGridHelper.SfColumn(key, caption, _gantt.DefaultColumnWidth, true, false));
        }
        #endregion
        #endregion initializeData

        #region BindData

        List<LotGantt.GanttInfo> _list;
        protected void BindData()
        {
            //_gantt.BarTitleGroup = this.BarTitleGroup;
#if DEBUG
            Debug.WriteLine("BindData Start");
            Timer timer = new Timer();

            Debug.WriteLine("Gantt Build");
            timer.Start();
#endif
            _gantt.Build(
                this.SelectedShopID,
                this.StartDate,
                this.EndDate,
                this._planStartTime,
                this.LotIdPattern,
                (int)this.fromShiftComboBox.Value,
                this.InTime
            );

#if DEBUG
            Debug.WriteLine(string.Format("Gantt Build : {0}", timer.Interval));
            timer.Stop();
#endif

            Dictionary<string, LotGantt.GanttInfo> collectData = _gantt.Table;
            _list = new List<LotGantt.GanttInfo>(collectData.Values);

            _gantt.Expand(false);

            BindGrid();
        }

        #endregion BindData

        #region BindGrid
        bool _isFirst = true;
        string _preRowKey = string.Empty;

        Color _preColor = XtraSheetHelper.AltColor;
        Color _currColor = XtraSheetHelper.AltColor2;
        //Color _preColor = SfGridHelper.AltColor;
        //Color _currColor = SfGridHelper.AltColor2;

        int _startSameEqpRowIdx = 0;
        int _startSameRowKeyIdx = 0;
        double _subTotalTI = 0;
        double _subTotalTO = 0;
        double _totalTO = 0;
        int _calCount = 0;
        double _totLoadRate = 0;

        double _rowsumti = 0; //2010.09.13 by ldw
        double _rowsumto = 0; //2010.09.13 by ldw
        double _rowRunHours = 0;
        double _totalRunHours = 0;
        double _totalRowCnt = 0;
        double _queryPeriod = 0;

        string PreLotID { get; set; }

        private void BindGrid()
        {
            _list.Sort(new LotGantt.CompareGanttInfo(LotGantt.SortOptions.LOT_ID));

            _gantt.Workbook.BeginUpdate();
            _gantt.ResetWorksheet();

            //_gantt.Grid.RowCount = 0;
            //_gantt.Grid.ColCount = 0;
            //_gantt.Grid.Cols.DefaultSize = _gantt.DefaultColumnWidth;
            //_gantt.Grid.Rows.DefaultSize = _gantt.DefaultRowHeight;

            SetColumnHeaders();

            var colHeader = _gantt.ColumnHeader;
            _gantt.SchedBarComparer = new CompareMBarList();

            _isFirst = true;
            _preRowKey = string.Empty;

            _preColor = XtraSheetHelper.AltColor;
            _currColor = XtraSheetHelper.AltColor2;
            //_preColor = SfGridHelper.AltColor;
            //_currColor = SfGridHelper.AltColor2;//;
            
#if DEBUG
            var debugValue = _list.FindAll(x => x.LotID == "FAAA611A5");
            { }
#endif

            _gantt.Bind(_list);

            if (this.cellSizeCheck.Checked == false)
            {
                _gantt.Worksheet.SetRowHeight(_gantt.FirstRowIndex, _gantt.LastRowIndex, 40);//this.CellHeightSize);
                //_gantt.Grid.RowHeights.SetRange(_gantt.FirstRowIndex, _gantt.LastRowIndex, 40);
            }
            
            _startSameEqpRowIdx = 0;
            _startSameRowKeyIdx = 0;
            _subTotalTI = 0;
            _subTotalTO = 0;
            _totalTO = 0;

            _calCount = 0;
            _totLoadRate = 0;

            _gantt.Workbook.EndUpdate();
            //_gantt.Grid.Refresh();
        }

        private void SetRowHeaderValue(int rowIndex, string lotID)
        {
#if DEBUG
            if (lotID == "FAAA611A5")
            { }
#endif
            string curKey = CommonHelper.CreateKey(lotID /*stepSeq*/);
            var colHeader = _gantt.ColumnHeader;

            if (_isFirst)
            {
                this.PreLotID = lotID;
                _preRowKey = curKey;
                _startSameEqpRowIdx = rowIndex;
                _startSameRowKeyIdx = rowIndex;

                _isFirst = false;
            }

            if (_isFirst == false && lotID.Equals(this.PreLotID) == false)
            {
                //SfGridHelper.SetTotCellValue(colHeader.GetCellInfo(_startSameEqpRowIdx, ColName.TITotal), _subTotalTI);
                //SfGridHelper.SetTotCellValue(colHeader.GetCellInfo(_startSameEqpRowIdx, ColName.TOTotal), _subTotalTO);

                _startSameEqpRowIdx = rowIndex;
                _subTotalTI = 0;
                _subTotalTO = 0;

                this.PreLotID = lotID;
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

            XtraSheetHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.LotID), lotID);
            //SfGridHelper.SetCellText(colHeader.GetCellInfo(rowIndex, ColName.LotID), lotID);

            colHeader.GetCellInfo(rowIndex, ColName.LotID).Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            colHeader.GetCellInfo(rowIndex, ColName.LotID).Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
        }

        private void MergeRows(int fromRowIdx, int toRowIdx, int colIdx = 0)
        {
            var worksheet = this._gantt.Worksheet;

            worksheet.MergeRowsOneColumn(fromRowIdx, toRowIdx, colIdx);
            //grid1.MergeRowsOneColumn(fromRowIdx, toRowIdx, colIdx);

            if (colIdx > 1)
                SetBorder(fromRowIdx, toRowIdx);
        }

        private void SetBorder(int fromRowIdx, int toRowIdx)
        {
            var worksheet = this._gantt.Worksheet;
            var color = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            //var transparent = Color.Transparent;

            for (int i = fromRowIdx; i <= toRowIdx; i++)
            {
                if (i == fromRowIdx)
                {
                    XtraSheetHelper.SetRowBorderTopLine(worksheet, i, color, BorderLineStyle.Thin);
                    //SfGridHelper.SetRowBorderTopLine(grid1, i, color, GridBorderWeight.Thin);
                }
                else
                {
                    XtraSheetHelper.SetRowBorderTopLine(worksheet, i, Color.Transparent, BorderLineStyle.Thin);
                    //SfGridHelper.SetRowBorderTopLine(grid1, i, transparent, GridBorderWeight.Thin);
                }

                XtraSheetHelper.SetRowBorderBottomLine(worksheet, i, Color.Transparent);
                //SfGridHelper.SetRowBorderBottomLine(grid1, i, transparent);
            }
        }

        private void PaintRowKeyedCell(int rowIndex, Color color)
        {
            var worksheet = this._gantt.Worksheet;

            // ColCount 없음 -> 확인 필요
            for (int colindex = 1; colindex < worksheet.Columns.LastUsedIndex + - 1 /*_columnIndexMap.Count -1 */ ; colindex++)
            {
                worksheet[rowIndex, colindex].FillColor = color;
            }

            //for (int colindex = 1; colindex < grid1.ColCount - 6; colindex++)
            //    grid1[rowIndex, colindex].BackColor = color;
        }

        private void PaintTotColumnCell()
        {
            var worksheet = this._gantt.Worksheet;

            var colHeader = this._gantt.ColumnHeader;

            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(ColName.LoadRate), Color.FromArgb(248, 223, 224));//, 2);
            //worksheet.SelectedCell[0, colHeader.TryGetColumnIndex(ColName.LoadRate)].Style = _gantt.Workbook.Styles["CustomHeaderCenter"];
            //grid1.ColStyles[colHeader.TryGetColumnIndex(ColName.LoadRate)].BackColor = Color.FromArgb(248, 223, 224);

            //grid1.ColStyles[colHeader.TryGetColumnIndex(ColName.ChangRage)].BackColor = Color.FromArgb(248, 223, 224);

            //grid1.ColStyles[colHeader.TryGetColumnIndex(ColName.TIQtySum)].BackColor = Color.FromArgb(219, 236, 216);

            worksheet.SetUsedColumnFillColor(colHeader.TryGetColumnIndex(ColName.RunQty), Color.FromArgb(219, 236, 216));//, 2);
            //worksheet.SelectedCell[0, colHeader.TryGetColumnIndex(ColName.TOQtySum)].Style = _gantt.Workbook.Styles["CustomHeaderCenter"];
            //grid1.ColStyles[colHeader.TryGetColumnIndex(ColName.TOQtySum)].BackColor = Color.FromArgb(219, 236, 216);

            //grid1.ColStyles[colHeader.TryGetColumnIndex(ColName.TITotal)].BackColor = Color.FromArgb(204, 255, 195);
            
            //grid1.ColStyles[colHeader.TryGetColumnIndex(ColName.TOTotal)].BackColor = Color.FromArgb(204, 255, 195);
        }

        #endregion BindGrid

        #region ColName

        struct ColName
        {
            public static string PrcGroup = "EQP_GRP_ID";
            public static string EqpId = "EQP_ID";
            public static string State = "State";
            public static string OperName = "OPERATIONNAME";
            public static string LotID = "LOT ID";
            public static string ProductID = "ProductID";

            public static string StartTime = "START_TIME";
            public static string EndTime = "END_TIME";
            public static string GapTime = "GAP_TIME";

            public static string TIQty = "T/I QTY";
            public static string TOQty = "T/O QTY";

            public static string LoadRate = "LOAD";
            public static string ChangRage = "CHANGE";

            public static string TIQtySum = "T/I\nQTY";
            public static string TOQtySum = "T/O\nQTY";
            public static string TITotal = "T/I\nTOTAL";
            public static string TOTotal = "T/O\nTOTAL";
            public static string RunQty = "RUN QTY";
        }

        #endregion

        #region // SetColumnHeader
        protected void SetColumnHeaders()
        {
            int colCount = Convert.ToInt32(fromShiftComboBox.Value) * (int)ShopCalendar.ShiftHours + 4;//부하율, 교체율(+3)
            //int colCount = Convert.ToInt32(fromShiftComboBox.Value) * 6 + 4;//부하율, 교체율(+3)
            _gantt.FixedColCount = 1;
            _gantt.FixedRowCount = 2;

            string lotIDCaption = new StringBuilder().AppendFormat("{0}({1})", ColName.LotID, _gantt.TableCount).ToString();

            _gantt.SetColumnHeaders(colCount, new XtraSheetHelper.SfColumn(ColName.LotID, ColName.LotID, 100));

            //_gantt.SetColumnHeaders(colCount,
            //     new XtraSheetHelper.SfColumn(ColName.LotID, "LOT_NO (STRIP_MARK)", headerWidth1 == null ? 150 : Convert.ToInt16(headerWidth1)),
            //    //new XtraSheetHelper.SfColumn(ColName.StripMark, "STRIP_MARK", 50),
            //     new XtraSheetHelper.SfColumn(ColName.SeqRate, "SEQ ", headerWidth2 == null ? 30 : Convert.ToInt16(headerWidth2)),
            //     new XtraSheetHelper.SfColumn(ColName.ShipBack, "SHIPBACK", headerWidth3 == null ? 50 : Convert.ToInt16(headerWidth3)),
            //     new XtraSheetHelper.SfColumn(ColName.Category, "구분 ", headerWidth4 == null ? 30 : Convert.ToInt16(headerWidth4))
            //     );
        }

        #endregion

        #region //Event Handlers

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


        private void btnQuery_Click(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            _queryPeriod = this.EndDate > this.StartDate ? (this.EndDate - this.StartDate).TotalHours : 0;

            Search();
        }

        private List<LotGantt.GanttInfo> GetGanttInfo(string lotID)
        {
            List<LotGantt.GanttInfo> result = new List<LotGantt.GanttInfo>();

            foreach (LotGantt.GanttInfo info in _list)
            {
                if (info.LotID == lotID)
                    result.Add(info);
            }
            if (result.Count > 0)
                return result;
            else
                return null;
        }
        #endregion

        private void ganttSizeControl1_CellHeightChanged(object sender, EventArgs e)
        {
            SaveCellSize();
        }

        private void ganttSizeControl1_CellWidthChanged(object sender, EventArgs e)
        {
            SaveCellSize();
        }

        private void SaveCellSize()
        {
            Globals.SetLocalSetting(this.ServiceProvider, _pageID + "ganttCellWidth", this.CellWidthSize.ToString());
            Globals.SetLocalSetting(this.ServiceProvider, _pageID + "ganttCellHeight", this.CellHeightSize.ToString());
        }

   
    }
}
