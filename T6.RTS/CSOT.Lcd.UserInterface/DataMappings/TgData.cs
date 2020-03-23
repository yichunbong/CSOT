using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.Lcd.UserInterface.Gantts;

namespace CSOT.Lcd.UserInterface.DataMappings
{
    public class TgData
    {        
        public class ToolSchedInfo
        {
            public string ShopID { get; private set; }
            public string EqpId { get; private set; }
            public string ProductId { get; private set; }
            public string ProcessId { get; private set; }
            public string Layer { get; private set; }
            public string ToolId { get; private set; }
            public string EqpGroup { get; private set; }
            public string StepId { get; private set; }
            public string LotId { get; private set; }                        
            public DateTime StartTime { get; private set; }
            public DateTime EndTime { get; private set; }
            public DateTime TkInTime { get; private set; }
            public DateTime TkOutTime { get; private set; }            
            public int Qty { get; private set; }
            public EqpState State { get; private set; }            
            public EqpMaster.Eqp EqpInfo { get; private set; }

            public ToolSchedInfo(
                string shopID,
                string eqpId,
                string productId,
                string processId,
                string layer,
                string toolID,
                string eqpGroup,
                string stepId,
                string lotId,
                DateTime startTime,
                DateTime endTime,
                DateTime tkInTime,
                DateTime tkOutTime,
                int qty,
                EqpState state,
                EqpMaster.Eqp eqpInfo)
            {
                this.ShopID = shopID;
                this.EqpId = eqpId;
                this.ProductId = productId;
                this.ProcessId = processId;
                this.Layer = layer;
                this.ToolId = toolID;
                this.EqpGroup = eqpGroup;
                this.StepId = stepId;
                this.LotId = lotId;
                this.StartTime = startTime;
                this.EndTime = endTime;
                this.TkInTime = tkInTime;
                this.TkOutTime = tkOutTime; 
                this.Qty = qty;
                this.State = state;                
                this.EqpInfo = eqpInfo;
            }
        }
    }
}
