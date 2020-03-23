using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DevExpress.XtraSpreadsheet;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using CSOT.Lcd.UserInterface.Common;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.UserInterface.Gantts;
using CSOT.Lcd.Scheduling;

namespace CSOT.Lcd.UserInterface.ToolGantts
{
    public class ToolGantt : ToolGanttMaster
    {
        public enum ViewMode
        {
            SHOP,
            MASK,
            EQP
        }
        
        
        Dictionary<string, GanttInfo> _table;

        public Dictionary<string, GanttInfo> Table
        {
            get { return _table; }
        }

        public ToolGantt(SpreadsheetControl grid,
            IExperimentResultItem result,
            ResultDataContext resultCtx,
            string targetShopID,
            DateTime planStartTime,
            EqpMaster eqpMgr
        )
            : base(grid, result, resultCtx, targetShopID, planStartTime, eqpMgr)
        {
            _table = new Dictionary<string, GanttInfo>();
        }

        public override void ClearData()
        {
            base.ClearData();
            _table.Clear();
        }

        public class GanttInfo : GanttItem
        {
            //EqpGantt
            public string ShopID;

            public string ToolID;
            public int LayerSortSeq;
            public int LayerSortSeqByOption;

            public EqpMaster.Eqp EqpInfo;

            public string EqpGroup 
            {
                get { return this.EqpInfo.EqpGroup; }
            }

            public string EqpID
            {
                get { return this.EqpInfo.EqpID;  }
            }


            public GanttInfo(EqpMaster.Eqp eqpInfo, string shopID, string toolID, int layerStdStepSeq = int.MaxValue)
                : base()
            {
                this.EqpInfo = eqpInfo;
                this.ShopID = shopID;
                this.ToolID = toolID;
                this.LayerSortSeq = layerStdStepSeq;
            }

            public override void AddLinkedNode(Bar bar, LinkedBarNode lnkBarNode)
            {
                base.AddLinkedNode((bar as ToolBar).BarKey, lnkBarNode);
            }

            protected override bool CheckConflict(bool isDefault, Bar currentBar, Bar prevBar)
            {
                return isDefault && (currentBar as ToolBar).BarKey != (prevBar as ToolBar).BarKey;
            }

            public void AddItem(string key, ToolBar bar, bool isOnlyToolMode, bool isSplitA = false)
            {
                BarList list;
                if (this.Items.TryGetValue(key, out list) == false)
                {
                    this.Items.Add(key, list = new BarList());
                    list.Add(bar);
                    return;
                }

                foreach (ToolBar it in list)
                {
                    if (it.ToolID != bar.ToolID || it.State != bar.State)
                        continue;

                    if (isOnlyToolMode == false)
                    {
                        if (it.EqpId != bar.EqpId || it.State != bar.State)
                            continue;
                    }

                    if (it.IsShiftSplit || (bar.IsShiftSplit && isSplitA == false)
                    && it.State.Equals(bar.State))
                        return;

                    if (it.Merge(bar))
                    {
                        it.TOQty += bar.TOQty;
                        return;
                    }
                }

                list.Add(bar);
            }

            public Bar GetBarItem(string key, string lotId)
            {
                BarList list;
                if (this.Items.TryGetValue(key, out list) == false)
                    return null;

                return list.FindLast(t => (t as ToolBar).LotId == lotId);
            }

        }

        public override void  AddItem(
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
            DataRow dispatchingInfo = null)
        {
            GanttInfo info = (GanttInfo)TryGetItem(eqpInfo, shopID, toolID, stepId);
            
            //BUSY
            DoAddItem(info, productId, processId, layer, stepId, lotId,
                startTime, endTime, qty, state, eqpInfo, dispatchingInfo);
            
            //IDLE RUN
            DateTime barStartTime = endTime;
            DateTime barEndTime = tkOutTime;

            if (barEndTime > nextStateStartTime)
                barEndTime = nextStateStartTime;
            
            if (barStartTime < barEndTime)
            {                
                DoAddItem(info, productId, processId, layer, stepId, lotId,
                    barStartTime, barEndTime, 0, EqpState.IDLERUN, eqpInfo, dispatchingInfo);
            }
        }

