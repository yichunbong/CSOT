using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using DevExpress.XtraEditors;
using DevExpress.XtraSpreadsheet;
using Mozart.Collections;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using CSOT.Lcd.UserInterface.Common;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.UserInterface.Gantts;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling;
using CSOT.Lcd.Scheduling.Outputs;

namespace CSOT.Lcd.UserInterface.ToolGantts
{
    public enum GanttType
    {
        Default
    }

    public enum MouseSelectType
    {
        Product,
        Mask,
        Process,
        Pattern
    }

    public class ToolGanttMaster : GanttView
    {
        public bool IsOnlyToolMode { get; private set; }
       
        public MouseSelectType MouseSelType { get; set; }

        public IExperimentResultItem _result;
        public ResultDataContext _resultCtx;

        protected DateTime _planStartTime;

        private EqpMaster _eqpMgr;
        //private ColorGenerator ColorGen { get; set; }

        private HashedSet<string> ToolList { get; set; }

        public string TargetShopID { get; set; }

        public HashSet<string> SelectedProdList { get; set; }
        public HashSet<string> SelectedStepList { get; set; }
        public IList<string> SelectedToolList { get; set; }

        public List<string> _visibleItems;

        public ToolGanttMaster(
            SpreadsheetControl grid,
            IExperimentResultItem result,
            ResultDataContext resultCtx, 
            string targetShopID,
            DateTime planStartTime,
            EqpMaster eqpMgr
        )
            : base(grid)
        {
            _result = result;
            _resultCtx = resultCtx;

            this.TargetShopID = targetShopID;

            _planStartTime = planStartTime;
            _eqpMgr = eqpMgr;

            this.EnableSelect = false;

            //this.ColorGen = new ColorGenerator();
            _visibleItems = new List<string>();

            this.ToolList = GetToolList(this.TargetShopID);
        }

        #region from GanttOption

        public void TurnOnSelectMode() { this.EnableSelect = true; }

        public void TurnOffSelectMode() { this.EnableSelect = false; }

        BrushInfo brushEmpth = new BrushInfo(Color.Transparent);
        public BrushInfo GetBrushInfo(ToolBar bar, string patternOfProdID)
        {
            BrushInfo brushinfo = null;

            if (bar.State == EqpState.SETUP)
                brushinfo = new BrushInfo(Color.Red);
            else if (bar.State == EqpState.PM)
                brushinfo = new BrushInfo(HatchStyle.Divot, Color.Black, Color.OrangeRed);
            else if (bar.State == EqpState.DOWN)
                brushinfo = new BrushInfo(HatchStyle.Percent30, Color.Gray, Color.Black);
            else if (bar.IsGhostBar)
                brushinfo = new BrushInfo(HatchStyle.Percent30, Color.LightGray, Color.White);
            else
            {
                var color = ColorGenHelper.GetColorByKey(bar.ToolID, ColorGenHelper.CATETORY_MASK); //ColorGen.GetColor(bar.ToolID);

                if (bar.State == EqpState.IDLERUN)
                    brushinfo = new BrushInfo(HatchStyle.Percent30, Color.Black, color);
                else
                    brushinfo = new BrushInfo(color);                       
            }

            var selBar = this.SelectedBar;

            if (!this.EnableSelect || selBar == null)
            {
                bar.BackColor = brushinfo.BackColor;
                return brushinfo;
            }

            if (!CompareToSelectedBar(bar, patternOfProdID))
            {
                bar.BackColor = brushEmpth.BackColor;
                return brushEmpth;
            }

            bar.BackColor = brushinfo.BackColor;
            return brushinfo;
        }

        public bool CompareToSelectedBar(ToolBar bar, string patternOfProdID)
        {
            if (bar.IsGhostBar)
                return true;

            if (this.MouseSelType == MouseSelectType.Pattern)
            {
                if (string.IsNullOrEmpty(patternOfProdID))
                    return true;

                bool isLike = StringHelper.Like(bar.ProductId, patternOfProdID);

                return isLike;
            }

            var selBar = this.SelectedBar as ToolBar;

            if (selBar == null)
                return true;

            if (this.MouseSelType == MouseSelectType.Product)
            {
                return selBar.ProductId == bar.ProductId;
            }
            else if (this.MouseSelType == MouseSelectType.Process)
            {
                return selBar.ProcessId == bar.ProcessId;
            }
            else if (this.MouseSelType == MouseSelectType.Mask)
            {
                return selBar.ToolID == bar.ToolID;
            }

            return false;
        }

