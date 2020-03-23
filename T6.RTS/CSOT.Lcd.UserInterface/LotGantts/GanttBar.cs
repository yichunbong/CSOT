using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.TaskModel.UserLibrary.GanttChart;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSOT.Lcd.UserInterface.LotGantts
{
    public class GanttBar : Bar
    {
        #region Property
        public string LotID { get; private set; }
        public string ProcessID { get; private set; }
        public string StepSeq { get; private set; }
        public string ProductID { get; private set; }

        public string BarKey { get { return this.LotID; } }

        public Color BackColor { get; set; }
        #endregion Property

        public GanttBar(string lotID, string processID, string stepSeq, string productID, DateTime tkin, DateTime tkout, int tiqty, int toqty, EqpState state)
            :base(tkin, tkout, tiqty, toqty, state)
        {
            this.LotID = lotID;
            this.ProcessID = processID;
            this.StepSeq = stepSeq;
            this.ProductID = productID;
        }

        public string GetTitle()
        {
            return string.Format("{0}/({1})", this.LotID, TIQty.ToString());
        }
    }
}
