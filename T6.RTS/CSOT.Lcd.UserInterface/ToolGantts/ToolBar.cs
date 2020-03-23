using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;

using CSOT.Lcd.UserInterface.Common;

using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using CSOT.UserInterface.Utils;
using CSOT.Lcd.UserInterface.Gantts;

namespace CSOT.Lcd.UserInterface.ToolGantts
{
    public class ToolBar : Bar
    {
        public string ShopID;
        public string LotId;
        public string ProductId;
        public string ProcessId;
        public string ToolID;
        public string Layer;
        public string EqpId;
        public string StepId;

        public EqpMaster.Eqp EqpInfo;
        public DataRow DispatchingInfo;        
        public Color BackColor;

        public bool IsGhostBar;

        public string BarKey
        {
            get
            {             
                return IsGhostBar == true ? this.ToolID : CommonHelper.CreateKey(this.ProductId, this.ToolID);
            }
        }

        public ToolBar(
        string shopID,
        string eqpId,
        string productId,
        string processId,
        string layer,
        string toolID,
        string stepId,
        string lotId,
        DateTime startTime,
        DateTime endTime,        
        int qty,                
        EqpState state,
        EqpMaster.Eqp info,
        DataRow dispatchingInfo = null,
        bool isGhostBar = false)
            : base(startTime, endTime, qty, qty, state)
        {
            this.ShopID = shopID;
            this.EqpId = eqpId;
            this.ProductId = productId;
            this.ProcessId = processId;
            this.Layer = layer;
            this.ToolID = toolID;
            this.StepId = stepId;
            this.LotId = lotId;
            this.EqpInfo = info;
            this.DispatchingInfo = dispatchingInfo;
            this.IsGhostBar = isGhostBar;
        }
        
        public string GetTitle(bool isOnlyToolMode)
        {
            if (this.State == EqpState.PM)
                return this.State.ToString();
            else if (this.State == EqpState.SETUP)
                return "MOVE";

            if (this.IsGhostBar)
            {
                return this.Layer;
            }

            if (isOnlyToolMode)
            {
                if (this.State != EqpState.BUSY)
                    return string.Format("{0}", this.State);

                return string.Format("{0}/{1}/{2}({3})", this.EqpId, this.ProductId, this.StepId, TIQty.ToString());
            }
            else
            {
                if (this.State != EqpState.BUSY)
                    return string.Format("{0}", this.State);

                return string.Format("{0}-{1}\n({2})", this.ProductId, this.StepId, TIQty.ToString());
            }
        }    
    }
}