        private string GetLotId(string lotId)
        {
            int idx = lotId.IndexOf('_');
            if (idx > 0)
                return lotId.Substring(0, idx);

            return lotId;
        }

        #endregion

        private HashedSet<string> GetToolList(string targetShopID)
        {
            HashedSet<string> list = new HashedSet<string>();
            
            var modelContext = this._result.GetCtx<ModelDataContext>();

            var table = modelContext.Tool;
            foreach (var item in table)
            {
                if (item.SHOP_ID != targetShopID)
                    continue;

                string toolID = item.TOOL_ID;
                if (string.IsNullOrEmpty(toolID))
                    continue;

                list.Add(toolID);
            }

            return list;
        }

        public virtual void ClearData()
        {
            this.Clear();
        }

        #region Job Change

        public string GetJobChgHourCntFormat(DateTime targetTime)
        {
            string hourString = targetTime.Hour.ToString();
            string chgTime = targetTime.ToString(this.DateKeyPattern);

            return string.Format("{0}", hourString);
        }

        public string GetJobChgShiftCntFormat(DateTime shiftTime)
        {
            string shift = shiftTime.ToString(this.DateGroupPattern);

            return string.Format("{0}", shift);
        }

        #endregion

        #region Visible Item

        // 특정 pattern으로 조회 했을 때
        // pattern 과 일치하는 prodCode 를 진행한 설비를 collection에 보관하고,
        // EqpGantt --> Expand() 에서 Filtering하여 화면에 보이지 않게 한다.
        public virtual void AddVisibleItem(string eqpId, string prodCode, string pattern)
        {
        }

        public void AddVisibleItem(string item)
        {
            if (_visibleItems.Contains(item) == false)
                _visibleItems.Add(item);
        }

        protected bool IsVisibleItem(string item)
        {
            if (_visibleItems == null || _visibleItems.Count == 0)
                return true;

            return _visibleItems.Contains(item);
        }

        #endregion
                                
        public void BuildGantt(
            bool isOnlyToolMode,
            string selectedShopId,
            HashSet<string> selectedProdList,
            HashSet<string> selectedStepList,
            IList<string> selectedToolList,
            DateTime fromTime,
            DateTime toTime,
            DateTime planStartTime,
            string eqpPattern
        )
        {
            ClearData();

            this.IsOnlyToolMode = isOnlyToolMode;

            this.FromTime = fromTime;
            this.ToTime = toTime;

            this.SelectedProdList = selectedProdList;
            this.SelectedStepList = selectedStepList;
            this.SelectedToolList = selectedToolList;

            var planList = GetPlanData();
            FIllGantt(planList, selectedShopId, fromTime, toTime);
        }

        protected List<EqpPlan> GetPlanData()
        {
            var eqpPlan = _resultCtx.EqpPlan;

            var finds = eqpPlan.Where(t => IsMatchedEqpPlan(t, this.FromTime, this.ToTime, this.SelectedProdList, this.SelectedStepList, this.SelectedToolList));

            var list = finds.ToList();
            list.Sort(ComparerEqpPlan);

            return list;
        }

        private bool IsMatchedEqpPlan(EqpPlan info, DateTime fromTime, DateTime toTime, HashSet<string> prodList, HashSet<string> stepList, IList<string> toolList)
        {
            if (info == null)
                return false;

            if (info.START_TIME >= toTime)
                return false;

            if (info.END_TIME <= fromTime)
                return false;

            if (prodList != null)
            {
                if (prodList.Contains(info.PRODUCT_ID) == false)
                    return false;
            }

            if (stepList != null)
            {
                if (stepList.Contains(info.STEP_ID) == false)
                    return false;
            }

            if (toolList != null)
            {
                if (toolList.Contains(info.TOOL_ID) == false)
                    return false;
            }

            return true;
        }

        public static int ComparerEqpPlan(EqpPlan x, EqpPlan y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = string.Compare(x.EQP_ID, y.EQP_ID);

            if (cmp == 0)
            {
                DateTime x_START_TIME = x.START_TIME.GetValueOrDefault();
                DateTime y_START_TIME = y.START_TIME.GetValueOrDefault();

                cmp = DateTime.Compare(x_START_TIME, y_START_TIME);
            }

            return cmp;
        }


