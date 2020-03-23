using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.Lcd.UserInterface.Common;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Gantts
{
    public class GanttBar : Bar
    {
        public string EqpGroup;
        public string EqpID;
        public string SubEqpID;
        public string Layer;        

        public string LotID;
        public string OrigLotID;
        public string WipInitRun;

        public string ProductID;
        public string ProductVersion;
        public string OwnerType;

        public string ProcessID;              
        public string StepID;         
       
        public string ToolID;

        public EqpMaster.Eqp EqpInfo;
        public DataRow DispatchingInfo;
        
        public Color BackColor;

        public bool IsGhostBar;
        public int LotPriority;
        public string EqpRecipe;
        public string StateInfo;

        //Eqp 투입을 기준으로 Bar 기록하므로 TkinTime = StateStartTime으로 처리
        public DateTime StartTime 
        { 
            get { return this.TkinTime; } 
        }

        //Eqp 투입을 기준으로 Bar 기록하므로 TkoutTime = StateEndTime으로 처리
        public DateTime EndTime
        {
            get { return this.TkoutTime; }
        }
        
        public string BarKey
        {
            get
            {
                if (this.IsGhostBar)
                    return this.Layer;

                return CommonHelper.CreateKey(this.ProductID, this.ProductVersion);
            }
        }

        public GanttBar(
            string eqpGroup,
            string eqpID,
            string subEqpID,
            string layer,
            string lotID,
            string origLotID,
            string wipInitRun,
            string productID,
            string productVersion,
            string ownerID,
            string processID,            
            string stepID,
            string toolID,            
            DateTime startTime,
            DateTime endTime,
            int inQty,                        
            EqpState state,
            EqpMaster.Eqp info,
            DataRow dispatchingInfo,
            int lotPriority,
            string eqpRecipe,
            string stateInfo,
            bool isGhostBar = false)
                : base(startTime, endTime, inQty, 0, state)
        {
            this.EqpGroup = eqpGroup;
            this.EqpID = eqpID;
            this.SubEqpID = subEqpID;
            this.Layer = layer;

            this.LotID = lotID;
            this.OrigLotID = origLotID;
            this.WipInitRun = wipInitRun;

            this.ProductID = productID;
            this.ProductVersion = productVersion;
            this.OwnerType = ownerID;            

            this.ProcessID = processID;          
            this.StepID = stepID;           
            
            this.ToolID = toolID;
            this.EqpInfo = info;

            this.DispatchingInfo = dispatchingInfo;

            this.LotPriority = lotPriority;
            this.EqpRecipe = eqpRecipe;
            this.StateInfo = stateInfo;

            this.IsGhostBar = isGhostBar;
        }
        
        public string GetTitle(bool isProduct)
        {
            if (this.State == EqpState.PM)
            {
                return this.State.ToString();
            }

            if (this.State == EqpState.SETUP)
            {
                return this.State.ToString();
            }                

            if (this.IsGhostBar)
            {
                return this.Layer;
            }

            if (isProduct)
            {
                if (this.State != EqpState.BUSY)
                    return string.Format("{0}", this.State);

                return string.Format("{0}/{1}/{2}({3}/{4})", this.ProductID, this.ProductVersion, this.StepID, this.OwnerType, TIQty.ToString());
            }
            else
            {
                if (this.State != EqpState.BUSY)
                    return string.Format("{0}", this.State);

                return string.Format("{0}-{1}\n({2})", this.LotID, this.StepID, TIQty.ToString());
            }
        }
    }
}
