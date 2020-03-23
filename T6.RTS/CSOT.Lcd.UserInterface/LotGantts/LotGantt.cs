using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DevExpress.XtraSpreadsheet;

//using Mzui.Windows.Forms.Grid;

using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;

using CSOT.Lcd.UserInterface.Common;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.LotGantts
{
    class LotGantt : GanttMaster
    {
        Dictionary<string, GanttInfo> _table;

        public Dictionary<string, GanttInfo> Table
        {
            get { return _table; }
        }

        public int TableCount 
        { 
            get 
            {
                if (this.Table == null)
                    return 0;

                return this.Table.Count;
            } 
        }

        public LotGantt
            (
                //GridControl grid,
                SpreadsheetControl grid,
                IExperimentResultItem result,
                DateTime planStartTime
            )
            : base(grid, result, planStartTime, GanttType.Lot)
        {
            _table = new Dictionary<string, GanttInfo>();
        }

        public override void ClearData()
        {
            base.ClearData();
            _table.Clear();
        }

        public override void AddItem(string lotID, string processID, string stepSeq, string productID, DateTime tkin, DateTime tkout, int qty, EqpState state, DateTime toTime)
        {
            var info = this.TryGetItem(lotID);
            this.DoAddItem(lotID, processID, stepSeq, productID, qty, tkin, tkout, toTime, info as GanttInfo);
        }

        private void DoAddItem(string lotId, string processID, string stepSeq, string productID, int qty, DateTime tkin, DateTime tkout, DateTime toTime, GanttInfo info)
        {
#if DEBUG
            if (lotId == "FAAA611A5")
            { }
#endif
            DateTime endOfShiftTime = toTime;

            GanttBar splitBarA = null;
            GanttBar splitBarB = null;
            GanttBar currentBar = null;

            bool isSplit = false;

            // Shift Time에 걸쳐 있을 경우
            if (tkin < endOfShiftTime && endOfShiftTime <= tkout)
            {
                splitBarA = new GanttBar(lotId, processID, stepSeq, productID, tkin, endOfShiftTime, qty, qty, EqpState.NONE);
                splitBarB = new GanttBar(lotId, processID, stepSeq, productID, endOfShiftTime, tkout, qty, qty, EqpState.NONE);

                splitBarA.IsShiftSplit = splitBarB.IsShiftSplit = isSplit = true;
            }
            else
            {
                currentBar = new GanttBar(lotId, processID, stepSeq, productID, tkin, tkout, qty, qty, EqpState.NONE);
            }

            if (isSplit == true)
            {
                info.AddItem(splitBarA.BarKey, splitBarA);
                info.AddItem(splitBarB.BarKey, splitBarB);
            }
            else
            {
                info.AddItem(currentBar.BarKey, currentBar);
            }
        }

        public override object TryGetItem(string lotID)
        {
            string key = CommonHelper.CreateKey(lotID);

            GanttInfo info;
            if (_table.TryGetValue(key, out info) == false)
            {
                info = new GanttInfo(lotID);
                _table.Add(key, info);
            }

            return info;
        }

        public void Expand(bool isDefault)
        {
            foreach (GanttInfo info in _table.Values)
            {
                info.Expand(isDefault);
                info.LinkBar(this, isDefault);
            }
        }

        public virtual void TryGetTotChgAndLoadRate(out double loadRate, out double chgRate, bool isFixMachine)
        {
            loadRate = 0;
            chgRate = 0;
        }

        public class GanttInfo : GanttItem
        {
            //EqpGantt 
            public string LotID { get; private set; }

            public GanttInfo(string lotID)
                : base()
            {
                this.LotID = lotID;
            }

            public override void AddLinkedNode(Bar bar, LinkedBarNode lnkBarNode)
            {
                base.AddLinkedNode((bar as GanttBar).BarKey, lnkBarNode);
            }

            protected override bool CheckConflict(bool isDefault, Bar currentBar, Bar prevBar)
            {
                return isDefault && (currentBar as GanttBar).BarKey != (prevBar as GanttBar).BarKey;
            }

            public void AddItem(string key, GanttBar bar)
            {
                BarList list;
                if (!this.Items.TryGetValue(key, out list))
                {
                    this.Items.Add(key, list = new BarList());
                    list.Add(bar);
                    return;
                }
                /* currentBar가 이전에 같은 step 조건에서 같은 lotid에 대한 작업일 때 
                               2013-03-13 정용호
                               */
                foreach (GanttBar it in list)
                {
                    if (it.LotID != bar.LotID || it.State != bar.State)
                        continue;

                    if (it.IsShiftSplit || bar.IsShiftSplit
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

                return list.FindLast(t => (t as GanttBar).LotID == lotId);
            }

        }

        public enum SortOptions
        {
            LAYER,
            LOT_ID,
            STEP_SEQ
            //PROCESS_GROUP
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
                if (sort == SortOptions.LOT_ID)
                {
                    return x.LotID.CompareTo(y.LotID);
                }

                return 0;
            }
        }
    }
}