        private void FIllGantt(List<EqpPlan> planList, string selectedShopId, DateTime fromTime, DateTime toTime)
        {
            Dictionary<string, List<TgData.ToolSchedInfo>> toolSchedListDic = new Dictionary<string, List<TgData.ToolSchedInfo>>();

            foreach (var item in planList)
            {
                string toolID = item.TOOL_ID;
                if (string.IsNullOrEmpty(toolID) || toolID == "-")
                    continue;

                if (this.ToolList.Contains(toolID))
                    this.ToolList.Remove(toolID);

                List<TgData.ToolSchedInfo> toolSchedList;
                if (toolSchedListDic.TryGetValue(toolID, out toolSchedList) == false)
                    toolSchedListDic.Add(toolID, toolSchedList = new List<TgData.ToolSchedInfo>());
                
                var eqp = _eqpMgr.FindEqp(item.EQP_ID);
                if(eqp == null)
                    continue;

                //상태시작시간                        
                DateTime startTime = item.START_TIME.GetValueOrDefault(toTime);
                if (startTime >= toTime)
                    continue;

                //상태종료시간
                DateTime endTime = item.END_TIME.GetValueOrDefault(toTime);
                if (endTime <= fromTime)
                    continue;

                DateTime tkInTime = item.TRACK_IN_TIME.GetValueOrDefault(toTime);
                DateTime tkInEndTime = item.TRACK_OUT_TIME.GetValueOrDefault(toTime);
               
                int qty = (int)item.UNIT_QTY;

                TgData.ToolSchedInfo toolSched = new TgData.ToolSchedInfo(item.SHOP_ID,
                    item.EQP_ID, item.PRODUCT_ID, item.PROCESS_ID, item.LAYER_ID, item.TOOL_ID,
                    item.EQP_GROUP_ID, item.STEP_ID, item.LOT_ID, startTime, endTime,
                    tkInTime, tkInEndTime, qty, EqpState.BUSY, eqp);

                toolSchedList.Add(toolSched);
            }

            foreach (List<TgData.ToolSchedInfo> list in toolSchedListDic.Values)
            {
                List<TgData.ToolSchedInfo>  sortedList = list.OrderBy(x => x.TkInTime).ToList();

                int count = sortedList.Count;
                for (int i = 0; i < count; i++)
                {
                    var info = sortedList[i];

                    bool isLast = i == count - 1;
                    var next = isLast ? null : sortedList[i + 1];

                    DateTime nextStateStartTime = next != null ? next.StartTime : toTime;
                                           
                    AddItem(info.EqpInfo,
                            info.ShopID, 
                            info.ToolId,
                            info.ProductId,
                            info.ProcessId, 
                            info.Layer, 
                            info.StepId,
                            info.LotId,
                            info.StartTime,
                            info.EndTime,
                            info.TkInTime,
                            info.TkOutTime,
                            nextStateStartTime,
                            info.Qty,
                            info.State);
                }
            }

            var dummyEqp = EqpMaster.Eqp.CreateDummy(this.TargetShopID, "-");
            foreach (var toolID in this.ToolList)
            {
                if (string.IsNullOrEmpty(toolID))
                    continue;

                if (this.SelectedToolList.Contains(toolID) == false)
                    continue;

                AddItem(dummyEqp, this.TargetShopID, toolID, 
                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                    _planStartTime, _planStartTime, _planStartTime, _planStartTime, _planStartTime, 0, EqpState.IDLE);
            }
        }

        public virtual void AddItem(
            EqpMaster.Eqp eqpInfo,
            string shopID,
            string toolID,
            string productId,
            string processId,
            string layer,
            string stepId,
            string lotId,
            DateTime startTime,
            DateTime endTime,
            DateTime tkInTime,
            DateTime tkOutTime,
            DateTime nextStateStartTime,
            int qty,
            EqpState state,
            DataRow dispatchingInfo = null
        )
        {
        }
    }
    
    public class CompareMBarList : IComparer<LinkedBarNode>
    {
        #region IComparer<List<LinkedBar>> 멤버

        public int Compare(LinkedBarNode x, LinkedBarNode y)
        {
            Bar a = x.LinkedBarList[0].BarList[0];
            Bar b = y.LinkedBarList[0].BarList[0];

            int cmp = a.TkinTime.CompareTo(b.TkinTime);

            if (cmp == 0)
                cmp = y.LinkedBarList.Count.CompareTo(x.LinkedBarList.Count);

            return cmp;
        }
        #endregion
    }
}