        private void DoAddItem(
            GanttInfo info,
            string productId,
            string processId,
            string layer,            
            string stepId,
            string lotId,
            DateTime startTime,
            DateTime endTime,
            int qty,                        
            EqpState state,
            EqpMaster.Eqp eqpInfo,
            DataRow dispatchingInfo)
        {
            string shopID = info.ShopID;
            string eqpId = info.EqpID;
            string toolID = info.ToolID;

            ToolBar currentBar = new ToolBar(
                shopID,
                eqpId,
                productId,
                processId,
                layer,
                toolID,
                stepId,
                lotId,
                startTime,
                endTime,
                qty,
                state,
                eqpInfo,
                dispatchingInfo,
                false
                );

            var barKey = state != EqpState.DOWN ? currentBar.BarKey : "DOWN";

            if (barKey != string.Empty)
                info.AddItem(barKey, currentBar, this.IsOnlyToolMode);
        }

        public object TryGetItem(EqpMaster.Eqp eqpInfo, string shopId, string toolID, string stepID = "-")
        {
            string eqpId = eqpInfo.EqpID;

            string key = string.Empty;
            if (this.IsOnlyToolMode)
                key = CommonHelper.CreateKey(shopId, toolID);
            else
                key = CommonHelper.CreateKey(eqpId, toolID);
            
            GanttInfo info;
            if (_table.TryGetValue(key, out info) == false)
            {
                info = new GanttInfo(eqpInfo, shopId, toolID, 0);
                _table.Add(key, info);
            }

            return info;
        }
        
        public void Expand(bool isDefault)
        {
            foreach (GanttInfo info in _table.Values)
            {
                if (IsVisibleItem(info.EqpID) == false)
                    continue;

                info.Expand(isDefault);
                info.LinkBar(this,isDefault);
            }
        }

        public override void AddVisibleItem(string eqpId, string prodCode, string pattern)
        {
            if (string.IsNullOrEmpty(pattern) == false)
            {
                if (StringHelper.Like(prodCode, pattern))
                    base.AddVisibleItem(eqpId);
            }
            else
            {
                base.AddVisibleItem(eqpId);
            }
        }

        public enum SortOptions
        {
            LAYER,
            EQP_ID,
            STEP_SEQ,
            TOOL_ID
        }

        public class CompareGanttInfo : IComparer<GanttInfo>
        {
            private SortOptions[] sortList;

            public CompareGanttInfo(params SortOptions[] sortList)
            {
                this.sortList = sortList;
            }

            public int Compare(GanttInfo x, GanttInfo y)
            {
                int iCompare = 0;
                foreach (var sort in sortList)
                {
                    if (iCompare == 0)
                        iCompare = Compare(x, y, sort);
                }

                return iCompare;
            }
            
            private int Compare(GanttInfo x, GanttInfo y, SortOptions sort)
            {
                //"설비그룹" , "EQP ID" , "설비명" , "STEP ID" , "PROD CODE"                
                if (sort == SortOptions.EQP_ID)
                {
                    bool empty_x = string.IsNullOrEmpty(x.EqpID) || x.EqpID == "-";
                    bool empty_y = string.IsNullOrEmpty(y.EqpID) || y.EqpID == "-";

                    int cmp = empty_x.CompareTo(empty_y);

                    if (cmp == 0)
                        cmp = x.EqpID.CompareTo(y.EqpID);

                    return cmp;
                }
                else if (sort == SortOptions.STEP_SEQ)
                {

                }
                else if (sort == SortOptions.TOOL_ID)
                {
                    return x.ToolID.CompareTo(y.ToolID);
                }
                else if (sort == SortOptions.LAYER)
                {
                    int xSeq = x.LayerSortSeq;
                    int ySeq = y.LayerSortSeq;

                    return xSeq.CompareTo(ySeq);
                }

                return 0;
            }
        }
    }
}
